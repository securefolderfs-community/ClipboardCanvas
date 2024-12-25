using System.Drawing;

namespace ClipboardCanvas.Sdk.ViewModels.Controls.Documents
{
    public sealed class OcrStringViewModel(string text, Rectangle rectangle)
    {
        public string Text { get; } = text;

        public Rectangle Rectangle { get; } = rectangle;
    }
}
