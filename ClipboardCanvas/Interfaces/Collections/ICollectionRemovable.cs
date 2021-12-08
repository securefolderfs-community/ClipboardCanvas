using ClipboardCanvas.EventArguments.Collections;
using System;

namespace ClipboardCanvas.Interfaces.Collections
{
    public interface ICollectionRemovable
    {
        event EventHandler<RemoveCollectionRequestedEventArgs> OnRemoveCollectionRequestedEvent;

        void RemoveCollection();
    }
}
