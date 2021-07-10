using System.Collections.Generic;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using System.Threading;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls.Collections;

namespace ClipboardCanvas.Helpers
{
    public static class SuggestedActionsHelpers
    {
        public static IEnumerable<SuggestedActionsControlItemViewModel> GetActionsForInvalidReference(ICanvasPreviewModel pasteCanvasModel)
        {
            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            var action_deleteReference = new SuggestedActionsControlItemViewModel(
                new AsyncRelayCommand(async () =>
                {
                    await pasteCanvasModel.TryDeleteData(true);
                }), "Delete Reference", "\uE738");

            actions.Add(action_deleteReference);

            return actions;
        }

        public static IEnumerable<SuggestedActionsControlItemViewModel> GetActionsForEmptyCanvasPage(ICanvasPreviewModel pasteCanvasControlModel)
        {
            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            var action_paste = new SuggestedActionsControlItemViewModel(
                new AsyncRelayCommand(async () =>
                {
                    CanvasPreviewControlViewModel.CanvasPasteCancellationTokenSource.Cancel();
                    CanvasPreviewControlViewModel.CanvasPasteCancellationTokenSource = new CancellationTokenSource();

                    SafeWrapper<DataPackageView> dataPackage = await ClipboardHelpers.GetClipboardData();

                    await pasteCanvasControlModel.TryPasteData(dataPackage, CanvasPreviewControlViewModel.CanvasPasteCancellationTokenSource.Token);
                }), "Paste from clipboard", "\uE77F");

            actions.Add(action_paste);

            return actions;
        }

        public static IEnumerable<SuggestedActionsControlItemViewModel> GetActionsForUnselectedCollection()
        {
            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            var action_addCollection = new SuggestedActionsControlItemViewModel(
                new AsyncRelayCommand(async () =>
                {
                    await CollectionsControlViewModel.AddCollectionViaUi();
                }), "Add Collection", "\uE710");

            actions.Add(action_addCollection);

            return actions;
        }
    }
}
