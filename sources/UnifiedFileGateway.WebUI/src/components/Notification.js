import React, { useEffect } from 'react';
import { X, CheckCircle, AlertCircle, Info, AlertTriangle } from 'lucide-react';

const Notification = ({ 
  type = 'info', 
  title, 
  message, 
  isVisible, 
  onClose, 
  autoClose = true, 
  duration = 5000 
}) => {
  useEffect(() => {
    if (autoClose && isVisible) {
      const timer = setTimeout(() => {
        onClose();
      }, duration);

      return () => clearTimeout(timer);
    }
  }, [isVisible, autoClose, duration, onClose]);

  if (!isVisible) return null;

  const getIcon = () => {
    switch (type) {
      case 'success':
        return <CheckCircle size={20} className="text-success-500" />;
      case 'error':
        return <AlertCircle size={20} className="text-danger-500" />;
      case 'warning':
        return <AlertTriangle size={20} className="text-warning-500" />;
      case 'info':
      default:
        return <Info size={20} className="text-primary-500" />;
    }
  };

  const getBackgroundColor = () => {
    switch (type) {
      case 'success':
        return 'bg-success-50 border-success-200';
      case 'error':
        return 'bg-danger-50 border-danger-200';
      case 'warning':
        return 'bg-warning-50 border-warning-200';
      case 'info':
      default:
        return 'bg-primary-50 border-primary-200';
    }
  };

  const getTextColor = () => {
    switch (type) {
      case 'success':
        return 'text-success-800';
      case 'error':
        return 'text-danger-800';
      case 'warning':
        return 'text-warning-800';
      case 'info':
      default:
        return 'text-primary-800';
    }
  };

  return (
    <div className={`
      fixed top-4 right-4 z-50 max-w-sm w-full
      border rounded-lg shadow-lg p-4
      transform transition-all duration-300 ease-in-out
      ${getBackgroundColor()}
      ${isVisible ? 'translate-x-0 opacity-100' : 'translate-x-full opacity-0'}
    `}>
      <div className="flex items-start space-x-3">
        <div className="flex-shrink-0">
          {getIcon()}
        </div>
        
        <div className="flex-1 min-w-0">
          {title && (
            <h3 className={`text-sm font-medium ${getTextColor()}`}>
              {title}
            </h3>
          )}
          {message && (
            <p className={`text-sm mt-1 ${getTextColor()}`}>
              {message}
            </p>
          )}
        </div>
        
        <div className="flex-shrink-0">
          <button
            onClick={onClose}
            className={`
              inline-flex items-center justify-center p-1 rounded-md
              hover:bg-white hover:bg-opacity-20 transition-colors
              ${getTextColor()}
            `}
          >
            <X size={16} />
          </button>
        </div>
      </div>
    </div>
  );
};

export default Notification; 