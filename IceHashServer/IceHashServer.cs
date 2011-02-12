using System;
using System.Collections;
using System.Threading;

using HashModule;

using Ice;

namespace IceHashServer
{
    public class IceHashServer : Ice.Application
    {
        private Ice.ObjectAdapter _adapter;
        private Thread _clientThread;
        private Communicator _communicator;
        private HashModuleImpl srvHashModule;
        private int _hashNodeId;
        
        public override int run(string []args)
        {
            HashModuleImpl srvHashModule = new HashModuleImpl();
            
            try
            {
                string endpoint = args[0];
                Ice.Communicator ic = Ice.Util.initialize(ref args);
                
                if (args.Length > 0)
                {
                    Console.WriteLine("service id: " + endpoint);
                    //srvHashModule.ID = Int32.Parse(args[0]);
                }
                
                /*
                Client cln = new Client(ic);
                _clientThread = new Thread(new ThreadStart(cln.Run));
                _clientThread.Start();
                */
                
                //polacz sie z registry
                //TODO: zweryfikować nazwę registry
                
                Ice.ObjectPrx obj = ic.stringToProxy (@"HashRegistry: tcp -h localhost -p 1231");
                Console.WriteLine("Registry proxy created");
                if (obj == null)
                {
                    Console.WriteLine("Created proxy is null");
                    return -1;
                }
                HashRegisterPrx registryModule = HashRegisterPrxHelper.checkedCast(obj.ice_twoway());
                if(registryModule == null)
                {
                    Console.WriteLine("Invalid proxy");
                    return -2;
                }
                
                //poproś o nadanie nazwy od naszego rejestru
                _hashNodeId = registryModule.getHashId(endpoint);
                
                //zarejestruj usługę w registry o otrzymanej nazwie:
                //_adapter.add(srvHashModule, Ice.Util.stringToIdentity(_myName));
                _adapter = ic.createObjectAdapterWithEndpoints("IceHash", endpoint);
                _adapter.add(srvHashModule, ic.stringToIdentity("IceHash"));
                _adapter.activate();
                Console.WriteLine("Wystartowalen IceHash");
                ic.waitForShutdown();
                
                _adapter.activate();
                Console.WriteLine("Wystartowalem serwer " + _hashNodeId);
                
                //sleep
                Thread.Sleep(5 * 1000);
                
                Ice.ObjectPrx hashObj;
                //poproś o ileś nazw innych węzłów żeby pobrać dane
                int count = registryModule.getIceHashNodesCount();
                NodeInfo[] nodesInfo = registryModule.getIceHashNodesInfo(_hashNodeId, (int)((double)count * 0.5));
                foreach(NodeInfo node in nodesInfo)
                {
                    hashObj = ic.stringToProxy (@"IceHash:" + node.endpoint);
                    if (hashObj == null)
                    {
                        Console.WriteLine("IceHash proxy with endpoint {0} is null", node.endpoint);
                        return -1;
                    }
                    HashPrx hashModule = HashPrxHelper.checkedCast(hashObj.ice_twoway());
                    if(hashModule == null)
                    {
                        Console.WriteLine("Invalid proxy");
                        return -2;
                    }
                    srvHashModule.AddDirectNeighbors(hashModule);
                }
                
            } catch (System.Exception ex) {
                Console.WriteLine(ex);   
            }
            
            return 0;
        }
        
        void Main(string []args)
        {
            IceHashServer srv = new IceHashServer();
            Environment.Exit(srv.run(args));
        }
        
    }
}

