using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.UserControls;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace ClipboardCanvas.Models
{
    /// <summary>
    /// This interface is part of containers which hold pasted items
    /// This interface contains functions to save data and delete to/from files
    /// </summary>
    public interface ICollectionsContainerModel
    {
        List<CollectionsContainerItemModel> Items { get; }

        string Name { get; }

        /// <summary>
        /// Gets whether current canvas is new unfilled or canvas is not new with already existing content - filled
        /// </summary>
        bool IsFilled { get; }

        /// <summary>
        /// Gets currently opened canvas
        /// </summary>
        ICollectionsContainerItemModel CurrentCanvas { get; }

        void DangerousSetIndex(int newIndex);

        /// <summary>
        /// Creates a file and returns it within this container 
        /// </summary>
        /// <returns>A <see cref="StorageFile"/> which can be written to, read from</returns>
        Task<SafeWrapper<StorageFile>> GetEmptyFileToWrite(string extension, string fileName = null);

        /// <summary>
        /// Determines whether current canvas can be changed to next
        /// </summary>
        /// <returns></returns>
        bool HasNext();

        /// <summary>
        /// Determines whether current canvas can be changed to back
        /// </summary>
        /// <returns></returns>
        bool HasBack();

        /// <summary>
        /// Navigates to new canvas
        /// </summary>
        void NavigateFirst(IPasteCanvasModel pasteCanvasModel);

        /// <summary>
        /// Navigates to next canvas
        /// </summary>
        /// <returns></returns>
        Task NavigateNext(IPasteCanvasModel pasteCanvasModel, CancellationToken cancellationToken);

        /// <summary>
        /// Navigates to oldest canvas in the list
        /// </summary>
        Task NavigateLast(IPasteCanvasModel pasteCanvasModel, CancellationToken cancellationToken);

        /// <summary>
        /// Navigates to back canvas
        /// </summary>
        /// <returns></returns>
        Task NavigateBack(IPasteCanvasModel pasteCanvasModel, CancellationToken cancellationToken);

        Task LoadCanvasFromCollection(IPasteCanvasModel pasteCanvasModel, CancellationToken cancellationToken, bool navigateNext);

        /// <summary>
        /// Adds item to the collection to refresh it with newly created canvas
        /// </summary>
        /// <param name="file"></param>
        /// <param name="contentType"></param>
        void RefreshAddItem(StorageFile file, BasePastedContentTypeDataModel contentType);

        /// <summary>
        /// Initializes collection's items
        /// </summary>
        /// <returns>Returns true if operation completed successfully, otherwise false</returns>
        Task<bool> InitializeItems();
    }
}
