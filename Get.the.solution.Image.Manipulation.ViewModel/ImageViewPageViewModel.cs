using Get.the.solution.Image.Manipulation.Contract;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Linq;

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
                //LoadImage(_applicationService.ActivatedEventArgs);
            }
            catch (Exception e)
            {
                _LoggerService?.LogException(nameof(ImageViewPageViewModel), e);
            }
        }

        private ImageFile _SelectedImage;
        public ImageFile SelectedImage
        {
            get { return _SelectedImage; }
            set { SetProperty(ref _SelectedImage, value, nameof(SelectedImage)); }
        }

    }
}
