using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Contract.Exceptions;
using Get.the.solution.Image.Manipulation.Contract;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Threading;
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
        protected readonly ILocalSettings<ResizeSettings> _LocalSettings;
        protected readonly IPageDialogService _pageDialogService;
        protected readonly IResizeService _resizeService;
        protected readonly IDragDropService _dragDropService;
        protected readonly IFileSystemPermissionDialogService _fileSystemPermissionDialogService;
        protected readonly ObservableCollection<ImageFile> _SelectedFiles;

        protected int RadioOptions;

        public ResizePageViewModel(IDragDropService dragDrop, IShareService shareService,
            IResizeService resizeService, IPageDialogService pageDialogService, IProgressBarDialogService progressBar,
            IFileSystemPermissionDialogService fileSystemPermissionDialogService, IApplicationService applicationService, IImageFileService imageFileService, ILocalSettings<ResizeSettings> localSettings,
            ILoggerService loggerService, ObservableCollection<ImageFile> selectedFiles,
            INavigation navigationService, IResourceService resourceLoader, AppStartType appStartType)
        {
            try
            {
                _fileSystemPermissionDialogService = fileSystemPermissionDialogService;
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

            _appStartType = appStartType;
        }
        public bool IsShareTarget
        {

            get
            {
                return AppStartType.AppIsShareTarget.Equals(_appStartType);
            }
        }
        /// <summary>
        /// Apply settings to viewmodel
        /// </summary>
        public void LoadSettings()
        {
            RadioOptions = _LocalSettings.Settings.RadioOptions;
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

            OverwriteFiles = _LocalSettings.Settings.OverwriteFiles;
            Width = _LocalSettings.Settings.WidthCustom;
            Height = _LocalSettings.Settings.HeightCustom;

            WidthPercent = _LocalSettings.Settings.WidthPercent;
            HeightPercent = _LocalSettings.Settings.HeightPercent;

            KeepAspectRatio = _LocalSettings.Settings.KeepAspectRatio;

        }
        private void ResizePageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                PropertyChanged -= ResizePageViewModel_PropertyChanged;
                if (KeepAspectRatio &&
                    (nameof(Width).Equals(e.PropertyName) || nameof(Height).Equals(e.PropertyName)))
                {
                    if (SelectedFile == null && ImageFiles.Count > 0)
                    {
                        SelectedFile = ImageFiles?.First();
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

        public void ApplyPreviewDimensions()
        {
            foreach (ImageFile currentImage in ImageFiles)
            {
                Tuple<int, int> dimension = PreviewDimensions(currentImage.Width, currentImage.Height, Width, Height, WidthPercent, HeightPercent, KeepAspectRatio);
                currentImage.NewWidth = dimension.Item1;
                currentImage.NewHeight = dimension.Item2;
            }
        }
        protected Tuple<int, int> PreviewDimensions(int widthimagefile, int heightimagefile, int entertedWidth, int entertedheight, int widthPercentage, int heightPercentage, bool keepAspect)
        {
            //consider aspect only for custom and percent
            int newWidth;
            int newHeight;

            if (SizeSmallChecked)
            {
                newWidth = 640;
                newHeight = 480;
            }
            else if (SizeMediumChecked)
            {
                newWidth = 800;
                newHeight = 600;
            }
            else if (SizePercentChecked)
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
            if (param != null && _applicationService.CtrlPressed(param))
            {
                OpenFilePickerCommand.Execute(param);
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
                _loggerService?.LogEvent(nameof(OpenFilePicker), new Dictionary<String, String>() { { nameof(files), $"{files?.Count}" } });
            }
            catch (Contract.Exceptions.UnauthorizedAccessException)
            {
                _loggerService?.LogEvent(nameof(ResizeImages), new Dictionary<String, String>()
                {
                    { nameof(Contract.Exceptions.UnauthorizedAccessException), $"{true}" },
                });
                await ShowPermissionDeniedDialog();
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
                if (SizeSmallChecked)
                {
                    _LocalSettings.Settings.RadioOptions = 1;
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
                if (SizeMediumChecked)
                {
                    _LocalSettings.Settings.RadioOptions = 2;
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
                if (SizeCustomChecked)
                {
                    _LocalSettings.Settings.RadioOptions = 3;
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
                if (SizePercentChecked)
                {
                    _LocalSettings.Settings.RadioOptions = 4;
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
                ApplyPreviewDimensions();
                RaisePropertyChanged(nameof(ShowOpenFilePicker));
                RaisePropertyChanged(nameof(SingleFile));
            }
        }

        private void ImageFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(ShowOpenFilePicker));
            RaisePropertyChanged(nameof(CanOverwriteFiles));
            RaisePropertyChanged(nameof(SingleFile));
            ApplyPreviewDimensions();
        }
        #endregion

        public async Task<bool> ResizeImages(ImageAction action, Action<ImageFile, String> ProcessedImageAction = null)
        {
            IProgressBarDialogService progressBarDialog = !IsShareTarget ? _progressBarDialogService.ProgressBarDialogFactory() : null;
            try
            {
                Resizing = true;
                //if no file is selected open file picker 
                if (ImageFiles == null || ImageFiles.Count == 0)
                {
                    await OpenFilePicker();
                }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                progressBarDialog?.StartAsync(ImageFiles.Count);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

                String SuggestedFileName = String.Empty;
                String targetStorageFolder = String.Empty;
                Exception lastException = null;
                if (ImageFiles != null)
                {
                    foreach (ImageFile currentImage in ImageFiles)
                    {
                        try
                        {
                            SelectedFile = currentImage;
                            SuggestedFileName = _imageFileService.GenerateResizedFileName(currentImage, currentImage.NewWidth, currentImage.NewHeight);
                            if (progressBarDialog != null)
                            {
                                progressBarDialog.CurrentItem = SuggestedFileName;
                            }
                            //if the stream is disposed CanSeek and CanRead is false
                            if (currentImage.Stream == null || !currentImage.Stream.CanSeek && !currentImage.Stream.CanRead)
                            {
                                var imageFile = (await _imageFileService.LoadImageFileAsync(currentImage.Path));
                                if (imageFile.Stream != null)
                                {
                                    currentImage.Stream = imageFile.Stream;
                                }
                                else
                                {
                                    //await ShowPermissionDeniedDialog(progressBarDialog); //todo file write/read test
                                    _loggerService.LogEvent("Could not load stream.", new Dictionary<string, string>() { { nameof(currentImage.Path), currentImage.Path } });
                                    continue;
                                }
                            }
                            TaskCompletionSource<MemoryStream> taskMemoryStreamCompletionSource = new TaskCompletionSource<MemoryStream>();
                            if (!IsShareTarget)
                            {
                                //do the resizing in a seperate thread
                                Thread thread = new Thread(() =>
                                {
                                    try
                                    {
                                        var ms = _resizeService.Resize(currentImage.Stream, currentImage.NewWidth, currentImage.NewHeight, SuggestedFileName, _LocalSettings.Settings.ImageQuality);
                                        taskMemoryStreamCompletionSource.SetResult(ms);
                                    }
                                    catch (Exception e)
                                    {
                                        taskMemoryStreamCompletionSource.SetException(e);
                                    }
                                });
                                thread.IsBackground = true;
                                thread.Priority = ThreadPriority.Normal;
                                thread.Start();
                            }
                            else
                            {
                                //do the resizing on the main thread in share target to avoid issues see #53
                                var ms = _resizeService.Resize(currentImage.Stream, currentImage.NewWidth, currentImage.NewHeight, SuggestedFileName, _LocalSettings.Settings.ImageQuality);
                                taskMemoryStreamCompletionSource.SetResult(ms);
                            }
                            using (MemoryStream resizedImageFileStream = await taskMemoryStreamCompletionSource.Task)
                            {
                                //log image size
                                _loggerService?.LogEvent(nameof(IResizeService.Resize), new Dictionary<String, String>()
                                {
                                    { $"{nameof(ImageFile)}{nameof(Width)}", $"{currentImage?.Width}" },
                                    { $"{nameof(ImageFile)}{nameof(Height)}", $"{currentImage?.Height}" },
                                    { nameof(Width), $"{currentImage?.NewHeight }" },
                                    { nameof(Height), $"{currentImage?.NewHeight}" },
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
                                    catch (Contract.Exceptions.UnauthorizedAccessException)
                                    {
                                        //log the UnauthorizedAccessException
                                        _loggerService?.LogEvent(nameof(ResizeImages), new Dictionary<String, String>()
                                        {
                                            { nameof(Contract.Exceptions.UnauthorizedAccessException), $"{true}" },
                                        });
                                        //we can't override the current file try for the next files saveAs
                                        action = ImageAction.SaveAs;
                                        await ShowPermissionDeniedDialog(progressBarDialog);
                                        ImageFile File = await _imageFileService.PickSaveFileAsync(currentImage.Path, SuggestedFileName);
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
                                            //File can be null when user aborted picksavefile dialog
                                            if (File != null)
                                            {
                                                targetStorageFolder = Path.GetDirectoryName(File.Path);
                                                await _imageFileService.WriteBytesAsync(File, resizedImageFileStream.ToArray());
                                                LastFile = File;
                                            }
                                        }
                                        else
                                        {
                                            await _imageFileService.WriteBytesAsync(targetStorageFolder, SuggestedFileName, currentImage, resizedImageFileStream.ToArray());
                                        }
                                    }
                                    catch (Contract.Exceptions.UnauthorizedAccessException)
                                    {
                                        //log the UnauthorizedAccessException
                                        _loggerService?.LogEvent(nameof(ResizeImages), new Dictionary<String, String>()
                                        {
                                            { nameof(Contract.Exceptions.UnauthorizedAccessException), $"{true}" },
                                        });
                                        await ShowPermissionDeniedDialog(progressBarDialog);
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
                                    }
                                    _loggerService?.LogEvent(nameof(ResizeImages), new Dictionary<String, String>()
                                {
                                    { nameof(currentImage.Path), $"{currentImage.Path}" }
                                });
                                }
                                else if (action.Equals(ImageAction.Process))
                                {
                                    String TempFolder = _applicationService.GetLocalCacheFolder();
                                    ImageFile temp = await _imageFileService.WriteBytesAsync(TempFolder, SuggestedFileName, currentImage, resizedImageFileStream.ToArray());
                                    ProcessedImageAction?.Invoke(temp, $"{SuggestedFileName}");
                                }
                                //open resized image depending whether only one image is resized and the user enabled this option
                                if (SingleFile && LastFile != null && action != ImageAction.Process &&
                                    _LocalSettings.Settings.EnabledOpenSingleFileAfterResize)
                                {
                                    OpenFileCommand.Execute(LastFile);
                                }
                            }
                            currentImage?.Stream?.Dispose();
                        }
                        catch (NotSupportedException e)
                        {
                            _loggerService?.LogException($"{nameof(ResizeImages)}{nameof(ImageFiles)}", e,
                                new Dictionary<string, string>() { { nameof(SuggestedFileName), SuggestedFileName } });
                            await _pageDialogService?.ShowAsync(_ResourceLoader.GetString("ImageTypNotSupported"));
                        }
                        catch (InvalidOperationException e)
                        {
                            //for example when enterted width and height is zero. 'Target width 0 and height 0 must be greater than zero.'
                            lastException = e;
                        }
                        catch (FileLoadException e)
                        {
                            //for example The process cannot access the file because it is being used by another process. 
                            lastException = e;
                        }
                        catch (UnknownImageFormatException e)
                        {
                            lastException = e;
                        }
                        catch (Exception e)
                        {
                            lastException = e;
                            _loggerService?.LogException($"{nameof(ResizeImages)}{nameof(ImageFiles)}", e);
                        }
                        if (progressBarDialog != null)
                        {
                            progressBarDialog.ProcessedItems++;
                        }
                    }
                    if (_LocalSettings != null && _LocalSettings.Settings.ShowSuccessMessage && LastFile != null)
                    {
                        await _pageDialogService?.ShowAsync(_imageFileService.GenerateSuccess(LastFile));
                    }
                    if (IsShareTarget)
                    {
                        _shareService.EndShareTargetOperation();
                    }
                    Resizing = false;
                    if (lastException != null)
                    {
                        if (lastException is InvalidOperationException || lastException is FileLoadException)
                        {
                            await _pageDialogService.ShowAsync(lastException.Message);
                        }
                        if (lastException is UnknownImageFormatException)
                        {
                            String message = _ResourceLoader.GetString("UnknownImageFormatException");
                            if (!String.IsNullOrEmpty(message))
                            {
                                message = String.Format(message, string.Join(", ", _imageFileService.FileTypeFilter));
                                await _pageDialogService.ShowAsync(message);
                            }
                        }
                    }
                }

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
                progressBarDialog?.Stop();
                Resizing = false;
            }
            //log image size
            _loggerService?.LogEvent(nameof(IResizeService.Resize), new Dictionary<String, String>()
            {
                { "ResizeFinished","true" }
            });
            return true;
        }

        private async Task ShowPermissionDeniedDialog(IProgressBarDialogService progressBarDialogService = null)
        {
            //uwp content dialog allows only one contentdialog so we need to stop the progressbar
            progressBarDialogService?.Stop();
            await _fileSystemPermissionDialogService.ShowFileSystemAccessDialogAsync();
            //restart progressbar
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            progressBarDialogService?.StartAsync(ImageFiles.Count);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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

        protected readonly SemaphoreSlim _SemaphoreSlimOnOkCommand = new SemaphoreSlim(1, 1);
        private readonly AppStartType _appStartType;

        protected async void OnOkCommand()
        {
            try
            {
                await _SemaphoreSlimOnOkCommand.WaitAsync();
                _loggerService?.LogEvent(nameof(OnOkCommand));
                ImageAction Action = OverwriteFiles ? ImageAction.Save : ImageAction.SaveAs;
                await ResizeImages(Action);
                if (_LocalSettings.Settings.ClearImageListAfterSuccess && ImageFiles?.Count != 0)
                {
                    CancelCommand?.Execute(ImageFiles);
                }

            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(OnOkCommand), e);
            }
            finally
            {
                _SemaphoreSlimOnOkCommand.Release();
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
                _LocalSettings.Settings.WidthCustom = _Width;
            }
        }


        private int _PercentWidth;

        public int WidthPercent
        {
            get { return _PercentWidth; }
            set
            {
                SetProperty(ref _PercentWidth, value, nameof(WidthPercent));
                _LocalSettings.Settings.WidthPercent = _PercentWidth;
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
                _LocalSettings.Settings.HeightCustom = _Height;
            }
        }

        private int _PercentHeight;

        public int HeightPercent
        {
            get { return _PercentHeight; }
            set
            {
                SetProperty(ref _PercentHeight, value, nameof(HeightPercent));
                _LocalSettings.Settings.HeightPercent = _PercentHeight;
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
                if ((!SharingProcess) && (ImageFiles == null || ImageFiles?.Count == 0) || (_SelectedFiles == null || _SelectedFiles?.Count() != 0))
                {
                    _applicationService.Exit();
                }
                else
                {
                    foreach (var imagefile in ImageFiles)
                    {
                        if (imagefile != null)
                        {
                            imagefile.Stream?.Dispose();
                            imagefile.Stream = null;
                        }

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
                _LocalSettings.Settings.OverwriteFiles = _OverwriteFiles;
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
                _LocalSettings.Settings.KeepAspectRatio = _KeepAspectRatio;
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
                    _loggerService?.LogEvent(nameof(OnDeleteFile), new Dictionary<string, string>() { { nameof(ImageFile), param?.Name } });
                }
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
                    if (!can)
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
                return single;
            }
        }

        public enum ImageAction
        {
            Save = 0,
            SaveAs,
            Share,
            Process
        }
    }
}
