using System.Diagnostics;

namespace BulkOperations
{
    public static class StopwatchExtension
    {
        public static string ElapsedTimeFmt(this Stopwatch stopwatch)
        {
            var elapsed = stopwatch.Elapsed;
            return $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds / 10:00}";
        }
    }
}
