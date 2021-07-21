using ClipboardCanvas.Enums;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClipboardCanvas.Helpers.SafetyHelpers
{
    public static class SafeWrapperRoutines
    {
        private static readonly SafeWrapperResult NullFunctionDelegateResult = new SafeWrapperResult(OperationErrorCode.InvalidArgument, new ArgumentException(), "Passed-in function delegate is null");

        public static SafeWrapper<T> SafeWrap<T>(Func<T> func, ISafeWrapperExceptionReporter reporter = null)
        {
            if (!AssertNotNull(func)) return new SafeWrapper<T>(default(T), NullFunctionDelegateResult);

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
            if (!AssertNotNull(action)) return NullFunctionDelegateResult;

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

        public static SafeWrapper<T2> OnSuccess<T1, T2>(this SafeWrapper<T1> wrapped, Func<SafeWrapper<T1>, T2> func)
        {
            if (!AssertNotNull(wrapped, func)) return new SafeWrapper<T2>(default(T2), NullFunctionDelegateResult);

            SafeWrapperResult result = wrapped;

            if (result)
            {
                return SafeWrap(() => func(wrapped));
            }

            return new SafeWrapper<T2>(default(T2), result);
        }

        public static SafeWrapper<T2> OnFailure<T1, T2>(this SafeWrapper<T1> wrapped, Func<SafeWrapper<T1>, T2> func)
        {
            if (!AssertNotNull(wrapped, func)) return new SafeWrapper<T2>(default(T2), NullFunctionDelegateResult);

            SafeWrapperResult result = wrapped;

            if (!result)
            {
                return SafeWrap(() => func(wrapped));
            }

            return new SafeWrapper<T2>(default(T2), result);
        }

        #region Async Functions

        public static async Task<SafeWrapper<T>> SafeWrapAsync<T>(Func<Task<T>> func, ISafeWrapperExceptionReporter reporter = null)
        {
            if (!AssertNotNull(func)) return new SafeWrapper<T>(default(T), NullFunctionDelegateResult);

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
            if (!AssertNotNull(func)) return NullFunctionDelegateResult;

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

        public static async Task<SafeWrapper<T2>> OnSuccessAsync<T1, T2>(this Task<SafeWrapper<T1>> wrapped, Func<SafeWrapper<T1>, Task<T2>> func)
        {
            if (!AssertNotNull(wrapped, func)) return new SafeWrapper<T2>(default(T2), NullFunctionDelegateResult);

            SafeWrapperResult result = await wrapped;

            if (result)
            {
                return await SafeWrapAsync(() => func(wrapped.Result));
            }

            return new SafeWrapper<T2>(default(T2), result);
        }

        public static async Task<SafeWrapper<T2>> OnFailureAsync<T1, T2>(this Task<SafeWrapper<T1>> wrapped, Func<SafeWrapper<T1>, Task<T2>> func)
        {
            if (!AssertNotNull(wrapped, func)) return new SafeWrapper<T2>(default(T2), NullFunctionDelegateResult);

            SafeWrapperResult result = await wrapped;

            if (!result)
            {
                return await SafeWrapAsync(() => func(wrapped.Result));
            }

            return new SafeWrapper<T2>(default(T2), result);
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
