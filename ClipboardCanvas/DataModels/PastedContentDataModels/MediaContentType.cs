namespace ClipboardCanvas.DataModels.PastedContentDataModels
{
    public class MediaContentType : BasePastedContentTypeDataModel
    {
        public override bool Equals(BasePastedContentTypeDataModel other)
        {
            if (other is BasePastedContentTypeDataModel thisOther)
            {
                return true;
            }

            return false;
        }
    }
}
