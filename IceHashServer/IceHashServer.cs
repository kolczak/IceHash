using System;
using HashModule;
using IceHashClient;
using System.Collections;

namespace IceHashServer
{
    public class IceHashServer : IceBox.Service
    {
        private Ice.ObjectAdapter _adapter;
        private Hashtable _nodes;    //contains <int Id, ConnectedNode node>
                        
        public void start(string name, Ice.Communicator communicator, string[] args)
        {
            nodes = new Hashtable();
            HashModuleImpl srvHashModule = new HashModuleImpl();
            
            if (args.Length > 0)
            {
                Console.WriteLine("service id: " + args[0]);
                srvHashModule.ID = Int32.Parse(args[0]);
            }
            
            //server part:
            Console.WriteLine("Wystartowalem serwer " + name);
            _adapter = communicator.createObjectAdapter(name);
            _adapter.add(srvHashModule, Ice.Util.stringToIdentity("IIceHashService"));
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

