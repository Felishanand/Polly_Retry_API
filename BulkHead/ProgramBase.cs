using Polly;
using System;
using System.Threading.Tasks;

namespace BulkHead
{
    public class ProgramBase
    {

        public static Task onBulkheadRejectedAsync(Context context)
        {
            Console.WriteLine($"PolyDemo OnBulkheadRejected Async Excuted.");
            return Task.CompletedTask;
        }
    }
}