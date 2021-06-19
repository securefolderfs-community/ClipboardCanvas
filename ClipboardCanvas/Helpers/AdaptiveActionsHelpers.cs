using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using System.Threading;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.Helpers
{
    public static class SuggestedActionsHelpers
    {
        public static IEnumerable<SuggestedActionsControlItemViewModel> GetActionsForInvalidReference(IPasteCanvasModel pasteCanvasModel)
        {
            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            var action_deleteReference = new SuggestedActionsControlItemViewModel(
                async () =>
                {
                    await pasteCanvasModel.TryDeleteData(true);
                }, "Remove Reference", "\uE738");

            actions.Add(action_deleteReference);

            return actions;
        }

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
