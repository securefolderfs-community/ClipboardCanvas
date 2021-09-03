using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.ContentDataModels;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.ContextMenu;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.Models
{
    public interface IReadOnlyCanvasPreviewModel : IDisposable
    {
        event EventHandler<ContentStartedLoadingEventArgs> OnContentStartedLoadingEvent;

        event EventHandler<ContentLoadedEventArgs> OnContentLoadedEvent;

        event EventHandler<ErrorOccurredEventArgs> OnContentLoadFailedEvent;

        event EventHandler<FileDeletedEventArgs> OnFileDeletedEvent;

        event EventHandler<ErrorOccurredEventArgs> OnErrorOccurredEvent;

        event EventHandler<TipTextUpdateRequestedEventArgs> OnTipTextUpdateRequestedEvent;

        event EventHandler<ProgressReportedEventArgs> OnProgressReportedEvent;

        /// <summary>
        /// Determines whether canvas content has been loaded
        /// </summary>
        bool IsContentLoaded { get; }

        /// <summary>
        /// Context menu options available for the canvas
        /// </summary>
        ObservableCollection<BaseMenuFlyoutItemViewModel> ContextMenuItems { get; }

        /// <summary>
        /// Attempts to load existing data to display
        /// </summary>
        /// <param name="itemData"></param>
        Task<SafeWrapperResult> TryLoadExistingData(CollectionItemViewModel itemData, CancellationToken cancellationToken);

        /// <inheritdoc cref="TryLoadExistingData(CollectionItemViewModel, CancellationToken)"/>
        Task<SafeWrapperResult> TryLoadExistingData(CanvasItem canvasFile, BaseContentTypeModel contentType, CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to delete the file and discard data
        /// </summary>
        /// <param name="hideConfirmation">Hides the delete confirmation dialog, overrides <see cref="ApplicationSettings.Interfaces.IUserSettingsService.ShowDeleteConfirmationDialog"/> if necessary</param>
        /// <returns></returns>
        Task<SafeWrapperResult> TryDeleteData(ICanvasItemReceiverModel canvasItemReceiver = null, bool hideConfirmation = false);

        /// <summary>
        /// Frees cached data and disposes the instance
        /// </summary>
        /// <returns></returns>
        void DiscardData();

        /// <summary>
        /// Sets the data to clipboard determined
        /// </summary>
        /// <returns></returns>
        Task<bool> SetDataToClipboard();

        /// <summary>
        /// Overrides the reference if possible
        /// </summary>
        /// <returns></returns>
        Task<SafeWrapper<CanvasItem>> PasteOverrideReference();
    }
}
