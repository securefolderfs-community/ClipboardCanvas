namespace ClipboardCanvas.ModelViews
{
    public interface ITextCanvasControlView
    {
        bool IsTextSelected { get; }

        int SelectedTextLength { get; }

        void TextSelectAll();

        void CopySelectedText();
    }
}
