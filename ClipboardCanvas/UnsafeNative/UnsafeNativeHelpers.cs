using System;
using System.IO;
using System.Text;
using static ClipboardCanvas.UnsafeNative.UnsafeNativeDataModels;

namespace ClipboardCanvas.UnsafeNative
{
    public static class UnsafeNativeHelpers
    {
        public static string ReadStringFromFile(string filePath)
        {
            IntPtr hStream = UnsafeNativeApis.CreateFileFromApp(filePath,
                GENERIC_READ,
                0,
                IntPtr.Zero,
                OPEN_EXISTING,
                (uint)File_Attributes.BackupSemantics,
                IntPtr.Zero);

            if (hStream.ToInt64() == -1)
                return null;

            byte[] buffer = new byte[4096];
            int dwBytesRead;
            string str = null;

            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    UnsafeNativeApis.ReadFile(hStream, pBuffer, 4096 - 1, &dwBytesRead, IntPtr.Zero);
                }
            }

            using (StreamReader reader = new StreamReader(new MemoryStream(buffer, 0, dwBytesRead), true))
            {
                str = reader.ReadToEnd();
            }

            UnsafeNativeApis.CloseHandle(hStream);
            return str;
        }

        public static bool WriteStringToFile(string filePath, string str)
        {
            IntPtr hStream = UnsafeNativeApis.CreateFileFromApp(filePath,
                GENERIC_WRITE,
                0,
                IntPtr.Zero,
                CREATE_ALWAYS,
                (uint)File_Attributes.BackupSemantics, IntPtr.Zero);

            if (hStream.ToInt64() == -1)
            {
                return false;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(str);
            int dwBytesWritten;
            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    UnsafeNativeApis.WriteFile(hStream, pBuffer, buffer.Length, &dwBytesWritten, IntPtr.Zero);
                }
            }

            UnsafeNativeApis.CloseHandle(hStream);
            return true;
        }
    }
}
