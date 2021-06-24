using System;

namespace ClipboardCanvas.EventArguments.CollectionPreviewItem
{
    public class CanvasRenameRequestedEventArgs : EventArgs
    {
        public readonly string newName;

        public CanvasRenameRequestedEventArgs(string newName)
        {
            this.newName = newName;
        }
    }
}
