using Microsoft.Toolkit.Mvvm.ComponentModel;
using Windows.UI.Xaml;

namespace ClipboardCanvas.AttachedProperties
{
    public class BaseViewModelAttachedProperty<TViewModel, TTarget> : BaseAttachedProperty<BaseViewModelAttachedProperty<TViewModel, TTarget>, TViewModel, TTarget>
        where TViewModel : ObservableObject
        where TTarget : FrameworkElement
    {
        public override void OnValueChanged(TTarget sender, TViewModel newValue)
        {
            sender.DataContext = newValue;
        }
    }
}
