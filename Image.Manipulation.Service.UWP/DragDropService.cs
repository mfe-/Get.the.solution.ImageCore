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
        protected IImageFileService _imageFileService;
        public DragDropService(IImageFileService imageFileService)
        {
            _imageFileService = imageFileService;
        }
        public bool IsDragAndDropEnabled => true;

        public void OnDragOverCommand(object param)
        {
            DragEventArgs e = param as DragEventArgs;

            e.DragUIOverride.IsContentVisible = true;
            e.DragUIOverride.IsGlyphVisible = true;
            IReadOnlyList<IStorageItem> Items = e.DataView.GetStorageItemsAsync().GetAwaiter().GetResult();
            //Flag determine wether draged files are allowed
            bool CanDrop = true;
            foreach (var item in Items)
            {
                if (item is IStorageFile)
                {
                    if (item is StorageFile)
                    {
                        CanDrop = _imageFileService.FileTypeFilter.Contains((item as StorageFile).FileType);
                    }
                }
                if (item is IStorageFolder && item is StorageFolder)
                {
                    CanDrop = false;
                }
            }

            if (CanDrop == false)
            {
                e.AcceptedOperation = DataPackageOperation.None;
            }
        }

        public async Task OnDropCommandAsync(object param, ObservableCollection<ImageFile> ImageFiles)
        {
            if (param != null && param as IReadOnlyList<IStorageItem> != null)
            {
                if (ImageFiles == null)
                {
                    ImageFiles = new ObservableCollection<ImageFile>();
                }
                foreach (var item in param as IReadOnlyList<IStorageItem>)
                {
                    if (item is IStorageFile)
                    {
                        if (item is StorageFile)
                        {
                            ImageFiles.Add(await _imageFileService.FileToImageFileConverter(item as StorageFile));
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
