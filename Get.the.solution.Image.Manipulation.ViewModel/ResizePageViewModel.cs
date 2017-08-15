using Get.the.solution.Image.Manipulation.Contract;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.ViewModel
{
    public class ResizePageViewModel : BindableBase
    {
        protected readonly IFilePicker _IFilePicker;
        protected readonly INavigation _NavigationService;
        protected readonly IResourceService _ResourceLoader;
        protected readonly ObservableCollection<FileInfo> _SelectedFiles;
        protected readonly bool _Sharing;
        //protected IStorageFile _LastFile;
        protected IEnumerable<String> _AllowedFileTyes = new List<String>() { ".jpg", ".png", ".gif", ".bmp" };
        protected int RadioOptions;
        //protected DataTransferManager _DataTransferManager;
        public ResizePageViewModel(IFilePicker filePicker, IResourceService resourceLoader, ObservableCollection<FileInfo> selectedFiles, INavigation navigationService, TimeSpan sharing)
        {
            //LocalSettings = ApplicationData.Current.LocalSettings;
            _IFilePicker = filePicker;
            _SelectedFiles = selectedFiles;
            _ResourceLoader = resourceLoader;
            _NavigationService = navigationService;
            //ImageFiles = new ObservableCollection<IStorageFile>();
            OpenFilePickerCommand = new DelegateCommand(OnOpenFilePickerCommand);
            //OkCommand = new DelegateCommand(OnOkCommand);
            //CancelCommand = new DelegateCommand(OnCancelCommand);
            //OpenFileCommand = new DelegateCommand<IStorageFile>(OnOpenFileCommand);
            //DragOverCommand = new DelegateCommand<object>(OnDragOverCommand);
            //DropCommand = new DelegateCommand<object>(OnDropCommand);
            //ShareCommand = new DelegateCommand(OnShareCommand);
            //DeleteFileCommand = new DelegateCommand<IStorageFile>(OnDeleteFile);

            //if (TimeSpan.MaxValue.Equals(sharing))
            //{
            //    _Sharing = true;
            //}
            //else
            //{
            //    _Sharing = false;
            //}

            //if (_SelectedFiles != null)
            //{
            //    ImageFiles = _SelectedFiles;
            //}
            ////get settings

            //RadioOptions = LocalSettings.Values[nameof(RadioOptions)] == null ? 1 : Int32.Parse(LocalSettings.Values[nameof(RadioOptions)].ToString());

            //if (RadioOptions == 1)
            //{
            //    SizeSmallChecked = true;
            //}
            //else if (RadioOptions == 2)
            //{
            //    SizeMediumChecked = true;
            //}
            //else if (RadioOptions == 3)
            //{
            //    SizeCustomChecked = true;
            //}
            //else if (RadioOptions == 4)
            //{
            //    SizePercentChecked = true;
            //}

            //OverwriteFiles = LocalSettings.Values[nameof(OverwriteFiles)] == null ? false : Boolean.Parse(LocalSettings.Values[nameof(OverwriteFiles)].ToString());
            //Width = LocalSettings.Values[nameof(Width)] == null ? 1024 : Int32.Parse(LocalSettings.Values[nameof(Width)].ToString());
            //Height = LocalSettings.Values[nameof(Height)] == null ? 768 : Int32.Parse(LocalSettings.Values[nameof(Height)].ToString());

            //WidthPercent = LocalSettings.Values[nameof(WidthPercent)] == null ? 100 : Int32.Parse(LocalSettings.Values[nameof(WidthPercent)].ToString());
            //HeightPercent = LocalSettings.Values[nameof(HeightPercent)] == null ? 100 : Int32.Parse(LocalSettings.Values[nameof(HeightPercent)].ToString());

            //KeepAspectRatio = LocalSettings.Values[nameof(KeepAspectRatio)] == null ? false : Boolean.Parse(LocalSettings.Values[nameof(KeepAspectRatio)].ToString());
            //PropertyChanged += ResizePageViewModel_PropertyChanged;
        }

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
                if(_IFilePicker.FileTypeFilter.Count==0)
                {
                    foreach (String Filetypeextension in _AllowedFileTyes)
                        _IFilePicker.FileTypeFilter.Add(Filetypeextension);
                }
                //fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

                IReadOnlyList<FileInfo> files = await _IFilePicker.PickMultipleFilesAsync();
                ImageFiles = new ObservableCollection<FileInfo>(files);
            }
            finally
            {
                Resizing = false;
            }

        }
        #endregion

        #region Images
        private ObservableCollection<FileInfo> _ImageFiles;

        public ObservableCollection<FileInfo> ImageFiles
        {
            get { return _ImageFiles; }
            set
            {
                SetProperty(ref _ImageFiles, value, nameof(ImageFiles));
                if (ImageFiles != null)
                {
                    ImageFiles.CollectionChanged += ImageFiles_CollectionChanged;
                }
                //OnPropertyChanged(nameof(ShowOpenFilePicker));
                //OnPropertyChanged(nameof(SingleFile));
            }
        }

        private void ImageFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //OnPropertyChanged(nameof(ShowOpenFilePicker));
            //OnPropertyChanged(nameof(CanOverwriteFiles));
            //OnPropertyChanged(nameof(SingleFile));
        }
        #endregion

        private bool _Resizing;

        public bool Resizing
        {
            get { return _Resizing; }
            protected set
            {
                SetProperty(ref _Resizing, value, nameof(Resizing));
            }
        }
    }
}
