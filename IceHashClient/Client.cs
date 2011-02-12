using System;
using HashModule;

namespace IceHashClient
{
    public class Client : Ice.Application
    {
        public override int run(string[] args)
        {
            Ice.Communicator ic = null;
            try
            {
                ic = Ice.Util.initialize(ref args);
                Ice.ObjectPrx obj = ic.stringToProxy (@"HashRegistry: tcp -h localhost -p 1231");
                HashRegisterPrx hashModule = HashRegisterPrxHelper.checkedCast(obj);
                /*
                Console.WriteLine("3");
                if(hashModule == null)
                    throw new ApplicationException("Invalid proxy");
                */
                Console.WriteLine("NAME: {0}", hashModule.getHashId("endpoint"));
                
                /*
                if(hashModule.SrvPing() == 1)
                    Console.WriteLine("Server is Alive!");
                else
                    Console.WriteLine("Server is Dead!");
                */
                /*
                PrinterPrx printer = PrinterPrxHelper.checkedCast(obj);
                if (printer == null)
                        throw new ApplicationException("Invalid proxy");
                Console.WriteLine (printer.printString ("registry!"));
                */
            } catch (System.Exception ex) {
                Console.WriteLine(ex);   
            }
            
            return 0;
                
        }
        
        public static void Main(string[] args)
        {
            Client cln = new Client ();
            Environment.Exit(cln.main (args));
        }
    }
}

