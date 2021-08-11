using System;
using Windows.Storage.Streams;
using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.EventArguments.InfiniteCanvasEventArgs
{
    public class InfiniteCanvasSaveRequestedEventArgs : EventArgs
    {
        public readonly IRandomAccessStream canvasImageStream;

        public InfiniteCanvasSaveRequestedEventArgs(IRandomAccessStream canvasImageStream)
        {
            this.canvasImageStream = canvasImageStream;
        }
    }

    public class InfiniteCanvasItemRemovalRequestedEventArgs : EventArgs
    {
        public readonly InteractableCanvasControlItemViewModel itemToRemove;

        public InfiniteCanvasItemRemovalRequestedEventArgs(InteractableCanvasControlItemViewModel itemToRemove)
        {
            this.itemToRemove = itemToRemove;
        }
    }
}
