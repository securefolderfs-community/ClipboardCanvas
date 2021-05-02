using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClipboardCanvas.Helpers.SafetyHelpers
{
    public static class SafeWrapperRoutines
    {
        public static SafeWrapper<T> SafeWrap<T>(Func<T> func, ISafeWrapperExceptionReporter reporter = null)
        {
            if (!AssertNotNull(func)) return new SafeWrapper<T>(default(T), OperationErrorCode.InvalidArgument, new ArgumentException(), "Passed-in function delegate is null");

            try
            {
                return new SafeWrapper<T>(func.Invoke(), OperationErrorCode.Success, "Operation completed successfully");
            }
            catch (Exception e)
            {
                reporter = TryGetReporter(reporter);

                return new SafeWrapper<T>(default(T), reporter.GetStatusResult(e, typeof(T)));
            }
        }

        public static SafeWrapperResult SafeWrap(Action action, ISafeWrapperExceptionReporter reporter = null)
        {
            if (!AssertNotNull(action)) return new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentException(), "Passed-in function delegate is null");

            try
            {
                action.Invoke();
                return new SafeWrapperResult(OperationErrorCode.Success, "Operation completed successfully");
            }
            catch (Exception e)
            {
                reporter = TryGetReporter(reporter);

                return new SafeWrapperResult(reporter.GetStatusResult(e));
            }
        }

        public static SafeWrapperResult OnSuccess<T>(this SafeWrapper<T> wrapped, Action<T> action)
        {
            if (!AssertNotNull(wrapped, action)) return new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentException(), "Passed-in function delegate is null");

            SafeWrapperResult result = wrapped;

            if (result)
            {
                return SafeWrap(() => action(wrapped.Result));
            }

            return result;
        }

        #region Async Functions

        public static async Task<SafeWrapper<T>> SafeWrapAsync<T>(Func<Task<T>> func, ISafeWrapperExceptionReporter reporter = null)
        {
            if (!AssertNotNull(func)) return new SafeWrapper<T>(default(T), OperationErrorCode.InvalidArgument, new ArgumentException(), "Passed-in function delegate is null");

            try
            {
                return new SafeWrapper<T>(await func.Invoke(), OperationErrorCode.Success, "Operation completed successfully");
            }
            catch (Exception e)
            {
                reporter = TryGetReporter(reporter);

                return new SafeWrapper<T>(default(T), reporter.GetStatusResult(e, typeof(T)));
            }
        }

        public static async Task<SafeWrapperResult> SafeWrapAsync(Func<Task> func, ISafeWrapperExceptionReporter reporter = null)
        {
            if (!AssertNotNull(func)) return new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentException(), "Passed-in function delegate is null");

            try
            {
                await func.Invoke();
                return new SafeWrapperResult(OperationErrorCode.Success, "Operation completed successfully");
            }
            catch (Exception e)
            {
                reporter = TryGetReporter(reporter);

                return new SafeWrapperResult(reporter.GetStatusResult(e));
            }
        }

        public static async Task<SafeWrapperResult> OnSuccess<T>(this Task<SafeWrapper<T>> wrapped, Func<T, Task> func)
        {
            if (!AssertNotNull(wrapped, func)) return new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentException(), "Passed-in function delegate is null");

            SafeWrapperResult result = await wrapped;

            if (result)
            {
                return await SafeWrapAsync(() => func(wrapped.Result));
            }

            return result;
        }

        #endregion

        #region Private Helpers

        private static bool AssertNotNull(params object[] objectsToCheck)
        {
            return !objectsToCheck.Any((item) => item == null);
        }

        private static ISafeWrapperExceptionReporter TryGetReporter(ISafeWrapperExceptionReporter defaultReporter = null)
        {
            if (defaultReporter == null)
            {
                return StaticExceptionReporters.DefaultSafeWrapperExceptionReporter;
            }

            return defaultReporter;
        }

        #endregion
    }

}
