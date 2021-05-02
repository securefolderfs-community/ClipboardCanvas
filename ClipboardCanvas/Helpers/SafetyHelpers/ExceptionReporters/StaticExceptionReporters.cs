namespace ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters
{
    public static class StaticExceptionReporters
    {
        public static ISafeWrapperExceptionReporter ClipboardGetDataExceptionReporter = new ClipboardGetDataExceptionReporter();

        public static ISafeWrapperExceptionReporter DefaultSafeWrapperExceptionReporter = new DefaultSafeWrapperExceptionReporter();
    }
}
