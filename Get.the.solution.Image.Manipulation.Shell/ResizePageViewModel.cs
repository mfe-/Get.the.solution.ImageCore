﻿using Get.the.solution.UWP.XAML;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Windows.AppModel;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System.Profile;
using Windows.UI.Popups;

namespace Get.the.solution.Image.Manipulation.Shell
{
    public class ResizePageViewModel : BindableBase
    {
        protected readonly INavigationService _NavigationService;
        protected readonly IResourceLoader _ResourceLoader;
        protected readonly IEnumerable<IStorageFile> _SelectedFiles;

        public ResizePageViewModel(IEnumerable<IStorageFile> selectedFiles, INavigationService navigationService, IResourceLoader resourceLoader)
        {
            _SelectedFiles = selectedFiles;
            _ResourceLoader = resourceLoader;
            _NavigationService = navigationService;
            ImageFiles = new List<IStorageFile>();
            OpenFilePickerCommand = new DelegateCommand(OnOpenFilePickerCommand);
            OkCommand = new DelegateCommand(OnOkCommand);
            CancelCommand = new DelegateCommand(OnCancelCommand);
            OpenFileCommand = new DelegateCommand<IStorageFile>(OnOpenFileCommand);

            SizeSmallChecked = true;

            if (_SelectedFiles != null)
            {
                ImageFiles = _SelectedFiles.ToList();
            }
            //get settings
            LocalSettings = ApplicationData.Current.LocalSettings;
            OverwriteFiles = LocalSettings.Values[nameof(OverwriteFiles)] == null ? false : Boolean.Parse(LocalSettings.Values[nameof(OverwriteFiles)].ToString());
        }
        /// <summary>
        /// Access for app settings
        /// </summary>
        private ApplicationDataContainer LocalSettings { get; set; }

        //private bool _ShowOpenFilePicker;

        public bool ShowOpenFilePicker
        {
            get { return ImageFiles != null && ImageFiles.Count == 0; }
            set { OnPropertyChanged(nameof(ShowOpenFilePicker)); }
        }

        #region FilePickerCommand
        public DelegateCommand OpenFilePickerCommand { get; set; }

        protected async void OnOpenFilePickerCommand()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker() { ViewMode = PickerViewMode.List };
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".gif");
            fileOpenPicker.FileTypeFilter.Add(".bmp");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

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
                if (SizeSmallChecked)
                {
                    Width = 640; Height = 480;
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
            set
            {
                SetProperty(ref _ImageFiles, value, nameof(ImageFiles));
                OnPropertyChanged(nameof(ShowOpenFilePicker));
            }
        }
        #endregion

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
                var randomAccessStream = await storage.OpenReadAsync();
                Stream stream = randomAccessStream.AsStreamForRead();
                try
                {
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
        protected IStorageFile _LastFile;

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

        protected async void OnCancelCommand()
        {
            if ((ImageFiles == null || ImageFiles.Count == 0) || (_SelectedFiles == null || _SelectedFiles?.Count() != 0))
            {
                CoreApplication.Exit();
            }
            else
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
                ImageFiles = new List<IStorageFile>();
            }
        }


        private bool _OverwriteFiles;

        public bool OverwriteFiles
        {
            get { return _OverwriteFiles; }
            set
            {
                SetProperty(ref _OverwriteFiles, value, nameof(OverwriteFiles));
                LocalSettings.Values[nameof(OverwriteFiles)] = _OverwriteFiles;
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

    }
}
