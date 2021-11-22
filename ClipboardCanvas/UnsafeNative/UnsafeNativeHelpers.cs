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
            IntPtr hFile = UnsafeNativeApis.CreateFileFromApp(filePath,
                GENERIC_READ,
                FILE_SHARE_READ,
                IntPtr.Zero,
                OPEN_EXISTING,
                (uint)File_Attributes.BackupSemantics,
                IntPtr.Zero);

            if (hFile.ToInt64() == -1)
            {
                return null;
            }

            const int BUFFER_LENGTH = 4096;

            byte[] buffer = new byte[BUFFER_LENGTH];
            int dwBytesRead;
            string szRead = string.Empty;

            unsafe
            {
                bool bRead = false;

                using (MemoryStream msBuffer = new MemoryStream(buffer))
                {
                    using (StreamReader reader = new StreamReader(msBuffer, true))
                    {
                        do
                        {
                            fixed (byte* pBuffer = buffer)
                            {
                                Array.Clear(buffer, 0, buffer.Length);
                                msBuffer.Position = 0;

                                if (bRead = UnsafeNativeApis.ReadFile(hFile, pBuffer, BUFFER_LENGTH - 1, &dwBytesRead, IntPtr.Zero) && dwBytesRead > 0)
                                {
                                    szRead += reader.ReadToEnd().Substring(0, dwBytesRead);
                                }
                                else
                                {
                                    break;
                                }
                            }

                        } while (bRead);
                    }
                }
            }

            UnsafeNativeApis.CloseHandle(hFile);

            return szRead;
        }

        public static bool WriteStringToFile(string filePath, string write)
        {
            IntPtr hFile = UnsafeNativeApis.CreateFileFromApp(filePath,
                GENERIC_WRITE,
                0,
                IntPtr.Zero,
                CREATE_ALWAYS,
                (uint)File_Attributes.BackupSemantics, IntPtr.Zero);

            if (hFile.ToInt64() == -1)
            {
                return false;
            }

            byte[] buffer = Encoding.UTF8.GetBytes(write);
            int dwBytesWritten;
            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    UnsafeNativeApis.WriteFile(hFile, pBuffer, buffer.Length, &dwBytesWritten, IntPtr.Zero);
                }
            }

            UnsafeNativeApis.CloseHandle(hFile);
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
