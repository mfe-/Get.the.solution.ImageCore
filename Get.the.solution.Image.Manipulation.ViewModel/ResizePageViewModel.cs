using Get.the.solution.Image.Manipulation.Contract;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;

namespace Get.the.solution.Image.Manipulation.ViewModel
{
    public class ResizePageViewModel : BindableBase
    {
        public ResizePageViewModel(ObservableCollection<Stream> selectedFiles, INavigation navigationService, TimeSpan sharing)
        {

        }
    }
}
