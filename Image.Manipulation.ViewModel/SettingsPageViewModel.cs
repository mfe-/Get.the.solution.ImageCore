using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using Prism.Mvvm;
using System;

namespace Get.the.solution.Image.Manipulation.ViewModel
{
    public class SettingsPageViewModel : BindableBase
    {
        protected readonly ILoggerService _LoggerService;
        protected readonly ILocalSettings<ResizeSettings> _localSettings;
        public SettingsPageViewModel(ILoggerService loggerService, ILocalSettings<ResizeSettings> localSettings)
        {
            try
            {
                _LoggerService = loggerService;
                _localSettings = localSettings;
                _LoggerService?.LogEvent(nameof(SettingsPageViewModel));
            }
            catch (Exception e)
            {
                _LoggerService?.LogException(nameof(AboutPageViewModel), e);
            }
        }
        public ILocalSettings<ResizeSettings> LocalSettings => _localSettings;
    }
}
