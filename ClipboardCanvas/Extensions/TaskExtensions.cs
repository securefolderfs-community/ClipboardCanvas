using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipboardCanvas.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<IEnumerable<T>> WaitAll<T>(this IEnumerable<Task<T>> tasks)
        {
            IList<T> results = new List<T>();

            foreach (var task in tasks)
            {
                results.Add(await task);
            }

            return results;
        }
    }
}
