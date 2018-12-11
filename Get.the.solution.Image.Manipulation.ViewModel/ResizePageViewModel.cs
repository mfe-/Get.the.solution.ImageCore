﻿using Get.the.solution.Image.Manipulation.Contract;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Get.the.solution.Image.Manipulation.ViewModel
{
    public class ResizePageViewModel : BindableBase
    {
        protected readonly ILoggerService _loggerService;
        protected readonly INavigation _NavigationService;
        protected readonly IResourceService _ResourceLoader;
        protected readonly IImageFileService _imageFileService;
        protected readonly IApplicationService _applicationService;
        protected readonly IProgressBarDialogService _progressBarDialogService;
        protected readonly IShareService _shareService;
        protected readonly ILocalSettings _LocalSettings;
        protected readonly IPageDialogService _pageDialogService;
        protected readonly IResizeService _resizeService;
        protected readonly IDragDropService _dragDropService;
        protected readonly ObservableCollection<ImageFile> _SelectedFiles;
        protected readonly bool _Sharing;
        protected int RadioOptions;

        public ResizePageViewModel(IDragDropService dragDrop, IShareService shareService,
            IResizeService resizeService, IPageDialogService pageDialogService, IProgressBarDialogService progressBar,
            IApplicationService applicationService, IImageFileService imageFileService, ILocalSettings localSettings,
            ILoggerService loggerService, ObservableCollection<ImageFile> selectedFiles,
            INavigation navigationService, IResourceService resourceLoader, TimeSpan sharing)
        {
            try
            {
                _dragDropService = dragDrop;
                _shareService = shareService;
                _resizeService = resizeService;
                _LocalSettings = localSettings;
                _pageDialogService = pageDialogService;
                _progressBarDialogService = progressBar;
                _applicationService = applicationService;
                _SelectedFiles = selectedFiles;
                _ResourceLoader = resourceLoader;
                _NavigationService = navigationService;
                _loggerService = loggerService;
                _imageFileService = imageFileService;
                ImageFiles = new ObservableCollection<ImageFile>();
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
                LoadSettings();
                //update preview
                ApplyPreviewDimensions();
                PropertyChanged += ResizePageViewModel_PropertyChanged;
                _loggerService?.LogEvent(nameof(ResizePageViewModel));
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(ResizePageViewModel), e);
            }
        }
        /// <summary>
        /// Apply settings to viewmodel
        /// </summary>
        public void LoadSettings()
        {
            RadioOptions = _LocalSettings.Values[nameof(RadioOptions)] == null ? 1 : Int32.Parse(_LocalSettings.Values[nameof(RadioOptions)].ToString());

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
            _loggerService?.LogEvent(nameof(LoadSettings), new Dictionary<string, string>()
            {
                { nameof(RadioOptions),$"{RadioOptions}"}
            });

            OverwriteFiles = _LocalSettings.Values[nameof(OverwriteFiles)] == null ? false : Boolean.Parse(_LocalSettings.Values[nameof(OverwriteFiles)].ToString());
            Width = _LocalSettings.Values[nameof(Width)] == null ? 1024 : Int32.Parse(_LocalSettings.Values[nameof(Width)].ToString());
            Height = _LocalSettings.Values[nameof(Height)] == null ? 768 : Int32.Parse(_LocalSettings.Values[nameof(Height)].ToString());

            WidthPercent = _LocalSettings.Values[nameof(WidthPercent)] == null ? 100 : Int32.Parse(_LocalSettings.Values[nameof(WidthPercent)].ToString());
            HeightPercent = _LocalSettings.Values[nameof(HeightPercent)] == null ? 100 : Int32.Parse(_LocalSettings.Values[nameof(HeightPercent)].ToString());

            KeepAspectRatio = _LocalSettings.Values[nameof(KeepAspectRatio)] == null ? false : Boolean.Parse(_LocalSettings.Values[nameof(KeepAspectRatio)].ToString());

        }
        private void ResizePageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                PropertyChanged -= ResizePageViewModel_PropertyChanged;
                if (KeepAspectRatio == true &&
                    (nameof(Width).Equals(e.PropertyName) || nameof(Height).Equals(e.PropertyName)))
                {
                    if (SelectedFile == null && ImageFiles.Count > 0)
                    {
                        SelectedFile = ImageFiles?.First() as ImageFile;
                    }
                    if (SelectedFile != null)
                    {
                        float R = (float)SelectedFile.Width / (float)SelectedFile.Height;
                        if (SelectedFile != null && nameof(Width).Equals(e.PropertyName))
                        {
                            Height = (int)(Width / R);
                        }
                        else if (SelectedFile != null && nameof(Height).Equals(e.PropertyName))
                        {
                            Width = (int)(Height * R);
                        }
                    }
                }
                if (e.PropertyName.Equals(nameof(SizeSmallChecked)) ||
                    e.PropertyName.Equals(nameof(SizeMediumChecked)) ||
                     e.PropertyName.Equals(nameof(SizeCustomChecked)) ||
                      e.PropertyName.Equals(nameof(SizePercentChecked)) ||
                      e.PropertyName.Equals(nameof(HeightPercent)) ||
                      e.PropertyName.Equals(nameof(WidthPercent)) ||
                      e.PropertyName.Equals(nameof(Width)) ||
                      e.PropertyName.Equals(nameof(Height)))
                {
                    ApplyPreviewDimensions();
                }
            }
            catch (Exception ex)
            {
                _loggerService?.LogException(nameof(ResizePageViewModel), ex);
            }
            finally
            {
                PropertyChanged -= ResizePageViewModel_PropertyChanged;
                PropertyChanged += ResizePageViewModel_PropertyChanged;
            }
        }
        public ResizeMode ResizeMode
        {
            get
            {
                if (SizeSmallChecked) return ResizeMode.SizeSmallChecked;
                if (SizeMediumChecked) return ResizeMode.SizeMediumChecked;
                if (SizeCustomChecked) return ResizeMode.SizeCustomChecked;
                if (SizePercentChecked) return ResizeMode.SizePercentChecked;
                return 0;
            }
        }

        public void ApplyPreviewDimensions()
        {
            foreach (ImageFile currentImage in ImageFiles)
            {
                Tuple<int, int> dimension = PreviewDimensions(ResizeMode, currentImage.Width, currentImage.Height, Width, Height, WidthPercent, HeightPercent, KeepAspectRatio);
                currentImage.NewWidth = dimension.Item1;
                currentImage.NewHeight = dimension.Item2;
            }
        }
        public Tuple<int, int> PreviewDimensions(ResizeMode resizeMode, int widthimagefile, int heightimagefile, int entertedWidth, int entertedheight, int widthPercentage, int heightPercentage, bool keepAspect)
        {
            //consider aspect only for custom and percent
            int newWidth = 0;
            int newHeight = 0;

            if (resizeMode == ResizeMode.SizeSmallChecked)
            {
                newWidth = 640;
                newHeight = 480;
            }
            else if (resizeMode == ResizeMode.SizeMediumChecked)
            {
                newWidth = 1024;
                newHeight = 768;
            }
            else if (resizeMode == ResizeMode.SizePercentChecked)
            {
                newWidth = widthimagefile * widthPercentage / 100;
                newHeight = heightimagefile * heightPercentage / 100;
            }
            else
            {
                newWidth = entertedWidth;
                newHeight = entertedheight;
            }
            return new Tuple<int, int>(newWidth, newHeight);
        }


        private ICommand _CtrlOpen;
        public ICommand CtrlOpenCommand => _CtrlOpen ?? (_CtrlOpen = new DelegateCommand<object>(OnCtrlOpen));

        protected void OnCtrlOpen(object param)
        {
            _loggerService?.LogEvent(nameof(OnCtrlOpen));
            if (param != null)
            {
                if (_applicationService.CtrlPressed(param))
                {
                    OpenFilePickerCommand.Execute(param);
                }
            }
        }

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
        private ICommand _OpenFilePickerCommand;
        public ICommand OpenFilePickerCommand => _OpenFilePickerCommand ?? (_OpenFilePickerCommand = new DelegateCommand(OnOpenFilePickerCommand));

        protected async void OnOpenFilePickerCommand()
        {
            await OpenFilePicker();
        }
        protected async Task OpenFilePicker()
        {
            try
            {
                Resizing = true;
                IReadOnlyList<ImageFile> files = await _imageFileService.PickMultipleFilesAsync();
                ImageFiles = new ObservableCollection<ImageFile>(files);
                ApplyPreviewDimensions();
                _loggerService?.LogEvent(nameof(OpenFilePicker), new Dictionary<String, String>() { { nameof(files), $"{files?.Count}" } });
            }
            catch (Contract.Exceptions.PermissionDeniedException e)
            {
                _loggerService?.LogException(nameof(ResizeImages), e);
                await _pageDialogService.ShowAsync(nameof(Contract.Exceptions.PermissionDeniedException));
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(OpenFilePicker), e);
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
                    _LocalSettings.Values[nameof(RadioOptions)] = 1;
                    SizeMediumChecked = false;
                    SizeCustomChecked = false;
                    SizePercentChecked = false;
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
                    _LocalSettings.Values[nameof(RadioOptions)] = 2;
                    SizeSmallChecked = false;
                    SizeCustomChecked = false;
                    SizePercentChecked = false;
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
                    _LocalSettings.Values[nameof(RadioOptions)] = 3;
                    SizeSmallChecked = false;
                    SizeMediumChecked = false;
                    SizePercentChecked = false;
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
                    _LocalSettings.Values[nameof(RadioOptions)] = 4;
                    SizeSmallChecked = false;
                    SizeMediumChecked = false;
                    SizeCustomChecked = false;

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

        public async Task<bool> ResizeImages(ImageAction action, Action<ImageFile, String> ProcessedImageAction = null)
        {
            IProgressBarDialogService progressBarDialog = _progressBarDialogService.ProgressBarDialogFactory();
            try
            {
                Resizing = true;
                //if no file is selected open file picker 
                if (ImageFiles == null || ImageFiles.Count == 0)
                {
                    await OpenFilePicker();
                }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                progressBarDialog.StartAsync(ImageFiles.Count);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                String SuggestedFileName = String.Empty;
                String targetStorageFolder = String.Empty;
                foreach (ImageFile currentImage in ImageFiles)
                {
                    try
                    {
                        SelectedFile = currentImage;
                        SuggestedFileName = _imageFileService.GenerateResizedFileName(currentImage, currentImage.NewWidth, currentImage.NewHeight);
                        progressBarDialog.CurrentItem = SuggestedFileName;
                        using (MemoryStream resizedImageFileStream = _resizeService.Resize(currentImage.Stream, currentImage.NewWidth, currentImage.NewHeight))
                        {
                            //log image size
                            _loggerService?.LogEvent(nameof(IResizeService.Resize), new Dictionary<String, String>()
                            {
                                { $"{nameof(ImageFile)}{nameof(Width)}", $"{currentImage?.Width}" },
                                { $"{nameof(ImageFile)}{nameof(Height)}", $"{currentImage?.Height}" },
                                { nameof(Width), $"{currentImage.NewHeight }" },
                                { nameof(Height), $"{currentImage.NewHeight}" },
                                { nameof(ImageFile.Path), $"{Path.GetDirectoryName(currentImage.Path)}" }
                            });

                            //log action type
                            _loggerService?.LogEvent(nameof(IResizeService.Resize), new Dictionary<String, String>()
                            {
                                { nameof(ImageAction), $"{action}" }
                            });
                            //overwrite current ImageStoreage
                            if (action.Equals(ImageAction.Save))
                            {
                                try
                                {
                                    await _imageFileService.WriteBytesAsync(currentImage, resizedImageFileStream.ToArray());
                                    LastFile = currentImage;
                                }
                                catch (Contract.Exceptions.UnauthorizedAccessException e)
                                {
                                    //log the UnauthorizedAccessException
                                    _loggerService?.LogEvent(nameof(ResizeImages), new Dictionary<String, String>()
                                    {
                                        { nameof(Contract.Exceptions.UnauthorizedAccessException), $"{true}" },
                                    });
                                    //we can't override the current file try for the next files saveAs
                                    action = ImageAction.SaveAs;
                                    await ShowPermissionDeniedDialog();
                                    var File = await _imageFileService.PickSaveFileAsync(currentImage.Path, SuggestedFileName);
                                    if (null != File)
                                    {
                                        //try to apply the new storagefolder (if the user selected a new location)
                                        targetStorageFolder = Path.GetDirectoryName(File.Path);

                                        await _imageFileService.WriteBytesAsync(File, resizedImageFileStream.ToArray());
                                        LastFile = File;
                                    }
                                }
                            } //create new ImageStoreage
                            else if (action.Equals(ImageAction.SaveAs))
                            {
                                ImageFile File = null;
                                try
                                {
                                    if (String.IsNullOrEmpty(targetStorageFolder))
                                    {
                                        //get default path
                                        File = await _imageFileService.PickSaveFileAsync(currentImage.Path, SuggestedFileName);
                                        targetStorageFolder = Path.GetDirectoryName(File.Path);
                                        await _imageFileService.WriteBytesAsync(File, resizedImageFileStream.ToArray());
                                        LastFile = File;
                                    }
                                    else
                                    {
                                        await _imageFileService.WriteBytesAsync(targetStorageFolder, SuggestedFileName, currentImage, resizedImageFileStream.ToArray());
                                    }
                                    //this operation can throw a UnauthorizedAccessException
                                    //if(SingleFile && _LocalSettings.EnabledOpenSingleFileAfterResize)
                                    //{
                                    //    ImageFile imageFile =  await _imageFileService.LoadImageFileAsync(Path.Combine(targetStorageFolder, SuggestedFileName));
                                    //    _applicationService.LaunchFileAsync(imageFile);
                                    //}
                                    //File = await _imageFileService.LoadImageFileAsync($"{targetStorageFolder}{Path.DirectorySeparatorChar}{SuggestedFileName}");
                                }
                                catch (Contract.Exceptions.UnauthorizedAccessException e)
                                {
                                    //log the UnauthorizedAccessException
                                    _loggerService?.LogEvent(nameof(ResizeImages), new Dictionary<String, String>()
                                    {
                                        { nameof(Contract.Exceptions.UnauthorizedAccessException), $"{true}" },
                                    });
                                    await ShowPermissionDeniedDialog();
                                    //tell the user to save the file in an other location
                                    File = await _imageFileService.PickSaveFileAsync(currentImage.Path, SuggestedFileName);
                                    //try to apply the new storagefolder (if the user selected a new location)
                                    if (File != null && Path.GetDirectoryName(targetStorageFolder) != Path.GetDirectoryName(File.Path))
                                    {
                                        targetStorageFolder = Path.GetDirectoryName(File.Path);
                                    }
                                }
                                if (null != File)
                                {
                                    LastFile = File;
                                    if (ImageFiles.Count == 1)
                                    {
                                        OpenFileCommand.Execute(LastFile);
                                    }
                                }
                                else
                                {
                                    _loggerService?.LogEvent(nameof(ResizeImages), new Dictionary<String, String>()
                                        {
                                            { nameof(currentImage.Path), $"{currentImage.Path}" }
                                        });
                                }

                            }
                            else if (action.Equals(ImageAction.Process))
                            {
                                String TempFolder = _applicationService.GetLocalCacheFolder();
                                ImageFile temp = await _imageFileService.WriteBytesAsync(TempFolder, SuggestedFileName, currentImage, resizedImageFileStream.ToArray());
                                ProcessedImageAction?.Invoke(temp, $"{SuggestedFileName}");
                            }
                            //open resized image depending whether only one image is resized and the user enabled this option
                            if (SingleFile && LastFile != null && action != ImageAction.Process &&
                                _LocalSettings.EnabledOpenSingleFileAfterResize)
                            {
                                await _applicationService.LaunchFileAsync(LastFile);
                            }
                        }

                        currentImage?.Stream?.Dispose();
                    }
                    catch (Exception e)
                    {
                        _loggerService?.LogException($"{nameof(ResizeImages)}{nameof(ImageFiles)}", e);
                        Resizing = false;
                        String message = string.Format(_ResourceLoader.GetString("ExceptionOnResize"), $"{e.Message} {e.InnerException}");
                        await _pageDialogService.ShowAsync(message);
                        return false;
                    }
                    progressBarDialog.ProcessedItems++;
                }
                Resizing = false;
            }
            catch (Contract.Exceptions.PermissionDeniedException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(ResizeImages), e);
                _loggerService?.LogEvent(nameof(IResizeService.Resize), new Dictionary<String, String>()
                {
                    { "ResizeFinished","false" }
                });
                return false;
            }
            finally
            {
                progressBarDialog.Stop();
                progressBarDialog = null;
                Resizing = false;
            }
            //log image size
            _loggerService?.LogEvent(nameof(IResizeService.Resize), new Dictionary<String, String>()
            {
                { "ResizeFinished","true" }
            });
            return true;
        }

        private async Task ShowPermissionDeniedDialog()
        {
            await _pageDialogService.ShowAsync(_ResourceLoader.GetString("NoWritePermissionDialog"));
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



        #region OkCommand / Resize Images
        private ICommand _OkCommand;
        public ICommand OkCommand => _OkCommand ?? (_OkCommand = new DelegateCommand(OnOkCommand));

        protected async void OnOkCommand()
        {
            try
            {
                _loggerService?.LogEvent(nameof(OnOkCommand));
                ImageAction Action = OverwriteFiles == true ? ImageAction.Save : ImageAction.SaveAs;
                bool Result = await ResizeImages(Action);
                if (ImageFiles?.Count != 0)
                {
                    CancelCommand?.Execute(ImageFiles);
                }

            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(OnOkCommand), e);
            }
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

        private ICommand _ShareCommand;
        public ICommand ShareCommand => _ShareCommand ?? (_ShareCommand = new DelegateCommand(OnShareCommand));
        protected async void OnShareCommand()
        {
            try
            {
                SharingProcess = true;
                _loggerService?.LogEvent(nameof(OnShareCommand));
                await _shareService.StartShareAsync(ImageFiles, async (action) => await ResizeImages(ImageAction.Process, action), OnCancelCommand);
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(OnShareCommand), e);
            }
            finally
            {
                SharingProcess = false;
            }
            SharingProcess = false;
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
                _LocalSettings.Values[nameof(Width)] = _Width;
            }
        }


        private int _PercentWidth;

        public int WidthPercent
        {
            get { return _PercentWidth; }
            set
            {
                SetProperty(ref _PercentWidth, value, nameof(WidthPercent));
                _LocalSettings.Values[nameof(WidthPercent)] = _PercentWidth;
                if (KeepAspectRatio)
                {
                    _PercentHeight = _PercentWidth;
                    RaisePropertyChanged(nameof(HeightPercent));
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
                _LocalSettings.Values[nameof(Height)] = _Height;
            }
        }

        private int _PercentHeight;

        public int HeightPercent
        {
            get { return _PercentHeight; }
            set
            {
                SetProperty(ref _PercentHeight, value, nameof(HeightPercent));
                _LocalSettings.Values[nameof(HeightPercent)] = _PercentHeight;
                if (KeepAspectRatio)
                {
                    _PercentWidth = _PercentHeight;
                    RaisePropertyChanged(nameof(WidthPercent));
                }
            }
        }
        #endregion

        #region CancelCommand
        private ICommand _CancelCommand;
        public ICommand CancelCommand => _CancelCommand ?? (_CancelCommand = new DelegateCommand(OnCancelCommand));

        protected void OnCancelCommand()
        {
            try
            {
                //dont exit app on sharing
                if ((SharingProcess != true) && (ImageFiles == null || ImageFiles?.Count == 0) || (_SelectedFiles == null || _SelectedFiles?.Count() != 0))
                {
                    _applicationService.Exit();
                }
                else
                {
                    foreach (var imagefile in ImageFiles)
                    {
                        imagefile?.Stream?.Dispose();
                        imagefile.Stream = null;
                    }
                    ImageFiles.Clear();
                    LastFile?.Stream?.Dispose();
                    LastFile = null;
                }
                SharingProcess = false;
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(OnCancelCommand), e);
            }
            _loggerService?.LogEvent(nameof(OnCancelCommand));
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
                _LocalSettings.Values[nameof(OverwriteFiles)] = _OverwriteFiles;
                RaisePropertyChanged(nameof(CanOverwriteFiles));
                _loggerService?.LogEvent(nameof(OverwriteFiles), $"{OverwriteFiles}");
            }
        }


        private bool _KeepAspectRatio;

        public bool KeepAspectRatio
        {
            get { return _KeepAspectRatio; }
            set
            {
                SetProperty(ref _KeepAspectRatio, value, nameof(KeepAspectRatio));
                _LocalSettings.Values[nameof(KeepAspectRatio)] = _KeepAspectRatio;
                _loggerService?.LogEvent(nameof(KeepAspectRatio), $"{KeepAspectRatio}");
            }
        }

        #region OpenFileCommand
        private ICommand _OpenFileCommand;
        public ICommand OpenFileCommand => _OpenFileCommand ?? (_OpenFileCommand = new DelegateCommand<ImageFile>(OnOpenFileCommand));

        protected async void OnOpenFileCommand(ImageFile file)
        {
            try
            {
                if (file != null)
                {
                    // Launch the bug query file.
                    await _applicationService.LaunchFileAsync(file);
                }
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(OnOpenFileCommand), e);
            }
        }
        #endregion

        #region DragOver
        private ICommand _DragOverCommand;
        public ICommand DragOverCommand => _DragOverCommand ?? (_DragOverCommand = new DelegateCommand<object>(OnDragOverCommand));

        /// <summary>
        /// Determine whether the draged file is a supported image.
        /// </summary>
        /// <param name="param">Provides event informations.</param>
        protected void OnDragOverCommand(object param)
        {
            try
            {
                _dragDropService.OnDragOverCommand(param);
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(OnDragOverCommand), e);
            }
            _loggerService?.LogEvent(nameof(OnDragOverCommand));
        }
        #endregion


        private ICommand _DropCommand;
        public ICommand DropCommand => _DropCommand ?? (_DropCommand = new DelegateCommand<object>(OnDropCommand));
        /// <summary>
        /// Add Dropped files to our <see cref="ImageFiles"/> list.
        /// </summary>
        /// <param name="param"></param>
        protected void OnDropCommand(object param)
        {
            try
            {
                _dragDropService.OnDropCommandAsync(param, ImageFiles);
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(OnDropCommand), e);
            }
            _loggerService?.LogEvent(nameof(OnDropCommand));
        }
        protected ImageFile _LastFile;
        public ImageFile LastFile
        {
            get
            {
                return _LastFile;
            }
            set
            {
                SetProperty(ref _LastFile, value, nameof(LastFile));
            }
        }

        private ICommand _DeleteFileCommand;
        public ICommand DeleteFileCommand => _DeleteFileCommand ?? (_DeleteFileCommand = new DelegateCommand<ImageFile>(OnDeleteFile));

        protected void OnDeleteFile(ImageFile param)
        {
            try
            {
                if (ImageFiles != null && param != null)
                {
                    ImageFiles.Remove(param);
                }
                _loggerService?.LogEvent(nameof(OnDeleteFile), new Dictionary<string, string>() { { nameof(ImageFile), param?.Name } });
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(OnDeleteFile), e);
            }
        }


        public bool CanOverwriteFiles
        {
            get
            {
                try
                {
                    bool can = ImageFiles?.Count(a => a.IsReadOnly) == 0;

                    if (can == false)
                    {
                        OverwriteFiles = false;
                    }
                    return can;
                }
                catch (Exception e)
                {
                    _loggerService?.LogException(nameof(CanOverwriteFiles), e);
                    return false;
                }
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

    }
}
