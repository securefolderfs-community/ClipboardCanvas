using System;
using System.Threading;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.ViewModels.UserControls.StatusCenter;

namespace ClipboardCanvas.Services
{
    public interface IStatusCenterService
    {
        StatusCenterItemViewModel AppendInstantOperationBanner(string operationName, StatusCenterOperationType operationType, CancellationTokenSource cancellationTokenSource);

        StatusCenterItemViewModel AppendOperationBanner(string operationName, StatusCenterOperationType operationType, CancellationTokenSource cancellationTokenSource, TimeSpan appendDelay);

        StatusCenterItemViewModel AppendInfoBanner(string infoTitle, string infoDescription, SafeWrapperResult result);

        void RemoveBanner(StatusCenterItemViewModel banner);

        void ShowStatusCenter();

        void HideStatusCenter();
    }
}
