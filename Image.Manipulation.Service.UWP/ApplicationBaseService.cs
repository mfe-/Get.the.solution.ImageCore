using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public abstract class ApplicationBaseService : IApplicationService
    {
        private const string fileProtocol = "file:///";
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
        /// <summary>
        /// Starts the file explorer by using the <seealso cref="fileProtocol"/>. If the <paramref name="protocol"/> contains only the
        /// directory it opens the file explorer with the provided directory. If <paramref name="protocol"/> points to a file, it opens 
        /// the file explorer with the related directory and selects the given file.
        /// </summary>
        /// <param name="protocol">E.g. file:///C:\Users\user\Downloads or file:///C:\Users\user\Downloads\2020.11.23-46.26.jpg</param>
        /// <returns></returns>
        public async Task<bool> LaunchFileExplorerAsync(string protocol)
        {
            IStorageItem storageItem = await TryGetStorageItemFromProtocol(protocol);

            if (storageItem is IStorageFolder storageFolder)
            {
                return await Launcher.LaunchFolderAsync(storageFolder, new FolderLauncherOptions() { });
            }
            else if (storageItem is IStorageFile storageFile)
            {
                FolderLauncherOptions folderLauncherOptions = new FolderLauncherOptions();
                folderLauncherOptions.ItemsToSelect.Add(storageFile);

                var path = System.IO.Path.GetDirectoryName(storageFile.Path);
                var storagefolder = await StorageFolder.GetFolderFromPathAsync(path);

                return await Launcher.LaunchFolderAsync(storagefolder, folderLauncherOptions);
            }
            throw new NotImplementedException($"{nameof(TryGetStorageItemFromProtocol)} should return {nameof(IStorageFolder)} or {nameof(IStorageFile)}");
        }
        /// <summary>
        /// Extracts the file or directory from the given <paramref name="protocol"/> and returns its <seealso cref="IStorageItem"/>
        /// </summary>
        /// <exception cref="Contract.Exceptions.UnauthorizedAccessException" />
        /// <param name="protocol"></param>
        /// <returns></returns>
        private async Task<IStorageItem> TryGetStorageItemFromProtocol(string protocol)
        {
            if (protocol.StartsWith(fileProtocol))
            {
                protocol = protocol.Replace(fileProtocol, string.Empty);

                try
                {
                    StorageFile storageFile = null;
                    StorageFolder storageFolder = null;
                    try
                    {
                        storageFile = await StorageFile.GetFileFromPathAsync(protocol);
                        return storageFile;
                    }
                    catch (ArgumentException)
                    {
                        //not a file
                    }

                    storageFolder = await StorageFolder.GetFolderFromPathAsync(protocol);
                    return storageFolder;
                }
                catch (System.UnauthorizedAccessException e)
                {
                    throw new Contract.Exceptions.UnauthorizedAccessException(e);
                }
            }
            return null;
        }
        /// <summary>
        /// Launches the given <paramref name="protocol"/> parameter 
        /// </summary>
        /// <example>
        /// file:///C:\Users\marti\Downloads
        /// </example>
        /// <example>
        /// ms-settings:privacy-broadfilesystemaccess
        /// </example>
        /// <exception cref="Contract.Exceptions.UnauthorizedAccessException" />
        /// <param name="protocol"></param>
        /// <returns></returns>
        public async Task<bool> LaunchUriAsync(string protocol)
        {
            if (protocol.StartsWith(fileProtocol))
            {
                IStorageItem storageItem = await TryGetStorageItemFromProtocol(protocol);
                //detect whether its a directory or file
                if (storageItem is IStorageFolder storageFolder)
                {
                    return await Launcher.LaunchFolderAsync(storageFolder, new FolderLauncherOptions() { });
                }
                else if (storageItem is IStorageFile storageFile)
                {
                    return await Launcher.LaunchFileAsync(storageFile);
                }
            }
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
            if (!IsAlwaysOnTop)
            {
                modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
            }
            else
            {
                modeSwitched = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
            }
            return modeSwitched;
        }
        private Size? previousSize = null;
        public bool TrySetWindowSize()
        {
            if (!previousSize.HasValue) return false;
            return TrySetWindowSize(previousSize.Value.Width, previousSize.Value.Height);
        }
        public bool TrySetWindowSize(double width, double height)
        {
            ApplicationView applicationView = ApplicationView.GetForCurrentView();
            if (previousSize == null)
            {
                previousSize = new Size(applicationView.VisibleBounds.Width, applicationView.VisibleBounds.Height);
            }
            var preferredSize = new Size(width, height);
            if (!applicationView.IsFullScreenMode)
            {
                var successfull = applicationView.TryResizeView(preferredSize);
                if (!successfull)
                {
                    applicationView.TryResizeView(previousSize.Value);
                }
                return successfull;
            }
            return false;
        }
    }
}
