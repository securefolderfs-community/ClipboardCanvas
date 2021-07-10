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
                return (OperationErrorCode.AccessUnauthorized, e, App.IsInRestrictedAccessMode ? "Couldn't access this path because Clipboard Canvas is in Restricted Access mode. Access is unauthorized."
                    : "Access is unauthorized.");
            }
            else if (e is FileNotFoundException) // File not found
            {
                return (OperationErrorCode.NotFound, e, "The file was not found.");
            }
            else if (e is DirectoryNotFoundException) // Folder not found
            {
                return (OperationErrorCode.NotFound, e, "The folder was not found.");
            }
            else if (e is ArgumentException) // Item was invalid
            {
                return callerType == typeof(StorageFolder) ?
                    (OperationErrorCode.NotAFolder, e, "Item is not a folder.") : (OperationErrorCode.NotAFile, e, "Item is not a file.");
            }
            else if ((uint)e.HResult == 0x800700B7)
            {
                return (OperationErrorCode.AlreadyExists, e, "Item already exists.");
            }
            else
            {
                return SafeWrapperResult.S_UNKNOWN_FAIL.Details;
            }
        }
    }
}
