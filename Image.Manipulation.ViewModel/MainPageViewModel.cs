using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Get.the.solution.Image.Manipulation.ViewModel.ResizeImage
{
    public class MainPageViewModel : NotifyPropertyChanged
    {
        protected readonly INavigationService _NavigationService;
        protected readonly IResourceService _ResourceLoader;
        protected readonly ILoggerService _LoggerService;
        protected readonly IApplicationService _applicationService;
        protected readonly ILocalSettings<ResizeSettings> _localSettings;

        public MainPageViewModel(INavigationService navigationService, IResourceService resourceLoader,
            ILoggerService loggerService, IApplicationService applicationService, ILocalSettings<ResizeSettings> localSettings) : base(loggerService)
        {
            _ResourceLoader = resourceLoader;
            _NavigationService = navigationService;
            _LoggerService = loggerService;
            _applicationService = applicationService;
            _localSettings = localSettings;
            Items = new List<MenuItem>()
            {
                //use symbols from namespace Windows.UI.Xaml.Controls.Symbol
                new MenuItem () { Name = resourceLoader.GetString("AppName"), Icon = "Folder", PageType = typeof(ResizePageViewModel) },
                new MenuItem () { Name = resourceLoader.GetString("Help") , Icon = "Help", PageType = typeof(HelpPageViewModel) },
                new MenuItem () { Name = resourceLoader.GetString("Contact"),Icon = "Contact", PageType = typeof(AboutPageViewModel) },
                new MenuItem () { Name = resourceLoader.GetString("Setting"),Icon = "Setting", PageType = typeof(SettingsPageViewModel) },

            };
            SelectedMenuItem = Items.FirstOrDefault();
            NavigateToCommand = new DelegateCommand<MenuItem>(OnNavigateToCommand);
            _NavigationService.Navigate(SelectedMenuItem.PageType, null);
        }


        public DelegateCommand<MenuItem> NavigateToCommand { get; set; }

        protected void OnNavigateToCommand(MenuItem param)
        {
            try
            {
                MenuItem clicked = param;

                if (clicked != null)
                {
                    _LoggerService.LogEvent(nameof(NavigateToCommand),
                        new Dictionary<String, String>() { { nameof(clicked.Name), clicked.Name } });
                    _NavigationService.Navigate(clicked.PageType, null);
                }
            }
            catch (Exception e)
            {
                _LoggerService.LogException(nameof(OnNavigateToCommand), e);
            }

        }

        private MenuItem _SelectedMenuItem;

        public MenuItem SelectedMenuItem
        {
            get { return _SelectedMenuItem; }
            set
            {
                SetProperty(ref _SelectedMenuItem, value, nameof(SelectedMenuItem));
                NavigateToCommand?.Execute(SelectedMenuItem);
            }
        }


        private List<MenuItem> _Items;

        public List<MenuItem> Items
        {
            get { return _Items; }
            set { SetProperty(ref _Items, value, nameof(Items)); }
        }


        private bool _IsExpanded;
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set { SetProperty(ref _IsExpanded, value, nameof(IsExpanded)); }
        }

    }
}
