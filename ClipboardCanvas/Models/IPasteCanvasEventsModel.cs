using System;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.EventArguments.CanvasControl;

namespace ClipboardCanvas.Models
{
    public interface IPasteCanvasEventsModel
    {
        event EventHandler<OpenOpenNewCanvasRequestedEventArgs> OnOpenNewCanvasRequestedEvent;

        event EventHandler<ContentLoadedEventArgs> OnContentLoadedEvent;

        event EventHandler<PasteRequestedEventArgs> OnPasteRequestedEvent;

        event EventHandler<FileCreatedEventArgs> OnFileCreatedEvent;

        event EventHandler<FileModifiedEventArgs> OnFileModifiedEvent;

        event EventHandler<FileDeletedEventArgs> OnFileDeletedEvent;

        event EventHandler<ErrorOccurredEventArgs> OnErrorOccurredEvent;

        event EventHandler<ProgressReportedEventArgs> OnProgressReportedEvent;
    }
}
