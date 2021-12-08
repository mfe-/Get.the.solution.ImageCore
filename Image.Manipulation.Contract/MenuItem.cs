using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Get.the.solution.Image.Manipulation.Contract
{
    public class MenuItem : INotifyPropertyChanged
    {
        private String _Name;
        public String Name
        {
            get { return _Name; }
            set { SetProperty(ref _Name, value, nameof(Name)); }
        }
        private String _Icon;
        public String Icon
        {
            get { return _Icon; }
            set { SetProperty(ref _Icon, value, nameof(Icon)); }
        }


        private Type _PageType;
        public Type PageType
        {
            get { return _PageType; }
            set { SetProperty(ref _PageType, value, nameof(PageType)); }
        }

        // boiler-plate
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
        public override string ToString()
        {
            return $"{nameof(MenuItem)} Name={Name}";
        }
    }
}
