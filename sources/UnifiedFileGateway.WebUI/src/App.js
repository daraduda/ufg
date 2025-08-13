import React, { useState, useEffect, useCallback } from 'react';
import { Shield, Upload, Folder, Settings } from 'lucide-react';
import FileUpload from './components/FileUpload';
import FileList from './components/FileList';
import Notification from './components/Notification';
import { fileService } from './services/api';

function App() {
  const [files, setFiles] = useState([]);
  const [isUploading, setIsUploading] = useState(false);
  const [selectedFolder, setSelectedFolder] = useState('CentralStorage');
  const [statistics, setStatistics] = useState({
    Clean: 0,
    Infected: 0,
    Scanning: 0,
    NotFound: 0
  });
  const [notification, setNotification] = useState({
    isVisible: false,
    type: 'info',
    title: '',
    message: ''
  });

  // Load file list
  const loadFiles = useCallback(async () => {
    try {
      const fileList = await fileService.getFiles(selectedFolder);
      
      // Get statuses for all files
      const filesWithStatus = await Promise.all(
        fileList.map(async (file) => {
          try {
            const status = await fileService.getFileStatus(file.name);
            console.log(`File: ${file.name}, Status: ${status}, Type: ${typeof status}`);
            
            return {
              ...file,
              status: status
            };
          } catch (error) {
            console.error(`Error getting status for ${file.name}:`, error);
            return {
              ...file,
              status: 'NotFound'
            };
          }
        })
      );
      
      setFiles(filesWithStatus);
      
      // Load statistics
      try {
        const stats = await fileService.getFileStatistics();
        console.log('Statistics from API:', stats);
        
        // Map status names to counts - API returns Dictionary<FileStatus, int>
        const mappedStats = {
          Clean: stats.Clean || 0,
          Infected: stats.Infected || 0,
          Scanning: stats.Scanning || 0,
          NotFound: stats.NotFound || 0
        };
        
        setStatistics(mappedStats);
      } catch (error) {
        console.error('Error loading statistics:', error);
      }
    } catch (error) {
      console.error('Error loading files:', error);
      showNotification('error', 'Loading Error', 'Failed to load file list');
    }
  }, [selectedFolder]);

  // File upload
  const handleUpload = async (file, onProgress) => {
    try {
      setIsUploading(true);
      
      // Upload progress simulation
      let progress = 0;
      const progressInterval = setInterval(() => {
        progress += Math.random() * 30;
        if (progress > 90) progress = 90;
        onProgress(progress);
      }, 200);

      await fileService.uploadFile(file, selectedFolder);
      
      clearInterval(progressInterval);
      onProgress(100);
      
      showNotification('success', 'Upload Successful', `File "${file.name}" uploaded successfully`);
      
      // Update file list
      setTimeout(() => {
        loadFiles();
      }, 1000);
      
    } catch (error) {
      console.error('Error uploading file:', error);
      showNotification('error', 'Upload Error', `Error uploading file "${file.name}": ${error.message}`);
    } finally {
      setIsUploading(false);
    }
  };

  // Upload completion
  const handleUploadComplete = (fileName, status, errorMessage) => {
    if (status === 'success') {
      showNotification('success', 'Upload Complete', `File "${fileName}" uploaded successfully`);
    } else {
      showNotification('error', 'Upload Error', errorMessage || `Error uploading file "${fileName}"`);
    }
  };

  // File deletion
  const handleFileDelete = (fileName) => {
    setFiles(prevFiles => prevFiles.filter(file => file.name !== fileName));
    showNotification('success', 'File Deleted', `File "${fileName}" deleted successfully`);
  };

  // Show notification
  const showNotification = (type, title, message) => {
    setNotification({
      isVisible: true,
      type,
      title,
      message
    });
  };

  // Close notification
  const closeNotification = () => {
    setNotification(prev => ({ ...prev, isVisible: false }));
  };

  // Change folder
  const handleFolderChange = (folder) => {
    setSelectedFolder(folder);
  };

  // Load files when folder changes
  useEffect(() => {
    loadFiles();
  }, [selectedFolder, loadFiles]);

  // Periodic file list update
  useEffect(() => {
    const interval = setInterval(() => {
      loadFiles();
    }, 30000); // Update every 30 seconds

    return () => clearInterval(interval);
  }, [loadFiles]);

  const folders = [
    { id: 'CentralStorage', name: 'Central Storage', icon: Folder }
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex items-center justify-between h-16">
            <div className="flex items-center space-x-4">
              <div className="flex items-center space-x-2">
                <Shield className="h-8 w-8 text-primary-600" />
                <h1 className="text-xl font-bold text-gray-900">
                  Unified File Gateway
                </h1>
              </div>
            </div>
            
            <div className="flex items-center space-x-4">
              <div className="flex items-center space-x-2 text-sm text-gray-500">
                <Settings size={16} />
                <span>Version 1.0.0</span>
              </div>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="grid grid-cols-1 lg:grid-cols-4 gap-8">
          {/* Sidebar */}
          <div className="lg:col-span-1">
            <div className="bg-white rounded-lg shadow p-6">
              <h2 className="text-lg font-medium text-gray-900 mb-4">Folders</h2>
              <div className="space-y-2">
                {folders.map((folder) => {
                  const Icon = folder.icon;
                  return (
                    <button
                      key={folder.id}
                      onClick={() => handleFolderChange(folder.id)}
                      className={`
                        w-full flex items-center space-x-3 px-3 py-2 rounded-md text-left transition-colors
                        ${selectedFolder === folder.id
                          ? 'bg-primary-100 text-primary-700 border border-primary-200'
                          : 'text-gray-700 hover:bg-gray-100'
                        }
                      `}
                    >
                      <Icon size={16} />
                      <span className="text-sm font-medium">{folder.name}</span>
                    </button>
                  );
                })}
              </div>
              
              <div className="mt-6 pt-6 border-t border-gray-200">
                <h3 className="text-sm font-medium text-gray-900 mb-2">Statistics</h3>
                <div className="space-y-2 text-sm text-gray-600">
                  <div className="flex justify-between">
                    <span>Total files:</span>
                    <span className="font-medium">{files.length}</span>
                  </div>
                  <div className="flex justify-between">
                    <span>Clean:</span>
                    <span className="font-medium text-success-600">
                      {statistics.Clean}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span>Scanning:</span>
                    <span className="font-medium text-warning-600">
                      {statistics.Scanning}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span>Infected:</span>
                    <span className="font-medium text-danger-600">
                      {statistics.Infected}
                    </span>
                  </div>
                  <div className="flex justify-between">
                    <span>Not found:</span>
                    <span className="font-medium text-gray-600">
                      {statistics.NotFound}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Main Content Area */}
          <div className="lg:col-span-3 space-y-6">
            {/* Upload Section */}
            <div className="bg-white rounded-lg shadow p-6">
              <div className="flex items-center space-x-2 mb-4">
                <Upload size={20} className="text-primary-600" />
                <h2 className="text-lg font-medium text-gray-900">File Upload</h2>
              </div>
              
              <div className="mb-4">
                <p className="text-sm text-gray-600">
                  Selected folder: <span className="font-medium">{folders.find(f => f.id === selectedFolder)?.name}</span>
                </p>
              </div>
              
              <FileUpload
                onUpload={handleUpload}
                onUploadComplete={handleUploadComplete}
                isUploading={isUploading}
                onFileListRefresh={loadFiles}
              />
            </div>
            
            {/* File List Section */}
            <div className="bg-white rounded-lg shadow p-6">
              <h2 className="text-lg font-medium text-gray-900 mb-4">Files</h2>
              <FileList
                files={files}
                onFileDelete={handleFileDelete}
                onRefresh={loadFiles}
              />
            </div>
          </div>
        </div>
      </main>

      {/* Notification */}
      <Notification
        type={notification.type}
        title={notification.title}
        message={notification.message}
        isVisible={notification.isVisible}
        onClose={closeNotification}
      />
    </div>
  );
}

export default App; 