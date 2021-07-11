using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using System.Threading;
using System.Collections.Generic;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.Enums;
using ClipboardCanvas.ViewModels.ContextMenu;

namespace ClipboardCanvas.Models
{
    public interface ICanvasPreviewModel : ICanvasPreviewEventsModel, IDisposable
    {
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
        Task<SafeWrapperResult> TryLoadExistingData(ICollectionItemModel itemData, CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to paste data from provided <see cref="DataPackageView"/> <paramref name="dataPackage"/>
        /// </summary>
        /// <param name="dataPackage">The data to paste</param>
        /// <returns></returns>
        Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to save current data to file in associated collection
        /// </summary>
        /// <returns></returns>
        Task<SafeWrapperResult> TrySaveData();

        /// <summary>
        /// Attempts to delete the file and discard data
        /// </summary>
        /// <param name="hideConfirmation">Hides the delete confirmation dialog, overrides <see cref="ApplicationSettings.Interfaces.IUserSettings.ShowDeleteConfirmationDialog"/> if necessary</param>
        /// <returns></returns>
        Task<SafeWrapperResult> TryDeleteData(bool hideConfirmation = false);

        /// <summary>
        /// Overrides the reference and pastes the file to the collection
        /// </summary>
        /// <returns></returns>
        Task<SafeWrapperResult> PasteOverrideReference();

        /// <summary>
        /// Frees cached data and disposes the instance
        /// </summary>
        /// <returns></returns>
        void DiscardData();

        /// <summary>
        /// Opens new canvas
        /// </summary>
        void OpenNewCanvas();

        /// <summary>
        /// Gets suggested actions based on this canvas
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<SuggestedActionsControlItemViewModel>> GetSuggestedActions();

        /// <summary>
        /// Sets the data to clipboard determined by <paramref name="dataSourceSetType"/>
        /// </summary>
        /// <param name="dataSourceSetType"></param>
        /// <returns></returns>
        bool SetDataToClipboard(SetClipboardDataSourceType dataSourceSetType);
    }
}
