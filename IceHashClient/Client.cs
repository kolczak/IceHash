using System;
using HashModule;

namespace IceHashClient
{
    public class Client : Ice.Application
    {
        public override int run(string[] args)
        {
            Ice.Communicator ic = null;
            Ice.ObjectPrx hashObj;
            try
            {
                if (args.Length >= 2)
                {
                    ic = Ice.Util.initialize(ref args);
                    hashObj = ic.stringToProxy (@"IceHash:" + args[0]);
                    if (hashObj == null)
                    {
                        Console.WriteLine("IceHash proxy with endpoint {0} is null", args[0]);
                        return -1;
                    }
                    HashPrx hashModule = HashPrxHelper.checkedCast(hashObj.ice_twoway());
                    if(hashModule == null)
                    {
                        Console.WriteLine("Invalid proxy");
                        return -2;
                    }
                    
                    switch (args[1])
                    {
                    case "get":
                        Console.WriteLine(hashModule.Get(Int32.Parse(args[2])));
                        break;
                    case "push":
                        hashModule.Push(Int32.Parse(args[2]), args[3]);
                        break;
                    case "delete":
                        hashModule.Delete(Int32.Parse(args[2]));
                        break;
                    }
                } 
                else   
                {
                    Console.WriteLine("Wywoływać z argumentami:\n" +
                     " 1). endpoint serwera do którego się odwołujemy \n" +
                     " 2). get|push|delete \n" +
                     " 3). indeks \n" +
                     " 4). wartość w przypadku wywoływania push \n");
                }
                /*
                ic = Ice.Util.initialize(ref args);
                Ice.ObjectPrx obj = ic.stringToProxy (@"HashRegistry: tcp -h localhost -p 1231");
                HashRegisterPrx hashModule = HashRegisterPrxHelper.checkedCast(obj);
                Console.WriteLine("NAME: {0}", hashModule.getHashId("endpoint"));
                */
                /*
                Console.WriteLine("3");
                if(hashModule == null)
                    throw new ApplicationException("Invalid proxy");
                */
                
                
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

