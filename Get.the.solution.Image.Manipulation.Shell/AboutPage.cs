using Get.the.solution.UWP.XAML;
using Prism.Mvvm;
using Prism.Windows.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;

namespace Get.the.solution.Image.Manipulation.Shell
{
    public class AboutPageViewModel : BindableBase
    {
        protected readonly INavigationService _NavigationService;
        public AboutPageViewModel(INavigationService navigationService)
        {

        }


        public String AppVersion
        {
            get
            {
                return AppHelper.GetAppVersion();
            }
        }

        public String LocalCacheFolder
        {
            get { return ApplicationData.Current.LocalCacheFolder.Path; }
        }
    }
}
