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
        protected Dictionary <Int32, string> _values;               //wartości posiadane lokalnie
        protected Dictionary <Range, string> _routingTable;         //zakres wartości, nazwa węzła
        protected Dictionary <string, HashPrx> _directNeighbors;    //nazwa węzła, proxy do niego
        private Ice.Communicator _communicator;
        
        public int ID
        {
            get;
            set;
        }
        
        private bool inRange(Range r, int key)
        {
            if(r.startRange <= key && r.endRange >= key)
                return true;
            else
                return false;
        }
        
        public AddDirectNeighbors(string name, HashPrx hashPrx)
        {
            _directNeighbors.Add(name, hashPrx);
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
        public override Status Push (int key, string value, Ice.Current current__)
        {
            if (inRange(_currentRange, key))
            {
                _values.Add(key, value);
                return Status.Correct;
            }
            else
            {
                return Status.NotExist;
            }
            
            return Status.Error;
        }
        
        
        public override string Get (int key, Ice.Current current__)
        {
            string result = null;
            
            if (inRange(_currentRange, key))
            {
                result = _values[key];
            }
            else
            {
                result = SrvLookup(key);
            }
            
            return result;
        }
        
        
        public override Status Delete (int key, Ice.Current current__)
        {
            if (inRange(_currentRange, key))
            {
                _values.Remove(key);
                return Status.Correct;
            }
            else
            {
                return Status.NotExist;
            }
            
            return Status.Error;
        }
        
        public override Range SrvRegister (int nodeId, Ice.Current current__)
        {
            HashPrx proxy = getClientObject(_communicator, "IIceHashService" + nodeId.ToString());            
            Range newRange = new Range();
            if (proxy != null)
            {
                
            }
            return newRange;
        }
        
        public override int SrvGetNodeId (Ice.Current current__)
        {
            return ID;
        }
        
        
        public override int SrvPing (Ice.Current current__)
        {
            return 1;
        }
        
        public override Range SrvGetRange (Ice.Current current__)
        {
            return _currentRange;
        }
        
        
        public override string SrvLookup (int key, Ice.Current current__)
        {
            string res = null;
            
            foreach (KeyValuePair<Range, string> kvp in _routingTable)
            {
                if(inRange(kvp.Key, key))
                {
                    return _directNeighbors[kvp.Value].Get(key);
                }
            }
            
            return res;
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
}
