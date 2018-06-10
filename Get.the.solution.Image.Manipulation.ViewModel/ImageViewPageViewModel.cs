using Get.the.solution.Image.Manipulation.Contract;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Get.the.solution.Image.Manipulation.ViewModel
{
    public class ImageViewPageViewModel : BindableBase
    {
        protected readonly INavigation _NavigationService;
        protected readonly IResourceService _ResourceLoader;
        protected readonly ILoggerService _LoggerService;
        protected readonly IApplicationService _applicationService;
        protected readonly IImageFileService _imageFileService;
        public ImageViewPageViewModel(IImageFileService imageFileService, INavigation navigationService, IResourceService resourceLoader,
            ILoggerService loggerService, IApplicationService applicationService, ObservableCollection<ImageFile> selectedFiles)
        {
            try
            {
                _LoggerService = loggerService;
                _NavigationService = navigationService;
                _ResourceLoader = resourceLoader;
                _applicationService = applicationService;
                _imageFileService = imageFileService;
                SelectedImage = selectedFiles.FirstOrDefault();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                LoadImagesAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
            catch (Exception e)
            {
                _LoggerService?.LogException(nameof(ImageViewPageViewModel), e);
            }
        }
        public async System.Threading.Tasks.Task LoadImagesAsync()
        {
            if(SelectedImage!=null)
            {
                ImageFiles = await _imageFileService.GetFilesFromFolderAsync(SelectedImage.Path);
                ImageFiles = ImageFiles.OrderBy(a => a.Path).ToList();
            }
        }

        private ICommand _OnKeyDownCommand;
        public ICommand OnKeyDownCommand => _OnKeyDownCommand ?? (_OnKeyDownCommand = new DelegateCommand<object>(OnOnKeyDownCommandAsync));

        protected async void OnOnKeyDownCommandAsync(object param)
        {
            if (param != null)
            {
                try
                {
                    ImageFile img = ImageFiles?.FirstOrDefault(a => a.Path == SelectedImage.Path);
                    if (img != null && "left".Equals($"{param}".ToLower()) || "right".Equals($"{param}".ToLower()))
                    {
                        int index = ImageFiles.IndexOf(img);
                        if ("left".Equals($"{param}".ToLower()))
                        {
                            index--;
                        }
                        else if ("right".Equals($"{param}".ToLower()))
                        {
                            index++;
                        }
                        if (index <= 0)
                        {
                            index = ImageFiles.Count - 1;
                        }
                        else if (index >= ImageFiles.Count)
                        {
                            index = 0;
                        }
                        SelectedImage = ImageFiles.ElementAt(index);
                        if (SelectedImage.Stream == null)
                        {
                            SelectedImage = await _imageFileService.LoadImageFileAsync(SelectedImage.Path);
                        }
                    }
                    else if(_applicationService.CtrlPressed(param))
                    {
                        SelectedImage = (await _imageFileService.PickMultipleFilesAsync())?.FirstOrDefault();
                        await LoadImagesAsync();
                    }
                }
                catch (Exception e)
                {
                    _LoggerService.LogException(nameof(OnOnKeyDownCommandAsync), e);
                }
            }
        }
        private IList<ImageFile> _ImageFiles;
        public IList<ImageFile> ImageFiles
        {
            get { return _ImageFiles; }
            set { SetProperty(ref _ImageFiles, value, nameof(ImageFiles)); }
        }


        private ImageFile _SelectedImage;
        public ImageFile SelectedImage
        {
            get { return _SelectedImage; }
            set { SetProperty(ref _SelectedImage, value, nameof(SelectedImage)); }
        }

    }
}
