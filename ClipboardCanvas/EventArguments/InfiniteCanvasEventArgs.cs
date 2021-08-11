using System;
using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.EventArguments.InfiniteCanvasEventArgs
{
    public class InfiniteCanvasSaveRequestedEventArgs : EventArgs
    {
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
