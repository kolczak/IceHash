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
        protected Dictionary <int, string> _values;               //wartości posiadane lokalnie
        protected SortedDictionary <Range, int> _routingTable;         //zakres wartości, nazwa węzła
        protected Dictionary <int, HashPrx> _directNeighbors;    //nazwa węzła, proxy do niego
        protected HashPrx _predecessor;
        private Ice.Communicator _communicator;
        
        public HashModuleImpl ()
        {
            _currentRange = new Range(0, Int32.MaxValue);
            _values = new Dictionary<int, string>();
            _routingTable = new SortedDictionary<Range, int>();
            _directNeighbors = new Dictionary<int, HashPrx>();
            _predecessor = null;
        }
        
        public int ID
        {
            get;
            set;
        }
        
        public void SetRange(Range range)
        {
            _currentRange = range;
        }
        
        public void SetValues(Dictionary<int, string> vals)
        {
            _values = new Dictionary<int, string>(vals);
        }
        
        private bool inRange(Range r, int key)
        {
            if(r.startRange <= key && r.endRange >= key)
                return true;
            else
                return false;
        }
        
        public void SetPredecessor(HashPrx predecessor)
        {
            _predecessor = predecessor;
        }
        
        public HashPrx GetPredecessor()
        {
            return _predecessor;   
        }
        
        public void AddDirectNeighbors(int id, HashPrx hashPrx)
        {
            _directNeighbors.Add(id, hashPrx);
            _routingTable.Add(hashPrx.SrvGetRange(), id);
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
        public override Status Push (int key, string val, Ice.Current current__)
        {
            if (inRange(_currentRange, key))
            {
                if (_values.ContainsKey(key))
                    _values.Remove(key);
                _values.Add(key, val);
                return Status.Correct;
            }
            else
            {
                return SrvLookup(key).Push(key, val);
            }
        }
        
        
        public override string Get (int key, Ice.Current current__)
        {
            string result = "";
            
            if (inRange(_currentRange, key))
            {
                if (_values.ContainsKey(key))
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
                return SrvLookup(key).Delete(key);
            }
        }
        
        public override RegisterResponse SrvRegister (int nodeId, Ice.Current current__)
        {
            RegisterResponse response = new RegisterResponse();
            Dictionary<int, string> values;
            Range newRange = new Range();
            
            values = new Dictionary<int, string>();
            
            /*
            HashPrx proxy = getClientObject(_communicator, "IIceHashService" + nodeId.ToString());            
            if (proxy != null)
            {
                
            }
            */
            
            lock (_currentRange)
            {
                int rangeSize = _currentRange.endRange - _currentRange.startRange;
                if (rangeSize <= 0)
                {
                    newRange.startRange = 0;
                    newRange.endRange = 0;
                    response.keysRange = newRange;
                    return response;
                }
                    
                newRange.startRange = _currentRange.startRange + rangeSize / 2;
                newRange.endRange = _currentRange.endRange;
            }
            Console.WriteLine("Zarejestrowano nowy wezel. range({0}, {1})",
                              newRange.startRange, newRange.endRange);
            lock (_values)
            {
                foreach (KeyValuePair<int, string> entry in _values)
                {
                    if (inRange(newRange, entry.Key))
                    {
                        values.Add(entry.Key, entry.Value);
                        _values.Remove(entry.Key);
                    }
                }
            }
            response.keysRange = newRange;
            response.values = _values;
            
            return response;
        }
        
        public override int SrvGetNodeId (Ice.Current current__)
        {
            return 0;
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
            int prevVal = -1;
            bool getLast = false;
            
            Console.WriteLine("Wywolano metode lookup dla klucza {0}", key);
            
            foreach (KeyValuePair<Range, int> kvp in _routingTable)
            {
                if(inRange(kvp.Key, key))
                {
                    return _directNeighbors[kvp.Value];
                }
                else if (kvp.Key.startRange > key)  //jezeli poczatkowe klucze w tablicy routingu sa juz 
                                                    //wieksze od poszukiwanego
                {
                    if(prevVal != -1)
                        return _directNeighbors[prevVal];
                    else
                        //przeslij do ostatniego na liscie
                        getLast = true;
                }
                prevVal = kvp.Value;
            }
            
            //w prevVal jest teraz ostatni id określający wezeł z najwyższym przedziałem
            if(getLast)
                return _directNeighbors[prevVal];
            
            return null;
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
    }
}
