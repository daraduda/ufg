using System.Collections.Concurrent;
using UnifiedFileGateway.Contracts;

namespace UnifiedFileGateway.Service
{
	public class FileService : IFileService
	{
		private readonly string _storagePath;
		private readonly MicrosoftDefenderScanner _antivirusScanner;
		private static readonly ConcurrentDictionary<string, FileStatus> FileStatuses = new ConcurrentDictionary<string, FileStatus>();
		private static int _deletedInfectedFilesCount = 0;

		public FileService()
		{
			// In a real application, the path should be taken from the configuration.
			_storagePath = @"d:\UnifiedFileGatewayStorage";
			if (!Directory.Exists(_storagePath))
			{
				Directory.CreateDirectory(_storagePath);
			}

			_antivirusScanner = new MicrosoftDefenderScanner();

			// Check Microsoft Defender availability on startup.
			var isDefenderAvailable = _antivirusScanner.IsMicrosoftDefenderAvailable();
			Console.WriteLine($"Microsoft Defender available: {isDefenderAvailable}");

			if (isDefenderAvailable)
			{
				var defenderStatus = _antivirusScanner.GetMicrosoftDefenderStatus();
				Console.WriteLine($"Microsoft Defender status: {defenderStatus}");
			}
			else
			{
				Console.WriteLine("Warning: Microsoft Defender is not available. Files will be marked as clean without scanning.");
			}

			// Scan existing files on startup
			_ = Task.Run(async () => await ScanExistingFilesOnStartup());
		}

		public async Task UploadFile(FileUploadMessage message)
		{
			if (message?.FileName == null)
				throw new ArgumentNullException(nameof(message), "File name cannot be null");

			if (message.FileData == null)
				throw new ArgumentNullException(nameof(message), "File data cannot be null");

			var filePath = Path.Combine(_storagePath, message.FileName);

			FileStatuses[message.FileName] = FileStatus.Scanning;

			using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			{
				await message.FileData.CopyToAsync(fileStream);
			}

			// Perform real Microsoft Defender antivirus scan.
			_ = Task.Run(async () =>
			{
				try
				{
					Console.WriteLine($"Starting Microsoft Defender scan for file: {message.FileName}");

					// Check if Microsoft Defender is available.
					if (_antivirusScanner.IsMicrosoftDefenderAvailable())
					{
						// Perform real antivirus scan
						var isClean = await _antivirusScanner.ScanFileAsync(filePath);

						if (isClean)
						{
							FileStatuses[message.FileName] = FileStatus.Clean;
							Console.WriteLine($"Microsoft Defender scan completed for '{message.FileName}'. Status: Clean");
						}
						else
						{
							FileStatuses[message.FileName] = FileStatus.Infected;
							Console.WriteLine($"Microsoft Defender scan completed for '{message.FileName}'. Status: Infected - Threat detected!");

							// Optionally delete infected file.
							try
							{
								File.Delete(filePath);
								Console.WriteLine($"Infected file '{message.FileName}' has been deleted.");
								
								// Remove status from dictionary since file is deleted
								FileStatuses.TryRemove(message.FileName, out _);
								Console.WriteLine($"Removed status for deleted infected file '{message.FileName}'.");
								
								// Increment deleted infected files counter
								_deletedInfectedFilesCount++;
								Console.WriteLine($"Deleted infected files count: {_deletedInfectedFilesCount}");
							}
							catch (Exception deleteEx)
							{
								Console.WriteLine($"Failed to delete infected file '{message.FileName}': {deleteEx.Message}");
							}
						}
					}
					else
					{
						// Fallback: assume file is clean if Microsoft Defender is not available.
						FileStatuses[message.FileName] = FileStatus.Clean;
						Console.WriteLine($"Microsoft Defender not available. File '{message.FileName}' marked as clean (no scan performed).");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error during antivirus scan for '{message.FileName}': {ex.Message}");

					// In case of error, assume file is clean to avoid blocking legitimate files.
					FileStatuses[message.FileName] = FileStatus.Clean;
				}
			});
		}

		public Task<Stream> DownloadFile(string fileName)
		{
			var filePath = Path.Combine(_storagePath, fileName);

			if (!File.Exists(filePath) || FileStatuses.GetValueOrDefault(fileName) != FileStatus.Clean)
			{
				// In a real-world scenario, a more meaningful exception would be here
				throw new FileNotFoundException("File not found or not clean.", fileName);
			}

			return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read));
		}

		public Task DeleteFile(string fileName)
		{
			var filePath = Path.Combine(_storagePath, fileName);
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
				FileStatuses.TryRemove(fileName, out _);
			}
			return Task.CompletedTask;
		}

		public async Task<string> GetFileStatus(string fileName)
		{
			// First check if file exists physically
			var filePath = Path.Combine(_storagePath, fileName);
			var fileExists = File.Exists(filePath);
			
			// If file doesn't exist but has status, remove the status
			if (!fileExists && FileStatuses.ContainsKey(fileName))
			{
				FileStatuses.TryRemove(fileName, out _);
				Console.WriteLine($"Removed status for non-existent file '{fileName}'.");
				return "NotFound";
			}
			
			if (!FileStatuses.ContainsKey(fileName))
			{
				if (fileExists)
				{
					// File exists but no status - scan it now
					Console.WriteLine($"File '{fileName}' exists but has no status. Scanning now...");
					
					FileStatuses[fileName] = FileStatus.Scanning;
					
					try
					{
						if (_antivirusScanner.IsMicrosoftDefenderAvailable())
						{
							var isClean = await _antivirusScanner.ScanFileAsync(filePath);
							FileStatuses[fileName] = isClean ? FileStatus.Clean : FileStatus.Infected;
							Console.WriteLine($"Scan completed for '{fileName}'. Status: {FileStatuses[fileName]}");
						}
						else
						{
							FileStatuses[fileName] = FileStatus.Clean;
							Console.WriteLine($"Microsoft Defender not available. File '{fileName}' marked as clean.");
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Error scanning file '{fileName}': {ex.Message}");
						FileStatuses[fileName] = FileStatus.Clean; // Assume clean on error
					}
				}
				else
				{
					return "NotFound";
				}
			}

			return FileStatuses[fileName].ToString();
		}

		public Task<string[]> GetFiles(string folder = "CentralStorage")
		{
			try
			{
				// For now, return all files in storage regardless of folder
				// In a real implementation, you would filter by folder
				var files = Directory.GetFiles(_storagePath)
					.Select(Path.GetFileName)
					.Where(name => name != null)
					.Select(name => name!)
					.ToArray();

				// Clean up statuses for files that no longer exist
				var existingFileNames = new HashSet<string>(files);
				var statusesToRemove = FileStatuses.Keys
					.Where(fileName => !existingFileNames.Contains(fileName))
					.ToArray();

				foreach (var fileName in statusesToRemove)
				{
					FileStatuses.TryRemove(fileName, out _);
					Console.WriteLine($"Cleaned up status for non-existent file: {fileName}");
				}

				return Task.FromResult(files);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting files: {ex.Message}");
				return Task.FromResult(Array.Empty<string>());
			}
		}

		private async Task ScanExistingFilesOnStartup()
		{
			try
			{
				Console.WriteLine("Starting scan of existing files on startup...");
				
				var existingFiles = Directory.GetFiles(_storagePath)
					.Select(Path.GetFileName)
					.Where(name => name != null)
					.Select(name => name!)
					.ToArray();

				Console.WriteLine($"Found {existingFiles.Length} existing files to scan");

				foreach (var fileName in existingFiles)
				{
					if (fileName != null && !FileStatuses.ContainsKey(fileName))
					{
						var filePath = Path.Combine(_storagePath, fileName);
						
						// Mark as scanning initially
						FileStatuses[fileName] = FileStatus.Scanning;
						Console.WriteLine($"Scanning existing file: {fileName}");

						try
						{
							if (_antivirusScanner.IsMicrosoftDefenderAvailable())
							{
								var isClean = await _antivirusScanner.ScanFileAsync(filePath);
								FileStatuses[fileName] = isClean ? FileStatus.Clean : FileStatus.Infected;
								Console.WriteLine($"Scan completed for '{fileName}'. Status: {FileStatuses[fileName]}");
							}
							else
							{
								// If Microsoft Defender is not available, mark as clean
								FileStatuses[fileName] = FileStatus.Clean;
								Console.WriteLine($"Microsoft Defender not available. File '{fileName}' marked as clean.");
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error scanning file '{fileName}': {ex.Message}");
							FileStatuses[fileName] = FileStatus.Clean; // Assume clean on error
						}
					}
				}

				Console.WriteLine("Startup file scan completed.");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during startup file scan: {ex.Message}");
			}
		}

		/// <summary>
		/// Gets file statistics for the current storage.
		/// </summary>
		/// <returns>Dictionary with status counts.</returns>
		public Task<Dictionary<string, int>> GetFileStatistics()
		{
			try
			{
				var statistics = new Dictionary<string, int>
				{
					{ "Clean", 0 },
					{ "Infected", 0 },
					{ "Scanning", 0 },
					{ "NotFound", 0 }
				};

				// Count statuses for existing files only
				var existingFiles = Directory.GetFiles(_storagePath)
					.Select(Path.GetFileName)
					.Where(name => name != null)
					.Select(name => name!)
					.ToArray();

				var existingFileNames = new HashSet<string>(existingFiles);

				foreach (var kvp in FileStatuses)
				{
					if (existingFileNames.Contains(kvp.Key))
					{
						var statusKey = kvp.Value.ToString();
						if (statistics.ContainsKey(statusKey))
						{
							statistics[statusKey]++;
						}
					}
				}

				// Count files without status as "Not Found"
				foreach (var fileName in existingFiles)
				{
					if (!FileStatuses.ContainsKey(fileName))
					{
						statistics["NotFound"]++;
					}
				}

				// Add deleted infected files to the count
				statistics["Infected"] += _deletedInfectedFilesCount;
				Console.WriteLine($"Statistics - Current Infected: {statistics["Infected"]}, Deleted Infected: {_deletedInfectedFilesCount}");

				return Task.FromResult(statistics);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error getting file statistics: {ex.Message}");
				return Task.FromResult(new Dictionary<string, int>());
			}
		}
	}
}