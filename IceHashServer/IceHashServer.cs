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
        private string _myName;
        
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
            
            //polacz sie z registry
            //TODO: zweryfikować nazwę registry
            Ice.ObjectPrx obj = communicator.stringToProxy( @"IIceHashRegistry");   
            Console.WriteLine("Registry proxy created");
            if (obj == null)
            {
                Console.WriteLine("Created proxy is null");
                return;
            }
            HashRegisterPrx registryModule = HashRegisterPrxHelper.checkedCast(obj.ice_twoway());
            if(registryModule == null)
            {
                Console.WriteLine("Invalid proxy");
                return;
            }
            
            //poproś o nadanie nazwy od naszego rejestru
            _myName = registryModule.getHashName();
            
            //zarejestruj usługę w registry o otrzymanej nazwie:
            _adapter = communicator.createObjectAdapter(name);  //TODO: czy name == _myName?
            _adapter.add(srvHashModule, Ice.Util.stringToIdentity(_myName));
            _adapter.activate();
            Console.WriteLine("Wystartowalem serwer " + _myName);
            
            //sleep
            Thread.Sleep(5 * 1000);
            
            //poproś o ileś nazw innych węzłów żeby poprać dane
            //registryModule.
        }
        
        public void stop()
        {
            _clientThread.Abort();
            _adapter.deactivate();
        }
        
    }
}

