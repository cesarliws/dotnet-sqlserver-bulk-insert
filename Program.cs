using System;

namespace BulkOperations
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            //Console.WriteLine("BUG");
            //StateMachineBug.Run();

            //Console.WriteLine("\nMEMBER");
            //StateMachineMember.Run();

            //Console.WriteLine("\nON/OFF");
            //StateMachineOnOff.Run();

            //Console.WriteLine("\nPHONE CALL");
            //StateMachinePhoneCall.Run();

            BulkOperation.Run();

            Console.WriteLine("");
            Console.WriteLine("Tecle <ENTER> para finalizar...");
            Console.ReadKey(false);
        }
    }
}