using ClipboardCanvas.Enums;

namespace ClipboardCanvas.Contexts.Operations
{
    public interface IOperationContextReceiver
    {
        IOperationContext GetOperationContext(string operationName, StatusCenterOperationType operationType);
    }
}
