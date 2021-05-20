using System;
using ClipboardCanvas.Enums;

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
}
