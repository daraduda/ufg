using System.ServiceModel;
using UnifiedFileGateway.Contracts;
using UnifiedFileGateway.Client;

var binding = new BasicHttpBinding(BasicHttpSecurityMode.None)
{
	MaxReceivedMessageSize = int.MaxValue,
	MessageEncoding = WSMessageEncoding.Mtom
};

var endpoint = new EndpointAddress("http://localhost:5000/FileService");
var factory = new ChannelFactory<IFileService>(binding, endpoint);
var client = factory.CreateChannel();

Console.WriteLine("Client started. Preparing to upload test files...");

// Display EICAR information
Console.WriteLine(TestVirusFile.GetEicarInfo());
Console.WriteLine();

try
{
	// 1. Create a clean test file
	var cleanFilePath = Path.GetTempFileName();
	var cleanFileName = $"clean_test_{Guid.NewGuid()}.txt";
	TestVirusFile.CreateCleanTestFile(cleanFilePath);

	// 2. Create an EICAR test virus file
	var virusFilePath = Path.GetTempFileName();
	var virusFileName = $"eicar_test_{Guid.NewGuid()}.txt";
	TestVirusFile.CreateEicarTestFile(virusFilePath);

	// Validate that files were created correctly
	if (!TestVirusFile.IsCleanTestFile(cleanFilePath))
	{
		Console.WriteLine("❌ Warning: Clean test file validation failed!");
	}
	else
	{
		Console.WriteLine("✅ Clean test file validation successful.");
	}

	if (!TestVirusFile.IsEicarFile(virusFilePath))
	{
		Console.WriteLine("❌ Warning: EICAR test file validation failed! File may not contain correct EICAR string.");
	}
	else
	{
		Console.WriteLine("✅ EICAR test file validation successful.");
	}

	Console.WriteLine($"Created test files: {cleanFileName} (clean) and {virusFileName} (EICAR test virus)");

	// Test 1: Upload and scan clean file
	Console.WriteLine("\n=== TEST 1: Clean File Scan ===");
	await TestFileUpload(client, cleanFilePath, cleanFileName, "Clean file");

	// Test 2: Upload and scan EICAR test virus file
	Console.WriteLine("\n=== TEST 2: EICAR Test Virus Scan ===");
	await TestFileUpload(client, virusFilePath, virusFileName, "EICAR test virus file");
}
catch (Exception ex)
{
	Console.WriteLine($"An error occurred: {ex.Message}");
}
finally
{
	(client as IClientChannel)?.Close();
	factory.Close();
	Console.WriteLine("Client finished.");
}

static async Task TestFileUpload(IFileService client, string filePath, string fileName, string fileDescription)
{
	try
	{
		Console.WriteLine($"Testing {fileDescription}: {fileName}");

		// Upload the file
		using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
		using (var uploadMessage = new FileUploadMessage { FileName = fileName, FileData = fileStream })
		{
			await client.UploadFile(uploadMessage);
			Console.WriteLine($"File upload initiated for {fileDescription}. Now polling for status...");
		}

		// Poll for status
		FileStatus currentStatus;

		var pollCount = 0;
		do
		{
			await Task.Delay(2000); // Wait 2 seconds between polls
			pollCount++;
			var statusString = await client.GetFileStatus(fileName);
			currentStatus = Enum.Parse<FileStatus>(statusString);
			Console.WriteLine($"Poll {pollCount}: Current status for '{fileName}': {currentStatus}");

			if (pollCount > 15) // 30 seconds timeout
			{
				Console.WriteLine($"Timeout waiting for scan completion for {fileName}");
				break;
			}

		} while (currentStatus == FileStatus.Scanning);

		// Handle scan result
		switch (currentStatus)
		{
			case FileStatus.Clean:
				Console.WriteLine($"✅ {fileDescription} is clean. Downloading...");
				await DownloadAndVerifyFile(client, fileName, filePath, fileDescription);
				break;

			case FileStatus.Infected:
				Console.WriteLine($"🚨 {fileDescription} is infected! Download blocked for security.");
				break;

			case FileStatus.NotFound:
				Console.WriteLine($"❌ {fileDescription} not found on server.");
				break;

			default:
				Console.WriteLine($"⚠️ {fileDescription} scan result: {currentStatus}");
				break;
		}

		// Clean up server file
		try
		{
			await client.DeleteFile(fileName);
			Console.WriteLine($"Server file '{fileName}' deleted.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Failed to delete server file '{fileName}': {ex.Message}");
		}
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Error testing {fileDescription}: {ex.Message}");
	}
}

static async Task DownloadAndVerifyFile(IFileService client, string fileName, string originalFilePath, string fileDescription)
{
	try
	{
		var downloadedStream = await client.DownloadFile(fileName);
		var downloadedFilePath = Path.Combine(Path.GetTempPath(), $"downloaded_{fileName}");

		using (var fileStream = new FileStream(downloadedFilePath, FileMode.Create, FileAccess.Write))
		{
			await downloadedStream.CopyToAsync(fileStream);
		}
		Console.WriteLine($"File downloaded successfully to: {downloadedFilePath}");

		// Verify content
		var originalText = await File.ReadAllTextAsync(originalFilePath);
		var downloadedText = await File.ReadAllTextAsync(downloadedFilePath);

		if (originalText == downloadedText)
		{
			Console.WriteLine($"✅ Verification successful: Downloaded {fileDescription} content matches original.");
		}
		else
		{
			Console.WriteLine($"❌ Error: Downloaded {fileDescription} content does not match original.");
		}

		// Clean up downloaded file
		File.Delete(downloadedFilePath);
	}
	catch (Exception ex)
	{
		Console.WriteLine($"Error downloading {fileDescription}: {ex.Message}");
	}
}
