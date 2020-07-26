using Get.the.solution.Image.Manipulation.Contract;
using Get.the.solution.Image.Manipulation.ServiceBase;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

namespace Get.the.solution.Image.Manipulation.Service.UWP
{
    public class LocalSettingsService<TSetting> : LocalSettingsBaseService<TSetting>
    {
        public LocalSettingsService(string xmlFilePath, Func<TSetting> createDefaultTSettingFunc, ILoggerService loggerService)
            : base(xmlFilePath, createDefaultTSettingFunc, loggerService)
        {
        }
        public override async Task<Stream> GetStreamAsync(string path)
        {
            FileInfo fileInfo = new FileInfo(path);
            string filename = fileInfo.Name;
            IStorageItem storageItem = await ApplicationData.Current.LocalCacheFolder.TryGetItemAsync(filename);
            if (storageItem == null)
            {
                storageItem = await ApplicationData.Current.LocalCacheFolder.CreateFileAsync(fileInfo.Name);
                _loggerService?.LogEvent(nameof(GetStreamAsync), nameof(ApplicationData.Current.LocalCacheFolder.CreateFileAsync));
            }
            if (storageItem is StorageFile storageFile)
            {
                _loggerService?.LogEvent(nameof(GetStreamAsync), storageFile.Path);
                return await storageFile.OpenStreamForWriteAsync();
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
