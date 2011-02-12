using System;
using HashModule;

namespace IceHashServer
{
    public class Client
    {
        private Ice.Communicator _communicator;
        
        public Client(Ice.Communicator communicator)
        {
            _communicator = communicator;
        }
        
        public void Run()
        {
            Thread.Sleep(10000);
            int i = 0;
            
            while (true)
            {
                HashPrx hashModule = getClientObject(_communicator);
                
                if (hashModule != null)
                {
                    if(hashModule.SrvPing() == 1)
                        Console.WriteLine("Server is Alive!");
                    else
                        Console.WriteLine("Server is Dead!");
                }
                else
                {
                    Console.WriteLine("HashModule proxy is null");
                }
                
                if (++i == 30)
                {
                    srvHashModule = new HashModuleImpl();
                    _adapter.add(srvHashModule, Ice.Util.stringToIdentity("IIceHashService"));
                    _adapter.activate();
                }
                
                Thread.Sleep(1000);
            }
            
            /*
            Console.WriteLine("1");
            Ice.ObjectPrx obj = communicator ().stringToProxy (
                                @"IIceHashService"
                                                              );
            if (obj == null)
                Console.WriteLine("obj jest nullem!");
            Console.WriteLine("2");
            //(@"SimplePrinter@SimplePrinterAdapter");
            HashPrx hashModule = HashPrxHelper.checkedCast(obj);
            Console.WriteLine("3");
            if(hashModule == null)
                throw new ApplicationException("Invalid proxy");
            
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
            
            return 0;
                
        }
        /*
        public static void Main(string[] args)
        {
            Client cln = new Client ();
            Environment.Exit(cln.main (args));
        }
        */
        
        protected HashPrx getClientObject()
        {
            Ice.ObjectPrx obj = _communicator.stringToProxy( @"IIceHashService");
            
            Console.WriteLine("Communicator proxy created");
            if (obj == null)
            {
                Console.WriteLine("Created proxy is null");
            }
            HashPrx hashModule = HashPrxHelper.checkedCast(obj.ice_twoway());
            if(hashModule == null)
                Console.WriteLine("Invalid proxy");
            
            return hashModule;
        }
    }
}

