using Get.the.solution.Image.Manipulation.Contract;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace Get.the.solution.Image.Manipulation.ViewModel
{
    public class NotifyPropertyChanged : INotifyPropertyChanged
    {
        private readonly ILoggerService _loggerService;

        public NotifyPropertyChanged()
        {

        }
        public NotifyPropertyChanged(ILoggerService loggerService)
        {
            _loggerService = loggerService;
        }
        private readonly SynchronizationContext synchronizationContext = SynchronizationContext.Current;
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            if (synchronizationContext != SynchronizationContext.Current)
            {
                synchronizationContext.Post(
                    propname => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propname.ToString())), propertyName);
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        protected virtual bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            _loggerService?.LogEvent(propertyName, value?.ToString() ?? "NULL");
            return true;
        }
    }
}
