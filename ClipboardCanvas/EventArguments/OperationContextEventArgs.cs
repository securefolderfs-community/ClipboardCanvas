using System;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.EventArguments
{
    public class OperationFinishedEventArgs : EventArgs
    {
        public readonly SafeWrapperResult result;

        public OperationFinishedEventArgs(SafeWrapperResult result)
        {
            this.result = result;
        }
    }
}
