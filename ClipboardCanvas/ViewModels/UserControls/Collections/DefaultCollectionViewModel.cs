using Windows.Storage;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;

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

        public override CollectionConfigurationModel ConstructConfigurationModel()
        {
            return new CollectionConfigurationModel(Constants.Collections.DEFAULT_COLLECTION_TOKEN, UsesCustomIcon, iconFile?.Name);
        }

        #endregion
    }
}
