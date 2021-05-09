using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using ClipboardCanvas.Helpers.SafetyHelpers;
using System.Threading;
using System.Collections.Generic;
using ClipboardCanvas.ViewModels.UserControls;

namespace ClipboardCanvas.Models
{
    public interface IPasteCanvasModel : IPasteCanvasEventsModel, IDisposable
    {
        /// <summary>
        /// Attempts to load existing data to display
        /// </summary>
        /// <param name="itemData"></param>
        Task<SafeWrapperResult> TryLoadExistingData(ICollectionsContainerItemModel itemData, CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to paste data from provided <see cref="DataPackageView"/> <paramref name="dataPackage"/>
        /// </summary>
        /// <param name="dataPackage">The data to paste</param>
        /// <returns></returns>
        Task<SafeWrapperResult> TryPasteData(DataPackageView dataPackage, CancellationToken cancellationToken);

        /// <summary>
        /// Attempts to save current data to file in associated container
        /// </summary>
        /// <returns></returns>
        Task<SafeWrapperResult> TrySaveData();

        /// <summary>
        /// Attempts to delete the file and discard data
        /// </summary>
        /// <returns></returns>
        Task<SafeWrapperResult> TryDeleteData();

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
        /// <param name="collectionContainer"></param>
        /// <returns></returns>
        Task<IEnumerable<SuggestedActionsControlItemViewModel>> GetSuggestedActions();
    }
}
