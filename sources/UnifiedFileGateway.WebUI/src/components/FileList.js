import React, { useState } from 'react';
import { 
  File, 
  Download, 
  Trash2, 
  Shield, 
  AlertTriangle, 
  Clock, 
  HelpCircle,
  RefreshCw
} from 'lucide-react';
import { fileService } from '../services/api';

const FileList = ({ files, onFileDelete, onRefresh }) => {
  const [downloadingFiles, setDownloadingFiles] = useState(new Set());
  const [deletingFiles, setDeletingFiles] = useState(new Set());

  const getStatusIcon = (status) => {
    switch (status) {
      case 'Clean':
        return <Shield className="h-4 w-4 text-green-600" />;
      case 'Infected':
        return <AlertTriangle className="h-4 w-4 text-red-600" />;
      case 'Scanning':
        return <Clock className="h-4 w-4 text-yellow-600" />;
      case 'NotFound':
        return <HelpCircle className="h-4 w-4 text-gray-600" />;
      default:
        return <HelpCircle className="h-4 w-4 text-gray-600" />;
    }
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'Clean':
        return 'text-green-600 bg-green-50 border-green-200';
      case 'Infected':
        return 'text-red-600 bg-red-50 border-red-200';
      case 'Scanning':
        return 'text-yellow-600 bg-yellow-50 border-yellow-200';
      case 'NotFound':
        return 'text-gray-600 bg-gray-50 border-gray-200';
      default:
        return 'text-gray-600 bg-gray-50 border-gray-200';
    }
  };

  const handleDownload = async (fileName) => {
    try {
      setDownloadingFiles(prev => new Set(prev).add(fileName));
      
      const blob = await fileService.downloadFile(fileName);
      
      // Create download link
      const url = window.URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
      window.URL.revokeObjectURL(url);
      
    } catch (error) {
      console.error('Error downloading file:', error);
      alert(`Error downloading file: ${error.message}`);
    } finally {
      setDownloadingFiles(prev => {
        const newSet = new Set(prev);
        newSet.delete(fileName);
        return newSet;
      });
    }
  };

  const handleDelete = async (fileName) => {
    if (!window.confirm(`Are you sure you want to delete "${fileName}"?`)) {
      return;
    }

    try {
      setDeletingFiles(prev => new Set(prev).add(fileName));
      
      await fileService.deleteFile(fileName);
      onFileDelete(fileName);
      
    } catch (error) {
      console.error('Error deleting file:', error);
      alert(`Error deleting file: ${error.message}`);
    } finally {
      setDeletingFiles(prev => {
        const newSet = new Set(prev);
        newSet.delete(fileName);
        return newSet;
      });
    }
  };

  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString() + ' ' + date.toLocaleTimeString();
  };

  if (files.length === 0) {
    return (
      <div className="text-center py-8">
        <File className="h-12 w-12 text-gray-400 mx-auto mb-4" />
        <h3 className="text-lg font-medium text-gray-900 mb-2">No files found</h3>
        <p className="text-gray-500">Upload some files to get started</p>
      </div>
    );
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-4">
        <h3 className="text-lg font-medium text-gray-900">
          Files ({files.length})
        </h3>
        <button
          onClick={onRefresh}
          className="flex items-center space-x-2 px-3 py-1 text-sm bg-primary-600 text-white rounded hover:bg-primary-700"
        >
          <RefreshCw className="h-4 w-4" />
          <span>Refresh</span>
        </button>
      </div>

      <div className="space-y-3">
        {files.map((file) => (
          <div
            key={file.name}
            className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
          >
            <div className="flex items-center justify-between">
              <div className="flex items-center space-x-3 flex-1">
                <File className="h-8 w-8 text-gray-500" />
                
                <div className="flex-1 min-w-0">
                  <div className="flex items-center space-x-2">
                    <h4 className="text-sm font-medium text-gray-900 truncate">
                      {file.name}
                    </h4>
                    {getStatusIcon(file.status)}
                  </div>
                  
                  <div className="flex items-center space-x-4 mt-1 text-xs text-gray-500">
                    <span>{formatFileSize(file.size || 0)}</span>
                    {file.lastModified && (
                      <span>{formatDate(file.lastModified)}</span>
                    )}
                  </div>
                </div>
              </div>

              <div className="flex items-center space-x-2">
                {/* Status Badge */}
                <span className={`
                  px-2 py-1 text-xs font-medium rounded-full border
                  ${getStatusColor(file.status)}
                `}>
                  {file.status}
                </span>

                {/* Download Button */}
                <button
                  onClick={() => handleDownload(file.name)}
                  disabled={downloadingFiles.has(file.name)}
                  className="p-2 text-gray-500 hover:text-primary-600 hover:bg-primary-50 rounded-md transition-colors disabled:opacity-50"
                  title="Download file"
                >
                  {downloadingFiles.has(file.name) ? (
                    <RefreshCw className="h-4 w-4 animate-spin" />
                  ) : (
                    <Download className="h-4 w-4" />
                  )}
                </button>

                {/* Delete Button */}
                <button
                  onClick={() => handleDelete(file.name)}
                  disabled={deletingFiles.has(file.name)}
                  className="p-2 text-gray-500 hover:text-red-600 hover:bg-red-50 rounded-md transition-colors disabled:opacity-50"
                  title="Delete file"
                >
                  {deletingFiles.has(file.name) ? (
                    <RefreshCw className="h-4 w-4 animate-spin" />
                  ) : (
                    <Trash2 className="h-4 w-4" />
                  )}
                </button>
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default FileList; 