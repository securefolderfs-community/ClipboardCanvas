using System;
using System.Linq;
using System.Threading.Tasks;
using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;

namespace ClipboardCanvas.Helpers.SafetyHelpers
{
    public class SafeWrapper<T> : SafeWrapperResult
    {
        public T Result { get; private set; }

        public SafeWrapper(T result, OperationErrorCode errorCode)
            : this (result, errorCode, null)
        {
        }

        public SafeWrapper(T result, OperationErrorCode errorCode, string message)
            : this(result, errorCode, null, message)
        {
        }

        public SafeWrapper(T result, OperationErrorCode errorCode, Exception innerException, string message)
            : this(result, new SafeWrapperResultDetails(errorCode, innerException, message))
        {
            this.Result = result;
        }

        public SafeWrapper(T result, SafeWrapperResultDetails details)
            : base(details)
        {
            this.Result = result;
        }

        public static implicit operator T(SafeWrapper<T> safeWrapper) => safeWrapper.Result;
    }

    public class SafeWrapperResult
    {
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
            : this (new SafeWrapperResultDetails(status, innerException, message))
        {
        }

        public SafeWrapperResult(SafeWrapperResultDetails details)
        {
            this.Details = details;
        }

        public static implicit operator OperationErrorCode(SafeWrapperResult wrapperResult) => wrapperResult?.Details?.errorCode ?? OperationErrorCode.InvalidArgument;

        public static implicit operator bool(SafeWrapperResult wrapperResult) => wrapperResult?.Details.errorCode == OperationErrorCode.Success;
    }
}
