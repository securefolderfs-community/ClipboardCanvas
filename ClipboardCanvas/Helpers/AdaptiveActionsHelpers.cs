using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.UnsafeNative;
using ClipboardCanvas.ViewModels.UserControls;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.ApplicationModel;
using System.Linq;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel.Core;
using Microsoft.Toolkit.Uwp;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using System.Threading;

namespace ClipboardCanvas.Helpers
{
    public static class AdaptiveActionsHelpers
    {
        private static async Task<(BitmapImage icon, string appName)> GetInfoFromFileHandlingApp(string fileExtension)
        {
            IReadOnlyList<AppInfo> apps = await Launcher.FindFileHandlersAsync(fileExtension);

            AppInfo app = apps.Last();

            RandomAccessStreamReference stream = app.DisplayInfo.GetLogo(new Size(64d, 64d));

            BitmapImage image = new BitmapImage();
            await CoreApplication.MainView.DispatcherQueue.EnqueueAsync(async () =>
            {
                image = await ImagingHelpers.ToBitmapAsync((await stream.OpenReadAsync()).AsStreamForRead());
            });

            return (image, app.DisplayInfo.DisplayName);
        }

        public static async Task<IEnumerable<AdaptiveOptionsControlItemViewModel>> GetActionsForFilledCanvasPage(ICollectionsContainerModel collectionsContainer)
        {
            List<AdaptiveOptionsControlItemViewModel> actions = new List<AdaptiveOptionsControlItemViewModel>();

            var action_openInFileExplorer = new AdaptiveOptionsControlItemViewModel(
                async () =>
                {
                    await collectionsContainer.CurrentCanvas.OpenContainingFolder();
                }, "Open containing folder", "\uE838");

            var (icon, appName) = await GetInfoFromFileHandlingApp(Path.GetExtension(collectionsContainer.CurrentCanvas.File.Path));
            var action_openFile = new AdaptiveOptionsControlItemViewModel(
                async () =>
                {
                    await collectionsContainer.CurrentCanvas.OpenFile();
                }, $"Open with {appName}", icon);

            actions.Add(action_openInFileExplorer);
            actions.Add(action_openFile);

            return actions;
        }

        public static IEnumerable<AdaptiveOptionsControlItemViewModel> GetActionsForEmptyCanvasPage(IPasteCanvasModel pasteCanvasControlModel)
        {
            List<AdaptiveOptionsControlItemViewModel> actions = new List<AdaptiveOptionsControlItemViewModel>();

            var action_paste = new AdaptiveOptionsControlItemViewModel(
                async () =>
                {
                    DynamicCanvasControlViewModel.CanvasPasteCancellationTokenSource.Cancel();
                    DynamicCanvasControlViewModel.CanvasPasteCancellationTokenSource = new CancellationTokenSource();

                    SafeWrapper<DataPackageView> dataPackage = await ClipboardHelpers.GetClipboardData();

                    await pasteCanvasControlModel.TryPasteData(dataPackage, DynamicCanvasControlViewModel.CanvasPasteCancellationTokenSource.Token);
                }, "Paste from clipboard", "\uE77F");

            actions.Add(action_paste);

            return actions;
        }

        public static IEnumerable<AdaptiveOptionsControlItemViewModel> GetActionsForUnselectedCollection()
        {
            List<AdaptiveOptionsControlItemViewModel> actions = new List<AdaptiveOptionsControlItemViewModel>();

            var action_addCollection = new AdaptiveOptionsControlItemViewModel(
                async () =>
                {
                    await CollectionsControlViewModel.AddCollectionViaUi();
                }, "Add Collection", "\uE710");

            actions.Add(action_addCollection);

            return actions;
        }
    }
}
