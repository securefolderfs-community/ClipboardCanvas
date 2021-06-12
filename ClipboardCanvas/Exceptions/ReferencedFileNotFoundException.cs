using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ClipboardCanvas.Exceptions
{
    public class ReferencedFileNotFoundException : Exception
    {
        public ReferencedFileNotFoundException()
            : base(new FileNotFoundException().Message)
        {
        }
    }
}
