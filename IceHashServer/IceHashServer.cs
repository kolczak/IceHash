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
        /*
        private Thread _clientThread;
        private Communicator _communicator;
        private HashModuleImpl srvHashModule;
        */
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
                srvHashModule.ID = _hashNodeId;
                
                //zarejestruj usługę w registry o otrzymanej nazwie:
                //_adapter.add(srvHashModule, Ice.Util.stringToIdentity(_myName));
                _adapter = ic.createObjectAdapterWithEndpoints("IceHash", endpoint);
                _adapter.add(srvHashModule, ic.stringToIdentity("IceHash"));
                _adapter.activate();
                Console.WriteLine("Wystartowalem serwer " + _hashNodeId);
                HashPrx local = HashPrxHelper.uncheckedCast(ic.stringToProxy(@"IceHash:" + endpoint));
                srvHashModule.SetOwnProxy(local);
                
                //sleep
                //Thread.Sleep(5 * 1000);
                
                //poproś o ileś nazw innych węzłów żeby pobrać dane
                int count = registryModule.getIceHashNodesCount();
                NodeInfo[] nodesInfo = registryModule.getIceHashNodesInfo(_hashNodeId, (int)((double)count * 0.5));
                Ice.ObjectPrx hashObj;
                if (nodesInfo.Length > 0)
                {   
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
                        Console.WriteLine("Utworzono proxy do IceHash:{0}", node.endpoint);
                        if (node.type == NodeType.Predecessor)
                        {
                            srvHashModule.SetPredecessor(hashModule);   
                            HashPrx predecessor = srvHashModule.GetPredecessor();
                            RegisterResponse response = predecessor.SrvRegister(_hashNodeId, local);
                            srvHashModule.SetValues(response.values);
                            srvHashModule.SetRange(response.keysRange);
                            Console.WriteLine("Ustawiam lokalny range ({0}, {1})",
                            response.keysRange.startRange, response.keysRange.endRange);
                            srvHashModule.AddDirectNeighbors(node.id, hashModule);
                        }
                        else if (node.id != _hashNodeId)
                        {
                            Console.WriteLine("Dodaje hashProxy dla node {0}", node.id);
                            srvHashModule.AddDirectNeighbors(node.id, hashModule);
                        }
                    }
                }
                else
                {
                    //pierwszy wezel
                }
                
                if (srvHashModule.SrvGetRange().endRange == Int32.MaxValue)
                {
                    try
                    {
                        HashPrx prx = srvHashModule.SrvLookup(0);
                        if (prx == null)
                            Console.WriteLine("Lookup zwrocil null'a");
                        int nodeId = prx.SrvGetNodeId();
                        if (nodeId != _hashNodeId)
                            srvHashModule.AddDirectNeighbors(prx.SrvGetNodeId(), prx);
                    } catch (System.Exception) {
                        Console.WriteLine("Nie udalo sie dodac wezla 0");
                    }
                }
                srvHashModule.SetInitialized(true);
                ic.waitForShutdown();
            } catch (System.Exception ex) {
                Console.WriteLine(ex);   
            }
            
            return 0;
        }
        
        public static void Main(string []args)
        {
            IceHashServer srv = new IceHashServer();
            Environment.Exit(srv.run(args));
        }
        
    }
}

