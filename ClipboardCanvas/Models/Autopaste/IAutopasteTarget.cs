using ClipboardCanvas.Helpers.SafetyHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace ClipboardCanvas.Models.Autopaste
{
    public interface IAutopasteTarget
    {
        string Name { get; }

        Task<SafeWrapperResult> PasteData(DataPackageView dataPackage);
    }
}
