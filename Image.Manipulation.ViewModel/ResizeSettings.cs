using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using System.Collections.Generic;
using System.ComponentModel;

namespace Get.the.solution.Image.Manipulation.ViewModel
{
    public class ResizeSettings : INotifyPropertyChanged, ILocalSettings
    {
        private readonly ILoggerService _loggerService;
        public ResizeSettings()
        {
            _Values = new Dictionary<string, object>();
            _PreferredSizeWidth = 515;
            _PreferredSizeHeight = 450;

            _ShowSuccessMessage = false;
            _EnabledOpenSingleFileAfterResize = false;
            _ClearImageListAfterSuccess = true;
            _ImageQuality = 75;
            _KeepAspectRatio = false;
            _HeightPercent = 100;
            _WidthPercent = 100;
            _HeightCustom = 768;
            _WidthCustom = 1024;
            _OverwriteFiles = false;
            _RadioOptions = 4;

        }
        public ResizeSettings(ILoggerService loggerService) : this()
        {
            _loggerService = loggerService;
        }

        private Dictionary<string, object> _Values;
        public Dictionary<string, object>  Values
        {
            get { return _Values; }
            set { SetProperty(ref _Values, value, nameof(Values)); }
        }

        IDictionary<string, object> ILocalSettings.Values => Values;

        private int _RadioOptions;
        public int RadioOptions
        {
            get { return _RadioOptions; }
            set { SetProperty(ref _RadioOptions, value, nameof(RadioOptions)); }
        }


        private bool _OverwriteFiles;
        public bool OverwriteFiles
        {
            get { return _OverwriteFiles; }
            set { SetProperty(ref _OverwriteFiles, value, nameof(OverwriteFiles)); }
        }


        private int _WidthCustom;
        public int WidthCustom
        {
            get { return _WidthCustom; }
            set { SetProperty(ref _WidthCustom, value, nameof(WidthCustom)); }
        }


        private int _HeightCustom;
        public int HeightCustom
        {
            get { return _HeightCustom; }
            set { SetProperty(ref _HeightCustom, value, nameof(HeightCustom)); }
        }


        private int _WidthPercent;
        public int WidthPercent
        {
            get { return _WidthPercent; }
            set { SetProperty(ref _WidthPercent, value, nameof(WidthPercent)); }
        }


        private int _HeightPercent;
        public int HeightPercent
        {
            get { return _HeightPercent; }
            set { SetProperty(ref _HeightPercent, value, nameof(HeightPercent)); }
        }


        private bool _KeepAspectRatio;
        public bool KeepAspectRatio
        {
            get { return _KeepAspectRatio; }
            set { SetProperty(ref _KeepAspectRatio, value, nameof(KeepAspectRatio)); }
        }

        protected int _ImageQuality;
        public int ImageQuality
        {
            get { return _ImageQuality; }
            set { SetProperty(ref _ImageQuality, value, nameof(ImageQuality)); }
        }


        private bool _EnabledOpenSingleFileAfterResize;
        public bool EnabledOpenSingleFileAfterResize
        {
            get { return _EnabledOpenSingleFileAfterResize; }
            set { SetProperty(ref _EnabledOpenSingleFileAfterResize, value, nameof(EnabledOpenSingleFileAfterResize)); }
        }

        private bool _ClearImageListAfterSuccess;
        public bool ClearImageListAfterSuccess
        {
            get { return _ClearImageListAfterSuccess; }
            set { SetProperty(ref _ClearImageListAfterSuccess, value, nameof(ClearImageListAfterSuccess)); }
        }

        protected bool _ShowSuccessMessage;
        public bool ShowSuccessMessage
        {
            get { return _ShowSuccessMessage; }
            set { SetProperty(ref _ShowSuccessMessage, value, nameof(ShowSuccessMessage)); }
        }

        private double _PreferredSizeWidth;
        public double PreferredSizeWidth
        {
            get { return _PreferredSizeWidth; }
            set { SetProperty(ref _PreferredSizeWidth, value, nameof(PreferredSizeWidth)); }
        }


        private double _PreferredSizeHeight;
        public double PreferredSizeHeight
        {
            get { return _PreferredSizeHeight; }
            set { SetProperty(ref _PreferredSizeHeight, value, nameof(PreferredSizeHeight)); }
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
            _loggerService?.LogEvent(propertyName, value.ToString());
            return true;
        }
    }
}
