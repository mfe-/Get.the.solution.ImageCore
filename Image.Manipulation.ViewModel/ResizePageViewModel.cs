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
        protected readonly INavigation _navigationService;
        protected readonly IResourceService _resourceLoader;
        protected readonly IImageFileService _imageFileService;
        protected readonly IApplicationService _applicationService;
        protected readonly IProgressBarDialogService _progressBarDialogService;
        protected readonly IShareService _shareService;
        protected readonly ILocalSettings<ResizeSettings> _localSettings;
        protected readonly IPageDialogService _pageDialogService;
        protected readonly IResizeService _resizeService;
        protected readonly IDragDropService _dragDropService;
        protected readonly IFileSystemPermissionDialogService _fileSystemPermissionDialogService;
        protected readonly ObservableCollection<ImageFile> _selectedFiles;

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
                _localSettings = localSettings;
                _pageDialogService = pageDialogService;
                _progressBarDialogService = progressBar;
                _applicationService = applicationService;
                _selectedFiles = selectedFiles;
                _resourceLoader = resourceLoader;
                _navigationService = navigationService;
                _loggerService = loggerService;
                _imageFileService = imageFileService;
                ImageFiles = new ObservableCollection<ImageFile>();

                if (_selectedFiles != null)
                {
                    ImageFiles = _selectedFiles;
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
            RadioOptions = Settings.RadioOptions;
            if (RadioOptions == 1)
            {
                SizeOptionOneChecked = true;
            }
            else if (RadioOptions == 2)
            {
                SizeOptionTwoChecked = true;
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

            OverwriteFiles = Settings.OverwriteFiles;

        }
        private void ResizePageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                PropertyChanged -= ResizePageViewModel_PropertyChanged;
                if (e.PropertyName.Equals(nameof(SizeOptionOneChecked)) ||
                    e.PropertyName.Equals(nameof(SizeOptionTwoChecked)) ||
                     e.PropertyName.Equals(nameof(SizeCustomChecked)) ||
                      e.PropertyName.Equals(nameof(SizePercentChecked)) ||
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
                int newWidth;
                int newHeight;

                if (SizePercentChecked)
                {
                    newWidth = currentImage.Width * Settings.WidthPercent / 100;
                    newHeight = currentImage.Height * Settings.HeightPercent / 100;
                }
                else
                {
                    newWidth = Width;
                    newHeight = Height;
                }
                currentImage.NewWidth = newWidth;
                currentImage.NewHeight = newHeight;
            }
        }

        private ICommand _CtrlOpen;
        public ICommand CtrlOpenCommand => _CtrlOpen ?? (_CtrlOpen = new DelegateCommand<object>(OnCtrlOpen));

        protected void OnCtrlOpen(object param)
        {
            if (param != null && _applicationService.CtrlPressed(param))
            {
                _loggerService?.LogEvent(nameof(OnCtrlOpen));
                OpenFilePickerCommand.Execute(param);
            }
        }

        public bool ShowOpenFilePicker
        {
            get { return ImageFiles != null && ImageFiles.Count == 0; }
        }

        private ImageFile _SelectedFile;
        public ImageFile SelectedFile
        {
            get { return _SelectedFile; }
            set { SetProperty(ref _SelectedFile, value, nameof(SelectedFile)); }
        }

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
        /// <summary>
        /// Updates the current Width Height according of the selected option
        /// </summary>
        public void UpdateOptionSettings()
        {
            if (SizeOptionOneChecked)
            {
                Width = Settings.SizeOptionOneWidth;
                Height = Settings.SizeOptionOneHeight;
            }
            if (SizeOptionTwoChecked)
            {
                Width = Settings.SizeOptionTwoWidth;
                Height = Settings.SizeOptionTwoHeight;
            }
            if (SizeCustomChecked)
            {
                Width = Settings.WidthCustom;
                Height = Settings.HeightCustom;
            }
            if (SizePercentChecked)
            {
                Width = Settings.WidthPercent;
                Height = Settings.HeightPercent;
            }
        }
        /// <summary>
        /// Get or sets whehter the option one is checked
        /// </summary>
        private bool _SizeOptionOneChecked;
        public bool SizeOptionOneChecked
        {
            get { return _SizeOptionOneChecked; }
            set
            {
                SetProperty(ref _SizeOptionOneChecked, value, nameof(SizeOptionOneChecked));
                if (SizeOptionOneChecked)
                {
                    Width = Settings.SizeOptionOneWidth;
                    Height = Settings.SizeOptionOneHeight;
                    Settings.RadioOptions = 1;
                    SizeOptionTwoChecked = false;
                    SizeCustomChecked = false;
                    SizePercentChecked = false;
                }
            }
        }
        /// <summary>
        /// Get or sets whether the option two is checked
        /// </summary>
        private bool _SizeOptionTwoChecked;
        public bool SizeOptionTwoChecked
        {
            get { return _SizeOptionTwoChecked; }
            set
            {
                SetProperty(ref _SizeOptionTwoChecked, value, nameof(SizeOptionTwoChecked));
                if (SizeOptionTwoChecked)
                {
                    Width = Settings.SizeOptionTwoWidth;
                    Height = Settings.SizeOptionTwoHeight;
                    Settings.RadioOptions = 2;
                    SizeOptionOneChecked = false;
                    SizeCustomChecked = false;
                    SizePercentChecked = false;
                }
            }
        }
        /// <summary>
        /// Get or sets whether the custom width and custom height is checked
        /// </summary>
        private bool _SizeCustomChecked;
        public bool SizeCustomChecked
        {
            get { return _SizeCustomChecked; }
            set
            {
                SetProperty(ref _SizeCustomChecked, value, nameof(SizeCustomChecked));
                if (SizeCustomChecked)
                {
                    Width = Settings.WidthCustom;
                    Height = Settings.HeightCustom;
                    Settings.RadioOptions = 3;
                    SizeOptionOneChecked = false;
                    SizeOptionTwoChecked = false;
                    SizePercentChecked = false;
                }
            }
        }

        /// <summary>
        /// Get or sets whether the percent is checked
        /// </summary>
        private bool _SizePercentChecked;
        public bool SizePercentChecked
        {
            get { return _SizePercentChecked; }
            set
            {
                SetProperty(ref _SizePercentChecked, value, nameof(SizePercentChecked));
                if (SizePercentChecked)
                {
                    Width = Settings.WidthPercent;
                    Height = Settings.HeightPercent;
                    Settings.RadioOptions = 4;
                    SizeOptionOneChecked = false;
                    SizeOptionTwoChecked = false;
                    SizeCustomChecked = false;
                }
            }
        }

        #endregion

        #region Images
        /// <summary>
        /// Get or sets the list of files which should be resized
        /// </summary>
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
        /// <summary>
        /// Starts resizing the image operation
        /// </summary>
        /// <param name="action">Determine whether the image files should be saved as, or updated or resize image is used from sharing</param>
        /// <param name="processedImageAction">Action method which retrievs the resized image. Can be used for sharing.</param>
        /// <returns>Task which indicates if the current resize operation was successfull</returns>
        public async Task<bool> ResizeImages(ImageAction action, Action<ImageFile, String> processedImageAction = null)
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

                String suggestedFileName = String.Empty;
                String targetStorageFolder = Settings.DefaultSaveAsTargetFolder;
                Exception lastException = null;
                if (ImageFiles != null)
                {
                    foreach (ImageFile currentImage in ImageFiles)
                    {
                        try
                        {
                            SelectedFile = currentImage;
                            suggestedFileName = _imageFileService.GenerateResizedFileName(currentImage, currentImage.NewWidth, currentImage.NewHeight);
                            if (progressBarDialog != null)
                            {
                                progressBarDialog.CurrentItem = suggestedFileName;
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
                                        var ms = _resizeService.Resize(currentImage.Stream, currentImage.NewWidth, currentImage.NewHeight, suggestedFileName, _localSettings.Settings.ImageQuality);
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
                                var ms = _resizeService.Resize(currentImage.Stream, currentImage.NewWidth, currentImage.NewHeight, suggestedFileName, _localSettings.Settings.ImageQuality);
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
                                        ImageFile File = await _imageFileService.PickSaveFileAsync(currentImage.Path, suggestedFileName);
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
                                    ImageFile imageFile = null;
                                    try
                                    {
                                        if (String.IsNullOrEmpty(targetStorageFolder))
                                        {
                                            if (SingleFile)
                                            {
                                                //get default path
                                                imageFile = await _imageFileService.PickSaveFileAsync(currentImage.Path, suggestedFileName);
                                            }
                                            else
                                            {
                                                imageFile = await _imageFileService.PickSaveFolderAsync(currentImage.Path, suggestedFileName);
                                            }
                                            if (imageFile == null)
                                            {
                                                //if user canceled dialog try again
                                                imageFile = await _imageFileService.PickSaveFileAsync(currentImage.Path, suggestedFileName);
                                            }
                                            //File can be null when user aborted picksavefile dialog
                                            if (imageFile != null)
                                            {
                                                targetStorageFolder = Path.GetDirectoryName(imageFile.Path);
                                                await _imageFileService.WriteBytesAsync(imageFile, resizedImageFileStream.ToArray());
                                                LastFile = imageFile;
                                            }
                                        }
                                        else
                                        {
                                            await _imageFileService.WriteBytesAsync(targetStorageFolder, suggestedFileName, currentImage, resizedImageFileStream.ToArray());
                                        }
                                    }
                                    catch (FileNotFoundException)
                                    {
                                        //happens if the targetStorageFolder from the settings was removed (or renamed) meanwhile
                                        _loggerService?.LogEvent(nameof(ResizeImages), new Dictionary<String, String>()
                                        {
                                            { nameof(FileNotFoundException), true.ToString() },
                                        });
                                        //happens if the targetStorageFolder was removed meanwhile
                                        Settings.DefaultSaveAsTargetFolder = String.Empty;
                                        //tell the user to save the file in an other location
                                        imageFile = await _imageFileService.PickSaveFileAsync(currentImage.Path, suggestedFileName);
                                        //try to apply the new storagefolder (if the user selected a new location)
                                        if (imageFile != null && Path.GetDirectoryName(targetStorageFolder) != Path.GetDirectoryName(imageFile.Path))
                                        {
                                            targetStorageFolder = Path.GetDirectoryName(imageFile.Path);
                                            await _imageFileService.WriteBytesAsync(imageFile, resizedImageFileStream.ToArray());
                                        }
                                    }
                                    catch (Contract.Exceptions.UnauthorizedAccessException)
                                    {
                                        //log the UnauthorizedAccessException
                                        _loggerService?.LogEvent(nameof(ResizeImages), new Dictionary<String, String>()
                                        {
                                            { nameof(Contract.Exceptions.UnauthorizedAccessException), true.ToString() },
                                        });
                                        //tell the user to save the file in an other location
                                        imageFile = await _imageFileService.PickSaveFileAsync(currentImage.Path, suggestedFileName);
                                        //try to apply the new storagefolder (if the user selected a new location)
                                        if (imageFile != null && Path.GetDirectoryName(targetStorageFolder) != Path.GetDirectoryName(imageFile.Path))
                                        {
                                            targetStorageFolder = Path.GetDirectoryName(imageFile.Path);
                                            await _imageFileService.WriteBytesAsync(imageFile, resizedImageFileStream.ToArray());
                                        }
                                        await ShowPermissionDeniedDialog(progressBarDialog);
                                    }
                                    if (null != imageFile)
                                    {
                                        LastFile = imageFile;
                                    }
                                    _loggerService?.LogEvent(nameof(ResizeImages), new Dictionary<String, String>()
                                    {
                                        { nameof(currentImage.Path), $"{currentImage.Path}" }
                                    });
                                }
                                else if (action.Equals(ImageAction.Process))
                                {
                                    String TempFolder = _applicationService.GetLocalCacheFolder();
                                    ImageFile temp = await _imageFileService.WriteBytesAsync(TempFolder, suggestedFileName, currentImage, resizedImageFileStream.ToArray());
                                    processedImageAction?.Invoke(temp, $"{suggestedFileName}");
                                }
                                //open resized image depending whether only one image is resized and the user enabled this option
                                if (SingleFile && LastFile != null && action != ImageAction.Process &&
                                    _localSettings.Settings.EnabledOpenSingleFileAfterResize)
                                {
                                    OpenFileCommand.Execute(LastFile);
                                }
                            }
                            currentImage?.Stream?.Dispose();
                        }
                        catch (NotSupportedException e)
                        {
                            _loggerService?.LogException($"{nameof(ResizeImages)}{nameof(ImageFiles)}", e,
                                new Dictionary<string, string>() { { nameof(suggestedFileName), suggestedFileName } });
                            await _pageDialogService?.ShowAsync(_resourceLoader.GetString("ImageTypNotSupported"));
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
                    if (_localSettings != null && _localSettings.Settings.ShowSuccessMessage && LastFile != null)
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
                            String message = _resourceLoader.GetString("UnknownImageFormatException");
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
        /// <summary>
        /// Get or sets the flag which indicates whether a resize operation is currently processed
        /// </summary>
        /// <remarks>Also this flag is set to true when the user selected (open) images for resizing</remarks>
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
                UpdateOptionSettings();
                _loggerService?.LogEvent(nameof(OnOkCommand));
                ImageAction Action = OverwriteFiles ? ImageAction.Save : ImageAction.SaveAs;
                await ResizeImages(Action);
                if (_localSettings.Settings.ClearImageListAfterSuccess && ImageFiles?.Count != 0)
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
        /// <summary>
        /// Current width which should applied to the image
        /// </summary>
        private int _Width;
        public int Width
        {
            get { return _Width; }
            set { SetProperty(ref _Width, value, nameof(Width)); }
        }
        /// <summary>
        /// Current height which should be applied to the image
        /// </summary>
        private int _Height;
        public int Height
        {
            get { return _Height; }
            set { SetProperty(ref _Height, value, nameof(Height)); }
        }

        #endregion

        #region CancelCommand
        private ICommand _CancelCommand;
        public ICommand CancelCommand => _CancelCommand ?? (_CancelCommand = new DelegateCommand(OnCancelCommand));

        protected async void OnCancelCommand()
        {
            try
            {
                //dont exit app on sharing
                if ((!SharingProcess) && (ImageFiles == null || ImageFiles?.Count == 0) || (_selectedFiles == null || _selectedFiles?.Count() != 0))
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
                await _localSettings.SaveSettingsAsync();
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(OnCancelCommand), e);
            }
            _loggerService?.LogEvent(nameof(OnCancelCommand));
        }
        #endregion

        /// <summary>
        /// This flag determines whether the existing file should be overwriten when resizing the image or a new file should be created
        /// </summary>
        private bool _OverwriteFiles;
        public bool OverwriteFiles
        {
            get { return _OverwriteFiles; }
            set
            {
                SetProperty(ref _OverwriteFiles, value, nameof(OverwriteFiles));
                Settings.OverwriteFiles = _OverwriteFiles;
                RaisePropertyChanged(nameof(CanOverwriteFiles));
                _loggerService?.LogEvent(nameof(OverwriteFiles), $"{OverwriteFiles}");
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
        public ICommand DragOverCommand => _DragOverCommand ?? (_DragOverCommand = new DelegateCommand<object>(OnDragOverCommandAsync));
        private readonly SemaphoreSlim _SemaphoreSlimDragOver = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Determine whether the draged file is a supported image.
        /// </summary>
        /// <param name="param">Provides event informations.</param>
        protected async void OnDragOverCommandAsync(object param)
        {
            try
            {
                await _SemaphoreSlimDragOver.WaitAsync();
                _dragDropService.OnDragOverCommand(param);
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(OnDragOverCommandAsync), e);
            }
            finally
            {
                _SemaphoreSlimDragOver.Release();
            }
            _loggerService?.LogEvent(nameof(OnDragOverCommandAsync));
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
        /// <summary>
        /// Get or sets the last file which was resized
        /// </summary>
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

        /// <summary>
        /// Gets whether all selected files for resizing are writeable
        /// </summary>
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
        /// <summary>
        /// Gets whether only one file is selected for resizing
        /// </summary>
        public bool SingleFile
        {
            get
            {
                bool single = ImageFiles?.Count == 1;
                return single;
            }
        }
        /// <summary>
        /// Gets the Settings object
        /// </summary>
        public ResizeSettings Settings
        {
            get
            {
                return _localSettings.Settings;
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
