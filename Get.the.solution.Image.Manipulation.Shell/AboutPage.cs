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
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;
                return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }

        public String LocalCacheFolder
        {
            get { return ApplicationData.Current.LocalCacheFolder.Path; }
        }
    }
}
