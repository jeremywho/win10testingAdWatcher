using System.ComponentModel;
using System.Runtime.CompilerServices;
using TestingBluetoothNativeWin10.Annotations;

namespace TestingBluetoothNativeWin10
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool AssignProperty<TProperty>(ref TProperty field, TProperty value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
                return false;

            field = value;

            // ReSharper disable once ExplicitCallerInfoArgument
            OnPropertyChanged(propertyName);

            return true;
        }
    }
}