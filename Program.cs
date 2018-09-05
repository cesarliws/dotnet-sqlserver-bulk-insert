using System;
using System.Diagnostics;

namespace BulkOperations
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Log.Write("Início processamento");

            BulkOperation.InsertAsync().Wait();

            stopwatch.Stop();


            Log.Write($"Processamento concluído em {stopwatch.ElapsedTimeFmt()}");

            Console.WriteLine("");
            Console.WriteLine("Tecle <ENTER> para finalizar...");
            Console.ReadLine();
        }
    }
}