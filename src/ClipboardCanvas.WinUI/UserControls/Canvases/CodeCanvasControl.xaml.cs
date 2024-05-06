using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using WinUIEditor;

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

        private void CodeEditor_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO: Memory leak + this event only accounts for when a character is added
            CodeEditor.Editor.CharAdded += Editor_CharAdded;
            CodeEditor.Editor.ReadOnly = !IsEditing;
        }

        private void Editor_CharAdded(Editor sender, CharAddedEventArgs args)
        {
            CodeEditor.Editor.CharAdded -= Editor_CharAdded;
            WasAltered = true;
        }

        public bool IsEditing
        {
            get => (bool)GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }
        public static readonly DependencyProperty IsEditingProperty =
            DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(CodeCanvasControl), new PropertyMetadata(false, (s, e) => 
            {
                if (s is not CodeCanvasControl control)
                    return;

                control.CodeEditor.Editor.ReadOnly = !(bool)e.NewValue;
            }));

        public bool WasAltered
        {
            get => (bool)GetValue(WasAlteredProperty);
            set => SetValue(WasAlteredProperty, value);
        }
        public static readonly DependencyProperty WasAlteredProperty =
            DependencyProperty.Register(nameof(WasAltered), typeof(bool), typeof(CodeCanvasControl), new PropertyMetadata(false, (s, e) =>
            {
                if (s is not CodeCanvasControl control)
                    return;

                if ((bool)e.OldValue && !(bool)e.NewValue && control.IsEditing)
                    control.Text = control.CodeEditor.Editor.GetText(control.CodeEditor.Editor.TextLength);
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
                    
                    // Clear entire undo history if the text has been reset
                    if (e.OldValue is null)
                        control.CodeEditor.Editor.EmptyUndoBuffer();
                }));

        public string? LanguageHint
        {
            get => (string?)GetValue(LanguageHintProperty);
            set => SetValue(LanguageHintProperty, value);
        }
        public static readonly DependencyProperty LanguageHintProperty =
            DependencyProperty.Register(nameof(LanguageHint), typeof(string), typeof(CodeCanvasControl), new PropertyMetadata(null,
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
                        ".js" or ".ts" or ".svelte" or ".jsx" => "javascript",
                        ".html" or ".htm" => "html",
                        ".json" => "json",
                        ".xml" or ".xaml" => "xml",

                        _ => null
                    };

                    // TODO: Apply custom styles based on the extension
                    // For now, set to plaintext
                    control.CodeEditor.HighlightingLanguage = highlightingLanguage ?? "plaintext";
                }));
    }
}
