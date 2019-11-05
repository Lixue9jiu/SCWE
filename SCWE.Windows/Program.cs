using System;

namespace SCWE.Windows
{
    class Program
    {
        const string DataFolder = "data";

        static void Main(string[] args)
        {
            Console.WriteLine(args.Length);
            // TODO: write a command-line interface
        }

        static void EnterToContinue()
        {
            Console.Write("按Enter以继续");
            Console.ReadLine();
        }
    }
}
