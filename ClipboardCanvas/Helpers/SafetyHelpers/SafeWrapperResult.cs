using System;
using ClipboardCanvas.Enums;

namespace ClipboardCanvas.Helpers.SafetyHelpers
{
    public class SafeWrapperResult
    {
        public static SafeWrapperResult S_SUCCESS => new SafeWrapperResult(OperationErrorCode.Success, null, "Operation completed successfully.");

        public static SafeWrapperResult S_CANCEL => new SafeWrapperResult(OperationErrorCode.Cancelled, "The operation was canceled");

        public static SafeWrapperResult S_UNKNOWN_FAIL => new SafeWrapperResult(OperationErrorCode.UnknownFailed, new Exception(), "An unknown error occurred.");

        public string Message => Details?.message;

        public OperationErrorCode ErrorCode => Details?.errorCode ?? OperationErrorCode.UnknownFailed;

        public Exception Exception => Details?.innerException;

        public SafeWrapperResultDetails Details { get; private set; }

        public SafeWrapperResult(OperationErrorCode status, Exception innerException)
            : this(status, innerException, null)
        {
        }

        public SafeWrapperResult(OperationErrorCode status, string message)
            : this(status, null, message)
        {
        }

        public SafeWrapperResult(OperationErrorCode status, Exception innerException, string message)
            : this(new SafeWrapperResultDetails(status, innerException, message))
        {
        }

        public SafeWrapperResult(SafeWrapperResultDetails details)
        {
            this.Details = details;
        }

        public static implicit operator OperationErrorCode(SafeWrapperResult wrapperResult) => wrapperResult?.Details?.errorCode ?? OperationErrorCode.InvalidArgument;

        public static implicit operator bool(SafeWrapperResult wrapperResult) => wrapperResult?.Details?.errorCode == OperationErrorCode.Success;

        public static implicit operator SafeWrapperResult(SafeWrapperResultDetails details) => new SafeWrapperResult(details);

        public static implicit operator SafeWrapperResult((OperationErrorCode errorCode, Exception innerException) details)
            => new SafeWrapperResultDetails(details.errorCode, details.innerException);

        public static implicit operator SafeWrapperResult((OperationErrorCode errorCode, string message) details)
            => new SafeWrapperResultDetails(details.errorCode, details.message);

        public static implicit operator SafeWrapperResult((OperationErrorCode errorCode, Exception innerException, string message) details)
            => new SafeWrapperResultDetails(details.errorCode, details.innerException, details.message);
    }
}
