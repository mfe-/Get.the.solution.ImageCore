using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class FileSystemPermissionDialogService : IFileSystemPermissionDialogService
    {
        private ContentDialog _broadFileSytemAccessDialog;
        private readonly IApplicationService _applicationService;
        private readonly ILoggerService _loggerService;
        public FileSystemPermissionDialogService(ILoggerService loggerService, IApplicationService applicationService, ContentDialog broadFileSytemAccessDialog)
        {
            _broadFileSytemAccessDialog = broadFileSytemAccessDialog;
            _loggerService = loggerService;
            //_broadFileSytemAccessDialog.Closed += ContentDialog_Closed;
            _applicationService = applicationService;
        }
        /// <summary>
        /// Shows the user a dialog that the app needs file access permissions.
        /// Afterwards it starts the windows 10 settings for privacy-broadfilesystemaccess.
        /// </summary>
        /// <returns></returns>
        public async Task ShowFileSystemAccessDialogAsync()
        {
            try
            {
                var dialog = _broadFileSytemAccessDialog;
                await dialog.ShowAsync();
                //give the user the ability to give the neccessary permissions
                await _applicationService?.LaunchUriAsync("ms-settings:privacy-broadfilesystemaccess");
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(ShowFileSystemAccessDialogAsync), e);
            }

        }
        private void ContentDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            _broadFileSytemAccessDialog.Closed -= ContentDialog_Closed;
            _broadFileSytemAccessDialog = null;
        }
    }
}
