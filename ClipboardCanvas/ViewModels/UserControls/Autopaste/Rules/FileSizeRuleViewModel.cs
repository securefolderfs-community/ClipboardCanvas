using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using ByteSizeLib; 

using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Models.Autopaste;

namespace ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules
{
    public sealed class FileSizeRuleViewModel : BaseAutopasteRuleViewModel
    {
        private double _MaxFileSize;
        public double MaxFileSize
        {
            get => _MaxFileSize;
            set
            {
                if (SetProperty(ref _MaxFileSize, value))
                {
                    RuleActions?.SerializeRules();
                }
            }
        }

        private double _MinFileSize;
        public double MinFileSize
        {
            get => _MinFileSize;
            set
            {
                if (SetProperty(ref _MinFileSize, value))
                {
                    RuleActions?.SerializeRules();
                }
            }
        }

        public FileSizeRuleViewModel(IRuleActions ruleActions)
            : base(ruleActions)
        {
            ruleName = "File size";
            _MaxFileSize = 8.0d;
        }

        public override async Task<bool> PassesRule(DataPackageView dataPackage)
        {
            if (dataPackage.Contains(StandardDataFormats.StorageItems))
            {
                SafeWrapper<IReadOnlyList<IStorageItem>> items = await dataPackage.SafeGetStorageItemsAsync();

                if (!items)
                {
                    return false;
                }

                foreach (var item in items.Result)
                {
                    if (item is IStorageFile storageFile)
                    {
                        long fileSize = await storageFile.GetFileSize();
                        return fileSize >= ByteSize.FromMegaBytes(MinFileSize).Bytes && fileSize <= ByteSize.FromMegaBytes(MaxFileSize).Bytes;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
