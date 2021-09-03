using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ClipboardCanvas.AttachedProperties
{
    public class TextBoxFocusAttachedProperty : BaseGenericAttachedProperty<TextBoxFocusAttachedProperty, bool>
    {
        protected override void OnValueChanged(DependencyObject sender, bool e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            // Focus
            if (e)
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
