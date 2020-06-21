using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class WhatsNewService : IWhatsNewService
    {
        protected ILocalSettings _localSettings;
        protected IApplicationService _applicationService;
        protected ILoggerService _loggerService;
        protected ContentDialog _contentDialog;
        public WhatsNewService(ILoggerService loggerService, ILocalSettings localSettings, IApplicationService applicationService, ContentDialog contentDialog)
        {
            try
            {
                _loggerService = loggerService;
                _localSettings = localSettings;
                _applicationService = applicationService;

                if (!_localSettings.Values.ContainsKey(_applicationService.GetAppVersion()))
                {
                    IsAppUpdated = true;
                }
                else
                {
                    IsAppUpdated = false;
                }
                _contentDialog = contentDialog;
                _contentDialog.Closed += ContentDialog_Closed;
            }
            catch (Exception e)
            {
                _loggerService.LogException(nameof(WhatsNewService), e);
            }
        }

        private void ContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            _contentDialog.Closed -= ContentDialog_Closed;
            _contentDialog = null;
        }

        public bool IsAppUpdated { get; protected set; }

        public async Task<bool> ShowIfAppropriateWhatsNewDisplayAsync()
        {
            if (IsAppUpdated)
            {
                if (!_localSettings.Values.ContainsKey(_applicationService.GetAppVersion()))
                {
                    _localSettings.Values.Add(_applicationService.GetAppVersion(), true);
                }
                if (_contentDialog != null)
                {
                    var dialog = _contentDialog;
                    await dialog.ShowAsync();
                }
            }
            return IsAppUpdated;
        }
    }
}
