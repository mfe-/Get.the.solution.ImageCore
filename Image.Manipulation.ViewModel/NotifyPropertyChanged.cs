using Get.the.solution.Image.Manipulation.Contract;
using System.Collections.Generic;
using System.ComponentModel;

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
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetProperty<T>(ref T field, T value, string propertyName)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            _loggerService?.LogEvent(propertyName, value.ToString());
            return true;
        }
    }
}
