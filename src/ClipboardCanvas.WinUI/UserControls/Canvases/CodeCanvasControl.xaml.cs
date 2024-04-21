using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClipboardCanvas.WinUI.UserControls.Canvases
{
    public sealed partial class CodeCanvasControl : UserControl
    {
        public CodeCanvasControl()
        {
            InitializeComponent();
        }

        private void CodeEditor_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        public bool IsReadOnly
        {
            get => (bool)GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(CodeCanvasControl), new PropertyMetadata(true, (s, e) => 
            {
                if (s is not CodeCanvasControl control)
                    return;

                control.CodeEditor.Editor.ReadOnly = (bool)e.NewValue;
            }));

        public string? Text
        {
            get => CodeEditor.Editor.GetText(CodeEditor.Editor.Length);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(CodeCanvasControl), new PropertyMetadata(null,
                (s, e) =>
                {
                    if (s is not CodeCanvasControl control)
                        return;

                    var newText = (string?)e.NewValue ?? string.Empty;
                    control.CodeEditor.Editor.SetText(newText);
                }));

        public string? Language
        {
            get => (string?)GetValue(LanguageProperty);
            set => SetValue(LanguageProperty, value);
        }
        public static readonly DependencyProperty LanguageProperty =
            DependencyProperty.Register(nameof(Language), typeof(string), typeof(CodeCanvasControl), new PropertyMetadata(null,
                (s, e) =>
                {
                    if (s is not CodeCanvasControl control)
                        return;

                    if (e.NewValue is not string extension)
                        return;

                    var highlightingLanguage = extension switch
                    {
                        ".cpp" => "cpp",
                        ".cs" => "csharp",
                        ".js" or ".ts" or ".svelte" => "javascript",
                        ".html" or ".htm" => "html",
                        ".json" => "json",
                        ".xml" => "xml",

                        _ => null
                    };

                    // TODO: Apply custom styles based on the extension
                    // For now, set to plaintext
                    control.CodeEditor.HighlightingLanguage = highlightingLanguage ?? "plaintext";
                }));
    }
}
