﻿using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Get.the.solution.Image.Manipulation.ServiceBase
{
    public abstract class LocalSettingsBaseService : ILocalSettings, INotifyPropertyChanged
    {
        protected ILoggerService _loggerService;
        public LocalSettingsBaseService(ILoggerService loggerService)
        {
        }

        public abstract IDictionary<string, object> Values { get; }

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