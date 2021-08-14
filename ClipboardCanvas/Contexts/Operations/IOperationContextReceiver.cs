using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardCanvas.Contexts.Operations
{
    public interface IOperationContextReceiver
    {
        IOperationContext GetOperationContext();
    }
}
