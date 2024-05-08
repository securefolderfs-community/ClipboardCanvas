using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls.Canvases
{
    public sealed partial class TextCanvasControl : UserControl
    {
        private int _surpressTextChanged;

        public TextCanvasControl()
        {
            InitializeComponent();
        }

        private void EditingBox_Loaded(object sender, RoutedEventArgs e)
        {
            // When the RichEditBox is loaded, the TextChanged event will be fired twice
            _surpressTextChanged = 2;

            EditingBox.Document.SetText(TextSetOptions.None, Text);
            EditingBox.Focus(FocusState.Programmatic);
        }

        private void EditingBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (_surpressTextChanged > 0)
            {
                _surpressTextChanged--;
                return;
            }

            WasAltered = true;
        }

        public string? Text
        {
            get => (string?)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(TextCanvasControl), new PropertyMetadata(null));

        public bool IsEditing
        {
            get => (bool)GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(TextCanvasControl), new PropertyMetadata(false));

        public bool WasAltered
        {
            get => (bool)GetValue(WasAlteredProperty);
            set => SetValue(WasAlteredProperty, value);
        }
        public static readonly DependencyProperty WasAlteredProperty =
            DependencyProperty.Register(nameof(WasAltered), typeof(bool), typeof(TextCanvasControl), new PropertyMetadata(false, (s, e) =>
            {
                if (s is not TextCanvasControl control)
                    return;

                if ((bool)e.OldValue && !(bool)e.NewValue && control.IsEditing)
                {
                    control.EditingBox.Document.GetText(TextGetOptions.None, out var text);
                    control.Text = text;
                }
            }));
    }
}
