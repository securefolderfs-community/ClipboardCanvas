using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using ClipboardCanvas.Helpers.Filesystem;
using ClipboardCanvas.Extensions;

namespace ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules
{
    public sealed class FileSizeRuleViewModel : BaseAutopasteRuleViewModel
    {
        private long _MaxFileSize;
        public long MaxFileSize
        {
            get => _MaxFileSize;
            set => SetProperty(ref _MaxFileSize, value);
        }

        private long _MinFileSize;
        public long MinFileSize
        {
            get => _MinFileSize;
            set => SetProperty(ref _MinFileSize, value);
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
                        return fileSize >= MinFileSize && fileSize <= MaxFileSize;
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
