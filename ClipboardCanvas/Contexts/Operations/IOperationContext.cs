using System;
using System.Threading;

using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.Contexts.Operations
{
    public interface IOperationContext
    {
        event EventHandler<OperationFinishedEventArgs> OnOperationFinishedEvent;

        bool IsOperationOngoing { get; set; }

        CancellationToken CancellationToken { get; set; }

        Action<float> ProgressDelegate { get; set; }

        void OperationFinished(SafeWrapperResult result);
    }
}
