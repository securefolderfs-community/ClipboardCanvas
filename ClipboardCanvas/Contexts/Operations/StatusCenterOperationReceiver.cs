using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Threading;

using ClipboardCanvas.Enums;
using ClipboardCanvas.Services;

namespace ClipboardCanvas.Contexts.Operations
{
    public sealed class StatusCenterOperationReceiver : IOperationContextReceiver
    {
        private IStatusCenterService StatusCenterService { get; } = Ioc.Default.GetService<IStatusCenterService>();

        public IOperationContext GetOperationContext(string operationName, StatusCenterOperationType operationType)
        {
            return StatusCenterService.AppendOperationBanner(operationName, operationType, new CancellationTokenSource(), TimeSpan.FromMilliseconds(850));
        }
    }
}
