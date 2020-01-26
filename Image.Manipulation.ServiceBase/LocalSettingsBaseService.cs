﻿using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using System.Collections.Generic;
using System.ComponentModel;

namespace Get.the.solution.Image.Manipulation.ServiceBase
{
    public abstract class LocalSettingsBaseService : ILocalSettings, INotifyPropertyChanged
    {
        protected ILoggerService _loggerService;
        protected LocalSettingsBaseService(ILoggerService loggerService)
        {
            _loggerService?.LogEvent(nameof(ImageQuality), $"{ImageQuality}");
        }

        public abstract IDictionary<string, object> Values { get; }

        protected int _ImageQuality;
        public int ImageQuality
        {
            get { return _ImageQuality; }
            set
            {
                SetProperty(ref _ImageQuality, value, nameof(ImageQuality));
                Values[nameof(ImageQuality)] = _ImageQuality;
            }
        }


        private bool _ClearImageListAfterSuccess;
        public bool ClearImageListAfterSuccess
        {
            get { return _ClearImageListAfterSuccess; }
            set
            {
                SetProperty(ref _ClearImageListAfterSuccess, value, nameof(ClearImageListAfterSuccess));
                _loggerService?.LogEvent(nameof(ClearImageListAfterSuccess), $"{_ClearImageListAfterSuccess}");
                Values[nameof(ClearImageListAfterSuccess)] = _ClearImageListAfterSuccess;
            }
        }


        protected bool _EnableImageViewer;
        public bool EnabledImageViewer
        {
            get { return _EnableImageViewer; }
            set
            {
                SetProperty(ref _EnableImageViewer, value, nameof(EnabledImageViewer));
                Values[nameof(EnabledImageViewer)] = _EnableImageViewer;
                _loggerService?.LogEvent(nameof(EnabledImageViewer), $"{EnabledImageViewer}");
            }
        }

        protected bool _EnableOpenSingleFileAfterResize;
        public bool EnabledOpenSingleFileAfterResize
        {
            get { return _EnableOpenSingleFileAfterResize; }
            set
            {
                SetProperty(ref _EnableOpenSingleFileAfterResize, value, nameof(EnabledOpenSingleFileAfterResize));
                Values[nameof(EnabledOpenSingleFileAfterResize)] = _EnableOpenSingleFileAfterResize;
                _loggerService?.LogEvent(nameof(EnabledOpenSingleFileAfterResize), $"{EnabledOpenSingleFileAfterResize}");
            }
        }


        protected bool _ShowSuccessMessage;
        public bool ShowSuccessMessage
        {
            get { return _ShowSuccessMessage; }
            set
            {
                SetProperty(ref _ShowSuccessMessage, value, nameof(ShowSuccessMessage));
                Values[nameof(ShowSuccessMessage)] = _ShowSuccessMessage;
                _loggerService?.LogEvent(nameof(ShowSuccessMessage), $"{ShowSuccessMessage}");
            }
        }

        protected bool _EnableAddImageToGallery;
        public bool EnableAddImageToGallery
        {
            get { return _EnableAddImageToGallery; }
            set { SetProperty(ref _EnableAddImageToGallery, value, nameof(EnableAddImageToGallery)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
