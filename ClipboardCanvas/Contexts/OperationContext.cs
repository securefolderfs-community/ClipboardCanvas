using System;
using System.Threading;
using ClipboardCanvas.EventArguments;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.Contexts
{
    public sealed class OperationContext
    {
        public event EventHandler<OperationFinishedEventArgs> OnOperationFinishedEvent;

        public bool IsEventAlreadyHooked { get; set; }

        public bool IsOperationOngoing { get; set; }

        public CancellationToken CancellationToken { get; set; }

        public Action<float> ProgressDelegate { get; set; }

        public void OperationFinished(SafeWrapperResult result)
        {
            IsOperationOngoing = false;
            ProgressDelegate = null;

            OnOperationFinishedEvent?.Invoke(this, new OperationFinishedEventArgs(result));
        }
    }
}
