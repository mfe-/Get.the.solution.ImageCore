using Prism.Commands;
using Prism.Mvvm;
using Prism.Windows.AppModel;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace Get.the.solution.Image.Manipulation.Shell
{
    public class ResizePageViewModel : BindableBase
    {
        protected readonly INavigationService _NavigationService;
        protected readonly IResourceLoader _ResourceLoader;

        public ResizePageViewModel(INavigationService navigationService, IResourceLoader resourceLoader)
        {
            _ResourceLoader = resourceLoader;
            _NavigationService = navigationService;
            ImageFiles = new List<IStorageFile>();
            OpenFilePickerCommand = new DelegateCommand(OnOpenFilePickerCommand);
            OkCommand = new DelegateCommand(OnOkCommand);
            CancelCommand = new DelegateCommand(OnCancelCommand);
        }

        #region FilePickerCommand
        public DelegateCommand OpenFilePickerCommand { get; set; }

        protected async void OnOpenFilePickerCommand()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker() { ViewMode = PickerViewMode.List };
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            IReadOnlyList<IStorageFile> files = await fileOpenPicker.PickMultipleFilesAsync();
            ImageFiles = files.ToList();
        }
        #endregion

        #region RadioOptions
        private bool _SizeSmallChecked;

        public bool SizeSmallChecked
        {
            get { return _SizeSmallChecked; }
            set
            {
                SetProperty(ref _SizeSmallChecked, value, nameof(SizeSmallChecked));
            }
        }

        private bool _SizeMediumChecked;

        public bool SizeMediumChecked
        {
            get { return _SizeMediumChecked; }
            set
            {
                SetProperty(ref _SizeMediumChecked, value, nameof(SizeMediumChecked));
            }
        }

        private bool _SizeCustomChecked;

        public bool SizeCustomChecked
        {
            get { return _SizeCustomChecked; }
            set { SetProperty(ref _SizeCustomChecked, value, nameof(SizeCustomChecked)); }
        }
        #endregion

        #region Images
        private List<IStorageFile> _ImageFiles;

        public List<IStorageFile> ImageFiles
        {
            get { return _ImageFiles; }
            set { SetProperty(ref _ImageFiles, value, nameof(ImageFiles)); }
        }
        #endregion

        public DelegateCommand OkCommand { get; set; }

        protected async void OnOkCommand()
        {
            if (SizeSmallChecked)
            {
                Width = 640; Height = 480;
            }
            if (SizeMediumChecked)
            {
                Width = 800; Height = 600;
            }
            foreach (IStorageFile storage in ImageFiles)
            {
                var randomAccessStream = await storage.OpenReadAsync();
                Stream stream = randomAccessStream.AsStreamForRead();
                using (MemoryStream filestream = ImageService.Resize(stream, Width, Height))
                {
                    if (OverwriteFiles)
                    {
                        
                    }
                    else
                    {
                        if (ImageFiles.Count == 1)
                        {
                            FileSavePicker FileSavePicker = new FileSavePicker();
                            FileSavePicker.DefaultFileExtension = storage.FileType;
                            FileSavePicker.FileTypeChoices.Add(storage.FileType, new List<string>() { storage.FileType });

                            // Default file name if the user does not type one in or select a file to replace
                            FileSavePicker.SuggestedFileName = $"{storage.Name.Replace(storage.FileType,String.Empty)}-{Width}x{Height}{storage.FileType}";
                            StorageFile file = await FileSavePicker.PickSaveFileAsync();
                            if (null != file)
                            {
                                await FileIO.WriteBytesAsync(file, filestream.ToArray());
                            }
                        }
                    }
                }

            }

        }


        private int _Width;

        public int Width
        {
            get { return _Width; }
            set { SetProperty(ref _Width, value, nameof(Width)); }
        }

        private int _Height;

        public int Height
        {
            get { return _Height; }
            set { SetProperty(ref _Height, value, nameof(Height)); }
        }

        public DelegateCommand CancelCommand { get; set; }

        protected void OnCancelCommand()
        {
            CoreApplication.Exit();
        }


        private bool _OverwriteFiles;

        public bool OverwriteFiles
        {
            get { return _OverwriteFiles; }
            set { SetProperty(ref _OverwriteFiles, value, nameof(OverwriteFiles)); }
        }

    }
}
