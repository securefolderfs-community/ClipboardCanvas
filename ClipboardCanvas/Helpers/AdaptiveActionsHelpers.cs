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
using ClipboardCanvas.ReferenceItems;
using Windows.Storage;

namespace ClipboardCanvas.Helpers
{
    public static class SuggestedActionsHelpers
    {
        public static IEnumerable<SuggestedActionsControlItemViewModel> GetActionsForEmptyCanvasPage(IPasteCanvasModel pasteCanvasControlModel)
        {
            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            var action_paste = new SuggestedActionsControlItemViewModel(
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

        public static IEnumerable<SuggestedActionsControlItemViewModel> GetActionsForUnselectedCollection()
        {
            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            var action_addCollection = new SuggestedActionsControlItemViewModel(
                async () =>
                {
                    await CollectionsControlViewModel.AddCollectionViaUi();
                }, "Add Collection", "\uE710");

            actions.Add(action_addCollection);

            return actions;
        }
    }
}
