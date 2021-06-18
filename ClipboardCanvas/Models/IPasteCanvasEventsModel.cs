using System;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.EventArguments.CanvasControl;

namespace ClipboardCanvas.Models
{
    public interface IPasteCanvasEventsModel
    {
        event EventHandler<OpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        event EventHandler<ContentLoadedEventArgs> OnContentLoadedEvent;

        event EventHandler<ContentStartedLoadingEventArgs> OnContentStartedLoadingEvent;

        event EventHandler<PasteInitiatedEventArgs> OnPasteInitiatedEvent;

        event EventHandler<FileCreatedEventArgs> OnFileCreatedEvent;

        event EventHandler<FileModifiedEventArgs> OnFileModifiedEvent;

        event EventHandler<FileDeletedEventArgs> OnFileDeletedEvent;

        event EventHandler<ErrorOccurredEventArgs> OnErrorOccurredEvent;

        event EventHandler<ProgressReportedEventArgs> OnProgressReportedEvent;

        event EventHandler<TipTextUpdateRequestedEventArgs> OnTipTextUpdateRequestedEvent;
    }
}
