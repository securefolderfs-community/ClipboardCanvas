using System;

using ClipboardCanvas.EventArguments.CollectionPreview;
using ClipboardCanvas.ViewModels;

namespace ClipboardCanvas.Models
{
    public interface ICollectionPreviewPageModel : IDisposable
    {
        event EventHandler<CanvasPreviewSelectedItemChangedEventArgs> OnCanvasPreviewSelectedItemChangedEvent;

        event EventHandler<CanvasPreviewPasteRequestedEventArgs> OnCanvasPreviewPasteRequestedEvent;

        CollectionPreviewItemViewModel SelectedItem { get; }
    }
}
