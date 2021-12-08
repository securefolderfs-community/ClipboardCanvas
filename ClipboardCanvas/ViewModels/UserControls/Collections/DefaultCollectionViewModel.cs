using Windows.Storage;
using ClipboardCanvas.GlobalizationExtensions;

using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Models;

namespace ClipboardCanvas.ViewModels.UserControls.Collections
{
    public class DefaultCollectionViewModel : BaseCollectionViewModel
    {
        #region Properties

        public override string DisplayName => "DefaultCollection".GetLocalized();

        #endregion

        #region Constructor

        public DefaultCollectionViewModel(StorageFolder collectionFolder)
            : base(collectionFolder)
        {
            canBeSetAsAutopasteTarget = true;
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
