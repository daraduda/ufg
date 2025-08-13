# UnifiedFileGateway

A WCF-based file management solution for ASP.NET Web applications with antivirus integration and client-side polling notification system.

## Overview

UnifiedFileGateway is a prototype solution designed to address file management challenges across multiple ASP.NET Web application instances (Local, Central, and Service portals) that share the same codebase but differ in configuration. The solution provides a unified interface for file operations with integrated antivirus scanning and user notification capabilities.

## Problem Statement

The system manages files across two ASP.NET Web application instances:
1. **Central Portal** - Centralized file management with Citrix Farm integration
2. **Service Portal** - External service access

Key challenges addressed:
- **File Processing**: Special folders like `CentralStorage` (processed by external antivirus)
- **User Notification**: Notifying users when files uploaded to `CentralStorage` have been processed by external antivirus
- **Cross-Platform Communication**: Enabling communication between different application instances
- **Large File Handling**: Supporting efficient upload/download of large files

## Solution Architecture

### Notification Method: Client Polling (Variant 1)

The solution implements **client-side polling** for antivirus completion notification:
- Clients periodically check file status using `GetFileStatus()` method
- Files start with `Scanning` status during antivirus processing
- Status changes to `Clean` or `Infected` when processing completes
- Clients can download files only when status is `Clean`

### Technology Stack

- **.NET 8.0** - .NET framework
- **CoreWCF** - WCF implementation for .NET Core/8.0
- **System.ServiceModel** - Standard WCF client library
- **SOAP Protocol** - For external client compatibility
- **MTOM (Message Transmission Optimization Mechanism)** - For efficient large file streaming
- **Kestrel** - Web server for hosting the WCF service

## Project Structure

```
UnifiedFileGateway/
├── UnifiedFileGateway.sln              # Main solution file
├── UnifiedFileGateway.Contracts/       # Shared WCF contracts
│   ├── IFileService.cs                 # Service interface
│   ├── FileUploadMessage.cs            # Upload message contract
│   └── FileStatus.cs                   # File status enumeration
├── UnifiedFileGateway.Service/         # Service implementation
│   ├── FileService.cs                  # WCF service implementation
│   └── MicrosoftDefenderScanner.cs     # Microsoft Defender integration
├── UnifiedFileGateway.Host/            # Service host application
│   └── Program.cs                      # Host configuration
└── UnifiedFileGateway.Client/          # Client application
│   ├── Program.cs                      # Client demonstration
│   └── TestVirusFile.cs                # EICAR test virus file helper
└── UnifiedFileGateway.WebUI/           # Web UI client
    ├── src/                            # React source code
    ├── public/                         # Static files
    ├── package.json                    # Node.js dependencies
    └── README.md                       # Web UI documentation
```

## Features

### Core Operations
- **Upload**: Upload files with automatic Microsoft Defender antivirus scanning
- **Download**: Download files (only when clean)
- **Delete**: Remove files from storage
- **Status Check**: Poll file processing status

### Antivirus Integration
- **Microsoft Defender Integration**: Real-time antivirus scanning using local Microsoft Defender
- **PowerShell Integration**: Uses PowerShell cmdlets for Microsoft Defender operations
- **Fallback Protection**: Graceful handling when Microsoft Defender is unavailable
- **Threat Detection**: Automatic detection and handling of infected files
- **EICAR Test Support**: Built-in support for testing with EICAR standard test virus files

### File Status Management
- `NotFound` - File doesn't exist
- `Scanning` - File is being processed by Microsoft Defender
- `Clean` - File passed antivirus scan
- `Infected` - File failed antivirus scan (threat detected)

### Large File Support
- MTOM encoding for efficient streaming
- Configurable message size limits
- Memory-efficient file handling

### Web UI Client
- **Modern React Interface**: Beautiful and intuitive web interface with Tailwind CSS
- **Drag & Drop Upload**: Convenient file upload with drag and drop functionality
- **Real-time Status**: Live updates of file scanning status
- **Responsive Design**: Works on all devices and screen sizes
- **Folder Management**: Support for different folders (CentralStorage, TP-Skizzen, Temp)
- **Notification System**: User-friendly notifications for all operations

## Setup Instructions

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code (optional)
- Node.js 16+ (for Web UI)
- npm or yarn (for Web UI)

### Building the Solution

1. **Clone or navigate to the project directory**
   ```bash
   cd UnifiedFileGateway
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

### Running the Service

1. **Start the WCF service host**
   ```bash
   dotnet run --project UnifiedFileGateway.Host
   ```

   The service will start on `http://localhost:8088` with the following endpoints:
   - **Service Endpoint**: `http://localhost:8088/FileService`
   - **Binding**: BasicHttpBinding with MTOM encoding
   - **Security**: None (for prototype purposes)

2. **Verify the service is running**
   - Look for: `Now listening on: http://[::]:8088`
   - The service is ready when you see: `Application started. Press Ctrl+C to shut down.`

### Running the Client

1. **In a new terminal, run the client**
   ```bash
   dotnet run --project UnifiedFileGateway.Client
   ```

2. **Observe the workflow**
   - Microsoft Defender status check on startup
   - Clean file upload and scanning
   - EICAR test virus file upload and detection
   - Status polling (every 2 seconds)
   - File download (only when clean)
   - Content verification
   - File cleanup

### Running the Web UI

1. **Navigate to the Web UI directory**
   ```bash
   cd sources/UnifiedFileGateway.WebUI
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start the development server**
   ```bash
   npm start
   ```

4. **Open your browser**
   ```
   http://localhost:3000
   ```

**Note**: Make sure the WCF service is running before using the Web UI.

## Usage Examples

### Basic File Operations

```csharp
// Create WCF client
var binding = new BasicHttpBinding(BasicHttpSecurityMode.None)
{
    MaxReceivedMessageSize = int.MaxValue,
    MessageEncoding = WSMessageEncoding.Mtom
};
var endpoint = new EndpointAddress("http://localhost:8088/FileService");
var factory = new ChannelFactory<IFileService>(binding, endpoint);
var client = factory.CreateChannel();

// Upload file
using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
using (var uploadMessage = new FileUploadMessage { FileName = fileName, FileData = fileStream })
{
    await client.UploadFile(uploadMessage);
}

// Poll for status
FileStatus status;
do
{
    await Task.Delay(2000);
    status = await client.GetFileStatus(fileName);
} while (status == FileStatus.Scanning);

// Download file (if clean)
if (status == FileStatus.Clean)
{
    var stream = await client.DownloadFile(fileName);
    // Process downloaded file
}

// Delete file
await client.DeleteFile(fileName);
```

### Integration with ASP.NET Applications

For integration with your ASP.NET applications:

1. **Add project reference** to `UnifiedFileGateway.Contracts`
2. **Create WCF client** using the same binding configuration
3. **Implement polling logic** in your web application
4. **Handle file operations** based on status responses

## Configuration

### Service Configuration

The service can be configured by modifying `UnifiedFileGateway.Host/Program.cs`:

```csharp
// Change port
options.ListenAnyIP(8088);

// Modify binding settings
var binding = new BasicHttpBinding(BasicHttpSecurityMode.None)
{
    MaxReceivedMessageSize = int.MaxValue,
    MaxBufferSize = int.MaxValue,
    MessageEncoding = WSMessageEncoding.Mtom
};
```

### Storage Configuration

File storage location is configured in `UnifiedFileGateway.Service/FileService.cs`:

```csharp
// Default: System temp directory
_storagePath = Path.Combine(Path.GetTempPath(), "UnifiedFileGatewayStorage");

// Custom path example:
_storagePath = @"C:\FileStorage\UnifiedFileGateway";
```

## Security Considerations

⚠️ **Important**: This prototype uses `BasicHttpSecurityMode.None` for simplicity. In production:

1. **Enable HTTPS** with proper certificates
2. **Implement authentication** (Windows Authentication, Basic Auth, or custom)
3. **Configure authorization** for file operations
4. **Use secure file storage** with proper access controls
5. **Implement audit logging** for file operations

## Troubleshooting

### Common Issues

1. **Port already in use**
   ```bash
   # Find process using port 8088
   netstat -ano | findstr :8088
   
   # Kill the process (replace PID with actual process ID)
   taskkill /PID <PID> /F
   ```

2. **Build errors**
   - Ensure .NET 8.0 SDK is installed
   - Run `dotnet restore` before building
   - Check all project references are correct

3. **Client connection issues**
   - Verify service is running on correct port
   - Check firewall settings
   - Ensure binding configuration matches between client and server

### Logging

The service provides detailed logging:
- **Host startup**: Service binding and endpoint information
- **Request processing**: File operations and status changes
- **Error handling**: Exception details and stack traces

## Development Notes

### Project Dependencies

- **UnifiedFileGateway.Contracts**: .NET Standard 2.0 (for compatibility)
- **UnifiedFileGateway.Service**: .NET 8.0
- **UnifiedFileGateway.Host**: .NET 8.0 Web SDK
- **UnifiedFileGateway.Client**: .NET 8.0

### Key Design Decisions

1. **Contract Separation**: Shared contracts in separate project for compatibility
2. **Async Operations**: All service methods are async for scalability
3. **Streaming Support**: MTOM encoding for large file efficiency
4. **Status Tracking**: In-memory status tracking (consider database for production)
5. **Error Handling**: Comprehensive exception handling and logging

## Future Enhancements

Potential improvements for production use:

1. **Database Integration**: Persistent file status tracking
2. **Multiple Antivirus Support**: Integration with other antivirus solutions (Kaspersky, Norton, etc.)
3. **WebSocket Notifications**: Real-time status updates
4. **File Versioning**: Support for file version management
5. **Compression**: Automatic file compression for storage efficiency
6. **Monitoring**: Health checks and performance metrics
7. **Load Balancing**: Support for multiple service instances
8. **Advanced Threat Detection**: Integration with Microsoft Defender Advanced Threat Protection (ATP)
9. **Quarantine Management**: Automatic quarantine of suspicious files
10. **Scan Scheduling**: Configurable scan schedules and priorities

## License

This is a prototype solution. Please ensure proper licensing for production use.

## Support

For questions or issues with this prototype, please refer to the troubleshooting section or create an issue in the project repository. 