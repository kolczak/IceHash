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
        protected SortedDictionary <Range, string> _routingTable;         //zakres wartości, nazwa węzła
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
        
        public void AddDirectNeighbors(string name, HashPrx hashPrx)
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
                result = SrvLookup(key).Get(key);
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
        
        
        public override HashPrx SrvLookup (int key, Ice.Current current__)
        {
            HashPrx res = null;
            string prevVal = null;
            bool getLast = false;
            
            foreach (KeyValuePair<Range, string> kvp in _routingTable)
            {
                if(inRange(kvp.Key, key))
                {
                    return _directNeighbors[kvp.Value];
                }
                else if (kvp.Key.startRange > key)  //jezeli poczatkowe klucze w tablicy routingu sa juz 
                                                    //wieksze od poszukiwanego
                {
                    if(prevVal != null)
                        return _directNeighbors[prevVal];
                    else
                        //przeslij do ostatniego na liscie
                        //TODO: przetestowac czy do Valuest mozna sie odwolywac przez indeks
                        getLast = true;
                }
                prevVal = kvp.Value;
            }
            
            //w prevVal jest teraz ostatni string określający wezeł z najwyższym przedziałem
            if(getLast)
                return _directNeighbors[prevVal];
            
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
