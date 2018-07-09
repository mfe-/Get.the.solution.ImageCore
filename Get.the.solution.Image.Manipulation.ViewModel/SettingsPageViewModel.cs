using Get.the.solution.Image.Manipulation.Contract;
using Prism.Mvvm;
using System;

namespace Get.the.solution.Image.Manipulation.ViewModel
{
    public class SettingsPageViewModel : BindableBase
    {
        protected readonly ILoggerService _LoggerService;
        protected readonly IApplicationService _applicationService;
        protected readonly ILocalSettings _localSettings;
        public SettingsPageViewModel(ILoggerService loggerService, ILocalSettings localSettings)
        {
            try
            {
                _LoggerService = loggerService;
                //_applicationService = applicationService;
                _localSettings = localSettings;
                
                _LoggerService?.LogEvent(nameof(SettingsPageViewModel));
            }
            catch (Exception e)
            {
                _LoggerService?.LogException(nameof(AboutPageViewModel), e);
            }
        }
        public ILocalSettings LocalSettings => _localSettings;
    }
}
