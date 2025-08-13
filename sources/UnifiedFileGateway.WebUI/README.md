# Unified File Gateway Web UI

Modern web interface for Unified File Gateway with drag & drop file upload support, antivirus scanning, and file management.

## Features

- 🎨 **Modern UI/UX** - Beautiful and intuitive interface with Tailwind CSS
- 📁 **Drag & Drop** - Convenient file upload by dragging
- 🛡️ **Antivirus Integration** - Display of file scanning status
- 📊 **Real-time** - Automatic file status updates
- 📱 **Responsive Design** - Works on all devices
- 🔔 **Notifications** - Event notification system
- 📂 **File Management** - Support for Central Storage folder

## Technologies

- **React 18** - Modern JavaScript framework
- **Tailwind CSS** - Utility-first CSS framework
- **Lucide React** - Beautiful icons
- **React Dropzone** - Drag & drop functionality
- **Axios** - HTTP client for API requests

## Installation

### Prerequisites

- Node.js 16+ 
- npm or yarn
- Running Unified File Gateway service

### Installation Steps

1. **Navigate to project directory**
   ```bash
   cd sources/UnifiedFileGateway.WebUI
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start project in development mode**
   ```bash
   npm start
   ```

4. **Open browser**
   ```
   http://localhost:3000
   ```

## Usage

### File Upload

1. **Drag files** to the upload area or click to select
2. **Select folder** from the sidebar (Central Storage)
3. **Monitor upload progress**
4. **Wait for antivirus scanning**

### File Management

- **View status** - Each file shows its status (Clean, Infected, Scanning)
- **Download** - Click download icon for safe files
- **Delete** - Click trash icon to delete file
- **Refresh** - Click "Refresh" button for manual update

### File Statuses

- 🟢 **Clean** - File is safe, can be downloaded
- 🔴 **Infected** - File is infected, download blocked
- 🟡 **Scanning** - File is being scanned by antivirus
- ⚪ **NotFound** - File not found

## Configuration

### Environment Variables

Create `.env` file in project root:

```env
REACT_APP_API_URL=http://localhost:5000
```

### API Configuration

API is configured to work with WCF service through REST API endpoints:

- `POST /api/files/upload` - Upload file
- `GET /api/files/status/{fileName}` - Get file status
- `GET /api/files/download/{fileName}` - Download file
- `DELETE /api/files/{fileName}` - Delete file
- `GET /api/files?folder={folder}` - File list

## Development

### Project Structure

```
src/
├── components/          # React components
│   ├── FileUpload.js   # Upload component
│   ├── FileList.js     # File list
│   └── Notification.js # Notifications
├── services/           # API services
│   └── api.js         # HTTP client
├── App.js             # Main component
└── index.js           # Entry point
```

### Development Commands

```bash
# Start in development mode
npm start

# Build for production
npm run build

# Run tests
npm test

# Code linting
npm run lint
```

## Supported File Formats

- **Documents**: PDF, DOC, DOCX, XLS, XLSX, TXT
- **Archives**: ZIP, RAR
- **Images**: JPEG, JPG, PNG, GIF
- **Maximum size**: 100MB

## Security

- All files are automatically scanned by Microsoft Defender
- Download is allowed only for safe files
- CORS support for secure API communication

## License

This project is part of the Unified File Gateway solution. 