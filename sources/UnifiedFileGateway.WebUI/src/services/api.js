import axios from 'axios';

const API_BASE_URL = process.env.REACT_APP_API_URL || 'http://localhost:5000';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor for error handling
api.interceptors.response.use(
  (response) => response,
  (error) => {
    console.error('API Error:', error);
    return Promise.reject(error);
  }
);

export const fileService = {
  // File upload
  uploadFile: async (file, folder = 'CentralStorage') => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('folder', folder);
    
    const response = await api.post('/api/files/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    });
    return response.data;
  },

  // Get file status
  getFileStatus: async (fileName) => {
    const response = await api.get(`/api/files/status/${encodeURIComponent(fileName)}`);
    return response.data;
  },

  // Download file
  downloadFile: async (fileName) => {
    const response = await api.get(`/api/files/download/${encodeURIComponent(fileName)}`, {
      responseType: 'blob',
    });
    return response.data;
  },

  // Delete file
  deleteFile: async (fileName) => {
    const response = await api.delete(`/api/files/${encodeURIComponent(fileName)}`);
    return response.data;
  },

  // Get list of files
  getFiles: async (folder = 'CentralStorage') => {
    const response = await api.get(`/api/files?folder=${encodeURIComponent(folder)}`);
    return response.data;
  },

  // Get file statistics
  getFileStatistics: async () => {
    const response = await api.get('/api/files/statistics');
    return response.data;
  },
};

export default api; 