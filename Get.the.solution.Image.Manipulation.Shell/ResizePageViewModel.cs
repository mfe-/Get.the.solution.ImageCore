using Get.the.solution.UWP.XAML;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Windows.AppModel;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace Get.the.solution.Image.Manipulation.Shell
{
    public class ResizePageViewModel : BindableBase
    {
        protected readonly INavigationService _NavigationService;
        protected readonly IResourceLoader _ResourceLoader;
        protected readonly ObservableCollection<IStorageFile> _SelectedFiles;
        protected IStorageFile _LastFile;
        protected IEnumerable<String> _AllowedFileTyes = new List<String>() { ".jpg", ".png", ".gif", ".bmp" };
        protected int RadioOptions;

        public ResizePageViewModel(ObservableCollection<IStorageFile> selectedFiles, INavigationService navigationService, IResourceLoader resourceLoader)
        {
            LocalSettings = ApplicationData.Current.LocalSettings;

            _SelectedFiles = selectedFiles;
            _ResourceLoader = resourceLoader;
            _NavigationService = navigationService;
            ImageFiles = new ObservableCollection<IStorageFile>();
            OpenFilePickerCommand = new DelegateCommand(OnOpenFilePickerCommand);
            OkCommand = new DelegateCommand(OnOkCommand);
            CancelCommand = new DelegateCommand(OnCancelCommand);
            OpenFileCommand = new DelegateCommand<IStorageFile>(OnOpenFileCommand);
            DragOverCommand = new DelegateCommand<object>(OnDragOverCommand);
            DropCommand = new DelegateCommand<object>(OnDropCommand);

            //SizeSmallChecked = true;

            if (_SelectedFiles != null)
            {
                ImageFiles = _SelectedFiles;
            }
            //get settings

            RadioOptions = LocalSettings.Values[nameof(RadioOptions)] == null ? 1 : Int32.Parse(LocalSettings.Values[nameof(RadioOptions)].ToString());

            if (RadioOptions == 1)
            {
                SizeSmallChecked = true;
            }
            else if (RadioOptions == 2)
            {
                SizeMediumChecked = true;
            }
            else if (RadioOptions == 3)
            {
                SizeCustomChecked = true;
            }

            OverwriteFiles = LocalSettings.Values[nameof(OverwriteFiles)] == null ? false : Boolean.Parse(LocalSettings.Values[nameof(OverwriteFiles)].ToString());
            Width = LocalSettings.Values[nameof(Width)] == null ? 1024 : Int32.Parse(LocalSettings.Values[nameof(Width)].ToString());
            Height = LocalSettings.Values[nameof(Height)] == null ? 768 : Int32.Parse(LocalSettings.Values[nameof(Height)].ToString());
        }
        /// <summary>
        /// Access for app settings
        /// </summary>
        private ApplicationDataContainer LocalSettings { get; set; }

        public bool ShowOpenFilePicker
        {
            get { return ImageFiles != null && ImageFiles.Count == 0; }
            set { OnPropertyChanged(nameof(ShowOpenFilePicker)); }
        }

        #region Selected File

        private StorageFile _SelectedFile;

        public StorageFile SelectedFile
        {
            get { return _SelectedFile; }
            set
            {
                SetProperty(ref _SelectedFile, value, nameof(SelectedFile));
            }
        }
        #endregion Selected File

        #region FilePickerCommand
        public DelegateCommand OpenFilePickerCommand { get; set; }

        protected async void OnOpenFilePickerCommand()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker() { ViewMode = PickerViewMode.List };
            foreach (String Filetypeextension in _AllowedFileTyes)
                fileOpenPicker.FileTypeFilter.Add(Filetypeextension);

            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            IReadOnlyList<IStorageFile> files = await fileOpenPicker.PickMultipleFilesAsync();
            ImageFiles = new ObservableCollection<IStorageFile>(files);
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
                if (SizeSmallChecked)
                {
                    Width = 640; Height = 480;
                }
                if(SizeSmallChecked==true)
                {
                    LocalSettings.Values[nameof(RadioOptions)] = 1;
                }

            }
        }

        private bool _SizeMediumChecked;

        public bool SizeMediumChecked
        {
            get { return _SizeMediumChecked; }
            set
            {
                SetProperty(ref _SizeMediumChecked, value, nameof(SizeMediumChecked));
                if (SizeMediumChecked)
                {
                    Width = 800; Height = 600;
                }
                if(SizeMediumChecked==true)
                {
                    LocalSettings.Values[nameof(RadioOptions)] = 2;
                }
            }
        }

        private bool _SizeCustomChecked;

        public bool SizeCustomChecked
        {
            get { return _SizeCustomChecked; }
            set
            {
                SetProperty(ref _SizeCustomChecked, value, nameof(SizeCustomChecked));
                if (SizeCustomChecked == true)
                {
                    LocalSettings.Values[nameof(RadioOptions)] = 3;
                }
            }
        }
        #endregion

        #region Images
        private ObservableCollection<IStorageFile> _ImageFiles;

        public ObservableCollection<IStorageFile> ImageFiles
        {
            get { return _ImageFiles; }
            set
            {
                SetProperty(ref _ImageFiles, value, nameof(ImageFiles));
                if (ImageFiles != null)
                {
                    ImageFiles.CollectionChanged += ImageFiles_CollectionChanged;
                }
                OnPropertyChanged(nameof(ShowOpenFilePicker));
            }
        }

        private void ImageFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(ShowOpenFilePicker));
            OnPropertyChanged(nameof(CanOverwriteFiles));
        }
        #endregion

        #region OkCommand / Resize Images
        public DelegateCommand OkCommand { get; set; }

        protected async void OnOkCommand()
        {
            //if no file is selected open file picker 
            if (ImageFiles == null || ImageFiles.Count == 0)
            {
                await OpenFilePickerCommand.Execute();
            }

            foreach (IStorageFile storage in ImageFiles)
            {
                try
                {
                    var randomAccessStream = await storage.OpenReadAsync();
                    Stream stream = randomAccessStream.AsStreamForRead();
                    using (MemoryStream filestream = ImageService.Resize(stream, Width, Height))
                    {
                        if (OverwriteFiles)
                        {
                            await FileIO.WriteBytesAsync(storage, filestream.ToArray());
                            _LastFile = storage;
                        }
                        else
                        {
                            //if (ImageFiles.Count == 1)
                            {
                                FileSavePicker FileSavePicker = new FileSavePicker();
                                FileSavePicker.DefaultFileExtension = storage.FileType;
                                FileSavePicker.FileTypeChoices.Add(storage.FileType, new List<string>() { storage.FileType });

                                // Default file name if the user does not type one in or select a file to replace
                                FileSavePicker.SuggestedFileName = $"{storage.Name.Replace(storage.FileType, String.Empty)}-{Width}x{Height}{storage.FileType}";
                                StorageFile file = await FileSavePicker.PickSaveFileAsync();
                                if (null != file)
                                {
                                    await FileIO.WriteBytesAsync(file, filestream.ToArray());
                                    _LastFile = file;
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    String message = string.Format(_ResourceLoader.GetString("ExceptionOnResize"), $"{e.Message} {e.InnerException}");

                    DataPackage Package = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
                    Package.SetText(message);
                    Clipboard.SetContent(Package);

                    MessageDialog Dialog = new MessageDialog(message);
                    await Dialog.ShowAsync();
                }

            }
            await CancelCommand.Execute();

        }
        #endregion

        #region Width & Height
        private int _Width;

        public int Width
        {
            get { return _Width; }
            set
            {
                SetProperty(ref _Width, value, nameof(Width));
                LocalSettings.Values[nameof(Width)] = _Width;
            }
        }

        private int _Height;

        public int Height
        {
            get { return _Height; }
            set
            {
                SetProperty(ref _Height, value, nameof(Height));
                LocalSettings.Values[nameof(Height)] = _Height;
            }
        }
        #endregion

        #region CancelCommand
        public DelegateCommand CancelCommand { get; set; }

        protected async void OnCancelCommand()
        {
            if ((ImageFiles == null || ImageFiles?.Count == 0) || (_SelectedFiles == null || _SelectedFiles?.Count() != 0))
            {
                CoreApplication.Exit();
            }
            else
            {
                if (_LastFile != null)
                {
                    //open the resized file on windows mobile
                    if (DeviceTypeHelper.GetDeviceFormFactorType() != DeviceFormFactorType.Desktop)
                    {
                        MessageDialog Dialog = new MessageDialog(_ResourceLoader.GetString("ShowLastFile"));
                        Dialog.Commands.Add(new UICommand(_ResourceLoader.GetString("Yes")) { Id = 0 });
                        Dialog.Commands.Add(new UICommand(_ResourceLoader.GetString("No")) { Id = 1 });
                        if ((int)(await Dialog.ShowAsync()).Id == 0)
                        {
                            await OpenFileCommand.Execute(_LastFile);
                        }
                    }
                }

                ImageFiles = new ObservableCollection<IStorageFile>();
                _LastFile = null;
            }
        }
        #endregion

        private bool _OverwriteFiles;
        /// <summary>
        /// This flag determines whether the existing file should be overwriten when resizing the image or a new file should be created
        /// </summary>
        public bool OverwriteFiles
        {
            get { return _OverwriteFiles; }
            set
            {
                SetProperty(ref _OverwriteFiles, value, nameof(OverwriteFiles));
                LocalSettings.Values[nameof(OverwriteFiles)] = _OverwriteFiles;
                OnPropertyChanged(nameof(CanOverwriteFiles));
            }
        }

        #region OpenFileCommand
        public DelegateCommand<IStorageFile> OpenFileCommand { get; set; }

        protected async void OnOpenFileCommand(IStorageFile file)
        {
            if (file != null)
            {
                // Launch the bug query file.
                bool sucess = await Windows.System.Launcher.LaunchFileAsync(file);
            }
        }
        protected bool CanOpenFileCommandExecuted(StorageFile file)
        {
            return true;
        }
        #endregion

        #region DragOver
        public DelegateCommand<object> DragOverCommand { get; set; }

        /// <summary>
        /// Determine whether the draged file is a supported image.
        /// </summary>
        /// <param name="param">Provides event informations.</param>
        protected void OnDragOverCommand(object param)
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
                        CanDrop = _AllowedFileTyes.Contains((item as StorageFile).FileType);
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
        #endregion

        public DelegateCommand<object> DropCommand { get; set; }
        /// <summary>
        /// Add Dropped files to our <see cref="ImageFiles"/> list.
        /// </summary>
        /// <param name="param"></param>
        protected void OnDropCommand(object param)
        {
            if (param != null && param as IReadOnlyList<IStorageItem> != null)
            {
                if (ImageFiles == null)
                {
                    ImageFiles = new ObservableCollection<IStorageFile>();
                }
                foreach (var item in param as IReadOnlyList<IStorageItem>)
                {
                    if (item is IStorageFile)
                    {
                        if (item is StorageFile)
                        {
                            ImageFiles.Add(item as StorageFile);
                        }
                    }
                    //if (item is IStorageFolder && item is StorageFolder)
                    //{
                    //    OpenFolder(item as StorageFolder);
                    //}
                }
            }
        }

        public bool CanOverwriteFiles
        {
            get
            {
                bool can= ImageFiles?.Count(a => ((a as StorageFile)?.Attributes & Windows.Storage.FileAttributes.ReadOnly) != 0) == 0;

                if(can==false)
                {
                    OverwriteFiles = false;
                }
                return can;

            }
        }

    }
}
