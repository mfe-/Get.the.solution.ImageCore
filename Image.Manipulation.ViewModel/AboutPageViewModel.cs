using Get.the.solution.Image.Manipulation.Contract;
using System;

namespace Get.the.solution.Image.Manipulation.ViewModel.ResizeImage
{
    public class AboutPageViewModel : NotifyPropertyChanged
    {
        protected readonly ILoggerService _LoggerService;
        protected readonly IApplicationService _applicationService;
        private readonly ILocalSettings<ResizeSettings> _localSettings;

        public AboutPageViewModel(ILoggerService loggerService, IApplicationService applicationService, ILocalSettings<ResizeSettings> localSettings) : base(loggerService)
        {
            try
            {
                _LoggerService = loggerService;
                _applicationService = applicationService;
                _LoggerService?.LogEvent(nameof(AboutPageViewModel));
                _localSettings = localSettings;
            }
            catch (Exception e)
            {
                _LoggerService?.LogException(nameof(AboutPageViewModel), e);
            }

        }

        public bool ShowDebugInformation
        {
            get { return _localSettings?.Settings?.LogToFile ?? false; }

        }

        public string GitRevision { get; set; }

        public string AdsjumboApplicationIdKey { get; set; }

        public string AppCenterId { get; set; }


        public String AppVersion
        {
            get
            {
                return _applicationService.GetAppVersion();
            }
        }

        public String LocalCacheFolder
        {
            get { return _applicationService.GetTemporaryFolderPath(); }
        }

        public String Culture
        {
            get
            {
                return _applicationService.GetCulture();
            }
        }
    }
}
