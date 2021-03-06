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
        protected HashPrx _ownProxy;
        protected bool _initialized;
        //private Ice.Communicator _communicator;
        
        public HashModuleImpl ()
        {
            _currentRange = new Range(0, Int32.MaxValue);
            _values = new Dictionary<int, string>();
            _routingTable = new SortedDictionary<Range, int>(new RangeComparator());
            _directNeighbors = new Dictionary<int, HashPrx>();
            _predecessor = null;
            _initialized = false;
        }
        
        public void SetOwnProxy(HashPrx prx)
        {
            _ownProxy = prx;
        }
        
        private bool inRange(Range r, int key)
        {
            if (r.startRange <= r.endRange)
            {
                if((r.startRange <= key) && (r.endRange >= key))
                    return true;
                else
                    return false;
            }
            else if (r.startRange > r.endRange)
            {
                if((r.startRange <= key) || (r.endRange >= key))
                    return true;
                else
                    return false;
            }
            return false;
        }
        
        public void SetInitialized(bool initialized)
        {
            _initialized = initialized;
        }
        
        private Range FindSuccessor()
        {
            foreach(KeyValuePair<Range, int> kvp in _routingTable)
            {
                if(kvp.Key.startRange == (_currentRange.endRange + 1))
                    return kvp.Key;
            }
            return null;
        }
        
        private Status FailureDetected(HashPrx proxy)
        {
            bool containsValue;
            lock (_directNeighbors)
            {
                containsValue = _directNeighbors.ContainsValue(proxy);
            }
            if (containsValue)
            {
                //znajdz id odpowiadające posiadanemu proxy
                int failerId = -1;
                lock (_directNeighbors)
                {
                    foreach(KeyValuePair<int, HashPrx> kvp in _directNeighbors)
                    {
                        if(kvp.Value == proxy)
                            failerId = kvp.Key;
                    }
                }
                if (failerId != -1)   //sprawdzamy czy znalazł dane proxy w liscie sasiadow
                {
                    //znajdz zakres padlego wezla
                    Range failerRagne = null;
                    lock (_directNeighbors)
                    {
                        foreach(KeyValuePair<Range, int> kvp in _routingTable)
                            if(kvp.Value == failerId)
                                failerRagne = kvp.Key;
                        
                        _routingTable.Remove(failerRagne);
                        _directNeighbors.Remove(failerId);
                    }
                    //polaczenie zakresow stworzy spojny zakres to przejmij jego pule
                    //uwzglednij przejscie przez 0s
                    if(((_currentRange.endRange + 1) == failerRagne.startRange) ||
                       		((_currentRange.endRange == Int32.MaxValue) && (failerRagne.startRange == 0)))
					{
                        _currentRange.endRange = failerRagne.endRange;
                        Console.WriteLine("Lokalny range się zmienił");
                        Console.WriteLine("Obecny lokalny range ({0}, {1})",
                                  _currentRange.startRange, _currentRange.endRange);
                    }
                    else
                    {
                        HashPrx prx = SrvLookup(failerRagne.startRange - 1);
                        try
                        {
                            lock (_directNeighbors)
                            {
                                Range range = prx.SrvGetRange();
                                int id = prx.SrvGetNodeId();
                                if (_routingTable.ContainsValue(id))
                                {
                                    Range tmpRange = null;
                                    foreach(KeyValuePair<Range, int> kvp in _routingTable)
                                    {
                                        if(kvp.Value == failerId)
                                            tmpRange = kvp.Key;
                                    }
                                    if (tmpRange != null)
                                        _routingTable.Remove(tmpRange);
                                    _directNeighbors[id] = prx;
                                    _routingTable[range] = id;
                                }
                            }
                        } catch (Exception) {
                            
                        }
                    }
                    
                    return Status.Correct;
                }
            }
            return Status.Error;
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
            Console.WriteLine("Dodaje proxy i range dla node {0}", id);
            lock (_directNeighbors)
            {
                if (!_directNeighbors.ContainsKey(id))
                {
                    _directNeighbors.Add(id, hashPrx);
                    _routingTable.Add(hashPrx.SrvGetRange(), id);
                }
            }
            
            Console.WriteLine("Pelna tablica routing'u:");
            foreach (KeyValuePair<Range, int> kvp in _routingTable)
            {
                 Console.WriteLine("\t<{0}; {1}>   nodeId {2}", kvp.Key.startRange, kvp.Key.endRange, kvp.Value);
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
        
//        public void SetCommunicator(Ice.Communicator communicator)
//        {
//              _communicator = communicator;
//        }
        
        #region implemented abstract members of HashModule.HashDisp_
        public override Status Push (int key, string val, Ice.Current current__)
        {
            bool res;
            Status result;
            
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
                HashPrx proxy = SrvLookup(key);
                if (proxy == null)
                    return Status.Error;
                try {
                    result = proxy.Push(key, val);
                } catch (Exception) {
                    return Status.Error;
                }
            }
            return result;
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
                try {
                    result = proxy.Get(key);
                } catch (Exception) {
                    return "ERROR";
                }
            }
            
            return result;
        }
        
        public override Status Delete (int key, Ice.Current current__)
        {
            bool res;
            
            lock (_currentRange)
            {
                res = inRange(_currentRange, key);
            }
            
            if (res)
            {
                try {
                    lock(_values)
                        _values.Remove(key);
                } catch (Exception) {
                    Console.WriteLine("Wartosc dla podanego klucza nie istnieje");
                    return Status.Error;
                }
                return Status.Correct;
            }
            else
            {
                HashPrx proxy = SrvLookup(key);
                if (proxy == null)
                    return Status.Error;
                
                try {
                    return proxy.Delete(key);
                } catch (Exception) {
                    return Status.Error;
                }
            }
        }
        
        private Range DivideRange()
        {
            Range newRange = new Range();
            if(_currentRange.startRange > _currentRange.endRange)
            {
                newRange.startRange = 0;
                newRange.endRange = _currentRange.endRange;
                _currentRange.endRange = Int32.MaxValue;
            }
            else
            {
                int rangeSize = _currentRange.endRange - _currentRange.startRange;
                if (rangeSize <= 0)
                {
                    newRange.startRange = 0;
                    newRange.endRange = 0;
                }
                    
                newRange.startRange = _currentRange.startRange + rangeSize / 2;
                newRange.endRange = _currentRange.endRange;
                _currentRange.endRange = newRange.startRange - 1;
            }
            return newRange;
        }
        
        public override RegisterResponse SrvRegister (int nodeId, HashPrx proxy, Ice.Current current__)
        {
            bool successorAlive = false;
            RegisterResponse response = new RegisterResponse();
            Dictionary<int, string> values;
            Range newRange;
            
            values = new Dictionary<int, string>();
            
            Range successorRange = FindSuccessor();
            if (successorRange != null)
            {
                int id;
                HashPrx prx = null;
                try
                {
                    lock (_directNeighbors)
                    {
                        if (_routingTable.ContainsKey(successorRange))
                        {
                            id = _routingTable[successorRange];
                            prx = _directNeighbors[id];
                        }
                    }
                    if (prx != null)
                    {
                        if (prx.SrvPing() == PingStatus.Ready)
                            successorAlive = true;
                        else
                            throw new Exception();
                    }
                } catch (Exception) {
                    Console.WriteLine("Excetion: Nastepnik padl");
                }
            }
            else
            {
                Console.WriteLine("Nie posiadam nastepnikow");
            }
            
            if (successorAlive || successorRange == null)
            {
                lock (_currentRange)
                {
                    newRange = DivideRange();
                }
                
                if(newRange.startRange == newRange.endRange)
                {
                    response.keysRange = newRange;
                    return response;
                }
                Console.WriteLine("Zarejestrowano nowy wezel ({0}). range({1}, {2})",
                                  nodeId, newRange.startRange, newRange.endRange);
                Console.WriteLine("Obecny lokalny range ({0}, {1})",
                                  _currentRange.startRange, _currentRange.endRange);
               
                //proxy do wezla, ktory stanie sie bezposrednim nastepnikiem
                lock (_directNeighbors)
                {
                    _directNeighbors.Add(nodeId, proxy);
                    _routingTable.Add(newRange, nodeId);
                }
                
                //Oddajemy wartosci do rejestrujacego sie wezla
                lock (_values)
                {
                    foreach (KeyValuePair<int, string> entry in _values)
                    {
                        if (inRange(newRange, entry.Key))
                        {
                            values.Add(entry.Key, entry.Value);
                            
                        }
                    }
                    foreach (KeyValuePair<int, string> entry in values)
                        _values.Remove(entry.Key);
                }
                response.keysRange = newRange;
                response.values = values;
            }
            else if (!successorAlive)
            {
                try
                {
                    lock (_directNeighbors)
                    {
                        int id = _routingTable[successorRange];
                        _routingTable.Remove(successorRange);
                        _directNeighbors.Remove(id);
                        _directNeighbors.Add(nodeId, proxy);
                        _routingTable.Add(successorRange, nodeId);
                        
                        
                        response.keysRange = successorRange;
                        response.values = values;
                        
                        Console.WriteLine("Zarejestrowano nowy wezel ({0}). range({1}, {2})",
                                  nodeId, successorRange.startRange, successorRange.endRange);
                        Console.WriteLine("Obecny lokalny range ({0}, {1})",
                                  _currentRange.startRange, _currentRange.endRange);
                    }
                } catch (Exception) {
                    Console.WriteLine("Exception: Problem podczas dodawania nowego wezla");
                    return null;
                }
            }
            
            return response;
        }
        
        public override int SrvGetNodeId (Ice.Current current__)
        {
            return ID;
        }
        
        
        public override PingStatus SrvPing (Ice.Current current__)
        {
            if (_initialized)
                return PingStatus.Ready;
            else
                return PingStatus.UnInitialized;
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
            
            if(inRange(_currentRange, key)) //jezeli po zmodyfikowaniu tablic okazuje sie ze przedzial mamy lokalnie
                return _ownProxy;
            
            foreach (KeyValuePair<Range, int> kvp in _routingTable)
            {
                if(inRange(kvp.Key, key))
                {
                    try {
                        if(inRange(_directNeighbors[kvp.Value].SrvGetRange(), key))
                            return _directNeighbors[kvp.Value];
                        else
                            return _directNeighbors[kvp.Value].SrvLookup(key);
                    } catch(System.Exception) { //wezel padl
                        if(FailureDetected(_directNeighbors[kvp.Value]) == Status.Correct)
                            return SrvLookup(key);    //sprobuj na zmodyfikownych tablicach routingu i sasiadow
                        else
                            return null;
                    }
                }
                else if (kvp.Key.startRange > key)  //jezeli poczatkowe klucze w tablicy routingu sa juz 
                                                    //wieksze od poszukiwanego
                {
                    if((prevVal != -1) && (getLast == false))
                    {
                        try {
                            return _directNeighbors[prevVal].SrvLookup(key);
                        } catch(System.Exception) {    //wezel padl
                            if(FailureDetected(_directNeighbors[prevVal]) == Status.Correct)
                                return SrvLookup(key);    //sprobuj na zmodyfikownych tablicach routingu i sasiadow
                            else
                                return null;
                        }
                    }
                    else
                        getLast = true; //przeslij do ostatniego na liscie
                }
                else
                {
                    getLast = true;
                }
                prevVal = kvp.Value;
            }
            
            //w prevVal jest teraz ostatni id określający wezeł z najwyższym przedziałem
            if(getLast)
            {
                try {
                    return _directNeighbors[prevVal].SrvLookup(key);
                } catch(System.Exception) {    //wezel padl
                    if(FailureDetected(_directNeighbors[prevVal]) == Status.Correct)
                        return SrvLookup(key);    //sprobuj na zmodyfikownych tablicach routingu i sasiadow
                    else
                        return null;
                }
            }
            
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
