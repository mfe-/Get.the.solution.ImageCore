﻿using Get.the.solution.Image.Manipulation.Contract;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Get.the.solution.Image.Manipulation.ViewModel
{
    public class ResizePageViewModel : BindableBase
    {
        protected readonly IImageFileService _IImageFileService;
        protected readonly INavigation _NavigationService;
        protected readonly IResourceService _ResourceLoader;
        protected readonly ObservableCollection<ImageFile> _SelectedFiles;
        protected readonly IDialogService _IPageDialogService;
        protected readonly ILocalSettings _ILocalSettings;
        protected readonly bool _Sharing;
        protected ImageFile _LastFile;
        protected IEnumerable<String> _AllowedFileTyes = new List<String>() { ".jpg", ".png", ".gif", ".bmp" };
        protected int RadioOptions;
        //protected DataTransferManager _DataTransferManager;
        public ResizePageViewModel(IImageFileService filePicker, IResourceService resourceLoader, 
            ObservableCollection<ImageFile> selectedFiles, INavigation navigationService, 
            TimeSpan sharing, ILocalSettings localSettings)
        {
            _ILocalSettings = localSettings;
            _IImageFileService = filePicker;
            _SelectedFiles = selectedFiles;
            _ResourceLoader = resourceLoader;
            _NavigationService = navigationService;
            ImageFiles = new ObservableCollection<ImageFile>();
            OpenFilePickerCommand = new DelegateCommand(OnOpenFilePickerCommand);
            OkCommand = new DelegateCommand(OnOkCommand);
            CancelCommand = new DelegateCommand(OnCancelCommand);
            OpenFileCommand = new DelegateCommand<ImageFile>(OnOpenFileCommand);
            DragOverCommand = new DelegateCommand<object>(OnDragOverCommand);
            DropCommand = new DelegateCommand<object>(OnDropCommand);
            ShareCommand = new DelegateCommand(OnShareCommand);
            DeleteFileCommand = new DelegateCommand<ImageFile>(OnDeleteFile);

            if (TimeSpan.MaxValue.Equals(sharing))
            {
                _Sharing = true;
            }
            else
            {
                _Sharing = false;
            }

            if (_SelectedFiles != null)
            {
                ImageFiles = _SelectedFiles;
            }
            //get settings

            //RadioOptions = LocalSettings.Values[nameof(RadioOptions)] == null ? 1 : Int32.Parse(LocalSettings.Values[nameof(RadioOptions)].ToString());

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
            else if (RadioOptions == 4)
            {
                SizePercentChecked = true;
            }

            //OverwriteFiles = LocalSettings.Values[nameof(OverwriteFiles)] == null ? false : Boolean.Parse(LocalSettings.Values[nameof(OverwriteFiles)].ToString());
            //Width = LocalSettings.Values[nameof(Width)] == null ? 1024 : Int32.Parse(LocalSettings.Values[nameof(Width)].ToString());
            //Height = LocalSettings.Values[nameof(Height)] == null ? 768 : Int32.Parse(LocalSettings.Values[nameof(Height)].ToString());

            //WidthPercent = LocalSettings.Values[nameof(WidthPercent)] == null ? 100 : Int32.Parse(LocalSettings.Values[nameof(WidthPercent)].ToString());
            //HeightPercent = LocalSettings.Values[nameof(HeightPercent)] == null ? 100 : Int32.Parse(LocalSettings.Values[nameof(HeightPercent)].ToString());

            //KeepAspectRatio = LocalSettings.Values[nameof(KeepAspectRatio)] == null ? false : Boolean.Parse(LocalSettings.Values[nameof(KeepAspectRatio)].ToString());
            //PropertyChanged += ResizePageViewModel_PropertyChanged;
        }

        /// <summary>
        /// Access for app settings
        /// </summary>
        private ILocalSettings LocalSettings => _ILocalSettings;

        public bool ShowOpenFilePicker
        {
            get { return ImageFiles != null && ImageFiles.Count == 0; }
            set { RaisePropertyChanged(nameof(ShowOpenFilePicker)); }
        }

        #region Selected File
        private ImageFile _SelectedFile;

        public ImageFile SelectedFile
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
            await OpenFilePicker();
        }
        protected async Task OpenFilePicker()
        {
            try
            {
                Resizing = true;
                if(_IImageFileService.FileTypeFilter.Count==0)
                {
                    foreach (String Filetypeextension in _AllowedFileTyes)
                        _IImageFileService.FileTypeFilter.Add(Filetypeextension);
                }
                //fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

                IReadOnlyList<ImageFile> files = await _IImageFileService.PickMultipleFilesAsync();
                ImageFiles = new ObservableCollection<ImageFile>(files);
            }
            finally
            {
                Resizing = false;
            }

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
                if (SizeSmallChecked == true)
                {
                    //LocalSettings.Values[nameof(RadioOptions)] = 1;
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
                if (SizeMediumChecked == true)
                {
                    //LocalSettings.Values[nameof(RadioOptions)] = 2;
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
                    //LocalSettings.Values[nameof(RadioOptions)] = 3;
                }
            }
        }



        private bool _SizePercentChecked;

        public bool SizePercentChecked
        {
            get { return _SizePercentChecked; }
            set
            {
                SetProperty(ref _SizePercentChecked, value, nameof(SizePercentChecked));
                if (SizePercentChecked == true)
                {
                    //LocalSettings.Values[nameof(RadioOptions)] = 4;
                }
            }
        }

        #endregion

        #region Images
        private ObservableCollection<ImageFile> _ImageFiles;

        public ObservableCollection<ImageFile> ImageFiles
        {
            get { return _ImageFiles; }
            set
            {
                SetProperty(ref _ImageFiles, value, nameof(ImageFiles));
                if (ImageFiles != null)
                {
                    ImageFiles.CollectionChanged += ImageFiles_CollectionChanged;
                }
                RaisePropertyChanged(nameof(ShowOpenFilePicker));
                RaisePropertyChanged(nameof(SingleFile));
            }
        }
        

        private void ImageFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(ShowOpenFilePicker));
            RaisePropertyChanged(nameof(CanOverwriteFiles));
            RaisePropertyChanged(nameof(SingleFile));
        }
        #endregion

        public async Task<bool> ResizeImages(ImageAction action, Action<ImageFile, String> ProcessedImage = null)
        {
            Resizing = true;
            //if no file is selected open file picker 
            if (ImageFiles == null || ImageFiles.Count == 0)
            {
                await OpenFilePicker();
            }

            foreach (ImageFile ImageStoreage in ImageFiles)
            {
                try
                {
                    //var RandomAccessStream = await Storeage.OpenReadAsync();
                    //Stream ImageStream = RandomAccessStream.AsStreamForRead();

                    if (SizePercentChecked == true)
                    {
                        //ImageProperties Properties = await (ImageStoreage as StorageFile)?.Properties.GetImagePropertiesAsync();
                        //if (Properties != null)
                        {
                            Width = (int)ImageStoreage.Width * WidthPercent / 100;
                            Height = (int)ImageStoreage.Height * HeightPercent / 100;
                        }
                    }
                    using (MemoryStream ImageFileStream = ImageService.Resize(ImageStoreage.Stream, Width, Height))
                    {
                        String SuggestedFileName = GenerateResizedFileName(ImageStoreage);
                        if (action.Equals(ImageAction.Save))
                        {
                            try
                            {
                                _IImageFileService.WriteBytesAsync(ImageStoreage, ImageFileStream.ToArray());
                                //await FileIO.WriteBytesAsync(ImageStoreage, ImageFileStream.ToArray());
                                _LastFile = ImageStoreage;
                            }
                            catch (UnauthorizedAccessException e)
                            {

                            }


                        }
                        else if (action.Equals(ImageAction.SaveAs))
                        {
                            if (ImageFiles.Count == 1)
                            {
                                //FileSavePicker FileSavePicker = new FileSavePicker();
                                //FileSavePicker.DefaultFileExtension = ImageStoreage.FileType;
                                //FileSavePicker.FileTypeChoices.Add(ImageStoreage.FileType, new List<string>() { ImageStoreage.FileType });
                                ////FileSavePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                                //// Default file name if the user does not type one in or select a file to replace
                                //FileSavePicker.SuggestedFileName = SuggestedFileName;
                                //StorageFile File = await FileSavePicker.PickSaveFileAsync();
                                //if (null != File)
                                //{
                                //    await FileIO.WriteBytesAsync(File, ImageFileStream.ToArray());
                                //    _LastFile = File;
                                //}
                            }
                        }
                        else if (action.Equals(ImageAction.Process))
                        {
                            //StorageFolder TempFolder = ApplicationData.Current.LocalCacheFolder;
                            //StorageFile TempFile = await TempFolder.CreateFileAsync(SuggestedFileName, CreationCollisionOption.ReplaceExisting);

                            //await FileIO.WriteBytesAsync(TempFile, ImageFileStream.ToArray());
                            //_LastFile = TempFile;
                            //ProcessedImage?.Invoke(TempFile, $"{SuggestedFileName}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Resizing = false;
                    String message = string.Format(_ResourceLoader.GetString("ExceptionOnResize"), $"{e.Message} {e.InnerException}");

                    //DataPackage Package = new DataPackage() { RequestedOperation = DataPackageOperation.Copy };
                    //Package.SetText(message);
                    //Clipboard.SetContent(Package);

                    //MessageDialog Dialog = new MessageDialog(message);
                    //await Dialog.ShowAsync();
                    return false;
                }

            }
            Resizing = false;
            return true;
        }
        private bool _Resizing;

        public bool Resizing
        {
            get { return _Resizing; }
            protected set
            {
                SetProperty(ref _Resizing, value, nameof(Resizing));
            }
        }

        private string GenerateResizedFileName(ImageFile storeage)
        {
            if (storeage != null)
            {
                //return $"{storeage.Name.Replace(storeage.FileType, String.Empty)}-{Width}x{Height}{storeage.FileType}";
                return $"{storeage.FileInfo.Name.Replace(storeage.FileInfo.Extension, String.Empty)}-{Width}x{Height}{storeage.FileInfo.Extension}";
            }
            else
            {
                return String.Empty;
            }
        }

        #region OkCommand / Resize Images
        public DelegateCommand OkCommand { get; set; }

        protected async void OnOkCommand()
        {
            ImageAction Action = OverwriteFiles == true ? ImageAction.Save : ImageAction.SaveAs;
            bool Result = await ResizeImages(Action);
            CancelCommand.Execute();

        }
        #endregion

        #region Share

        private bool _SharingProcess;
        /// <summary>
        /// Indicates whether the app is in share process
        /// </summary>
        public bool SharingProcess
        {
            get { return _SharingProcess; }
            set { SetProperty(ref _SharingProcess, value, nameof(SharingProcess)); }
        }
        public DelegateCommand ShareCommand { get; set; }

        protected async void OnShareCommand()
        {
            //try
            //{
            //    LocalCachedResizedImages = new List<IStorageItem>();
            //    Action<IStorageFile, string> ProcessImage = new Action<IStorageFile, string>((ImageFileStream, FileName) =>
            //    {
            //        LocalCachedResizedImages.Add(ImageFileStream);
            //    });
            //    bool Result = await ResizeImages(ImageAction.Process, ProcessImage);
            //    DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            //    dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            //    DataTransferManager.ShowShareUI();

            //}
            //catch (Exception e)
            //{
            //    SharingProcess = false;
            //}
        }
        //protected List<IStorageItem> LocalCachedResizedImages = new List<IStorageItem>();

        //private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        //{
        //DataRequest Request = args.Request;
        //DataRequestDeferral Deferral = Request.GetDeferral();
        //try
        //{
        //    if (ImageFiles != null && ImageFiles.Count != 0)
        //    {
        //        //our DataPackage we want to share
        //        DataPackage Package = new DataPackage();
        //        Package.OperationCompleted += Package_OperationCompleted1;
        //        Package.Destroyed += Package_Destroyed;
        //        Package.RequestedOperation = DataPackageOperation.Copy;
        //        foreach (var File in ImageFiles)
        //        {
        //            Package.Properties.Description = $"{Package.Properties.Title } {GenerateResizedFileName(File)}";
        //        }
        //        Package.Properties.Title = _ResourceLoader.GetString("AppName");
        //        //Package.SetDataProvider(StandardDataFormats.StorageItems, Share_DataProvider);
        //        Package.Properties.ApplicationName = _ResourceLoader.GetString("AppName");
        //        foreach (String Extension in _AllowedFileTyes)
        //        {
        //            Package.Properties.FileTypes.Add(Extension);
        //        }
        //        Package.SetStorageItems(LocalCachedResizedImages);
        //        Request.Data = Package;
        //        SharingProcess = false;
        //    }
        //    else
        //    {
        //        args.Request.FailWithDisplayText("Nothing to share");
        //        SharingProcess = false;
        //    }
        //}
        //finally
        //{
        //    Deferral.Complete();
        //}

        //}

        //private void Package_Destroyed(DataPackage sender, object args)
        //{

        //}

        //private void Package_OperationCompleted1(DataPackage sender, OperationCompletedEventArgs args)
        //{
        //    CancelCommand.Execute();
        //}
        #endregion

        #region Width & Height
        private int _Width;

        public int Width
        {
            get { return _Width; }
            set
            {
                SetProperty(ref _Width, value, nameof(Width));
                //LocalSettings.Values[nameof(Width)] = _Width;
            }
        }


        private int _PercentWidth;

        public int WidthPercent
        {
            get { return _PercentWidth; }
            set
            {
                SetProperty(ref _PercentWidth, value, nameof(WidthPercent));
                //LocalSettings.Values[nameof(WidthPercent)] = _PercentWidth;
                if (KeepAspectRatio)
                {
                    _PercentHeight = _PercentWidth;
                    OnPropertyChanged(nameof(HeightPercent));
                }
            }
        }

        private int _Height;

        public int Height
        {
            get { return _Height; }
            set
            {
                SetProperty(ref _Height, value, nameof(Height));
                //LocalSettings.Values[nameof(Height)] = _Height;
            }
        }

        private int _PercentHeight;

        public int HeightPercent
        {
            get { return _PercentHeight; }
            set
            {
                SetProperty(ref _PercentHeight, value, nameof(HeightPercent));
                //LocalSettings.Values[nameof(HeightPercent)] = _PercentHeight;
                if (KeepAspectRatio)
                {
                    _PercentWidth = _PercentHeight;
                    OnPropertyChanged(nameof(WidthPercent));
                }
            }
        }
        #endregion

        #region CancelCommand
        public DelegateCommand CancelCommand { get; set; }

        protected async void OnCancelCommand()
        {
            if ((ImageFiles == null || ImageFiles?.Count == 0) || (_SelectedFiles == null || _SelectedFiles?.Count() != 0))
            {
                //if (Sharing == true)
                //{
                //    ShareOperation shareOperation = ServiceLocator.Current.GetInstance<ShareOperation>();
                //    if (shareOperation != null)
                //    {
                //        shareOperation.ReportCompleted();
                //    }
                //}
                //CoreApplication.Exit();
            }
            else
            {
                //if (_LastFile != null)
                //{
                //    //open the resized file on windows mobile
                //    if (DeviceTypeHelper.GetDeviceFormFactorType() != DeviceFormFactorType.Desktop)
                //    {
                //        MessageDialog Dialog = new MessageDialog(_ResourceLoader.GetString("ShowLastFile"));
                //        Dialog.Commands.Add(new UICommand(_ResourceLoader.GetString("Yes")) { Id = 0 });
                //        Dialog.Commands.Add(new UICommand(_ResourceLoader.GetString("No")) { Id = 1 });
                //        if ((int)(await Dialog.ShowAsync()).Id == 0)
                //        {
                //            OpenFileCommand.Execute(_LastFile);
                //        }
                //    }
                //}

                ImageFiles = new ObservableCollection<ImageFile>();
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
                //LocalSettings.Values[nameof(OverwriteFiles)] = _OverwriteFiles;
                RaisePropertyChanged(nameof(CanOverwriteFiles));
            }
        }


        private bool _KeepAspectRatio;

        public bool KeepAspectRatio
        {
            get { return _KeepAspectRatio; }
            set
            {
                SetProperty(ref _KeepAspectRatio, value, nameof(KeepAspectRatio));
                //LocalSettings.Values[nameof(KeepAspectRatio)] = _KeepAspectRatio;
            }
        }

        #region OpenFileCommand
        public DelegateCommand<ImageFile> OpenFileCommand { get; set; }

        protected async void OnOpenFileCommand(ImageFile file)
        {
            if (file != null)
            {
                // Launch the bug query file.
                //bool sucess = await Windows.System.Launcher.LaunchFileAsync(file);
            }
        }
        protected bool CanOpenFileCommandExecuted(ImageFile file)
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
            //DragEventArgs e = param as DragEventArgs;

            //e.DragUIOverride.IsContentVisible = true;
            //e.DragUIOverride.IsGlyphVisible = true;
            //IReadOnlyList<IStorageItem> Items = e.DataView.GetStorageItemsAsync().GetAwaiter().GetResult();
            ////Flag determine wether draged files are allowed
            //bool CanDrop = true;
            //foreach (var item in Items)
            //{
            //    if (item is IStorageFile)
            //    {
            //        if (item is StorageFile)
            //        {
            //            CanDrop = _AllowedFileTyes.Contains((item as StorageFile).FileType);
            //        }
            //    }
            //    if (item is IStorageFolder && item is StorageFolder)
            //    {
            //        CanDrop = false;
            //    }
            //}

            //if (CanDrop == false)
            //{
            //    e.AcceptedOperation = DataPackageOperation.None;
            //}
        }
        #endregion

        public DelegateCommand<object> DropCommand { get; set; }
        /// <summary>
        /// Add Dropped files to our <see cref="ImageFiles"/> list.
        /// </summary>
        /// <param name="param"></param>
        protected void OnDropCommand(object param)
        {
            //if (param != null && param as IReadOnlyList<IStorageItem> != null)
            //{
            //    if (ImageFiles == null)
            //    {
            //        ImageFiles = new ObservableCollection<IStorageFile>();
            //    }
            //    foreach (var item in param as IReadOnlyList<IStorageItem>)
            //    {
            //        if (item is IStorageFile)
            //        {
            //            if (item is StorageFile)
            //            {
            //                ImageFiles.Add(item as StorageFile);
            //            }
            //        }
            //        //if (item is IStorageFolder && item is StorageFolder)
            //        //{
            //        //    OpenFolder(item as StorageFolder);
            //        //}
            //    }
            //}
        }

        public DelegateCommand<ImageFile> DeleteFileCommand { get; set; }

        protected void OnDeleteFile(ImageFile param)
        {
            if (ImageFiles != null && param != null)
            {
                ImageFiles.Remove(param);
            }
        }


        public bool CanOverwriteFiles
        {
            get
            {
                //bool can = ImageFiles?.Count(a => ((a as StorageFile)?.Attributes & Windows.Storage.FileAttributes.ReadOnly) != 0) == 0;
                bool can = false;
                if (can == false)
                {
                    OverwriteFiles = false;
                }
                return can;

            }
        }
        public bool SingleFile
        {
            get
            {
                bool single = ImageFiles?.Count == 1;

                //if (single == false)
                //{
                //    KeepAspectRatio = false;
                //}
                return single;
            }
        }

        /// <summary>
        /// Returns a flag which indicates whether the app is used as share target
        /// </summary>
        public bool Sharing
        {
            get
            {
                return _Sharing;
            }
        }

    }
}