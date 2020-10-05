using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public abstract class ApplicationBaseService : IApplicationService
    {
        private readonly ILoggerService _loggerService;

        public ApplicationBaseService(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }
        public virtual string UriDefinitionOpen => "image-viewer:view";

        public virtual string UriDefinitionResize => "resize-image:resize";

        public async Task LaunchFileAsync(ImageFile imageFile, bool openWith = false)
        {
            if (!openWith)
            {
                await Launcher.LaunchFileAsync(imageFile.Tag as IStorageFile);
            }
            else
            {
                await Launcher.LaunchFileAsync(imageFile.Tag as IStorageFile, new LauncherOptions() { DisplayApplicationPicker = true });
            }
        }

        public async Task LaunchFileAsync(string protocol, object param)
        {
            ImageFile imageFile = param as ImageFile;
            protocol = $"{UriDefinitionResize}?{protocol}={imageFile?.Path}";
            await Launcher.LaunchUriAsync(new Uri(protocol));
        }
        public async Task<bool> LaunchUriAsync(string protocol)
        {
            return await Launcher.LaunchUriAsync(new Uri(protocol));
        }
        public void Exit() => CoreApplication.Exit();

        public bool CtrlPressed(object param)
        {
            if (param == null) return false;
            VirtualKey virtualKey;
            bool parseResult = Enum.TryParse(param.ToString(), true, out virtualKey);
            if (parseResult)
            {
                return (CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control) == CoreVirtualKeyStates.Down
                    && virtualKey.Equals(VirtualKey.O));
            }
            else
            {
                return false;
            }

        }

        public void SetAppTitlebar(string titleText)
        {
            try
            {
                ApplicationView.GetForCurrentView().Title = titleText;
            }
            catch (Exception e)
            {
                _loggerService.LogException(e);
            }

        }

        public abstract string GetAppVersion();

        public string GetLocalCacheFolder()
        {
            return ApplicationData.Current.TemporaryFolder.Path;
        }

        public string GetCulture()
        {
            return Windows.System.UserProfile.GlobalizationPreferences.Languages.FirstOrDefault();
        }

        public abstract string GetDeviceFormFactorType();

        public void SetActivatedEventArgs(String args)
        {
            ActivatedEventArgs = args;
        }
        public string ActivatedEventArgs { get; protected set; } = String.Empty;

        public void AddToClipboard(String content)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(content);
            Clipboard.SetContent(dataPackage);
        }
        public bool IsAlwaysOnTop
        {
            get
            {
                return !ApplicationViewMode.Default.Equals(ApplicationView.GetForCurrentView().ViewMode);
            }
        }
        public async Task<bool> ToggleAlwaysOnTopAsync()
        {
            bool modeSwitched = false;
            if (!ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
            {
                return modeSwitched;
            }
            if(!IsAlwaysOnTop)
            {
                modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
            }
            else
            {
                modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
            }
            return modeSwitched;
        }
    }
}
