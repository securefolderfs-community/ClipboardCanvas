using System;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.EventArguments.CollectionPreview;
using ClipboardCanvas.ViewModels;

namespace ClipboardCanvas.Models
{
    public interface ICollectionPreviewPageModel : IDisposable
    {
        event EventHandler<CanvasPreviewSelectedItemChangedEventArgs> OnCanvasPreviewSelectedItemChangedEvent;

        CollectionPreviewItemViewModel SelectedItem { get; }
    }
}
