using ClipboardCanvas.UnsafeNative;
using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ClipboardCanvas.Extensions
{
    public static class SafetyExtensions
    {
        public static T PreventNull<T>(this T element, T defaultValue, bool throwOnNullDefaultValue = true)
        {
            if (element.IsNull()) // Is null
            {
                if (defaultValue.IsNull() && throwOnNullDefaultValue) // Default value is null
                {
                    Debugger.Break();
                    throw new NullReferenceException($"[PreventNull] Provided defautValue was null! Type: {typeof(T)}");
                }
                else
                {
                    return defaultValue;
                }
            }
            else
            {
                return element;
            }
        }

        public static TSafeHandle ToSafeHandle<TSafeHandle>(this IntPtr unsafeHandle, Func<IntPtr, TSafeHandle> createSafeHandle) where TSafeHandle : SafeHandle =>
            createSafeHandle(unsafeHandle);

        public static SafeFileHandle ToSafeFileHandle(this IntPtr unsafeHandle, bool ownsHandle = true) =>
            ToSafeHandle(unsafeHandle, (h) => new SafeFileHandle(h, ownsHandle));

        public static bool CloseFileHandle(this IntPtr unsafeFileHandle) =>
            UnsafeNativeApis.CloseHandle(unsafeFileHandle);
    }
}
