using System;
using HashModule;

namespace IceHashClient
{
    public class Client : Ice.Application
    {
        public override int run(string[] args)
        {
            Console.WriteLine("1");
            Ice.ObjectPrx obj = communicator ().stringToProxy (@"HashServer");
            Console.WriteLine("2");
            //(@"SimplePrinter@SimplePrinterAdapter");
            HashPrx hashModule = HashPrxHelper.checkedCast(obj);
            Console.WriteLine("3");
            if(hashModule == null)
                throw new ApplicationException("Invalid proxy");
            /*
            if(hashModule.SrvPing() == 1)
                Console.WriteLine("Server is Alive!");
            else
                Console.WriteLine("Server is Ded!");
            */
            /*
            PrinterPrx printer = PrinterPrxHelper.checkedCast(obj);
            if (printer == null)
                    throw new ApplicationException("Invalid proxy");
            Console.WriteLine (printer.printString ("registry!"));
            */
            
            return 0;
                
        }
        
        public static void Main(string[] args)
        {
            Client cln = new Client ();
            Environment.Exit(cln.main (args));
        }

    }
}

