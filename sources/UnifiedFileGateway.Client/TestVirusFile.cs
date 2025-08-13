using System.Text;

namespace UnifiedFileGateway.Client
{
	/// <summary>
	/// Helper class for creating test virus files (EICAR standard).
	/// </summary>
	public static class TestVirusFile
	{
		/// <summary>
		/// EICAR standard test virus string - this is a harmless test file that all antivirus software should detect.
		/// </summary>
		private static readonly string EICAR_STRING = @"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";

		/// <summary>
		/// Creates an EICAR test virus file.
		/// </summary>
		/// <param name="filePath">Path where to create the test file.</param>
		/// <returns>True if file was created successfully.</returns>
		public static bool CreateEicarTestFile(string filePath)
		{
			try
			{
				File.WriteAllText(filePath, EICAR_STRING, Encoding.ASCII);
				Console.WriteLine($"EICAR test virus file created: {filePath}");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to create EICAR test file: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Creates a clean test file for comparison.
		/// </summary>
		/// <param name="filePath">Path where to create the clean file.</param>
		/// <returns>True if file was created successfully.</returns>
		public static bool CreateCleanTestFile(string filePath)
		{
			try
			{
				var cleanContent = "This is a clean test file for UnifiedFileGateway. No viruses here!";
				File.WriteAllText(filePath, cleanContent, Encoding.UTF8);
				Console.WriteLine($"Clean test file created: {filePath}");
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to create clean test file: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Checks if a file is an EICAR test file.
		/// </summary>
		/// <param name="filePath">Path to the file to check.</param>
		/// <returns>True if the file contains EICAR string.</returns>
		public static bool IsEicarFile(string filePath)
		{
			try
			{
				if (!File.Exists(filePath))
					return false;

				var content = File.ReadAllText(filePath, Encoding.ASCII);
				return content.Contains(EICAR_STRING);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error checking EICAR file {filePath}: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Checks if a file is a clean test file (contains expected clean content).
		/// </summary>
		/// <param name="filePath">Path to the file to check.</param>
		/// <returns>True if the file contains expected clean content.</returns>
		public static bool IsCleanTestFile(string filePath)
		{
			try
			{
				if (!File.Exists(filePath))
					return false;

				var content = File.ReadAllText(filePath, Encoding.UTF8);
				return content.Contains("This is a clean test file for UnifiedFileGateway");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error checking clean test file {filePath}: {ex.Message}");
				return false;
			}
		}

		/// <summary>
		/// Gets information about EICAR test files.
		/// </summary>
		/// <returns>Information string about EICAR.</returns>
		public static string GetEicarInfo()
		{
			return @"EICAR Test Virus File Information:
- EICAR is a standard test file used by antivirus software vendors
- It contains a harmless string that all antivirus software should detect as a virus
- Purpose: Testing antivirus software without using real malware
- File size: 68 bytes
- Content: ASCII string that triggers antivirus detection
- Safe to use: Cannot harm your computer in any way
- Standard: Recognized by all major antivirus vendors including Microsoft Defender";
		}
	}
}