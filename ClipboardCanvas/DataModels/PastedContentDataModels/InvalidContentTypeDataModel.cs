using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.DataModels.PastedContentDataModels
{
    public sealed class InvalidContentTypeDataModel : BasePastedContentTypeDataModel
    {
        public readonly SafeWrapperResult error;

        public readonly bool needsReinitialization;

        public InvalidContentTypeDataModel(SafeWrapperResult error, bool needsReinitialization)
        {
            this.error = error;
            this.needsReinitialization = needsReinitialization;
        }
    }
}
