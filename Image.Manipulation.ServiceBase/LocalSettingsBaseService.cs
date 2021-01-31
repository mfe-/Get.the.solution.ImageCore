using Get.the.solution.Image.Manipulation.Contract;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Get.the.solution.Image.Manipulation.ServiceBase
{
    public abstract class LocalSettingsBaseService<TSetting> : ILocalSettings<TSetting>, INotifyPropertyChanged
    {
        protected ILoggerService _loggerService;
        protected readonly string _xmlFilePath;
        private readonly Func<TSetting> _createDefaultTSettingFunc;

        protected LocalSettingsBaseService(string xmlFilePath, Func<TSetting> createDefaultTSettingFunc, ILoggerService loggerService)
        {
            _xmlFilePath = xmlFilePath;
            _createDefaultTSettingFunc = createDefaultTSettingFunc;
            _loggerService = loggerService;
        }
        public abstract Task<Stream> GetStreamAsync(string path);

        /// <summary>
        /// Load user settings which contains the user drugs
        /// </summary>
        /// <seealso cref="https://docs.microsoft.com/en-us/xamarin/essentials/preferences?tabs=android"/>
        public async Task LoadSettingsAsync()
        {
            string xml;
            Stream stream1 = await GetStreamAsync(_xmlFilePath);
            using (StreamReader file = new StreamReader(stream1))
            {
                xml = file.ReadToEnd();
            }
            //stream1 will be disposed by streamreader

            if (!String.IsNullOrEmpty(xml))
            {
                using (Stream stream = new MemoryStream())
                {
                    byte[] data = Encoding.UTF8.GetBytes(xml);
                    stream.Write(data, 0, data.Length);
                    stream.Position = 0;
                    DataContractSerializer deserializer = new DataContractSerializer(typeof(TSetting));
                    var setting = deserializer.ReadObject(stream);
                    if (setting is TSetting setting1)
                    {
                        Settings = setting1;
                    }
                    else
                    {
                        throw new InvalidOperationException($"We expected the generic type of {nameof(TSetting)}");
                    }
                }
            }
            else
            {
                Settings = _createDefaultTSettingFunc.Invoke();
            }
        }

        private TSetting _Settings;
        public TSetting Settings
        {
            get
            {
                return _Settings;
            }
            set
            {
                SetProperty(ref _Settings, value, nameof(Settings));
                if (_Settings is INotifyPropertyChanged notifyPropertyChanged)
                {
                    notifyPropertyChanged.PropertyChanged -= NotifyPropertyChanged_PropertyChanged;
                    notifyPropertyChanged.PropertyChanged += NotifyPropertyChanged_PropertyChanged;
                }
            }
        }

        private void NotifyPropertyChanged_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SaveSettingsAsync().ContinueWith(a =>
            {
                if (a.Exception != null)
                {
                    _loggerService.LogException(a.Exception);
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Saves the user drugs to the user settings
        /// </summary>
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);
        public async Task SaveSettingsAsync()
        {
            try
            {
                await semaphoreSlim.WaitAsync();
                string serializedXml;
                MemoryStream memStm = new MemoryStream();

                var serializer = new DataContractSerializer(typeof(TSetting));
                serializer.WriteObject(memStm, Settings);

                memStm.Seek(0, SeekOrigin.Begin);

                using (var streamReader = new StreamReader(memStm))
                {
                    serializedXml = streamReader.ReadToEnd();
                }
                Stream stream = await GetStreamAsync(_xmlFilePath);
                using (StreamWriter file = new StreamWriter(stream))
                {
                    file.WriteLine(serializedXml);
                }
                //stream will be disposed by streamwriter

            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetProperty<TProperty>(ref TProperty field, TProperty value, string propertyName)
        {
            if (EqualityComparer<TProperty>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public abstract Task ResetSettingsAsync();
    }
}
