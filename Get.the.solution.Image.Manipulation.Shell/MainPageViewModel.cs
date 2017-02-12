using Get.the.solution.UWP.XAML;
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
    public class  MainPageViewModel  : BindableBase
    {
        protected readonly INavigationService _NavigationService;
        protected readonly IResourceLoader _ResourceLoader;

        public MainPageViewModel(INavigationService navigationService, IResourceLoader resourceLoader)
        {
            _ResourceLoader = resourceLoader;
            _NavigationService = navigationService;

            Items = new List<MenuItem>()
            {
                new MenuItem () { Name = "Resize Image", Icon = Symbol.Folder, PageType = typeof(ResizePage) },
                new MenuItem () { Name = "About" , Icon = Symbol.Help, PageType = typeof(HelpPage) }
            };
            SelectedMenuItem = Items[0];
            NavigateToCommand = new DelegateCommand<MenuItem>(OnNavigateToCommand);

            _NavigationService.Navigate(Items[0].PageType.AssemblyQualifiedName, null);
        }


        public DelegateCommand<MenuItem> NavigateToCommand { get; set; }

        protected void OnNavigateToCommand(MenuItem param)
        {
            if (SelectedMenuItem.PageType == typeof(MainPage))
            {
                _NavigationService.Navigate(Items[1].PageType.AssemblyQualifiedName, null);
            }
            else
            {
                _NavigationService.Navigate(Items[0].PageType.AssemblyQualifiedName, null);
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
