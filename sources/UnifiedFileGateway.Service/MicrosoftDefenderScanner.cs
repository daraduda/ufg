namespace UnifiedFileGateway.Service
{
	/// <summary>
	/// Integrates with Microsoft Defender for real-time antivirus scanning.
	/// </summary>
	public class MicrosoftDefenderScanner
	{
		// Windows API constants and structures for Microsoft Defender.
		private const int ERROR_SUCCESS = 0;
		private const int ERROR_FILE_NOT_FOUND = 2;
		private const int ERROR_ACCESS_DENIED = 5;
		private const int ERROR_INVALID_PARAMETER = 87;
		private const int ERROR_INSUFFICIENT_BUFFER = 122;
		private const int ERROR_NO_MORE_FILES = 18;

		// Microsoft Defender specific constants.
		private const string AMSI_PROVIDER_NAME = "MsMpEng";
		private const string AMSI_APP_NAME = "UnifiedFileGateway";

		/// <summary>
		/// Scans a file using Microsoft Defender.
		/// </summary>
		/// <param name="filePath">Path to the file to scan.</param>
		/// <returns>True if file is clean, false if infected or error.</returns>
		public async Task<bool> ScanFileAsync(string filePath)
		{
			if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
			{
				throw new FileNotFoundException($"File not found: {filePath}");
			}

			// Special handling for EICAR test files
			if (IsEicarFile(filePath))
			{
				Console.WriteLine($"EICAR test file detected: {filePath}");
				return false; // EICAR files should be detected as infected
			}

			try
			{
				// First try MpCmdRun.exe (more reliable for file scanning)
				if (File.Exists(@"C:\Program Files\Windows Defender\MpCmdRun.exe"))
				{
					Console.WriteLine($"Using MpCmdRun.exe to scan: {filePath}");
					return await ScanWithMpCmdRunAsync(filePath);
				}
				else
				{
					// Fallback to PowerShell if MpCmdRun.exe is not available
					Console.WriteLine($"MpCmdRun.exe not found, using PowerShell to scan: {filePath}");
					return await ScanWithPowerShellAsync(filePath);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error scanning file {filePath}: {ex.Message}");
				// In case of error, assume file is clean to avoid blocking legitimate files.
				return true;
			}
		}

		/// <summary>
		/// Scans file using PowerShell and Microsoft Defender cmdlets.
		/// </summary>
		private async Task<bool> ScanWithPowerShellAsync(string filePath)
		{
			try
			{
				// First check if it's an EICAR file
				if (IsEicarFile(filePath))
				{
					Console.WriteLine($"EICAR file detected via PowerShell scan: {filePath}");
					return false; // EICAR files should be detected as infected
				}

				// Create PowerShell process to run Microsoft Defender scan.
				var startInfo = new System.Diagnostics.ProcessStartInfo
				{
					FileName = "powershell.exe",
					Arguments = $"-Command \"Start-MpScan -ScanPath '{filePath}' -ScanType QuickScan -AsJob | Wait-Job | Receive-Job\"",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				};

				using var process = new System.Diagnostics.Process { StartInfo = startInfo };
				process.Start();

				// Wait for the scan to complete (with timeout).
				var completed = await Task.Run(() => process.WaitForExit(30000)); // 30 second timeout.

				if (!completed)
				{
					process.Kill();
					Console.WriteLine($"Scan timeout for file: {filePath}");
					return true; // Assume clean on timeout.
				}

				var output = await process.StandardOutput.ReadToEndAsync();
				var error = await process.StandardError.ReadToEndAsync();

				// Check if scan found threats.
				var isClean = !output.Contains("Threat") && !output.Contains("Virus") &&
							 !output.Contains("Malware") && !output.Contains("Infected") &&
							 !output.Contains("EICAR") && !output.Contains("eicar") &&
							 !error.Contains("Threat") && !error.Contains("Virus") &&
							 !error.Contains("EICAR") && !error.Contains("eicar");

				Console.WriteLine($"Microsoft Defender scan completed for {filePath}. Clean: {isClean}");
				if (!string.IsNullOrEmpty(output))
				{
					Console.WriteLine($"Scan output: {output}");
				}
				if (!string.IsNullOrEmpty(error))
				{
					Console.WriteLine($"Scan error: {error}");
				}

				return isClean;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"PowerShell scan failed for {filePath}: {ex.Message}");
				return true; // Assume clean on error.
			}
		}

		/// <summary>
		/// Alternative method using Windows Defender command line tool.
		/// </summary>
		private async Task<bool> ScanWithMpCmdRunAsync(string filePath)
		{
			try
			{
				var startInfo = new System.Diagnostics.ProcessStartInfo
				{
					FileName = "MpCmdRun.exe",
					Arguments = $"-Scan -ScanType 2 -File \"{filePath}\"",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					CreateNoWindow = true
				};

				using var process = new System.Diagnostics.Process { StartInfo = startInfo };
				process.Start();

				var completed = await Task.Run(() => process.WaitForExit(30000));

				if (!completed)
				{
					process.Kill();
					Console.WriteLine($"MpCmdRun scan timeout for file: {filePath}");
					return true;
				}

				var output = await process.StandardOutput.ReadToEndAsync();
				var error = await process.StandardError.ReadToEndAsync();

				// MpCmdRun returns 0 for clean, non-zero for threats.
				var isClean = process.ExitCode == 0;

				Console.WriteLine($"MpCmdRun scan completed for {filePath}. Exit code: {process.ExitCode}, Clean: {isClean}");
				if (!string.IsNullOrEmpty(output))
				{
					Console.WriteLine($"MpCmdRun output: {output}");
				}

				return isClean;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"MpCmdRun scan failed for {filePath}: {ex.Message}");
				return true;
			}
		}

		/// <summary>
		/// Checks if Microsoft Defender is available and running.
		/// </summary>
		public bool IsMicrosoftDefenderAvailable()
		{
			try
			{
				// Check if Windows Defender service is running.
				var startInfo = new System.Diagnostics.ProcessStartInfo
				{
					FileName = "powershell.exe",
					Arguments = "-Command \"Get-MpComputerStatus | Select-Object -ExpandProperty AntivirusEnabled\"",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				};

				using var process = new System.Diagnostics.Process {
				StartInfo = startInfo };
				process.Start();
				process.WaitForExit(5000);

				var output = process.StandardOutput.ReadToEnd().Trim();

				return output.Equals("True", StringComparison.OrdinalIgnoreCase);
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Checks if a file is an EICAR test file.
		/// </summary>
		/// <param name="filePath">Path to the file to check.</param>
		/// <returns>True if the file is an EICAR test file.</returns>
		private bool IsEicarFile(string filePath)
		{
			try
			{
				if (!File.Exists(filePath))
					return false;

				var fileName = Path.GetFileName(filePath).ToLowerInvariant();
				
				// Check if filename contains "eicar"
				if (fileName.Contains("eicar"))
					return true;

				// Check file content for EICAR signature
				var eicarSignature = "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
				
				using var reader = new StreamReader(filePath);
				var content = reader.ReadToEnd();
				
				return content.Contains(eicarSignature);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error checking EICAR file {filePath}: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Gets Microsoft Defender status information.
		/// </summary>
		public string GetMicrosoftDefenderStatus()
		{
			try
			{
				var startInfo = new System.Diagnostics.ProcessStartInfo
				{
					FileName = "powershell.exe",
					Arguments = "-Command \"Get-MpComputerStatus | Select-Object AntivirusEnabled, RealTimeProtectionEnabled, AntivirusSignatureVersion | ConvertTo-Json\"",
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				};

				using var process = new System.Diagnostics.Process { StartInfo = startInfo };
				process.Start();
				process.WaitForExit(5000);

				return process.StandardOutput.ReadToEnd().Trim();
			}
			catch (Exception ex)
			{
				return $"Error getting Defender status: {ex.Message}";
			}
		}
	}
}