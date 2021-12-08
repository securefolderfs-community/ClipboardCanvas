using System;
using System.Threading;

using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.Contexts.Operations
{
    public interface IOperationContext
    {
        event EventHandler<OperationFinishedEventArgs> OnOperationFinishedEvent;

        bool IsOperationStarted { get; }

        bool IsOperationFinished { get; }

        CancellationToken CancellationToken { get; }

        IProgress<double> OperationProgress { get; }

        void StartOperation();

        void FinishOperation(SafeWrapperResult result);
    }
}
