using System;

namespace ClipboardCanvas.ModelViews
{
    public interface IWebViewCanvasControlView : IDisposable
    {
        void NavigateToHtml(string html);

        void NavigateToSource(string source);
    }
}
