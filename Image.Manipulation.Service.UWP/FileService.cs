using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Collections.Generic;
using System.Globalization;
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
        public async Task OnPickStorageItemsAsync(IReadOnlyList<IStorageItem> storageItems)
        {
            foreach (IStorageItem storageFile in storageItems)
            {
                if (storageFile != null)
                {
                    bool storeToken = true;

                    string pathKey = storageFile.Path.ToLowerInvariant();
                    //only check if the file is covered by an folder token
                    if (!(storageFile is IStorageFolder))
                    {
                        //check if we have the dictionary key then we dont need to add the file key
                        string directoryKey = Path.GetDirectoryName(storageFile.Path).ToLowerInvariant();

                        //check if the directory of the file is already stored in our token list
                        if (_localSettings.Values.ContainsKey(directoryKey))
                        {
                            //we dont need to store the token
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
                            { nameof(storageFile),pathKey},
                            { nameof(guidStorageFile),guidStorageFile.ToString() },
                            { nameof(mruToken), mruToken},
                            { nameof(StorageApplicationPermissions.FutureAccessList.Entries.Count), StorageApplicationPermissions.FutureAccessList.Entries.Count.ToString() },
                            { nameof(StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed), StorageApplicationPermissions.FutureAccessList.MaximumItemsAllowed.ToString() }
                        });
                    }

                }

            }
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
            try
            {
                string pathKey;
                //detect whether its a directory or file
                if ((attr & System.IO.FileAttributes.Directory) == System.IO.FileAttributes.Directory)
                {
                    pathKey = path.ToLowerInvariant();
                    if (_localSettings.Values.ContainsKey(pathKey))
                    {
                        string guidTokenDate = _localSettings.Values[pathKey].ToString();
                        string guidToken = ExtractGuidFromKeyToken(guidTokenDate);
                        return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(guidToken);
                    }
                }
                else
                {
                    pathKey = path.ToLowerInvariant();
                    if (_localSettings.Values.ContainsKey(pathKey))
                    {
                        string guidTokenDate = _localSettings.Values[pathKey].ToString();
                        string guidToken = ExtractGuidFromKeyToken(guidTokenDate);
                        return await StorageApplicationPermissions.FutureAccessList.GetFileAsync(guidToken);
                    }
                    else
                    {
                        //we dont have the file index maybe we got the directory of it...
                        pathKey = Path.GetDirectoryName(path).ToLowerInvariant();
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
            }
            catch (ArgumentException)
            {
                //this happens when calling FutureAccessList.GetFileAsync with a wrong guid
            }
            return null;
        }
        /// <summary>
        /// Extracts the guid from the encoded guid date string
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
