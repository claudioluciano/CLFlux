using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CLFlux.Test.Mock
{
    public class MockState : IState, INotifyPropertyChanged
    {
        private int _value;

        public int Value
        {
            get { return _value; }
            set { _value = value; NotifyPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
