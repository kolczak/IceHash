using System;
using System.Collections;
using System.Threading;

using HashModule;

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
            
            Client cln = new Client(communicator);
            _clientThread = new Thread(new ThreadStart(cln.Run));
            _clientThread.Start();
            
            _adapter = communicator.createObjectAdapter(name);
            _adapter.add(srvHashModule, Ice.Util.stringToIdentity("IIceHashService"));
            _adapter.activate();
            Console.WriteLine("Wystartowalem serwer " + name);
        }
        
        public void stop()
        {
            _clientThread.Abort();
            _adapter.deactivate();
        }
        
    }
}

