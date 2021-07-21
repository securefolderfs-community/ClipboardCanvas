using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ClipboardCanvas.AttachedProperties
{
    public class TextBoxFocusAttachedProperty : BaseGenericAttachedProperty<TextBoxFocusAttachedProperty, bool>
    {
        public override void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is not TextBox textBox || e.NewValue is not bool value)
            {
                return;
            }

            // Focus
            if (value)
            {
                textBox.Focus(FocusState.Programmatic);
                textBox.SelectAll();
            }
            else // Unfocus
            {
                // Cannot be unfocused
            }
        }
    }
}
