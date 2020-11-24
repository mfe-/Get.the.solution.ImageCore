using Get.the.solution.Image.Manipulation.Contract;
using Get.the.solution.Image.Manipulation.ServiceBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.System.UserProfile;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class ImageFileService : ImageFileBaseService, IImageFileService
    {
        private readonly FileService _fileService;

        public ImageFileService(ILoggerService loggerService, FileService fileService)
            : base(loggerService)
        {
            _fileService = fileService;
        }
        ///<inheritdoc/>
        public async Task<IReadOnlyList<ImageFile>> PickMultipleFilesAsync(bool readStream = false)
        {
            var files = await _fileService.PickMultipleFilesAsync(FileTypeFilter);
            if (files != null)
            {
                //create imageFiles
                List<ImageFile> imageFiles = new List<ImageFile>();
                foreach (IStorageFile storageFile in files)
                {
                    try
                    {
                        imageFiles.Add(await FileToImageFileAsync(storageFile, readStream));
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        throw new Contract.Exceptions.UnauthorizedAccessException(e);
                    }
                    catch (Exception e)
                    {
                        _loggerService.LogException(nameof(PickMultipleFilesAsync), e);
                    }
                }

                return imageFiles;
            }
            return new List<ImageFile>();
        }
        ///<inheritdoc/>
        public virtual async Task<ImageFile> PickSaveFileAsync(String preferredSaveLocation, String suggestedFileName)
        {
            IStorageFile file = await _fileService.PickSaveFileAsync(preferredSaveLocation, suggestedFileName, FileTypeFilter);
            if (file != null)
            {
                return await FileToImageFileConverterAsync(file);
            }
            return null;
        }
        ///<inheritdoc/>
        public async Task<ImageFile> PickSaveFolderAsync(String preferredSaveLocation, String suggestedFileName)
        {
            var folder = await _fileService.PickFolderAsync();
            if (folder != null)
            {
                //create desired file
                IStorageFile file = await folder.CreateFileAsync(suggestedFileName, CreationCollisionOption.OpenIfExists);
                return await FileToImageFileConverterAsync(file);
            }
            return null;
        }

        public static async Task<ImageFile> FileToImageFileAsync(IStorageFile storageFile, bool readStream = true)
        {
            Stream imageStream = null;
            if (readStream)
            {
                IRandomAccessStreamWithContentType RandomAccessStream = await storageFile?.OpenReadAsync();
                imageStream = RandomAccessStream?.AsStreamForWrite();
            }
            ImageProperties imageProp;
            int width = 0;
            int height = 0;
            string filepath = String.Empty;
            if (storageFile is StorageFile sFile)
            {
                if (sFile.Properties != null)
                {
                    imageProp = await sFile.Properties.GetImagePropertiesAsync();
                    width = (int)imageProp.Width;
                    height = (int)imageProp.Height;
                }
                //path can be empty if from the file picker dialog 
                //a image path from the internet was enterted
                if (String.IsNullOrEmpty(storageFile.Path))
                {
                    filepath = sFile.FolderRelativeId;
                }
                else
                {
                    filepath = storageFile.Path;
                }
            }

            if (String.IsNullOrEmpty(filepath))
            {
                filepath = storageFile.Path;
            }

            FileInfo fileInfo = String.IsNullOrEmpty(filepath) ? null : new FileInfo(filepath);
            ImageFile imageFile = new ImageFile(filepath, imageStream, width, height, fileInfo);
            imageFile.Tag = storageFile;
            imageFile.IsReadOnly = (Windows.Storage.FileAttributes.ReadOnly & storageFile.Attributes) == Windows.Storage.FileAttributes.ReadOnly;
            return imageFile;
        }
        ///<inheritdoc/>
        public Task<ImageFile> FileToImageFileConverterAsync(object storageFile, bool readStream = true)
        {
            return FileToImageFileConverterAsync(storageFile as IStorageFile, readStream);
        }
        public Task<ImageFile> FileToImageFileConverterAsync(IStorageFile storageFile, bool readStream = true)
        {
            return FileToImageFileAsync(storageFile, readStream);
        }

        public async Task WriteBytesAsync(IStorageFile file, byte[] buffer)
        {
            await FileIO.WriteBytesAsync(file, buffer);
        }
        ///<inheritdoc/>
        public async Task WriteBytesAsync(ImageFile file, byte[] buffer)
        {
            try
            {
                IStorageFile storageFile = null;
                //the file is readonly
                if (file.IsReadOnly)
                {
                    IStorageItem storageItem = await _fileService.TryGetWriteAbleStorageItemAsync(file.Tag as IStorageFile);
                    if (storageItem is IStorageFile storageFile1)
                    {
                        storageFile = storageFile1;
                    }
                    else
                    {
                        //if we dont have the token we cant access the file
                        throw new Contract.Exceptions.UnauthorizedAccessException();
                    }
                }
                else
                {
                    storageFile = file.Tag as IStorageFile;
                }
                await WriteBytesAsync(storageFile, buffer);
            }
            catch (UnauthorizedAccessException e)
            {
                //for other reasons we dont have acess to the file
                throw new Contract.Exceptions.UnauthorizedAccessException(e);
            }
        }
        ///<inheritdoc/>
        public async Task<ImageFile> WriteBytesAsync(string folderPath, string suggestedFileName, byte[] buffer)
        {
            StorageFolder targetStorageFolder;
            try
            {
                //look up if we have this folder stored in our access list
                IStorageItem storageItem = await _fileService.TryGetWriteAbleStorageItemAsync(folderPath, System.IO.FileAttributes.Directory);
                if (storageItem is StorageFolder storageFolder)
                {
                    targetStorageFolder = storageFolder;
                }
                else
                {
                    //try to save the ImageFile into the targetStorageFolder - can throw a UnauthorizedAccessException
                    targetStorageFolder = await StorageFolder.GetFolderFromPathAsync(folderPath);
                }

                //this operation can throw a UnauthorizedAccessException
                StorageFile storageFile = await targetStorageFolder.CreateFileAsync(suggestedFileName, CreationCollisionOption.GenerateUniqueName);
                await WriteBytesAsync(storageFile, buffer);
                return await FileToImageFileConverterAsync(storageFile);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new Contract.Exceptions.UnauthorizedAccessException(e);
            }
        }
        ///<inheritdoc/>
        public async Task<IList<ImageFile>> GetFilesFromFolderAsync(string folderPath)
        {
            folderPath = Path.GetDirectoryName(folderPath);
            StorageFolder targetStorageFolder = null;
            try
            {
                targetStorageFolder = await StorageFolder.GetFolderFromPathAsync(folderPath);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new Contract.Exceptions.UnauthorizedAccessException(e);
            }

            var storageFiles = (await targetStorageFolder.GetFilesAsync());
            List<ImageFile> imageFiles = new List<ImageFile>();
            foreach (var item in storageFiles)
            {
                if (FileTypeFilter.Contains(item.FileType.ToLower()))
                {
                    imageFiles.Add(await FileToImageFileAsync(item, false));
                }
            }

            return imageFiles;
        }
        bool? hasGlobalWriteAccess = null;
        ///<inheritdoc/>
        public async Task<ImageFile> LoadImageFileAsync(string filepath)
        {
            if (hasGlobalWriteAccess == null)
            {
                hasGlobalWriteAccess = FileService.HasGlobalWritePermission();
            }

            IStorageFile storageFile = null;
            //look up only if we dont have global write access
            if (hasGlobalWriteAccess == null || !hasGlobalWriteAccess.Value)
            {
                //look up if we have this folder stored in our access list
                IStorageItem storageItem = await _fileService.TryGetWriteAbleStorageItemAsync(filepath, System.IO.FileAttributes.Normal);
                if (storageItem is StorageFile storageFileLookedUp)
                {
                    storageFile = storageFileLookedUp;
                }
            }
            else
            {
                storageFile = await StorageFile.GetFileFromPathAsync(filepath);
            }
            if (storageFile != null)
            {
                return await FileToImageFileAsync(storageFile);
            }
            return null;
        }
        ///<inheritdoc/>
        public async Task<bool> TrySetWallpaperImageAsync(string imageFilePath)
        {
            if (String.IsNullOrEmpty(imageFilePath)) throw new ArgumentNullException(nameof(imageFilePath));

            bool success = false;
            //remove old wallpapers
            await FileService.CleanUpFolderAsync(ApplicationData.Current.LocalFolder, FileTypeFilter, 0);

            if (UserProfilePersonalizationSettings.IsSupported())
            {
                StorageFile wallpaper;
                if (!imageFilePath.Contains(ApplicationData.Current.LocalFolder.Path))
                {
                    StorageFile originalStorageFile = await StorageFile.GetFileFromPathAsync(imageFilePath);

                    wallpaper = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                        new FileInfo(imageFilePath).Name, CreationCollisionOption.GenerateUniqueName);

                    await originalStorageFile.CopyAndReplaceAsync(wallpaper);
                }
                else
                {
                    wallpaper = await StorageFile.GetFileFromPathAsync(imageFilePath);
                }
                UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                success = await profileSettings.TrySetWallpaperImageAsync(wallpaper);
            }
            return success;
        }

    }
}
