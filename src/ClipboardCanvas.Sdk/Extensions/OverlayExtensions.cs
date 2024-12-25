using ClipboardCanvas.Sdk.Enums;
using ClipboardCanvas.Shared.ComponentModel;
using ClipboardCanvas.Shared.Helpers;

namespace ClipboardCanvas.Sdk.Extensions
{
    public static class OverlayExtensions
    {
        public static IResult ParseOverlayOption(this DialogOption dialogOption)
        {
            return dialogOption switch
            {
                DialogOption.Cancel => Result<DialogOption>.Failure(dialogOption),
                _ => Result<DialogOption>.Success(dialogOption)
            };
        }

        public static bool Positive(this IResult result)
        {
            return result is IResult<DialogOption> optionResult
                    ? optionResult.Value == DialogOption.Primary
                    : result.Successful;
        }

        public static bool InBetween(this IResult result)
        {
            return result is IResult<DialogOption> optionResult
                    ? optionResult.Value == DialogOption.Secondary
                    : !result.Successful;
        }

        public static bool Aborted(this IResult<DialogOption> result)
        {
            return result is IResult<DialogOption> optionResult
                    ? optionResult.Value == DialogOption.Cancel
                    : !result.Successful;
        }
    }
}
