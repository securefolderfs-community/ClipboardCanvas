using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using System.Collections.ObjectModel;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.DataModels;

namespace ClipboardCanvas.Models
{
    public interface ICollectionModel
    {
        ObservableCollection<CollectionItemViewModel> CollectionItems { get; }

        /// <summary>
        /// Saved search data by the Search function
        /// </summary>
        SearchDataModel SavedSearchData { get; set; }

        bool IsCollectionAvailable { get; }

        bool IsOnNewCanvas { get; }

        string DisplayName { get; }

        bool IsCollectionInitialized { get; }
        
        bool IsCollectionInitializing { get; }

        CollectionItemViewModel CurrentCollectionItemViewModel { get; }

        Task<SafeWrapper<StorageFile>> GetOrCreateNewCollectionFileFromExtension(string extension);

        Task<SafeWrapper<StorageFile>> GetOrCreateNewCollectionFile(string fileName);

        /// <summary>
        /// Navigates to new canvas
        /// </summary>
        void NavigateFirst(ICanvasPreviewModel pasteCanvasModel);

        /// <summary>
        /// Navigates to next canvas
        /// </summary>
        /// <returns></returns>
        Task NavigateNext(ICanvasPreviewModel pasteCanvasModel, CancellationToken cancellationToken);

        /// <summary>
        /// Navigates to last canvas in the list
        /// </summary>
        Task NavigateLast(ICanvasPreviewModel pasteCanvasModel, CancellationToken cancellationToken);

        /// <summary>
        /// Navigates to back canvas
        /// </summary>
        /// <returns></returns>
        Task NavigateBack(ICanvasPreviewModel pasteCanvasModel, CancellationToken cancellationToken);

        /// <summary>
        /// Tries to load current canvas from collection
        /// </summary>
        /// <param name="pasteCanvasModel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task LoadCanvasFromCollection(ICanvasPreviewModel pasteCanvasModel, CancellationToken cancellationToken, ICollectionItemModel collectionItemModel = null);

        /// <summary>
        /// Manually adds item to collection
        /// </summary>
        /// <param name="collectionItemViewModel"></param>
        void AddCollectionItem(CollectionItemViewModel collectionItemViewModel);

        /// <summary>
        /// Manually removes item from collection
        /// </summary>
        /// <param name="collectionItemViewModel"></param>
        void RemoveCollectionItem(CollectionItemViewModel collectionItemViewModel);

        /// <summary>
        /// Returns true, if it's possible to navigate canvas forward
        /// </summary>
        /// <returns></returns>
        bool HasNext();

        /// <summary>
        /// Returns true, if it's possible to navigate canvas back
        /// </summary>
        /// <returns></returns>
        bool HasBack();

        /// <summary>
        /// Sets current index at the stack end
        /// </summary>
        void SetIndexOnNewCanvas();

        /// <summary>
        /// Sets current index to index of <paramref name="collectionItemModel"/>
        /// </summary>
        /// <param name="collectionItemModel"></param>
        void UpdateIndex(ICollectionItemModel collectionItemModel);

        bool CheckCollectionAvailability();

        Task<bool> InitializeCollectionItems();

        Task<bool> InitializeCollectionFolder();
    }
}
