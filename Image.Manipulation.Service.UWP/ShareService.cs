using Get.the.solution.Image.Contract;
using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Storage;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class ShareService : IShareService
    {
        protected ILoggerService _loggerService;
        protected DataTransferManager _DataTransferManager;
        protected readonly IResourceService _ResourceLoader;
        protected readonly IImageFileService _imageFileService;
        public ShareService(ILoggerService loggerService, IResourceService resourceService, IImageFileService imageFileService)
        {
            _loggerService = loggerService;
            _ResourceLoader = resourceService;
            _imageFileService = imageFileService;
        }
        protected List<ImageFile> LocalCachedResizedImages = new List<ImageFile>();
        protected Action ShareCompleteAction;
        public TaskCompletionSource<IReadOnlyList<ImageFile>> PickImageTaskCompletionSource { set; get; }
        public async Task StartShareAsync(IList<ImageFile> imageFiles, Func<Action<ImageFile, String>, Task<bool>> viewModelReiszeImageFunc, Action shareCompleteAction = null)
        {
            try
            {
                //init
                SharingProcess = true;
                LocalCachedResizedImages = new List<ImageFile>();
                ShareCompleteAction = shareCompleteAction;
                Action<ImageFile, string> ProcessImageAction = new Action<ImageFile, string>((resizedImage, FileName) =>
                {
                    LocalCachedResizedImages.Add(resizedImage);
                });
                PickImageTaskCompletionSource = new TaskCompletionSource<IReadOnlyList<ImageFile>>();
                //call bool Result = await ResizeImages(ImageAction.Process, ProcessImageAction); from viewmodel
                bool Result = await viewModelReiszeImageFunc(ProcessImageAction);
                _DataTransferManager = DataTransferManager.GetForCurrentView();
                _DataTransferManager.DataRequested += DataTransferManager_DataRequested;
                DataTransferManager.ShowShareUI();
                await PickImageTaskCompletionSource.Task;
            }
            catch(Exception e)
            {
                _loggerService.LogException(nameof(StartShareAsync), e);
            }
            finally
            {
                SharingProcess = false;
            }
        }
        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest Request = args.Request;
            // Because we are making async calls in the DataRequested event handler, we need to get the deferral first.
            DataRequestDeferral deferral = Request.GetDeferral();
            try
            {
                if (LocalCachedResizedImages != null && LocalCachedResizedImages.Count != 0)
                {
                    //our DataPackage we want to share
                    DataPackage Package = new DataPackage();
                    Package.RequestedOperation = DataPackageOperation.Copy;
                    Package.Properties.Title = _ResourceLoader.GetString("AppName");
                    foreach (var file in LocalCachedResizedImages)
                    {
                        Package.Properties.Description = $"{Package.Properties.Description} {file.Name}";
                        //Package.Properties.Description = $"{Package.Properties.Title } {GenerateResizedFileName(File)}";
                    }
                    //Package.SetDataProvider(StandardDataFormats.StorageItems, Share_DataProvider);
                    foreach (String Extension in _imageFileService.FileTypeFilter)
                    {
                        Package.Properties.FileTypes.Add(Extension);
                    }
                    List<IStorageFile> storageFiles = new List<IStorageFile>();
                    foreach (ImageFile storageFile in LocalCachedResizedImages)
                    {
                        storageFiles.Add(storageFile.Tag as IStorageFile);
                    }
                    Package.SetStorageItems(storageFiles);
                    Request.Data = Package;
                    SharingProcess = false;
                }
                else
                {
                    args.Request.FailWithDisplayText("Nothing to share");
                    SharingProcess = false;
                }
            }
            catch (Exception e)
            {
                _loggerService?.LogException(nameof(DataTransferManager_DataRequested), e);
                SharingProcess = false;
            }
            finally
            {
                deferral.Complete();
                ShareCompleteAction?.Invoke();
            }
            _loggerService?.LogEvent(nameof(DataTransferManager_DataRequested));
        }
        private bool _SharingProcess;
        /// <summary>
        /// Indicates whether the app is in share process
        /// </summary>
        public bool SharingProcess
        {
            get { return _SharingProcess; }
            set { SetProperty(ref _SharingProcess, value, nameof(SharingProcess)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private ShareOperation _shareOperation = null;
        public void StartShareTargetOperation(object shareOperation)
        {
            if(shareOperation is ShareOperation shareOperation1)
            {
                _shareOperation = shareOperation1;
                _shareOperation.ReportDataRetrieved();
            }
            else
            {
                throw new ArgumentException($"Parameter {nameof(shareOperation)} must be of type {nameof(ShareOperation)}");
            }

        }
        public void EndShareTargetOperation()
        {
            _shareOperation?.ReportCompleted();
        }

    }
}
