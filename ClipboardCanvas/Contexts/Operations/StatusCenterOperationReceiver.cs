using ClipboardCanvas.Enums;
using ClipboardCanvas.Services;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Threading;

namespace ClipboardCanvas.Contexts.Operations
{
    public sealed class StatusCenterOperationReceiver : IOperationContextReceiver
    {
        private IStatusCenterService StatusCenterService { get; } = Ioc.Default.GetService<IStatusCenterService>();

        public IOperationContext GetOperationContext(string operationName, StatusCenterOperationType operationType) // TODO: When StatusCenter is implemented, implement this too!
        {
            return StatusCenterService.AppendOperationBanner(operationName, operationType, new CancellationTokenSource(), TimeSpan.FromMilliseconds(1000));
        }
    }
}
