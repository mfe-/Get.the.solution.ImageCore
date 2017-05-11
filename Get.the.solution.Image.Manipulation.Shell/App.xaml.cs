using Prism.Mvvm;
using Prism.Unity.Windows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Prism.Windows.Navigation;
using Prism.Windows.AppModel;
using Windows.ApplicationModel.Resources;
using Microsoft.Practices.Unity;
using System.Reflection;
using System.Globalization;
using Windows.UI.ViewManagement;
using Windows.Storage;
using Windows.Foundation.Metadata;
using System.Collections.ObjectModel;
using Windows.Globalization;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;

namespace Get.the.solution.Image.Manipulation.Shell
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : PrismUnityApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
#if DEBUG
            //ApplicationLanguages.PrimaryLanguageOverride = "ru";
#endif
        }
        /// <summary>
        /// Removes the StatusBar from the Windows Mobile Device
        /// </summary>
        public void RemoveStatusBar()
        {
            //draw UI to the edge of the screen
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar.GetForCurrentView().HideAsync();
            }
        }
        /// <summary>
        /// Creates the shell of the app. Set the default page of the app.
        /// </summary>
        /// <param name="rootFrame">The root frame.</param>
        /// <returns>The initialized root frame.</returns>
        protected override UIElement CreateShell(Frame rootFrame)
        {
            var shell = Container.Resolve<MainPage>();
            shell.SetContentFrame(rootFrame);
            return shell;
        }
        /// <summary>
        /// Used for setting up the list of known types for the SessionStateService, using the RegisterKnownType method.
        /// </summary>
        protected override void OnRegisterKnownTypesForSerialization()
        {
            // Set up the list of known types for the SuspensionManager
        }
        protected override void OnFileActivated(FileActivatedEventArgs args)
        {
            base.OnFileActivated(args);
            //init prism bootstrap
            OnActivated(args);
        }
        protected override void OnShareTargetActivated(ShareTargetActivatedEventArgs args)
        {
            base.OnShareTargetActivated(args);
            //init prism bootstrap
            OnActivated(args);
        }

        protected override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            UnhandledException += App_UnhandledException;

            Container.RegisterInstance<INavigationService>(NavigationService);
            Container.RegisterInstance<ISessionStateService>(SessionStateService);

            ResourceLoaderAdapter resourceLoader = new ResourceLoaderAdapter(ResourceLoader.GetForCurrentView("Get.the.solution.Image.Manipulation.Resources/Resources"));
            Container.RegisterInstance<IResourceLoader>(resourceLoader);

            if ((args as LaunchActivatedEventArgs)?.PrelaunchActivated == false)
            {
                SetViewModelLocationProvider();
            }
            if (args as FileActivatedEventArgs != null || args as ShareTargetActivatedEventArgs != null)
            {
                IReadOnlyList<IStorageItem> Files = null;
                if (args.GetType().Equals(typeof(FileActivatedEventArgs)))
                {
                    FileActivatedEventArgs fileargs = args as FileActivatedEventArgs;
                    Files = fileargs.Files;

                    Container.RegisterInstance(TimeSpan.MinValue);
                }
                else if (args.GetType().Equals(typeof(ShareTargetActivatedEventArgs)))
                {
                    ShareTargetActivatedEventArgs fileargs = args as ShareTargetActivatedEventArgs;


                    if (fileargs.ShareOperation.Data.Contains(StandardDataFormats.StorageItems))
                    {
                        Files = fileargs.ShareOperation.Data.GetStorageItemsAsync().AsTask().GetAwaiter().GetResult();
                    }
                    else
                    {
                        Files = Array.Empty<IStorageItem>();
                        //fileargs("ShareNoDataRetrieved");


                    }
                    Container.RegisterInstance<ShareOperation>(fileargs.ShareOperation);
                    Container.RegisterInstance(TimeSpan.MaxValue);
                }

                ObservableCollection<IStorageFile> storagefiles = new ObservableCollection<IStorageFile>();

                foreach (var item in Files)
                {

                    if (item is IStorageFile)
                    {
                        if (item is StorageFile)
                        {
                            storagefiles.Add(item as StorageFile);
                        }
                    }
                    //if (item is IStorageFolder && item is StorageFolder)
                    //{
                    //    OpenFolder(item as StorageFolder);
                    //}
                }

                Container.RegisterInstance<ObservableCollection<IStorageFile>>(storagefiles);
            }
            else
            {
                Container.RegisterInstance<ObservableCollection<IStorageFile>>(new ObservableCollection<IStorageFile>());
                Container.RegisterInstance(TimeSpan.MinValue);
            }

            RemoveStatusBar();

            return base.OnInitializeAsync(args);
        }

        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

        }

        public void SetViewModelLocationProvider()
        {
            //override default behaviour for discovering viewmodels
            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver((viewType) =>
            {
                var viewName = viewType.FullName;
                Assembly viewassembly = viewType.GetTypeInfo().Assembly;
                var viewAssemblyName = viewassembly.FullName;
                var suffix = viewName.Replace("Page", "PageViewModel");

                Type type = viewassembly.GetType(suffix);
                return type;

            });
            //override default behaviour for returning the proper viewmodel instance
            ViewModelLocationProvider.SetDefaultViewModelFactory((type) =>
            {
                //resolve type
                object instance = Container.Resolve(type);
                //check whether instance is registered
                if (Container.IsRegistered(type))
                {
                    return instance;
                }
                else
                {
                    //register instance as singleton
                    if (Container.Registrations.Count(a => a.Name == type.Name) == 0)
                    {
                        Container.RegisterInstance(type, instance, new ContainerControlledLifetimeManager());
                    }
                    return instance;
                }
            });
        }
        /// <summary>
        /// Override this method with logic that will be performed after the application is initialized. For example, navigating to the application's home page.
        /// </summary>
        /// <param name="args">The <see cref="LaunchActivatedEventArgs"/> instance containing the event data.</param>
        protected override Task OnLaunchApplicationAsync(LaunchActivatedEventArgs args)
        {
            if (args.PrelaunchActivated == true)
            {
                SetViewModelLocationProvider();
            }
            return Task.FromResult<object>(null);
        }
        /// <summary>
        /// Gets the type of the requested page based on a page token.
        /// </summary>
        /// <remarks>
        /// Prisms default behaviour is to get the view type in the App class assembly. We have override the function and expect that the pagetoken contains the view class AssemblyQualifiedName.
        /// </remarks>
        /// <param name="pageToken">The page token which represents the typeof().AssemblyQualifiedName</param>
        /// <returns>The type of the page which corresponds to the specified token.</returns>
        protected override Type GetPageType(string pageToken)
        {
            var viewType = Type.GetType(pageToken);

            if (viewType == null)
            {
                var resourceLoader = ResourceLoader.GetForCurrentView("/Prism.Windows/Resources/");
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, resourceLoader.GetString("DefaultPageTypeLookupErrorMessage"), pageToken, this.GetType().Namespace + ".Views"),
                    nameof(pageToken));
            }

            return viewType;
        }
    }
}
