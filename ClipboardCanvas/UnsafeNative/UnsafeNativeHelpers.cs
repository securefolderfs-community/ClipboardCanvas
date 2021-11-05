using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using static ClipboardCanvas.UnsafeNative.UnsafeNativeDataModels;

namespace ClipboardCanvas.UnsafeNative
{
    public static class UnsafeNativeHelpers
    {
        public static IntPtr CreateFileForWrite(string filePath, bool overwrite = true)
        {
            return UnsafeNativeApis.CreateFileFromApp(filePath, GENERIC_WRITE, 0, IntPtr.Zero, overwrite ? CREATE_ALWAYS : OPEN_ALWAYS, (uint)File_Attributes.BackupSemantics, IntPtr.Zero);
        }

        public static IntPtr CreateFileForRead(string filePath)
        {
            return UnsafeNativeApis.CreateFileFromApp(filePath, GENERIC_READ, 0, IntPtr.Zero, OPEN_EXISTING, (uint)File_Attributes.BackupSemantics, IntPtr.Zero);
        }

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
            {
                return null;
            }

            byte[] buffer = new byte[4096];
            int dwBytesRead;
            string szRead = null;

            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    UnsafeNativeApis.ReadFile(hStream, pBuffer, 4096 - 1, &dwBytesRead, IntPtr.Zero);
                }
            }

            using (StreamReader reader = new StreamReader(new MemoryStream(buffer, 0, dwBytesRead), true))
            {
                szRead = reader.ReadToEnd();
            }

            UnsafeNativeApis.CloseHandle(hStream);

            return szRead;
        }

        public static bool WriteStringToFile(string filePath, string write)
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

            byte[] buffer = Encoding.UTF8.GetBytes(write);
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

        public static long GetFileId(string filePath)
        {
            IntPtr hFile = UnsafeNativeApis.CreateFileFromApp(filePath,
                GENERIC_READ,
                (uint)FileShare.ReadWrite,
                IntPtr.Zero,
                OPEN_EXISTING,
                (uint)File_Attributes.BackupSemantics, IntPtr.Zero);

            if (hFile.ToInt64() == -1)
            {
                return -1;
            }

            BY_HANDLE_FILE_INFORMATION lpFileInformation = new BY_HANDLE_FILE_INFORMATION();
            uint dwFileInformationLength = (uint)Marshal.SizeOf(lpFileInformation);

            bool result = UnsafeNativeApis.GetFileInformationByHandleEx(hFile, FILE_INFO_BY_HANDLE_CLASS.FileIdBothDirectoryInfo, out lpFileInformation, dwFileInformationLength);

            long lFileId = -1;
            if (result)
            {
                lFileId = ((long)lpFileInformation.FileIndexHigh << 32) + (long)lpFileInformation.FileIndexLow;
            }

            UnsafeNativeApis.CloseHandle(hFile);

            return lFileId;
        }
    }
}
