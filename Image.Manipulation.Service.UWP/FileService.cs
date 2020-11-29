using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class FileService : IFileService
    {
        private readonly ApplicationDataContainer _localSettings;
        private readonly ILoggerService _loggerService;
        public static readonly string FileServiceContainer = "FileServiceContainer";
        public static readonly string HasGlobalWriteAccess = "HasGlobalWriteAccess";

        public FileService(ILoggerService loggerService)
        {
            ApplicationDataContainer localSettingsContainer = ApplicationData.Current.LocalSettings;
            if (!localSettingsContainer.Containers.ContainsKey(FileServiceContainer))
            {
                localSettingsContainer.CreateContainer(FileServiceContainer, ApplicationDataCreateDisposition.Always);
            }
            _localSettings = localSettingsContainer.Containers[FileServiceContainer];

            _loggerService = loggerService;
        }
        ///<inheritdoc/>
        public async Task<Stream> LoadStreamFromFileAsync(string path)
        {
            try
            {
                StorageFile storageFile = await StorageFile.GetFileFromPathAsync(path);
                return await LoadStreamFromFileAsync(storageFile);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new Contract.Exceptions.UnauthorizedAccessException(e);
            }

        }
        ///<inheritdoc/>
        public Task<Stream> LoadStreamFromFileAsync(object file)
        {
            if (file is StorageFile storageFile)
            {
                return storageFile.OpenStreamForReadAsync();
            }
            throw new NotSupportedException($"Parameter {nameof(file)} expected of type {nameof(StorageFile)}");
        }
        /// <summary>
        /// Opens the Save As Dialog and adds the file to the Future Access List
        /// </summary>
        /// <param name="preferredSaveLocation"></param>
        /// <param name="suggestedFileName"></param>
        /// <param name="fileTypeChoicesFilter"></param>
        /// <returns></returns>
        public virtual async Task<IStorageFile> PickSaveFileAsync(String preferredSaveLocation, String suggestedFileName, IList<string> fileTypeChoicesFilter)
        {
            FileSavePicker fileSavePicker = new FileSavePicker();
            FileInfo fileInfo = new FileInfo(suggestedFileName);

            fileSavePicker.DefaultFileExtension = fileInfo.Extension;
            fileSavePicker.FileTypeChoices.Add(fileInfo.Extension, fileTypeChoicesFilter);
            fileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            // Default file name if the user does not type one in or select a file to replace
            fileSavePicker.SuggestedFileName = suggestedFileName;
            IStorageFile file = await fileSavePicker.PickSaveFileAsync();
            //file is null when the user clicked on "abort" on save as dialog
            if (file != null)
            {
                if (!FileService.HasGlobalWritePermission())
                    await AddStorageItemsToFutureAccessListAsync(new IStorageItem[] { file });

                return file;
            }
            return null;
        }
        /// <summary>
        /// Opens the FilePicker and adds the selected files to the Future Access List
        /// </summary>
        /// <param name="fileTypeChoicesFilter"></param>
        /// <returns></returns>
        public async Task<IReadOnlyList<IStorageFile>> PickMultipleFilesAsync(IList<string> fileTypeChoicesFilter)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker() { ViewMode = PickerViewMode.List };
            foreach (String Filetypeextension in fileTypeChoicesFilter)
                fileOpenPicker.FileTypeFilter.Add(Filetypeextension);

            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            //show picker
            IReadOnlyList<IStorageFile> files = await fileOpenPicker.PickMultipleFilesAsync();
            if (files != null)
            {
                if (!FileService.HasGlobalWritePermission())
                {
                    if (files.Count < 20)
                    {
                        //process files (adding to future access list)
                        //call files.ToArray() otherwise we can encounter System.AccessViolationException (happens on windows mobile)
                        await AddStorageItemsToFutureAccessListAsync(files.ToArray());
                    }
                }
                return files;
            }
            return new List<IStorageFile>();
        }
        /// <summary>
        /// Opens the Folder Picker and adds it to the Future Access List
        /// </summary>
        /// <returns></returns>
        public async Task<IStorageFolder> PickFolderAsync()
        {
            FolderPicker folderPicker = new FolderPicker();
            folderPicker.FileTypeFilter.Add("*");
            StorageFolder folder = await folderPicker.PickSingleFolderAsync();

            if (folder != null)
            {
                if (!FileService.HasGlobalWritePermission())
                {
                    //add location to future access list
                    await AddStorageItemsToFutureAccessListAsync(new IStorageItem[] { folder });
                }
            }
            return folder;
        }
        /// <summary>
        /// Opens the Folder Picker adds the folder to the Future Access List
        /// </summary>
        /// <returns></returns>
        public async Task<DirectoryInfo> PickDirectoryAsync()
        {
            var folder = await PickFolderAsync();
            if (folder != null)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(folder.Path);
                return directoryInfo;
            }
            return default(DirectoryInfo);
        }
        public async Task<bool> DeleteFileAsync(string path)
        {
            StorageFile storageFile;
            try
            {
                storageFile = await StorageFile.GetFileFromPathAsync(path);
            }
            catch (UnauthorizedAccessException e)
            {
                //for other reasons we dont have acess to the file
                throw new Contract.Exceptions.UnauthorizedAccessException(e);
            }
            await storageFile.DeleteAsync();
            return true;
        }
        /// <summary>
        /// Determines depending on the <seealso cref="HasGlobalWriteAccess"/> Flag which is stored in <seealso cref="ApplicationData.Current.LocalSettings"/> if the app can write globaly
        /// </summary>
        /// <returns>True if the app can write globaly</returns>
        public static bool HasGlobalWritePermission()
        {
            var hasGlobalWriteAccess = ApplicationData.Current.LocalSettings.Values[FileService.HasGlobalWriteAccess];
            if (hasGlobalWriteAccess == null) return false;
            if (hasGlobalWriteAccess is bool b) return b;
            return false;
        }
        /// <summary>
        /// Checks if the app has global write access by creating a temp file in the user profile and removes it
        /// </summary>
        /// <returns>If the file could be created and the app has global write permission it retuns true</returns>
        public async Task<bool> HasGlobalWriteAccessAsync()
        {
            bool hasGlobalWriteAccess = false;
            try
            {
                //try to create a test file to check if we have "global" write access
                string path = Environment.GetEnvironmentVariable("USERPROFILE");
                //Environment.GetEnvironmentVariable("USERPROFILE") works only for > Windows 10 15063
                if (!String.IsNullOrEmpty(path))
                {

                    StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(path);
                    Guid guidtestfileName = Guid.NewGuid();
                    StorageFile storageFile = await storageFolder.CreateFileAsync(guidtestfileName.ToString(), CreationCollisionOption.GenerateUniqueName);
                    await storageFile.DeleteAsync();
                    hasGlobalWriteAccess = true;
                }
            }
            catch (System.UnauthorizedAccessException)
            {
                hasGlobalWriteAccess = false;
            }
            if (ApplicationData.Current.LocalSettings.Values.ContainsKey(FileService.HasGlobalWriteAccess))
            {
                ApplicationData.Current.LocalSettings.Values[FileService.HasGlobalWriteAccess] = hasGlobalWriteAccess;
            }
            else
            {
                ApplicationData.Current.LocalSettings.Values.Add(FileService.HasGlobalWriteAccess, hasGlobalWriteAccess);
            }
            return hasGlobalWriteAccess;
        }

        /// <summary>
        /// Pass result of <seealso cref="FileSavePicker.PickSaveFileAsync"/>, 
        /// <seealso cref="FileOpenPicker.PickMultipleFilesAsync"/> or 
        /// <seealso cref="FolderPicker.PickSingleFolderAsync"/> to this method to save future access to the picked files/folders
        /// </summary>
        /// <param name="storageItems">The picked items of the file/folder dialog</param>
        /// <returns>A task which indicates the process of executing this method</returns>
        public async Task AddStorageItemsToFutureAccessListAsync(IReadOnlyList<IStorageItem> storageItems)
        {
            foreach (IStorageItem storageFile in storageItems)
            {
                if (storageFile != null)
                {
                    if (String.IsNullOrEmpty(storageFile.Path)) return;
                    bool storeToken = true;
                    //ApplicationDataContainer cannot handle path seperators "/" - therefore create a key
                    string pathKey = GenerateBase64Key(storageFile);
                    //only check if the file is covered by an folder token
                    if (!(storageFile is IStorageFolder))
                    {
                        //check if we have the dictionary key then we dont need to add the file key
                        string directoryKey = GenerateBase64Key(Path.GetDirectoryName(storageFile.Path));
                        try
                        {
                            //check if the directory of the file is already stored in our token list
                            if (_localSettings.Values.ContainsKey(directoryKey))
                            {
                                //we dont need to store the token
                                storeToken = false;

                            }
                        }
                        catch (Exception e)
                        {
                            _loggerService.LogException(e);
                            storeToken = false;
                        }

                    }

                    if (storeToken)
                    {
                        Guid guidStorageFile = Guid.NewGuid();
                        //check if there is space left for futureaccess tokens
                        if (StorageApplicationPermissions.FutureAccessList.Entries.Count == StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed)
                        {
                            //maybe we can remove some old tokens
                            await CleanUpStorageItemTokensAsync(7);
                            //if nothing was removed we need to remove a random key
                            if (StorageApplicationPermissions.FutureAccessList.Entries.Count == StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed)
                            {
                                _loggerService.LogEvent("Too much FutureAccessList.Entries. CleanUpStorageItemTokensAsync did not remove any tokens.");
                                string guiddateTokenKey = string.Empty;
                                string guidToken = string.Empty;
                                //we got too many items - we have to remove a guidtoken from the list
                                foreach (string key in _localSettings.Values.Keys)
                                {
                                    object value = _localSettings.Values[key];
                                    guidToken = ExtractGuidFromKeyToken(value.ToString());
                                    if (!String.IsNullOrEmpty(guidToken))
                                    {
                                        guiddateTokenKey = key;
                                        break;
                                    }
                                }
                                if (!String.IsNullOrEmpty(guiddateTokenKey))
                                {
                                    _localSettings.Values.Remove(guiddateTokenKey);
                                    StorageApplicationPermissions.FutureAccessList.Remove(guidToken);
                                    _loggerService.LogEvent(nameof(guidStorageFile), nameof(_localSettings.Values.Remove));
                                }
                            }
                            if (StorageApplicationPermissions.FutureAccessList.Entries.Count == StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed)
                            {
                                _loggerService.LogEvent("Too much FutureAccessList.Entries. Settings file seems to be corrupt. Removing all FA tokens");
                                //remove all tokens from FA
                                foreach (AccessListEntry accessListEntry in StorageApplicationPermissions.FutureAccessList.Entries.ToArray())
                                {
                                    StorageApplicationPermissions.FutureAccessList.Remove(accessListEntry.Token);
                                }
                            }
                        }
                        // Application now has read/write access to all contents in the picked folder (including other sub-folder contents)
                        StorageApplicationPermissions.FutureAccessList.AddOrReplace(guidStorageFile.ToString(), storageFile);
                        // Add to FA without metadata
                        string mruToken = StorageApplicationPermissions.MostRecentlyUsedList.Add(storageFile, guidStorageFile.ToString());


                        //encode guid and date into string (date to save access of file)
                        string keyvalue = $"{guidStorageFile:D}{DateTime.Now:yyMMdd}";
                        try
                        {
                            if (!_localSettings.Values.ContainsKey(pathKey))
                            {
                                _localSettings.Values.Add(pathKey, keyvalue);
                                _loggerService.LogEvent(nameof(guidStorageFile), nameof(_localSettings.Values.Add));
                            }
                            else
                            {
                                _localSettings.Values[pathKey] = keyvalue;
                                _loggerService.LogEvent(nameof(guidStorageFile), nameof(StorageApplicationPermissions.FutureAccessList.AddOrReplace));
                            }
                            _loggerService.LogEvent(nameof(storageFile), new Dictionary<string, string>()
                            {
                                { nameof(storageFile),storageFile?.Path ?? ""},
                                { nameof(StorageApplicationPermissions.FutureAccessList.Entries.Count), StorageApplicationPermissions.FutureAccessList.Entries.Count.ToString() },
                                { nameof(StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed), StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed.ToString() }
                            });
                        }
                        catch (Exception e)
                        {
                            HandleGenericException(e);
                        }
                    }

                }

            }
        }
        /// <summary>
        /// encode path to base 64
        /// </summary>
        /// <param name="storageItem"></param>
        /// <returns></returns>
        private static string GenerateBase64Key(IStorageItem storageItem)
        {
            return GenerateBase64Key(storageItem.Path);
        }
        /// <summary>
        /// encode path to base 64
        /// </summary>
        /// <param name="storageItem"></param>
        /// <returns></returns>
        private static string GenerateBase64Key(string path)
        {
            path = path.ToLowerInvariant();
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(path);
            return Convert.ToBase64String(plainTextBytes);
        }
        private static string DecodeBase64Key(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
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
            string pathKey = string.Empty;
            try
            {
                Uri uri = null;
                try
                {
                    uri = new Uri(path);
                }
                catch (System.UriFormatException)
                {
                    return null;
                }

                //network path unsupported right now
                if (uri.IsUnc) return null;
                //detect whether its a directory or file
                if ((attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                {
                    pathKey = GenerateBase64Key(path);
                    if (_localSettings.Values.ContainsKey(pathKey))
                    {
                        string guidTokenDate = _localSettings.Values[pathKey].ToString();
                        string guidToken = ExtractGuidFromKeyToken(guidTokenDate);
                        return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(guidToken);
                    }
                }
                else
                {
                    pathKey = GenerateBase64Key(path);
                    if (_localSettings.Values.ContainsKey(pathKey))
                    {
                        string guidTokenDate = _localSettings.Values[pathKey].ToString();
                        string guidToken = ExtractGuidFromKeyToken(guidTokenDate);
                        return await StorageApplicationPermissions.FutureAccessList.GetFileAsync(guidToken);
                    }
                    else
                    {
                        //we dont have the file index maybe we got the directory of it...
                        pathKey = GenerateBase64Key(Path.GetDirectoryName(path));
                        if (_localSettings.Values.ContainsKey(pathKey))
                        {
                            string guidTokenDate = _localSettings.Values[pathKey].ToString();
                            string guidToken = ExtractGuidFromKeyToken(guidTokenDate);
                            StorageFolder storageFolder = await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(guidToken);

                            StorageFile storageFile = await storageFolder.GetFileAsync(new FileInfo(path).Name);
                            return storageFile;
                        }
                    }
                }
                //no guidtoken available for the overgiven path
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                //this happens when calling FutureAccessList.GetFileAsync with a empty guid
                //our token key is outdated so we remove it
                if (!string.IsNullOrEmpty(pathKey))
                {
                    _localSettings.Values.Remove(pathKey);
                }
            }
            catch (ArgumentException)
            {
                //this happens when calling FutureAccessList.GetFileAsync with a wrong guid
                //our token key is outdated so we remove it
                if (!string.IsNullOrEmpty(pathKey))
                {
                    _localSettings.Values.Remove(pathKey);
                }
            }
            catch (Exception e)
            {
                HandleGenericException(e);
            }
            return null;
        }

        private void HandleGenericException(Exception e)
        {
            if (e.HResult == -2147024735)
            {
                //The specified path is invalid.
                //Error trying to query the application data container item info
                //this happens for network paths
            }
            else
            {
                _loggerService.LogException(e);
            }
        }

        /// <summary>
        /// Extracts the guid from the encoded "guid date" string
        /// </summary>
        /// <remarks>The string is expected in the format "{guidStorageFile:D}{DateTime.Now:yyMMdd}"</remarks>
        /// <param name="guiddateToken">The string from where the guid should be extracted</param>
        /// <returns>Returns the guid as string. If the string is empty no guid is available from the overgiven parameter value</returns>
        private string ExtractGuidFromKeyToken(string guiddateToken)
        {
            if (guiddateToken.Length >= 36)
            {
                string guidstring = guiddateToken.Substring(0, 36);
                Guid guid;
                //check if its really a guid maybe the string was just 36 charachters long
                if (Guid.TryParse(guidstring, out guid))
                {
                    return guidstring;
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// Extracts the date from the encoded "guid date" string
        /// </summary>
        /// <param name="guiddateToken"></param>
        /// <returns></returns>
        private DateTime ExtractDateFromKeyToken(string guiddateToken)
        {
            if (guiddateToken.Length >= 42)
            {
                string date = guiddateToken.Substring(36, 6);
                CultureInfo provider = CultureInfo.InvariantCulture;
                DateTime dateTime;
                if (DateTime.TryParseExact(date, "yyMMdd", provider, DateTimeStyles.None, out dateTime))
                {
                    return dateTime;
                }
            }
            return DateTime.MaxValue;
        }
        /// <summary>
        /// Cleans up old tokens which are older than the value supplied by <paramref name="removeOlderThanDays"/>
        /// </summary>
        /// <param name="removeOlderThanDays">the amount of days on which the token was not updated</param>
        /// <returns>Task which indicates the process of computing the method</returns>
        public Task CleanUpStorageItemTokensAsync(int removeOlderThanDays = 14)
        {
            try
            {
                //collect data which should be removed
                List<string> keysToRemove = new List<string>();
                foreach (var keyValuePair in _localSettings.Values)
                {
                    DateTime accessDateTime = ExtractDateFromKeyToken(keyValuePair.Value.ToString());
                    if ((DateTime.Now - accessDateTime).TotalDays >= removeOlderThanDays)
                    {
                        string guidToken = ExtractGuidFromKeyToken(keyValuePair.Value.ToString());
                        if (!String.IsNullOrEmpty(guidToken))
                        {
                            //remove from windows future access list
                            StorageApplicationPermissions.FutureAccessList.Remove(guidToken);
                            keysToRemove.Add(keyValuePair.Key);
                        }

                    }
                }
                //remove guid token from our settings
                foreach (string keys in keysToRemove)
                {
                    _localSettings.Values.Remove(keys);
                }
            }
            catch (Exception e)
            {
                _loggerService?.LogException(e);
            }
            return Task.CompletedTask;
        }

        public static async Task CleanUpFolderAsync(StorageFolder storageFolder, IList<string> filesToRemoveWithFileExtension, int removeOlderThanDays = 7)
        {
            //remove old cached images
            IReadOnlyList<StorageFile> storageFiles = await storageFolder.GetFilesAsync();
            foreach (StorageFile file in storageFiles)
            {
                if (filesToRemoveWithFileExtension.Any(a => a.Contains(file.FileType.ToLowerInvariant())))
                {
                    DateTime createdDateTime = file.DateCreated.DateTime;
                    if ((DateTime.Now - createdDateTime).TotalDays >= removeOlderThanDays)
                    {
                        await file.DeleteAsync();
                    }
                }
            }
        }
    }
}
