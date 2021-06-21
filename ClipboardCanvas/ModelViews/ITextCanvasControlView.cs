namespace ClipboardCanvas.ModelViews
{
    public interface ITextCanvasControlView
    {
        bool IsTextSelected { get; }

        void TextSelectAll();

        void CopySelectedText();
    }
}
