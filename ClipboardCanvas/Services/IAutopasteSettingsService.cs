using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.Services
{
    public interface IAutopasteSettingsService
    {
        string AutopastePath { get; set; }
    }
}
