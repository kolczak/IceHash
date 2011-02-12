using System;
using System.Threading;
using System.Collections.Generic;

using IceBox;

using HashModule;

namespace IceHashServer
{
    public class HashModuleImpl : HashDisp_
    {
        protected Range _currentRange;
        protected Dictionary <Int32, string> _values;
        protected Dictionary <Range, string> _routingTable;
        protected List<HashModuleImpl> _directNeighbors;
        private Ice.Communicator _communicator;
        
        public int ID
        {
            get;
            set;
        }
        
        protected HashPrx getClientObject(Ice.Communicator communicator, string proxyName)
        {
            Ice.ObjectPrx obj = communicator.stringToProxy(proxyName);;
            if (obj == null)
            {
                Console.WriteLine("Created proxy is null");
                return null;
            }
            else
            {
                HashPrx hashModule = HashPrxHelper.checkedCast(obj);
                if(hashModule == null)
                    Console.WriteLine("Invalid proxy");
                return hashModule;
            }
        }
        
        public void SetCommunicator(Ice.Communicator communicator)
        {
              _communicator = communicator;
        }
        
        #region implemented abstract members of HashModule.HashDisp_
        public override Status Push (string key, string value, Ice.Current current__)
        {
            throw new System.NotImplementedException();
        }
        
        
        public override string Get (string key, Ice.Current current__)
        {
            string result = null;
            /*
            if (_currentRange.startRange >= key && _currentRange.endRange <= key)
            {
                result = _values[key];
            }
            else
            {
                SrvLookup(key);
            }
            */
            return result;
        }
        
        
        public override Status Delete (string key, Ice.Current current__)
        {
            throw new System.NotImplementedException();
        }
        
        public override Range SrvRegister (int nodeId, Ice.Current current__)
        {
            HashPrx proxy = getClientObject(_communicator, "IIceHashService" + nodeId.ToString());            
            Range newRange = new Range();
            if (proxy != null)
            {
                
            }
            return new Range();
        }
        
        public override int SrvGetNodeId (Ice.Current current__)
        {
            return ID;
        }
        
        
        public override int SrvPing (Ice.Current current__)
        {
            //Console.WriteLine("Dostalem Pinga");
            //Console.WriteLine(current__.);
            return 1;
        }
        
        public override Range SrvGetRange (Ice.Current current__)
        {
            return _currentRange;
        }
        
        
        public override string SrvLookup (int key, Ice.Current current__)
        {
            throw new System.NotImplementedException();
        }
        
        
        public override string SrvGet (int key, Ice.Current current__)
        {
            throw new System.NotImplementedException();
        }
        
        public override void SrvKeepAlive (Ice.Current current__)
        {
            return;
        }
        
        #endregion
        public HashModuleImpl ()
        {
        }
    }
    
    class HashRegisterImpl : HashRegisterDisp_
    {
        private bool _pingerRunning;
        private List<string> _hashServiceNames;
        private Dictionary<string, HashPrx> _hashServices;
        private Thread _clientThread;
        
        public HashRegisterImpl()
        {
            _hashServiceNames = new List<string>();
            _hashServices = new Dictionary<string, HashPrx>();
            _pingerRunning = true;
            _clientThread = new Thread(new ThreadStart(this.pingerThread));
            _clientThread.Start();
        }
        
        public override void register (string name, HashPrx proxy, Ice.Current current__)
        {
            if (!_hashServiceNames.Contains(name))
            {
                _hashServiceNames.Add(name);
                _hashServices.Add(name, proxy);
            }
        }
        
        public override string[] getIceHashNames (int count, Ice.Current current__)
        {
            string []names;
            Random rand = new Random();
            List<string> tmpList = new List<string>();
            
            lock (_hashServiceNames)
            {
                tmpList.AddRange(_hashServiceNames);
            }
            
            if (count > tmpList.Count)
            {
                count = _hashServiceNames.Count;
            }
            
            names = new string[count];
            
            return names;
        }
        
        public override int getIceHashNodesCount (Ice.Current current__)
        {
            return _hashServiceNames.Count;
        }
        
        private void pingerThread()
        {
            while (_pingerRunning)
            {
                foreach (string serviceName in _hashServiceNames)
                {
                    
                }
            }
        }
    }
}
