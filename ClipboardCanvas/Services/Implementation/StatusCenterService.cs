using System;
using System.Threading;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;
using ClipboardCanvas.ViewModels.UserControls.StatusCenter;

namespace ClipboardCanvas.Services.Implementation
{
    public class StatusCenterService : IStatusCenterService
    {
        public StatusCenterViewModel StatusCenterViewModel { get; set; }

        public INavigationToolBarControlModel NavigationToolBarControlModel { get; set; }

        public StatusCenterItemViewModel AppendInstantOperationBanner(string operationName, StatusCenterOperationType operationType, CancellationTokenSource cancellationTokenSource)
        {
            var banner = StatusCenterItemViewModel.ConstructOperationBanner(operationName, operationType, cancellationTokenSource);
            StatusCenterViewModel.AddItem(banner);

            return banner;
        }

        public StatusCenterItemViewModel AppendOperationBanner(string operationName, StatusCenterOperationType operationType, CancellationTokenSource cancellationTokenSource, TimeSpan appendDelay)
        {
            var banner = StatusCenterItemViewModel.ConstructOperationBanner(operationName, operationType, cancellationTokenSource);
            StatusCenterViewModel.AddItem(banner, appendDelay);

            return banner;
        }

        public StatusCenterItemViewModel AppendInfoBanner(string infoTitle, string infoDescription, SafeWrapperResult result)
        {
            var banner = StatusCenterItemViewModel.ConstructInfoBanner(infoTitle, infoDescription, result);
            StatusCenterViewModel.AddItem(banner);

            return banner;
        }

        public void RemoveBanner(StatusCenterItemViewModel banner)
        {
            StatusCenterViewModel.RemoveItem(banner);
        }

        public void ShowStatusCenter()
        {
            if (NavigationToolBarControlModel != null)
            {
                NavigationToolBarControlModel.IsStatusCenterButtonVisible = true;
            }
        }

        public void HideStatusCenter()
        {
            if (NavigationToolBarControlModel != null)
            {
                NavigationToolBarControlModel.IsStatusCenterButtonVisible = false;
            }
        }
    }
}
