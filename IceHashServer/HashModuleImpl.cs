using System;
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
            if (_currentRange.startRange >= key && _currentRange.endRange <= key)
            {
                result = _values[key];
            }
            else
            {
                SrvLookup(key);
            }
            
            return result;
        }
        
        
        public override Status Delete (string key, Ice.Current current__)
        {
            throw new System.NotImplementedException();
        }
        
        public Range SrvRegister (int nodeId, Ice.Current current__)
        {
            HashPrx proxy = getClientObject(_communicator, "IIceHashService" + nodeId.ToString());            
            Range newRange = new Range();
            if (proxy != null)
            {
                
            }
            return;
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
        
        public Range SrvGetRange ()
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
        
        #endregion
        public HashModuleImpl ()
        {
        }
    }
    
    class HashRegisterImpl : HashRegisterDisp_
    {
        public override void register (HashPrx proxy, Ice.Current current__)
        {
            throw new NotImplementedException ();
        }
        
        public override string[] getIceHashNames (int count, Ice.Current current__)
        {
            throw new NotImplementedException ();
        }
    }
}

