using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;

using ClipboardCanvas.Models;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models.Configuration;
using ClipboardCanvas.DataModels;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Services;
using ClipboardCanvas.DataModels.Navigation;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.Filesystem;

namespace ClipboardCanvas.ViewModels.Widgets.Timeline
{
    public class TimelineSectionItemViewModel : ObservableObject, IDisposable
    {
        #region Properties

        private INavigationService NavigationService { get; } = Ioc.Default.GetService<INavigationService>();

        public IReadOnlyCanvasPreviewModel ReadOnlyCanvasPreviewModel { get; set; }

        public ICollectionModel CollectionModel { get; private set; }

        public CanvasItem CanvasItem { get; private set; }

        private string _FileName;
        public string FileName
        {
            get => _FileName;
            set => SetProperty(ref _FileName, value);
        }

        public string SourceCollectionName
        {
            get => $"in {CollectionModel.DisplayName}";
        }

        #endregion

        #region Events

        public event EventHandler<RemoveSectionItemRequestedEventArgs> OnRemoveSectionItemRequestedEvent;

        #endregion

        #region Commands

        public ICommand OpenFileCommand { get; private set; }

        public ICommand OpenCanvasCommand { get; private set; }

        public ICommand ShowInCollectionPreviewCommand { get; private set; }

        public ICommand RemoveFromSectionCommand { get; private set; }

        #endregion

        #region Constructor

        public TimelineSectionItemViewModel(ICollectionModel collectionModel, CanvasItem canvasItem)
        {
            this.CollectionModel = collectionModel;
            this.CanvasItem = canvasItem;

            // Create commands
            OpenFileCommand = new AsyncRelayCommand(OpenFile);
            OpenCanvasCommand = new AsyncRelayCommand(OpenCanvas);
            ShowInCollectionPreviewCommand = new RelayCommand(ShowInCollectionPreview);
            RemoveFromSectionCommand = new RelayCommand(RemoveFromSection);
        }

        #endregion

        #region Command Implementation

        private async Task OpenFile()
        {
            await StorageHelpers.OpenFile(await CanvasItem.SourceItem);
        }

        private async Task OpenCanvas()
        {
            if (!CollectionModel.IsCollectionInitialized && !CollectionModel.IsCollectionInitializing)
            {
                // Initialize if not initialized
                await CollectionModel.InitializeCollectionItems();

                CollectionModel.UpdateIndex(CollectionModel.FindCollectionItem(CanvasItem));
                NavigationService.OpenCanvasPage(CollectionModel);
            }
            else if (CollectionModel.IsCollectionInitialized)
            {
                CollectionModel.UpdateIndex(CollectionModel.FindCollectionItem(CanvasItem));
                NavigationService.OpenCanvasPage(CollectionModel);
            }
        }

        private void ShowInCollectionPreview()
        {
            NavigationService.OpenCollectionPreviewPage(CollectionModel, new CollectionPreviewPageNavigationParameterModel(CollectionModel, CanvasHelpers.GetDefaultCanvasType(), CanvasItem));
        }

        private void RemoveFromSection()
        {
            this.OnRemoveSectionItemRequestedEvent?.Invoke(this, new RemoveSectionItemRequestedEventArgs(this));
        }

        #endregion

        #region Helpers

        public async Task<SafeWrapperResult> InitializeSectionItemContent(bool withLoadDelay = true)
        {
            FileName = await CanvasHelpers.SafeGetCanvasItemName(CanvasItem);

            if (withLoadDelay)
            {
                // Wait for control to load
                await Task.Delay(Constants.UI.CONTROL_LOAD_DELAY);
            }

            if (ReadOnlyCanvasPreviewModel != null)
            {
                return await ReadOnlyCanvasPreviewModel.TryLoadExistingData(CanvasItem, null, TimelineWidgetViewModel.LoadCancellationToken.Token);
            }
            else
            {
                return SafeWrapperResult.CANCEL;
            }
        }

        public TimelineSectionItemConfigurationModel ConstructConfigurationModel()
        {
            return new TimelineSectionItemConfigurationModel(
                CanvasItem.AssociatedItem.Path,
                CollectionModel.ConstructConfigurationModel());
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            this.ReadOnlyCanvasPreviewModel?.Dispose();

            this.ReadOnlyCanvasPreviewModel = null;
        }

        #endregion
    }
}
