using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using IceBox;
using HashModule;

namespace IceHashRegistry
{
    class HashRegistryImpl : HashRegisterDisp_
    {
        private bool _pingerRunning;
        private List<string> _hashServiceNames;
        private Dictionary<int, HashPrx> _hashServices;
        private Dictionary<int, string> _endpoints;
        //private Thread _clientThread;
        private SortedList<int, bool> _ids;
        private int _currLevel;
        private Ice.Communicator _ic;
       
        private const int MAX_ID = 1024;
        private const int MAX_LEVELS = 10;
        
        public HashRegistryImpl()
        {
            _hashServiceNames = new List<string>();
            _hashServices = new Dictionary<int, HashPrx>();
            _endpoints = new Dictionary<int, string>();
            _pingerRunning = true;
            //_clientThread = new Thread(new ThreadStart(this.pingerThread));
            //_clientThread.Start();
            _ids = new SortedList<int, bool>();
            _currLevel = 0;
        }
        
        public void SetCommunicator(Ice.Communicator ic)
        {
            _ic = ic;
        }
        
        public override void register (string name, HashPrx proxy, Ice.Current current__)
        {
            if (!_hashServiceNames.Contains(name))
            {
                /*
                _hashServiceNames.Add(name);
                _hashServices.Add(name, proxy);
                */
            }
        }
        
        public bool checkNodeIsAlive(int id)
        {
            try
            {
                string tmpEndpoint;
                lock (_endpoints)
                {
                    tmpEndpoint = _endpoints[id];
                }
                Ice.ObjectPrx hashObj = _ic.stringToProxy (@"IceHash:" + tmpEndpoint);
                HashPrx hashModule = HashPrxHelper.checkedCast(hashObj.ice_twoway());
                if (hashModule.SrvPing() != PingStatus.Ready)
                {
                    throw new Exception("Server ping error");
                }
            } catch (Exception) {
                Console.WriteLine("Node {0} is dead", id);
                lock (_ids)
                {
                    _ids[id] = true;
                }
                
                lock (_endpoints)
                {
                    if (_endpoints.ContainsKey(id))
                    {
                        _endpoints.Remove(id);
                    }
                }
                return false;
            }
            return true;
        }
        
        public override int getHashId (string endpoint, Ice.Current current__)
        {
            int step, steps;
            
            lock(this)
            {            
                if (_currLevel == 0)
                {
                    lock (_ids)
                    {
                        _currLevel = 1;
                        _ids[0] = true;
                    }
                    Console.WriteLine("Zarejestrowalem endpoint dla wezla 0: {0}", endpoint);
                    _endpoints.Add(0, endpoint);
                    return 0;
                }
                
                while (true)
                {
                    step = (int)Math.Pow(2, MAX_LEVELS - _currLevel);
                    steps = (int)Math.Pow(2, _currLevel);
                    
                    for (int i = 0; i < steps; i++)
                    {
                        int id = i * step;
                        bool exists;
                        lock (_ids)
                        {
                            exists = _ids.ContainsKey(id) && ((bool)_ids[id]);
                        }
                        
                        if (!exists)
                        {
                            lock (_ids)
                            {
                                _ids[id] = true;
                            }
                            Console.WriteLine("Zarejestrowalem endpoint dla wezla {0}: {1}", id.ToString(), endpoint);
                            lock (_endpoints)
                            {
                                if (_endpoints.ContainsKey(id))
                                {
                                    _endpoints.Remove(id);
                                }
                                _endpoints.Add(id, endpoint);
                            }
                            return id;
                        }
                        else  // jezeli istnieje w tablicy, ale wezel upadl
                        {
                            try
                            {
                                string tmpEndpoint;
                                lock (_endpoints)
                                {
                                    tmpEndpoint = _endpoints[id];
                                }
                                Ice.ObjectPrx hashObj = _ic.stringToProxy (@"IceHash:" + tmpEndpoint);
                                HashPrx hashModule = HashPrxHelper.checkedCast(hashObj.ice_twoway());
                                if (hashModule.SrvPing() != PingStatus.Ready)
                                    throw new Exception("Server ping error");
                            } catch (Exception) {
                                Console.WriteLine("Node {0} is dead", id);
                                lock (_ids)
                                {
                                    _ids[id] = true;
                                }
                                
                                lock (_endpoints)
                                {
                                    if (_endpoints.ContainsKey(id))
                                    {
                                        _endpoints.Remove(id);
                                    }
                                    _endpoints.Add(id, endpoint);
                                }
                                return id;
                            }
                        }
                    }
                    
                    _currLevel++;
                }
            }
            //return "";
        }
        
        public override NodeInfo[] getIceHashNodesInfo (int id, int count, Ice.Current current__)
        {
            double step;
            int idx, offset;
            NodeInfo []nodes;
            List<int> tmpList = new List<int>();
            
            if (count == 0)
                return null;
            
            lock (_ids)
            {
                foreach (KeyValuePair<int, bool> de in _ids)
                {
                    if ((bool)de.Value)
                    {
                        tmpList.Add(de.Key);
                    }
                }
            }
            
            count += 1;
            if (count > tmpList.Count - 1)
            {
                count = tmpList.Count - 1;
            }
            
            idx = tmpList.IndexOf(id);
            nodes = new NodeInfo[count];
            for (int i = 0; i < count; i++)
                nodes[i] = new NodeInfo();
            
            int j = 0;
            while (j < tmpList.Count)
            {
                ++j;
                if (idx == 0)
                {
                    if (tmpList.Count - j < 0)
                        break;
                    nodes[0].id = tmpList[tmpList.Count - j];
                }
                else
                {
                    if (idx - j < 0)
                        break;
                    nodes[0].id = tmpList[idx - j];
                }
                if (checkNodeIsAlive(nodes[0].id))
                    break;
            }
            
            nodes[0].endpoint = _endpoints[nodes[0].id];
            nodes[0].type = NodeType.Predecessor;
            offset = idx;
            
            idx = 1;
            double dIdx = 1.0;
            step = Math.Pow(tmpList.Count, (double)1/(double)(count - 1));
            for (int i = 1; i < count; i++)
            {
                dIdx = dIdx * step;
                if (dIdx - idx < 1)
                {
                    dIdx = idx + 1;
                }
                idx = (int)dIdx;
                //Console.WriteLine("Zwracam {0} (idx+offset {1}) ((idx+offset) % tmpList.Count: {2})  | step {3}", idx, idx + offset, (idx + offset) % tmpList.Count, step);
                nodes[i].id = tmpList[(idx + offset) % tmpList.Count];
                nodes[i].endpoint = _endpoints[nodes[i].id];
                nodes[i].type = NodeType.Successor;
            }
            return nodes;
        }
        
        public override int getIceHashNodesCount (Ice.Current current__)
        {
            Console.WriteLine("get count");
            lock(_endpoints)
            {
                Console.WriteLine("count: {0}", _endpoints.Count);
                return _endpoints.Count;
            }
        }
        
        private void pingerThread()
        {
            while (_pingerRunning)
            {
                foreach (KeyValuePair<int, HashPrx> prx in _hashServices)
                {
                    Console.WriteLine("Ping {0}", prx.Key);
                    prx.Value.SrvKeepAlive();
                    Thread.Sleep(10000);
                }
            }
        }
    }
}

