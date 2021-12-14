using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ClipboardCanvas.AttachedProperties
{
    public class TextBoxFocusAttachedProperty : BaseGenericAttachedProperty<TextBoxFocusAttachedProperty, bool>
    {
        protected override void OnValueChanged(DependencyObject sender, bool newValue)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            // Focus
            if (newValue)
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
