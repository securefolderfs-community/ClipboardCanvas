using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using ClipboardCanvas.DataModels;
using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.EventArguments.CanvasControl;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.ContextMenu;
using ClipboardCanvas.ViewModels.UserControls;

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
        List<BaseMenuFlyoutItemViewModel> ContextMenuItems { get; }

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
        Task<SafeWrapperResult> TryDeleteData(bool hideConfirmation = false);

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
    }
}
