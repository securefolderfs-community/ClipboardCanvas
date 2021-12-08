using ClipboardCanvas.EventArguments.Collections;
using System;
using System.Threading.Tasks;

namespace ClipboardCanvas.Interfaces.Collections
{
    public interface ICollectionNameEditable
    {
        event EventHandler<CheckRenameCollectionRequestedEventArgs> OnCheckRenameCollectionRequestedEvent;

        void StartRename();

        void CancelRename();

        Task<bool> ConfirmRename();
    }
}
