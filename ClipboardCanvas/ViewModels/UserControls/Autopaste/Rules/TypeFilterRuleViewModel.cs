using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Newtonsoft.Json;

using ClipboardCanvas.Models.Autopaste;
using ClipboardCanvas.Helpers;
using ClipboardCanvas.Helpers.SafetyHelpers;
using System.Collections.Generic;
using Windows.Storage;
using System.Linq;

namespace ClipboardCanvas.ViewModels.UserControls.Autopaste.Rules
{
    internal class TypeFilterRuleViewModel : BaseAutopasteRuleViewModel
    {
        [JsonIgnore]
        private int _SelectedIndex;
        public int SelectedIndex
        {
            get => _SelectedIndex;
            set
            {
                if (SetProperty(ref _SelectedIndex, value))
                {
                    RuleActions?.SerializeRules();
                }
            }
        }

        public TypeFilterRuleViewModel(IRuleActions ruleActions)
            : base(ruleActions)
        {
            ruleName = "Type filter";
            ruleFontIconGlyph = "\uE71C";
        }

        public override async Task<bool> PassesRule(DataPackageView dataPackage)
        {
            switch (_SelectedIndex)
            {
                case 0: // Image
                    {
                        if (dataPackage.Contains(StandardDataFormats.Bitmap))
                        {
                            return false;
                        }

                        break;
                    }

                case 1: // Text
                    {
                        if (dataPackage.Contains(StandardDataFormats.Text))
                        {
                            SafeWrapper<string> result = await dataPackage.SafeGetTextAsync();

                            if (result && !await WebHelpers.IsValidUrl(result))
                            {
                                return false;
                            }
                        }

                        break;
                    }

                case 2: // File
                    {
                        if (dataPackage.Contains(StandardDataFormats.StorageItems))
                        {
                            SafeWrapper<IReadOnlyList<IStorageItem>> result = await dataPackage.SafeGetStorageItemsAsync();

                            if (result && result.Result.Any((item) => item is IStorageFile))
                            {
                                return false;
                            }
                        }

                        break;
                    }

                case 3: // Url
                    {
                        if (dataPackage.Contains(StandardDataFormats.Text))
                        {
                            SafeWrapper<string> result = await dataPackage.SafeGetTextAsync();

                            if (result && await WebHelpers.IsValidUrl(result))
                            {
                                return false;
                            }
                        }

                        break;
                    }

                case 4: // Folder
                    {
                        if (dataPackage.Contains(StandardDataFormats.StorageItems))
                        {
                            SafeWrapper<IReadOnlyList<IStorageItem>> result = await dataPackage.SafeGetStorageItemsAsync();

                            if (result && result.Result.Any((item) => item is IStorageFolder))
                            {
                                return false;
                            }
                        }

                        break;
                    }

                default:
                    return false;
            }

            return true;
        }
    }
}
