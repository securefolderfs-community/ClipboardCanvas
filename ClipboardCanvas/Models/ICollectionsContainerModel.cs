using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.Models
{
    /// <summary>
    /// This interface is part of containers which hold pasted items
    /// This interface contains functions to save data and delete to/from files
    /// </summary>
    public interface ICollectionsContainerModel
    {
        List<CollectionsContainerItemViewModel> Items { get; }

        bool CanOpenCollection { get; }

        bool IsOnNewCanvas { get; }

        bool CanvasInitialized { get; }

        bool CanvasInitializing { get; }

        string Name { get; }

        int CurrentIndex { get; }

        /// <summary>
        /// Gets whether current canvas is new unfilled or canvas is not new with already existing content - filled
        /// </summary>
        bool IsFilled { get; }

        /// <summary>
        /// Gets currently opened canvas
        /// </summary>
        ICollectionsContainerItemModel CurrentCanvas { get; }

        /// <summary>
        /// Checks whether can open collection and updates UI and <see cref="CanOpenCollection"/> if necessary
        /// </summary>
        bool CheckCollectionAvailability();

        /// <summary>
        /// Sets index of currently selected canvas
        /// <br/><br/>
        /// Note:
        /// <br/>
        /// This function is considered as *dangerous* since calling it may yield unexpected results
        /// </summary>
        /// <param name="newIndex">New value</param>
        void DangerousSetIndex(int newIndex);

        /// <summary>
        /// Gets folder associated with the collection
        /// <br/><br/>
        /// Note:
        /// <br/>
        /// This function is considered as *dangerous* since <see cref="ICollectionsContainerModel"/> contains wrapper functions for provided return value
        /// </summary>
        /// <returns></returns>
        IStorageFolder DangerousGetCollectionFolder();

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

        Task LoadCanvasFromCollection(IPasteCanvasModel pasteCanvasModel, CancellationToken cancellationToken);

        /// <summary>
        /// Silently adds item to the collection instead of refreshing
        /// </summary>
        /// <param name="file"></param>
        /// <param name="contentType"></param>
        void RefreshAddItem(StorageFile file, BasePastedContentTypeDataModel contentType);

        /// <summary>
        /// Silently removes an item from the collection to prevent from reloading it
        /// </summary>
        void RefreshRemoveItem(ICollectionsContainerItemModel collectionsContainerItem);

        /// <summary>
        /// Initializes collection's items
        /// </summary>
        /// <returns>Returns true if operation completed successfully, otherwise false</returns>
        Task<bool> InitializeItems();
    }
}
