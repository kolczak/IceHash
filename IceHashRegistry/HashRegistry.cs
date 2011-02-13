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
                        bool not_exists;
                        lock (_ids)
                        {
                            not_exists = !(_ids.ContainsKey(id)) || !((bool)_ids[id]);
                        }
                        
                        if (not_exists)
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
            if (idx == 0)
                nodes[0].id = tmpList[tmpList.Count - 1];
            else
                nodes[0].id = tmpList[idx - 1];
            nodes[0].endpoint = _endpoints[nodes[0].id];
            nodes[0].type = NodeType.Predecessor;
            offset = idx;
            idx = 1;
            step = Math.Pow(tmpList.Count, (double)1/(double)(count - 1));
            for (int i = 1; i < count; i++)
            {
                idx = (int)(idx * step);
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

