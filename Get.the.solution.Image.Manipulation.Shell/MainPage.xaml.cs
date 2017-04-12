using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Get.the.solution.Image.Manipulation.Shell
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Size _PreferredSize = new Size(515, 410);
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = _PreferredSize;
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            String lang = Windows.System.UserProfile.GlobalizationPreferences.Languages.FirstOrDefault();
            if (lang.Contains("pt"))
            {
                _PreferredSize = new Size(535, 410);
            }
            ApplicationView.GetForCurrentView().SetPreferredMinSize(_PreferredSize);
        }
        public void SetContentFrame(Frame frame)
        {
            rootFrame.Content = frame;
        }
    }
}
