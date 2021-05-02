using System;
using System.IO;
using ClipboardCanvas.Enums;
using Windows.Storage;

namespace ClipboardCanvas.Helpers.SafetyHelpers.ExceptionReporters
{
    public class DefaultSafeWrapperExceptionReporter : ISafeWrapperExceptionReporter
    {
        // TODO: Implement proper error reporting here

        public SafeWrapperResultDetails GetStatusResult(Exception e)
        {
            return GetStatusResult(e, null);
        }

        public SafeWrapperResultDetails GetStatusResult(Exception e, Type callerType)
        {
            if (e is UnauthorizedAccessException)
            {
                return (OperationErrorCode.Unauthorized, e, "Access is unauthorized");
            }
            else if (e is FileNotFoundException // Item was deleted
                || e is System.Runtime.InteropServices.COMException // Item's drive was ejected
                || (uint)e.HResult == 0x8007000F) // The system cannot find the drive specified
            {
                return (OperationErrorCode.NotFound, e, "File was not found");
            }
            else if (e is IOException || e is FileLoadException)
            {
                return (OperationErrorCode.InUse, e, "The file is in use");
            }
            else if (e is PathTooLongException)
            {
                return (OperationErrorCode.NameTooLong, e, "Path is too long");
            }
            else if (e is ArgumentException) // Item was invalid
            {
                return (callerType == typeof(StorageFolder) || callerType == typeof(OperationErrorCode)) ?
                    (OperationErrorCode.NotAFolder, e, "Item is not a folder") : (OperationErrorCode.NotAFile, e, "Item is not a file");
            }
            else if ((uint)e.HResult == 0x800700B7)
            {
                return (OperationErrorCode.AlreadyExists, e, "Item already exists");
            }
            else if ((uint)e.HResult == 0x800700A1 // The specified path is invalid (usually an mtp device was disconnected)
                || (uint)e.HResult == 0x8007016A // The cloud file provider is not running
                || (uint)e.HResult == 0x8000000A) // The data necessary to complete this operation is not yet available)
            {
                return (OperationErrorCode.Generic, e, "Generic error");
            }
            else
            {
                return (OperationErrorCode.Generic, e, "Generic error");
            }
        }
    }
}
