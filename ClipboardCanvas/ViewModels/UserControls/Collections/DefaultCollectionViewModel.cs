using Windows.Storage;
using ClipboardCanvas.Helpers.SafetyHelpers;

namespace ClipboardCanvas.ViewModels.UserControls.Collections
{
    public class DefaultCollectionViewModel : BaseCollectionViewModel
    {
        #region Constructor

        public DefaultCollectionViewModel(StorageFolder collectionFolder)
            : base(collectionFolder)
        {
        }

        #endregion

        #region Override

        public override bool CheckCollectionAvailability()
        {
            SetCollectionError(SafeWrapperResult.SUCCESS);
            return true;
        }

        #endregion
    }
}
