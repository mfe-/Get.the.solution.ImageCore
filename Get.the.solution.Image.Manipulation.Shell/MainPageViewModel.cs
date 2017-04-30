using Get.the.solution.UWP.XAML;
using Microsoft.Services.Store.Engagement;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Windows.AppModel;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Get.the.solution.Image.Manipulation.Shell
{
    public class MainPageViewModel : BindableBase
    {
        protected readonly INavigationService _NavigationService;
        protected readonly IResourceLoader _ResourceLoader;
        protected readonly StoreServicesCustomEventLogger _Logger;
        public MainPageViewModel(INavigationService navigationService, IResourceLoader resourceLoader)
        {
            _ResourceLoader = resourceLoader;
            _NavigationService = navigationService;
            _Logger = StoreServicesCustomEventLogger.GetDefault();
            Items = new List<MenuItem>()
            {
                new MenuItem () { Name = resourceLoader.GetString("AppName"), Icon = Symbol.Folder, PageType = typeof(ResizePage) },
                new MenuItem () { Name = resourceLoader.GetString("Help") , Icon = Symbol.Help, PageType = typeof(HelpPage) },
                new MenuItem () { Name = resourceLoader.GetString("Contact"),Icon = Symbol.Contact, PageType = typeof(AboutPage) }
            };
            SelectedMenuItem = Items[0];
            NavigateToCommand = new DelegateCommand<object>(OnNavigateToCommand);

            _NavigationService.Navigate(Items[0].PageType.AssemblyQualifiedName, null);
        }


        public DelegateCommand<object> NavigateToCommand { get; set; }

        protected void OnNavigateToCommand(object param)
        {
            MenuItem clicked = (param as ItemClickEventArgs)?.ClickedItem as MenuItem;

            if (clicked != null)
            {
                _Logger.Log($"NavigateToCommand {clicked.Name}");
                _NavigationService.Navigate(clicked.PageType.AssemblyQualifiedName, null);
            }

        }

        private MenuItem _SelectedMenuItem;

        public MenuItem SelectedMenuItem
        {
            get { return _SelectedMenuItem; }
            set { SetProperty(ref _SelectedMenuItem, value, nameof(SelectedMenuItem)); }
        }


        private List<MenuItem> _Items;

        public List<MenuItem> Items
        {
            get { return _Items; }
            set { SetProperty(ref _Items, value, nameof(Items)); }
        }
    }
}
