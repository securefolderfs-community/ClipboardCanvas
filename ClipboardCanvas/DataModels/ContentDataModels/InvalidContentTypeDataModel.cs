using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.DataModels.ContentDataModels
{
    public sealed class InvalidContentTypeDataModel : BaseContentTypeModel
    {
        public readonly SafeWrapperResult error;

        public readonly bool needsReinitialization;

        public InvalidContentTypeDataModel(SafeWrapperResult error)
            : this(error, false)
        {
        }

        public InvalidContentTypeDataModel(SafeWrapperResult error, bool needsReinitialization)
        {
            this.error = error;
            this.needsReinitialization = needsReinitialization;
        }
    }
}
