using System;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters
{
    public class ClipboardGetDataExceptionReporter : ISafeWrapperExceptionReporter
    {
        // TODO: Implement proper error reporting here

        public SafeWrapperResultDetails GetStatusResult(Exception e)
        {
            return GetStatusResult(e, null);
        }

        public SafeWrapperResultDetails GetStatusResult(Exception e, Type callerType)
        {
            return StaticExceptionReporters.DefaultSafeWrapperExceptionReporter.GetStatusResult(e, callerType);
        }
    }
}
