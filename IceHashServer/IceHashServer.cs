using System;
using System.Collections;
using System.Threading;

using HashModule;
using IceHashClient;

using Ice;

namespace IceHashServer
{
    public class IceHashServer : IceBox.Service
    {
        private Ice.ObjectAdapter _adapter;
        private Hashtable _nodes;    //contains <int Id, ConnectedNode node>
        private Thread _clientThread;
        private Communicator _communicator;
        private HashModuleImpl srvHashModule;
        public void start(string name, Ice.Communicator communicator, string[] args)
        {
            HashModuleImpl srvHashModule = new HashModuleImpl();
            _nodes = new Hashtable();
            
            if (args.Length > 0)
            {
                Console.WriteLine("service id: " + args[0]);
                srvHashModule.ID = Int32.Parse(args[0]);
            }
            
            /*
            _adapter = communicator.createObjectAdapter(name);
            _communicator = communicator;
            _clientThread = new Thread(new ThreadStart(this.clientThread));
            _clientThread.Start();
            */
            
            _adapter = communicator.createObjectAdapter(name);
            _adapter.add(srvHashModule, Ice.Util.stringToIdentity("IIceHashService"));
            _adapter.activate();
            Console.WriteLine("Wystartowalem serwer " + name);
        }
        
        protected HashPrx getClientObject(Ice.Communicator communicator)
        {
            Ice.ObjectPrx obj = communicator.stringToProxy( @"IIceHashService");
            obj.
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
        
        private void clientThread()
        {
            //client part:
            //Communicator com = Ice.Util.initialize(null, new InitializationData());
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
        }
        
        public void stop()
        {
                _adapter.deactivate();
        }
        
    }
}

