using System;

namespace BulkOperations
{
    public static class ConsoleLog
    {
        public static void Write(string value)
        {
            Console.WriteLine($"{DateTime.Now} {value}");
        }
    }
}