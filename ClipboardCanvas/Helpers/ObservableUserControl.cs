using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardCanvas.Helpers
{
    public class ObservableUserControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetValueDP(DependencyProperty dependencyProperty, object value, [CallerMemberName] string callerName = "")
        {
            SetValue(dependencyProperty, value);
            OnPropertyChanged(callerName);
        }

        protected void OnPropertyChanged([CallerMemberName] string callerName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(callerName));
        }
    }
}
