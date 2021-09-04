using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClipboardCanvas.Helpers
{
    public static class TaskHelpers
    {
        public static bool RunOperationWithTimeout(Action action, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Task<Action> task = Task.Run(() => action, cancellationToken);

            return task.Wait(timeout);
        }

        public static async Task<bool> RunTaskWithTimeout(Task task, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (await Task.WhenAny(task, Task.Delay(timeout, cancellationToken)) == task)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
