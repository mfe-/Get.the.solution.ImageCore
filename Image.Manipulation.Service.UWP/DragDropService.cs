using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class DragDropService : IDragDropService
    {
        private readonly IImageFileService _imageFileService;
        private readonly FileService _fileService;
        public DragDropService(IImageFileService imageFileService)
        {
            _imageFileService = imageFileService;
        }
        public DragDropService(IImageFileService imageFileService, ILoggerService loggerService, ILocalSettings localSettings)
            : this(imageFileService)
        {
            _fileService = new FileService(loggerService);
        }
        public bool IsDragAndDropEnabled => true;

        public void OnDragOverCommand(object param)
        {
            if(param is DragEventArgs e)
            {
                e.DragUIOverride.IsContentVisible = true;
                e.DragUIOverride.IsGlyphVisible = true;
                IReadOnlyList<IStorageItem> storeageItems = e.DataView.GetStorageItemsAsync().GetAwaiter().GetResult();
                //Flag determine wether draged files are allowed
                bool CanDrop = true;
                foreach (IStorageItem storageItem in storeageItems)
                {
                    if (storageItem is IStorageFile)
                    {
                        if (storageItem is StorageFile storageFile)
                        {
                            CanDrop = _imageFileService.FileTypeFilter.Contains((storageFile).FileType.ToLowerInvariant());
                        }
                    }
                    if (storageItem is IStorageFolder)
                    {
                        CanDrop = false;
                    }
                }
                if (!CanDrop)
                {
                    e.AcceptedOperation = DataPackageOperation.None;
                }
                e.Handled = true;
            }
        }

        public async Task OnDropCommandAsync(object param, ObservableCollection<ImageFile> ImageFiles)
        {
            if (param is IReadOnlyList<IStorageItem> readOnlyListStorageItems)
            {
                if (ImageFiles == null)
                {
                    ImageFiles = new ObservableCollection<ImageFile>();
                }
                foreach (IStorageItem storageItem in readOnlyListStorageItems)
                {
                    if (storageItem is IStorageFile)
                    {
                        if (storageItem is StorageFile storageFile)
                        {
                            StorageFile imageStorageFile = storageFile;
                            //check if its readonly
                            if (_fileService != null && storageFile.Attributes.HasFlag(FileAttributes.ReadOnly))
                            {
                                //check if we can retriev the dragged file from our future access list 
                                IStorageItem storageItem1 = await _fileService.TryGetWriteAbleStorageItemAsync(storageItem);
                                //if a value was returned use it
                                if(storageItem1 is StorageFile storageFile1)
                                {
                                    imageStorageFile = storageFile1;
                                }
                            }
                            ImageFiles.Add(await _imageFileService.FileToImageFileConverterAsync(imageStorageFile));
                        }
                    }
                    //if (item is IStorageFolder && item is StorageFolder)
                    //{
                    //    OpenFolder(item as StorageFolder);
                    //}
                }
            }
        }
    }
}
