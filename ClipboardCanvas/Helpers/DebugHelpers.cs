using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.Helpers
{
    public static class DebugHelpers
    {
        public static void PrintEnumerable(this IEnumerable enumerable)
        {
#if !DEBUG
            return;
#endif

            foreach (var item in enumerable)
            {
                Debug.WriteLine(item);
            }
        }
    }
}
