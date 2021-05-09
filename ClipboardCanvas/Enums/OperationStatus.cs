using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.Enums
{
    /// <summary>
    /// Contains all kinds of return statuses
    /// </summary>
    public enum OperationErrorCode : uint
    {
        Success = 0,
        Generic = 1,
        Unauthorized = 2,
        NotFound = 4,
        InUse = 8,
        NameTooLong = 16,
        AlreadyExists = 32,
        NotAFolder = 64,
        NotAFile = 128,
        InProgress = 256,
        InvalidArgument = 512
    }
}
