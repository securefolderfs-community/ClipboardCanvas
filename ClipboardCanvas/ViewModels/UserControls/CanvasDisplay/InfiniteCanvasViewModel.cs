using ClipboardCanvas.DataModels.PastedContentDataModels;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters;
using ClipboardCanvas.ModelViews;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace ClipboardCanvas.ViewModels.UserControls.CanvasDisplay
{
    public class InfiniteCanvasViewModel : BaseCanvasViewModel
    {
        #region Constructor

        public InfiniteCanvasViewModel(IBaseCanvasPreviewControlView view)
            : base (StaticExceptionReporters.DefaultSafeWrapperExceptionReporter, new InfiniteCanvasContentType(), view)
        {
        }

        #endregion

        #region Override

        public override Task<SafeWrapperResult> TrySaveData()
        {
            return Task.FromResult(SafeWrapperResult.S_CANCEL);
        }

        protected override Task<SafeWrapperResult> SetData(DataPackageView dataPackage)
        {
            return Task.FromResult(SafeWrapperResult.S_CANCEL);
        }

        protected override Task<SafeWrapperResult> SetDataFromExistingFile(IStorageItem item)
        {
            return Task.FromResult(SafeWrapperResult.S_CANCEL);
        }

        protected override Task<SafeWrapperResult> TryFetchDataToView()
        {
            return Task.FromResult(SafeWrapperResult.S_CANCEL);
        }

        protected override Task<SafeWrapper<CollectionItemViewModel>> TrySetFileWithExtension()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
