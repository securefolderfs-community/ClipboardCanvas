using System;
using System.Runtime.InteropServices;
using static ClipboardCanvas.UnsafeNative.UnsafeNativeDataModels;

namespace ClipboardCanvas.UnsafeNative
{
    public static unsafe class UnsafeNativeApis
    {
        [DllImport("api-ms-win-core-file-fromapp-l1-1-0.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall,
        SetLastError = true)]
        public static extern IntPtr CreateFileFromApp(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr SecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile
        );

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        [DllImport("api-ms-win-core-file-fromapp-l1-1-0.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall,
        SetLastError = true)]
        public static extern bool CreateDirectoryFromApp(
            string lpPathName,
            IntPtr SecurityAttributes
        );

        [DllImport("api-ms-win-core-file-l1-2-1.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall,
        SetLastError = true)]
        public unsafe static extern bool ReadFile(
            IntPtr hFile,
            byte* lpBuffer,
            int nBufferLength,
            int* lpBytesReturned,
            IntPtr lpOverlapped
        );

        [DllImport("api-ms-win-core-file-l1-2-1.dll", CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall,
        SetLastError = true)]
        public unsafe static extern bool WriteFile(
            IntPtr hFile,
            byte* lpBuffer,
            int nBufferLength,
            int* lpBytesWritten,
            IntPtr lpOverlapped
        );

        [DllImport("api-ms-win-core-file-fromapp-l1-1-0.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileAttributesExFromApp(
            string lpFileName,
            GET_FILEEX_INFO_LEVELS fInfoLevelId,
            out WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);

        [DllImport("api-ms-win-core-file-l2-1-1.dll", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetFileInformationByHandleEx(
            IntPtr hFile,
            FILE_INFO_BY_HANDLE_CLASS FileInformationClass,
            out BY_HANDLE_FILE_INFORMATION lpFileInformation,
            uint dwBufferSize);

        [DllImport("api-ms-win-core-errorhandling-l1-1-1.dll")]
        public static extern uint GetLastError();

        [DllImport("api-ms-win-core-handle-l1-1-0.dll")]
        public static extern bool CloseHandle(IntPtr hObject);
    }
}
