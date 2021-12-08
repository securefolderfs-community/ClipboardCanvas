using System;
using Windows.Storage.Streams;

using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.EventArguments.InfiniteCanvasEventArgs
{
    public class InfiniteCanvasSaveRequestedEventArgs : EventArgs
    {
        public readonly IBuffer canvasImageBuffer;

        public readonly uint pixelWidth;

        public readonly uint pixelHeight;

        public InfiniteCanvasSaveRequestedEventArgs(IBuffer canvasImageBuffer, uint pixelWidth, uint pixelHeight)
        {
            this.canvasImageBuffer = canvasImageBuffer;
            this.pixelWidth = pixelWidth;
            this.pixelHeight = pixelHeight;
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
