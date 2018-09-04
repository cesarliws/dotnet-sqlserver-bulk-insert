using System;
using System.Diagnostics;

namespace BulkOperations
{
    static class Program
    {
        static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Console.WriteLine($"{DateTime.Now} Início processamento");

            BulkOperation.InsertAsync().Wait();

            stopwatch.Stop();

            Console.WriteLine($"{DateTime.Now} Processamento concluído em {stopwatch.ElapsedTimeFmt()}");

            Console.WriteLine("");
            Console.WriteLine("Tecle <ENTER> para finalizar...");
            Console.ReadLine();
        }
    }
}
