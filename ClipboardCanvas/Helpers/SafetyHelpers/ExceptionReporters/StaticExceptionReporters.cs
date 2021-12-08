namespace ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters
{
    public static class StaticExceptionReporters
    {
        public static ISafeWrapperExceptionReporter DefaultSafeWrapperExceptionReporter = new DefaultSafeWrapperExceptionReporter();
    }
}
