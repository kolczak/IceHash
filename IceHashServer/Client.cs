using System;
using HashModule;
using System.Threading;

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
            
            while (true)
            {
                HashPrx hashModule = getClientObject();
                
                if (hashModule != null)
                {
                    if(hashModule.SrvPing() == 1)
                    {
                        Console.WriteLine("Server is Alive!");
                        Console.WriteLine("id servera: " + hashModule.SrvGetNodeId());
                    }
                    else
                        Console.WriteLine("Server is Dead!");
                }
                else
                {
                    Console.WriteLine("HashModule proxy is null");
                }
                
                Thread.Sleep(1000);
            }
            
        }
        
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

