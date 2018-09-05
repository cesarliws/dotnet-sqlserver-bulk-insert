using System;

namespace BulkOperations
{
    public static class Log
    {
        public static void Write(string value)
        {
            Console.WriteLine($"{DateTime.Now} {value}");
        }
    }
}