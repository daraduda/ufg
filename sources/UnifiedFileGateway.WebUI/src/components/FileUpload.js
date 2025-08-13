import React, { useCallback, useState } from 'react';
import { useDropzone } from 'react-dropzone';
import { Upload, File, AlertCircle, CheckCircle, Loader2, Shield, Bug, Info } from 'lucide-react';
import { createEicarTestFile, createMultipleEicarFiles, getEicarInfo } from '../utils/eicarGenerator';

const FileUpload = ({ onUpload, onUploadComplete, isUploading, onFileListRefresh }) => {
  const [uploadProgress, setUploadProgress] = useState({});
  const [uploadErrors, setUploadErrors] = useState({});
  const [showEicarInfo, setShowEicarInfo] = useState(false);

  const onDrop = useCallback(async (acceptedFiles) => {
    const newProgress = {};
    const newErrors = {};

    acceptedFiles.forEach(file => {
      newProgress[file.name] = 0;
    });

    setUploadProgress(newProgress);
    setUploadErrors(newErrors);

    for (const file of acceptedFiles) {
      try {
        await onUpload(file, (progress) => {
          setUploadProgress(prev => ({
            ...prev,
            [file.name]: progress
          }));
        });
        
        setUploadProgress(prev => ({
          ...prev,
          [file.name]: 100
        }));

        if (onUploadComplete) {
          onUploadComplete(file.name, 'success');
        }
      } catch (error) {
        console.error(`Error uploading ${file.name}:`, error);
        setUploadErrors(prev => ({
          ...prev,
          [file.name]: error.message || 'Upload error'
        }));
        
        if (onUploadComplete) {
          onUploadComplete(file.name, 'error', error.message);
        }
      }
    }
  }, [onUpload, onUploadComplete]);

  const { getRootProps, getInputProps, isDragActive, isDragReject } = useDropzone({
    onDrop,
    accept: {
      'image/*': ['.jpeg', '.jpg', '.png', '.gif'],
      'application/pdf': ['.pdf'],
      'application/msword': ['.doc'],
      'application/vnd.openxmlformats-officedocument.wordprocessingml.document': ['.docx'],
      'application/vnd.ms-excel': ['.xls'],
      'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet': ['.xlsx'],
      'text/plain': ['.txt'],
      'application/zip': ['.zip'],
      'application/x-rar-compressed': ['.rar']
    },
    maxSize: 100 * 1024 * 1024, // 100MB
    multiple: true
  });

  const formatFileSize = (bytes) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const handleCreateEicarFile = async () => {
    try {
      const eicarFile = createEicarTestFile();
      await onUpload(eicarFile);
      if (onUploadComplete) {
        onUploadComplete(eicarFile.name, 'success');
      }
      // Refresh file list after upload
      setTimeout(() => {
        if (onFileListRefresh) {
          onFileListRefresh();
        }
      }, 1000);
    } catch (error) {
      console.error('Error creating EICAR file:', error);
      if (onUploadComplete) {
        onUploadComplete('eicar_test.txt', 'error', error.message);
      }
    }
  };

  const handleCreateMultipleEicarFiles = async () => {
    try {
      const eicarFiles = createMultipleEicarFiles();
      for (const file of eicarFiles) {
        await onUpload(file);
        if (onUploadComplete) {
          onUploadComplete(file.name, 'success');
        }
      }
      // Refresh file list after upload
      setTimeout(() => {
        if (onFileListRefresh) {
          onFileListRefresh();
        }
      }, 1000);
    } catch (error) {
      console.error('Error creating multiple EICAR files:', error);
      if (onUploadComplete) {
        onUploadComplete('multiple_eicar_files', 'error', error.message);
      }
    }
  };

  return (
    <div>
      {/* EICAR Test Files Section */}
      <div className="mb-6 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
        <div className="flex items-center justify-between mb-3">
          <div className="flex items-center space-x-2">
            <Bug className="h-5 w-5 text-yellow-600" />
            <h3 className="text-sm font-medium text-yellow-800">Antivirus Test Files</h3>
          </div>
          <button
            onClick={() => setShowEicarInfo(!showEicarInfo)}
            className="text-yellow-600 hover:text-yellow-800"
          >
            <Info className="h-4 w-4" />
          </button>
        </div>
        
        {showEicarInfo && (
          <div className="mb-3 p-3 bg-yellow-100 rounded text-sm text-yellow-800">
            <p className="mb-2">
              <strong>EICAR Test File:</strong> A harmless test file that antivirus software detects as malicious. 
              Used to verify that antivirus scanning is working correctly.
            </p>
            <p className="text-xs">
              Content: <code className="bg-yellow-200 px-1 rounded">X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*</code>
            </p>
          </div>
        )}
        
        <div className="flex space-x-2">
          <button
            onClick={handleCreateEicarFile}
            disabled={isUploading}
            className="px-3 py-1 text-xs bg-yellow-600 text-white rounded hover:bg-yellow-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Create EICAR Test File
          </button>
          <button
            onClick={handleCreateMultipleEicarFiles}
            disabled={isUploading}
            className="px-3 py-1 text-xs bg-yellow-600 text-white rounded hover:bg-yellow-700 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            Create Multiple EICAR Files
          </button>
        </div>
      </div>

      {/* File Upload Area */}
      <div
        {...getRootProps()}
        className={`
          border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors
          ${isDragActive ? 'border-primary-500 bg-primary-50' : 'border-gray-300 hover:border-primary-400'}
          ${isDragReject ? 'border-red-500 bg-red-50' : ''}
        `}
      >
        <input {...getInputProps()} />
        
        <div className="flex flex-col items-center space-y-4">
          <div className="flex items-center justify-center w-16 h-16 bg-primary-100 rounded-full">
            <Upload className="h-8 w-8 text-primary-600" />
          </div>
          
          <div>
            <p className="text-lg font-medium text-gray-900">
              {isDragActive ? 'Drop files here' : 'Drag & drop files here'}
            </p>
            <p className="text-sm text-gray-500 mt-1">
              or click to select files
            </p>
          </div>
          
          <div className="text-xs text-gray-400">
            <p>Supported formats: PDF, DOC, DOCX, XLS, XLSX, TXT, ZIP, RAR, Images</p>
            <p>Maximum file size: 100MB</p>
          </div>
        </div>
      </div>

      {/* Upload Progress */}
      {Object.keys(uploadProgress).length > 0 && (
        <div className="mt-6 space-y-3">
          {Object.entries(uploadProgress).map(([fileName, progress]) => (
            <div key={fileName} className="bg-gray-50 rounded-lg p-3">
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center space-x-2">
                  <File className="h-4 w-4 text-gray-500" />
                  <span className="text-sm font-medium text-gray-700">{fileName}</span>
                </div>
                <span className="text-sm text-gray-500">{Math.round(progress)}%</span>
              </div>
              
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className="bg-primary-600 h-2 rounded-full transition-all duration-300"
                  style={{ width: `${progress}%` }}
                />
              </div>
              
              {uploadErrors[fileName] && (
                <div className="mt-2 flex items-center space-x-2 text-red-600">
                  <AlertCircle className="h-4 w-4" />
                  <span className="text-sm">{uploadErrors[fileName]}</span>
                </div>
              )}
              
              {progress === 100 && !uploadErrors[fileName] && (
                <div className="mt-2 flex items-center space-x-2 text-green-600">
                  <CheckCircle className="h-4 w-4" />
                  <span className="text-sm">Upload complete</span>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Loading State */}
      {isUploading && (
        <div className="mt-4 flex items-center justify-center space-x-2 text-primary-600">
          <Loader2 className="h-4 w-4 animate-spin" />
          <span className="text-sm">Processing files...</span>
        </div>
      )}
    </div>
  );
};

export default FileUpload; 