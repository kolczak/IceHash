using System;
using System.Collections;
using System.Threading;

using HashModule;

using Ice;

namespace IceHashRegistry
{
    public class HashRegistryServer : IceBox.Service
    {
        private Ice.ObjectAdapter _adapter;
        private HashRegistryImpl _registry;
        
        public void start(string name, Ice.Communicator communicator, string[] args)
        {
            _registry = new HashRegistryImpl();
            
            _adapter = communicator.createObjectAdapter("HashRegistry");
            _adapter.add(_registry, Ice.Util.stringToIdentity("HashRegistry"));
            _adapter.activate();
            Console.WriteLine("Wystartowalem serwer HashRegistry");
        }
        
        public void stop()
        {
            _adapter.deactivate();
        }
    }
}

