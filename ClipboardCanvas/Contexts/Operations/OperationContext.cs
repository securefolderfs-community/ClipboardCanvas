using System;
using System.Threading;

using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.Contexts.Operations
{
    public sealed class OperationContext : IOperationContext
    {
        public event EventHandler<OperationFinishedEventArgs> OnOperationFinishedEvent;

        public bool IsEventAlreadyHooked { get; set; }

        public bool IsOperationStarted { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public bool IsOperationFinished { get; private set; }

        IProgress<double> IOperationContext.OperationProgress { get; }

        public void FinishOperation(SafeWrapperResult result)
        {
            IsOperationFinished = true;
            IsOperationStarted = false;

            OnOperationFinishedEvent?.Invoke(this, new OperationFinishedEventArgs(result));
        }

        public void StartOperation()
        {
            if (IsOperationFinished)
            {
                IsOperationStarted = false;
                return;
            }

            IsOperationStarted = true;
        }
    }
}
