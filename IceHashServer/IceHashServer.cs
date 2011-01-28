using System;
using HashModule;
using IceHashClient;

namespace IceHashServer
{
    public class IceHashServer : IceBox.Service
    {
        private Ice.ObjectAdapter _adapter;
        
        public void start(string name, Ice.Communicator communicator, string[] args)
        {
            //server part:
            Console.WriteLine("Wystartowalem serwer " + name);
            _adapter = communicator.createObjectAdapter(name);
            _adapter.add(new HashModuleImpl(), Ice.Util.stringToIdentity("IIceHashService"));
            _adapter.activate();
            
            
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

