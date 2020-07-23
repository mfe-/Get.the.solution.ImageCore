using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class FileService : IFileService
    {
        private readonly ILocalSettings _localSettings;
        private readonly ILoggerService _loggerService;

        public FileService(ILocalSettings localSettings, ILoggerService loggerService)
        {
            _localSettings = localSettings;
            _loggerService = loggerService;
        }
        public async Task<bool> HasGlobalWriteAccessAsync()
        {
            bool hasGlobalWriteAccess = false;
            try
            {
                //try to create a test file to check if we have "global" write access
                string path = Environment.GetEnvironmentVariable("USERPROFILE");

                StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(path);
                Guid guidtestfileName = Guid.NewGuid();
                StorageFile storageFile = await storageFolder.CreateFileAsync(guidtestfileName.ToString(), CreationCollisionOption.GenerateUniqueName);
                await storageFile.DeleteAsync();
                hasGlobalWriteAccess = true;
            }
            catch (System.UnauthorizedAccessException)
            {
                hasGlobalWriteAccess = false;
            }
            if (!_localSettings.Values.ContainsKey(nameof(HasGlobalWriteAccessAsync)))
            {
                _localSettings.Values.Add(nameof(HasGlobalWriteAccessAsync), hasGlobalWriteAccess);
            }
            else
            {
                _localSettings.Values[nameof(HasGlobalWriteAccessAsync)] = hasGlobalWriteAccess;
            }

            return hasGlobalWriteAccess;
        }
        /// <summary>
        /// Pass result of <seealso cref="Windows.Storage.Pickers.FileSavePicker.PickSaveFileAsync"/>, 
        /// <seealso cref="Windows.Storage.Pickers.FileOpenPicker.PickMultipleFilesAsync"/> or 
        /// <seealso cref="Windows.Storage.Pickers.FolderPicker.PickSingleFolderAsync"/> to this method to save future access to the picked files/folders
        /// </summary>
        /// <param name="storageItems">The picked items of the file/folder dialog</param>
        /// <returns>A task which indicates the process of executing this method</returns>
        public Task OnPickStorageItemsAsync(IReadOnlyList<IStorageItem> storageItems)
        {
            foreach (IStorageItem storageFile in storageItems)
            {
                if (storageFile != null)
                {
                    Guid guidStorageFile = Guid.NewGuid();
                    // Application now has read/write access to all contents in the picked folder (including other sub-folder contents)
                    StorageApplicationPermissions.FutureAccessList.AddOrReplace(guidStorageFile.ToString(), storageFile);
                    // Add to FA without metadata
                    string faToken = StorageApplicationPermissions.FutureAccessList.Add(storageFile);
                    string mruToken = StorageApplicationPermissions.MostRecentlyUsedList.Add(storageFile, guidStorageFile.ToString());

                    string pathKey = storageFile.Path.ToLowerInvariant();

                    //check if we have the dictionary key then we dont need to add the file key
                    string directoryKey = Path.GetDirectoryName(storageFile.Path).ToLowerInvariant();
                    //todo- wenn man verzeichnis hinzufügt alle files raus löschen

                    if (!_localSettings.Values.ContainsKey(pathKey))
                    {
                        _localSettings.Values.Add(pathKey, guidStorageFile.ToString());
                        _loggerService.LogEvent(nameof(guidStorageFile), nameof(_localSettings.Values.Add));
                    }
                    else
                    {
                        _localSettings.Values[pathKey] = guidStorageFile.ToString();
                        _loggerService.LogEvent(nameof(guidStorageFile), nameof(StorageApplicationPermissions.FutureAccessList.AddOrReplace));
                    }

                    _loggerService.LogEvent(nameof(storageFile), new Dictionary<string, string>()
                    {
                        { nameof(storageFile),pathKey},
                        { nameof(guidStorageFile),guidStorageFile.ToString() },
                        { nameof(mruToken), mruToken},
                        { nameof(faToken), faToken},
                    });
                }

            }
            return Task.CompletedTask;
        }
        public async Task<IStorageItem> TryGetWriteAbleStorageItemAsync(IStorageItem item)
        {
            // get the file attributes for file or directory
            System.IO.FileAttributes attr;
            string path = item.Path;

            if (item is IStorageFolder)
            {
                attr = System.IO.FileAttributes.Directory;
            }
            else
            {
                attr = System.IO.FileAttributes.Normal;
            }

            return await TryGetWriteAbleStorageItemAsync(path, attr);
        }
        /// <summary>
        /// Looks up the overgiven file or path and checks if a token was stored for it, to retriev the file/folder by 
        /// <seealso cref="StorageApplicationPermissions.FutureAccessList"/> in order to have write access to it
        /// </summary>
        /// <param name="path">The path of the dictionary or file</param>
        /// <param name="attr">This parameter should indicates whether we should retriev a Dictionary or a file</param>
        /// <returns>The found file or dictionary. If not access token was stored it returns null.</returns>
        public async Task<IStorageItem> TryGetWriteAbleStorageItemAsync(string path, System.IO.FileAttributes attr)
        {
            string pathKey;
            //detect whether its a directory or file
            if ((attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
            {
                pathKey = path.ToLowerInvariant();
                if (_localSettings.Values.ContainsKey(pathKey))
                {
                    string guidToken = _localSettings.Values[pathKey].ToString();
                    return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(guidToken);
                }
            }
            else
            {
                pathKey = path.ToLowerInvariant();
                if (_localSettings.Values.ContainsKey(pathKey))
                {
                    string guidToken = _localSettings.Values[pathKey].ToString();
                    return await StorageApplicationPermissions.FutureAccessList.GetFileAsync(guidToken);
                }
                else
                {
                    //we dont have the file index maybe we got the directory of it...
                    pathKey = Path.GetDirectoryName(path).ToLowerInvariant();
                    if (_localSettings.Values.ContainsKey(pathKey))
                    {
                        string guidToken = _localSettings.Values[pathKey].ToString();
                        StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(guidToken);

                        StorageFile storageFile = await storageFolder.GetFileAsync(new FileInfo(path).Name);
                        return storageFile;
                    }
                }
            }
            //no guidtoken available for the overgiven path
            return null;
        }

        public static async Task CleanUpFolderAsync(StorageFolder storageFolder, IList<string> filesToRemoveWithFileExtension)
        {
            //remove old cached images
            IReadOnlyList<StorageFile> storageFiles = await storageFolder.GetFilesAsync();
            foreach (StorageFile file in storageFiles)
            {
                if (filesToRemoveWithFileExtension.Any(a => a.Contains(file.FileType.ToLowerInvariant())))
                {
                    DateTime createdDateTime = file.DateCreated.DateTime;
                    if ((DateTime.Now - createdDateTime).TotalDays >= 7)
                    {
                        await file.DeleteAsync();
                    }
                }
            }
        }
    }
}
