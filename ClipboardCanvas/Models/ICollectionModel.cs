using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Contexts;
using ClipboardCanvas.CanvasFileReceivers;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.Models
{
    public interface ICollectionModel : ICanvasFileReceiverModel
    {
        ObservableCollection<CollectionItemViewModel> CollectionItems { get; }

        /// <summary>
        /// Saved search data by the Search function
        /// </summary>
        SearchContext SearchContext { get; set; }

        bool IsCollectionAvailable { get; }

        bool IsOnNewCanvas { get; }

        string DisplayName { get; }

        bool IsCollectionInitialized { get; }
        
        bool IsCollectionInitializing { get; }

        CollectionItemViewModel CurrentCollectionItemViewModel { get; }

        Task<SafeWrapper<CollectionItemViewModel>> CreateNewCollectionItemFromExtension(string extension);

        Task<SafeWrapper<CollectionItemViewModel>> CreateNewCollectionItem(string fileName);

        Task<SafeWrapperResult> DeleteCollectionItem(CollectionItemViewModel itemToDelete, bool permanently);

        CollectionItemViewModel FindCollectionItem(CanvasItem canvasItem);

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
        Task LoadCanvasFromCollection(ICanvasPreviewModel pasteCanvasModel, CancellationToken cancellationToken, CollectionItemViewModel collectionItemViewModel = null);

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
        /// Sets current index to index of <paramref name="collectionItemViewModel"/>
        /// </summary>
        /// <param name="collectionItemViewModel"></param>
        void UpdateIndex(CollectionItemViewModel collectionItemViewModel);

        bool IsOnOpenedCanvas(CollectionItemViewModel collectionItemViewModel);

        bool CheckCollectionAvailability();

        CollectionConfigurationModel ConstructConfigurationModel();

        Task<bool> InitializeCollectionItems();

        Task<bool> InitializeCollectionFolder();
    }
}
