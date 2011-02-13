using System;
using System.Threading;
using System.Collections.Generic;

using IceBox;

using HashModule;

namespace IceHashServer
{
    public class RangeComparator : IComparer<Range>
    {
        public int Compare (Range x, Range y)
        {
            if (x.startRange.CompareTo(y.startRange) != 0)
                return x.startRange.CompareTo(y.startRange);
            if (x.endRange.CompareTo(y.endRange) != 0)
                return x.endRange.CompareTo(y.endRange);
            
            return 0;
        }
    }
    
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
            _routingTable = new SortedDictionary<Range, int>(new RangeComparator());
            _directNeighbors = new Dictionary<int, HashPrx>();
            _predecessor = null;
        }
        
        private bool inRange(Range r, int key)
        {
            if(r.startRange <= key && r.endRange >= key)
                return true;
            else
                return false;
        }
        
        private Range FindSuccessor()
        {
            foreach(KeyValuePair<Range, int> kvp in _routingTable)
            {
                if(kvp.Key.startRange == (_currentRange.startRange + 1))
                    return kvp.Key;
            }
        }
        
        private Status FalitureDetected(HashPrx proxy)
        {
            if (_directNeighbors.ContainsValue(proxy))
            {
                //znajdz id odpowiadające posiadanemu proxy
                int failerId = -1;
                foreach(KeyValuePair<int, HashPrx> kvp in _directNeighbors)
                {
                    if(kvp.Value == proxy)
                        failerId = kvp.Key;
                }
                if (failerId != -1)   //sprawdzamy czy znalazł dane proxy w liscie sasiadow
                {
                    //znajdz zakres padlego wezla
                    Range failerRagne = null;
                    foreach(KeyValuePair<Range, int> kvp in _routingTable)
                        if(kvp.Value == failerId)
                            failerRagne = kvp.Key;
                    
                    //usun dane o nim
                    _routingTable.Remove(failerRagne);
                    _directNeighbors.Remove(failerId);
                    
                    //polaczenie zakresow stworzy spojny zakres to przejmij jego pule
                    if((_currentRange.endRange + 1) == failerRagne.startRange)
                        _currentRange.endRange = failerRagne.endRange;
                    return Status.Correct;
                }
                else {
                    return Status.Error;
                }
            }
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
            if (!_directNeighbors.ContainsKey(id))
            {
                _directNeighbors.Add(id, hashPrx);
                _routingTable.Add(hashPrx.SrvGetRange(), id);
            }
            
            foreach (KeyValuePair<Range, int> kvp in _routingTable)
            {
                 Console.WriteLine("Range: <{0}; {1}>", kvp.Key.startRange, kvp.Key.endRange);
            }
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
            bool res;
            
            lock (_currentRange)
            {
                res = inRange(_currentRange, key);
            }
            
            if (res)
            {
                if (_values.ContainsKey(key))
                    _values.Remove(key);
                Console.WriteLine("Pushing localy key: {0} value:{1}", key, val);
                _values.Add(key, val);
                return Status.Correct;
            }
            else
            {
                //return SrvLookup(key).Push(key, val);
                HashPrx proxy = null;
                proxy = SrvLookup(key);
                if (proxy == null)
                {
                    return Status.Error;
                }
                
                Range range = proxy.SrvGetRange();
                Console.WriteLine("Range: <{0}; {1}>", range.startRange, range.endRange);
                
                if (!inRange(range, key))
                {
                    proxy = proxy.SrvLookup(key);
                }
                
                if (proxy != null)
                {
                    proxy.Push(key, val);
                    return Status.Correct;
                }
                else
                {
                    return Status.Error;
                }
            }
        }
        
        
        public override string Get (int key, Ice.Current current__)
        {
            string result = "";
            bool res;
            
            lock (_currentRange)
            {
                res = inRange(_currentRange, key);
            }
            
            if (res)
            {
                if (_values.ContainsKey(key))
                    result = _values[key];
            }
            else
            {
                HashPrx proxy = SrvLookup(key);
                if (proxy == null)
                    return "ERROR";
                try{
                    result = proxy.Get(key);
                }catch(System.Exception ex){    //węzeł padł
                    if(FalitureDetected(proxy) == Status.Correct)
                        return Get(key);    //sprobuj geta 
                    else
                        return "ERROR";
                }
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
                HashPrx proxy = SrvLookup(key);
                if (proxy == null)
                    return Status.Error;
                return proxy.Delete(key);
            }
        }
        
        public override RegisterResponse SrvRegister (int nodeId, HashPrx proxy, Ice.Current current__)
        {
            RegisterResponse response = new RegisterResponse();
            Dictionary<int, string> values;
            Range newRange = new Range();
            
            values = new Dictionary<int, string>();
            /*
            Ice.ObjectPrx hashObj;
            hashObj = _communicator.stringToProxy (@"IceHash:" + endpoint);
            if (hashObj == null)
            {
                Console.WriteLine("IceHash proxy with endpoint {0} is null", endpoint);
                return response;
            }
            HashPrx hashModule = HashPrxHelper.checkedCast(hashObj.ice_twoway());
            if(hashModule == null)
            {
                Console.WriteLine("Invalid proxy");
                return response;
            }
            AddDirectNeighbors(nodeId, hashModule);
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
                _currentRange.endRange = newRange.startRange - 1;
            }
            Console.WriteLine("Zarejestrowano nowy wezel. range({0}, {1})",
                              newRange.startRange, newRange.endRange);
           
            _directNeighbors.Add(nodeId, proxy);
            _routingTable.Add(newRange, nodeId);
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
                else
                {
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
