using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using Get.the.solution.Image.Manipulation.ServiceBase;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class ImageFileService : ImageFileBaseService, IImageFileService
    {
        protected readonly IResourceService _resourceService;
        public ImageFileService(ILoggerService loggerService, IResourceService resourceService) : base(loggerService)
        {
            _resourceService = resourceService;
        }
        public string FutureAccessToken { get; private set; }

        public async Task<IReadOnlyList<ImageFile>> PickMultipleFilesAsync()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker() { ViewMode = PickerViewMode.List };
            foreach (String Filetypeextension in FileTypeFilter)
                fileOpenPicker.FileTypeFilter.Add(Filetypeextension);

            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            //show picker
            IReadOnlyList<IStorageFile> files = await fileOpenPicker.PickMultipleFilesAsync();
            //create imageFiles
            List<ImageFile> imageFiles = new List<ImageFile>();
            foreach (IStorageFile storage in files)
            {
                try
                {
                    imageFiles.Add(await FileToImageFile(storage, false));
                    StorageApplicationPermissions.MostRecentlyUsedList.Add(storage);
                    StorageFolder storageFolder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(storage.Path));
                    //todo save this tokens!!!
                    FutureAccessToken = StorageApplicationPermissions.FutureAccessList.Add(storageFolder);
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
        public virtual async Task<ImageFile> PickSaveFileAsync(String preferredSaveLocation, String SuggestedFileName)
        {
            FileSavePicker FileSavePicker = new FileSavePicker();
            FileInfo fileInfo = new FileInfo(SuggestedFileName);

            FileSavePicker.DefaultFileExtension = fileInfo.Extension;
            FileSavePicker.FileTypeChoices.Add(fileInfo.Extension, FileTypeFilter);
            //if (Path.GetDirectoryName(preferredSaveLocation).Contains(System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)))
            //{
            //    FileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            //}
            //else
            //{
                FileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            //}
            // Default file name if the user does not type one in or select a file to replace
            FileSavePicker.SuggestedFileName = SuggestedFileName;
            IStorageFile File = await FileSavePicker.PickSaveFileAsync();
            //file is null when the user clicked on "abort" on save as dialog
            if (File != null)
            {
                return await FileToImageFileConverter(File);
            }
            return null;
        }
        public Task<ImageFile> FileToImageFileConverter(object storageFile)
        {
            return FileToImageFileConverter(storageFile as IStorageFile);
        }
        public static async Task<ImageFile> FileToImageFile(IStorageFile storageFile, bool readStream = true)
        {
            Stream ImageStream = null;
            if (readStream)
            {
                IRandomAccessStreamWithContentType RandomAccessStream = await storageFile?.OpenReadAsync();
                ImageStream = RandomAccessStream?.AsStreamForWrite();
            }
            ImageProperties imageProp;
            int width = 0;
            int height = 0;
            if (storageFile is StorageFile sFile)
            {
                if (sFile.Properties != null)
                {
                    imageProp = await sFile.Properties.GetImagePropertiesAsync();
                    width = (int)imageProp.Width;
                    height = (int)imageProp.Height;
                }
            }
            FileInfo fileInfo = new FileInfo(storageFile.Path);
            ImageFile imageFile = new ImageFile(storageFile.Path, ImageStream, width, height, fileInfo);
            imageFile.Tag = storageFile;
            imageFile.IsReadOnly = (Windows.Storage.FileAttributes.ReadOnly & storageFile.Attributes) == Windows.Storage.FileAttributes.ReadOnly;
            return imageFile;
        }
        public async Task<ImageFile> FileToImageFileConverter(IStorageFile storageFile)
        {
            return await FileToImageFile(storageFile);
        }
        public async Task WriteBytesAsync(IStorageFile file, byte[] buffer)
        {
            await FileIO.WriteBytesAsync(file, buffer);
            StorageApplicationPermissions.MostRecentlyUsedList.Add(file);
        }

        public async Task WriteBytesAsync(ImageFile file, byte[] buffer)
        {
            try
            {
                if (file.IsReadOnly) throw new Contract.Exceptions.UnauthorizedAccessException();
                await WriteBytesAsync(file.Tag as IStorageFile, buffer);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new Contract.Exceptions.UnauthorizedAccessException(e);
            }
        }

        public async Task<ImageFile> WriteBytesAsync(string folderPath, string suggestedFileName, ImageFile file, byte[] buffer)
        {
            try
            {
                //try to save the ImageFile into the targetStorageFolder
                var targetStorageFolder = await StorageFolder.GetFolderFromPathAsync(folderPath);
                //this operation can throw a UnauthorizedAccessException
                StorageFile storageFile = await targetStorageFolder.CreateFileAsync(suggestedFileName, CreationCollisionOption.GenerateUniqueName);
                await WriteBytesAsync(storageFile, buffer);
                return await FileToImageFileConverter(storageFile);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new Contract.Exceptions.UnauthorizedAccessException(e);
            }
        }

        public async Task<IList<ImageFile>> GetFilesFromFolderAsync(string folderPath)
        {
            folderPath = Path.GetDirectoryName(folderPath);
            var targetStorageFolder = await StorageFolder.GetFolderFromPathAsync(folderPath);
            IReadOnlyList<IStorageFile> storageFiles = await targetStorageFolder.GetFilesAsync();
            List<ImageFile> imageFiles = new List<ImageFile>();
            foreach (var item in storageFiles)
            {
                if (FileTypeFilter.Contains(item.FileType.ToLower()))
                {
                    imageFiles.Add(await FileToImageFile(item, false));
                }
                else
                {

                }
            }

            return imageFiles;
        }

        public async Task<ImageFile> LoadImageFileAsync(string filepath)
        {
            IStorageFile storageFile = await StorageFile.GetFileFromPathAsync(filepath);
            return await FileToImageFile(storageFile);
        }
        public override string GenerateSuccess(ImageFile imageFile)
        {
            string successMessage = _resourceService.GetString("SavedTo");
            if (!String.IsNullOrEmpty(successMessage))
            {
                successMessage = String.Format(successMessage, imageFile.Path);
            }
            return successMessage;
        }
    }
}
