using Get.the.solution.Image.Manipulation.Contract;
using System;

namespace Get.the.solution.Image.Manipulation.ViewModel.ResizeImage
{
    public class AboutPageViewModel : NotifyPropertyChanged
    {
        protected readonly ILoggerService _LoggerService;
        protected readonly IApplicationService _applicationService;
        public AboutPageViewModel(ILoggerService loggerService, IApplicationService applicationService) : base(loggerService)
        {
            try
            {
                _LoggerService = loggerService;
                _applicationService = applicationService;
                _LoggerService?.LogEvent(nameof(AboutPageViewModel));
            }
            catch (Exception e)
            {
                _LoggerService?.LogException(nameof(AboutPageViewModel), e);
            }
        }


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
