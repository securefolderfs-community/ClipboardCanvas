using System;

namespace ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters
{
    public interface ISafeWrapperExceptionReporter
    {
        SafeWrapperResultDetails GetStatusResult(Exception e);

        SafeWrapperResultDetails GetStatusResult(Exception e, Type callerType);
    }
}
