using System.Collections.Generic;
using Microsoft.Toolkit.Mvvm.Input;
using Windows.ApplicationModel.DataTransfer;
using System.Threading;
using Microsoft.Toolkit.Uwp;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.UserControls;
using ClipboardCanvas.ViewModels.UserControls.CanvasDisplay;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls.Collections;
using ClipboardCanvas.ViewModels.UserControls.CanvasPreview;

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
                }), "DeleteReference".GetLocalized(), "\uE738");

            actions.Add(action_deleteReference);

            return actions;
        }

        public static IEnumerable<SuggestedActionsControlItemViewModel> GetActionsForEmptyCanvasPage(ICanvasPreviewModel pasteCanvasControlModel)
        {
            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            var action_paste = new SuggestedActionsControlItemViewModel(
                new AsyncRelayCommand(async () =>
                {
                    BaseReadOnlyCanvasPreviewControlViewModel<BaseReadOnlyCanvasViewModel>.CanvasPasteCancellationTokenSource.Cancel();
                    BaseReadOnlyCanvasPreviewControlViewModel<BaseReadOnlyCanvasViewModel>.CanvasPasteCancellationTokenSource = new CancellationTokenSource();

                    SafeWrapper<DataPackageView> dataPackage = ClipboardHelpers.GetClipboardData();

                    await pasteCanvasControlModel.TryPasteData(dataPackage, BaseReadOnlyCanvasPreviewControlViewModel<BaseReadOnlyCanvasViewModel>.CanvasPasteCancellationTokenSource.Token);
                }), "PasteFromClipboard".GetLocalized(), "\uE77F");

            actions.Add(action_paste);

            return actions;
        }

        public static IEnumerable<SuggestedActionsControlItemViewModel> GetActionsForHomepage()
        {
            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            var action_addCollection = new SuggestedActionsControlItemViewModel(
                new AsyncRelayCommand(async () =>
                {
                    await CollectionsWidgetViewModel.AddCollectionViaUi();
                }), "AddCollection".GetLocalized(), "\uE710");

            actions.Add(action_addCollection);

            return actions;
        }

        public static IEnumerable<SuggestedActionsControlItemViewModel> GetActionsForCollectionPreviewPage()
        {
            List<SuggestedActionsControlItemViewModel> actions = new List<SuggestedActionsControlItemViewModel>();

            return actions;
        }
    }
}
