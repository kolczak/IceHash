using System;
using System.Collections;
using System.Threading;

using HashModule;

using Ice;

namespace IceHashRegistry
{
    public class HashRegistryServer : Ice.Application
    {
        private Ice.ObjectAdapter _adapter;
        private HashRegistryImpl _registry;
        
        public override int run (string[] args)
        {
            Ice.Communicator ic = null;
            try
            {
                ic = Ice.Util.initialize(ref args);
                _registry = new HashRegistryImpl();
                _registry.SetCommunicator(ic);
                _adapter = ic.createObjectAdapterWithEndpoints("HashRegistry", args[0]);
                _adapter.add(_registry, ic.stringToIdentity("HashRegistry"));
                _adapter.activate();
                Console.WriteLine("Wystartowalen HashRegistry");
                ic.waitForShutdown();
                
                if (interrupted())
                    Console.WriteLine ("Koniec");
            } catch (System.Exception ex) {
                Console.WriteLine(ex);
            }
            
            return 0;
        }
        
        /*
        public void start(string name, Ice.Communicator communicator, string[] args)
        {
            _registry = new HashRegistryImpl();
            
            _adapter = communicator.createObjectAdapter(name);
            _adapter.add(_registry, Ice.Util.stringToIdentity(name));
            _adapter.activate();
            Console.WriteLine("Wystartowalem serwer {0}", name);
        }
        
        public void stop()
        {
            _adapter.deactivate();
        }
        */
        
        public static void Main(string[] args)
        {
            HashRegistryServer srv = new HashRegistryServer();
            Environment.Exit(srv.run(args));
        }
    }
}
