using System;
using HashModule;
using IceHashClient;

namespace IceHashServer
{
    public class IceHashServer : IceBox.Service
    {
        private Ice.ObjectAdapter _adapter;
        
        public int ID 
        {
            get;
            set;
        }
                
        public void start(string name, Ice.Communicator communicator, string[] args)
        {
            //server part:
            Console.WriteLine("Wystartowalem serwer " + name);
            _adapter = communicator.createObjectAdapter(name);
            _adapter.add(new HashModuleImpl(), Ice.Util.stringToIdentity("IIceHashService"));
            _adapter.activate();

            if (args.Length > 0)
            {
                Console.WriteLine("service id: " + args[0]);
                ID = Int32.Parse(args[0]);
            }
            
            //client part:
            Ice.ObjectPrx obj = communicator.stringToProxy(@"IIceHashService");
            HashPrx hashModule = HashPrxHelper.checkedCast(obj);
            if(hashModule == null)
                Console.WriteLine("Invalid proxy");
            
            if(hashModule.SrvPing() == 1)
                Console.WriteLine("Server is Alive!");
            else
                Console.WriteLine("Server is Ded!");
             
        }
        
        public void stop()
        {
                _adapter.deactivate();
        }
        
    }
}

