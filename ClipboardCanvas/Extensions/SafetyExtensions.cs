using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.Extensions
{
    public static class SafetyExtensions
    {
        public static void InitNull<T>(ref T value) where T : new()
        {
            if (EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                value = new();
            }
        }
    }
}
