using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.EventArguments
{
    public class CanvasRenameRequestedEventArgs : EventArgs
    {
        public readonly string newName;

        public CanvasRenameRequestedEventArgs(string newName)
        {
            this.newName = newName;
        }
    }
}
