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
        private Dictionary<string, HashPrx> _hashServices;
        private Dictionary<string, string> _endpoints;
        private Thread _clientThread;
        private SortedList _ids;
        private int _currLevel;
       
        private const int MAX_ID = 1024;
        private const int MAX_LEVELS = 10;
        
        public HashRegistryImpl()
        {
            _hashServiceNames = new List<string>();
            _hashServices = new Dictionary<string, HashPrx>();
            _endpoints = new Dictionary<string, string>();
            _pingerRunning = true;
            _clientThread = new Thread(new ThreadStart(this.pingerThread));
            _clientThread.Start();
            _ids = new SortedList();
            _currLevel = 0;
        }
        
        public override void register (string name, HashPrx proxy, Ice.Current current__)
        {
            if (!_hashServiceNames.Contains(name))
            {
                _hashServiceNames.Add(name);
                _hashServices.Add(name, proxy);
            }
        }
        
        public override string getHashName (string endpoint, Ice.Current current__)
        {
            string stringId;
            int step, steps;
            
            lock(this)
            {            
                if (_currLevel == 0)
                {
                    _currLevel = 1;
                    _ids[0] = true;
                    stringId = "0";
                    Console.WriteLine("Zarejestrowalem endpoint dla wezla 0: {0}", endpoint);
                    _endpoints.Add(stringId, endpoint);
                    return stringId;
                }
                
                while (true)
                {
                    step = (int)Math.Pow(2, MAX_LEVELS - _currLevel);
                    steps = (int)Math.Pow(2, _currLevel);
                    
                    for (int i = 0; i < steps; i++)
                    {
                        int id = i * step;
                        if (!(_ids.ContainsKey(id)))
                        {
                            _ids[id] = true;
                            stringId = id.ToString();
                            Console.WriteLine("Zarejestrowalem endpoint dla wezla {0}: {1}", id.ToString(), endpoint);
                            _endpoints.Add(id.ToString(), endpoint);
                            return stringId;
                        }
                    }
                    
                    _currLevel++;
                }
            }
            //return "";
        }
        
        public override string[] getIceHashNames (int count, Ice.Current current__)
        {
            int idx;
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
            for (int i = 0; i < count; i++)
            {
                idx = rand.Next() % tmpList.Count;
                names[i] = tmpList[idx];
                tmpList.RemoveAt(idx);
            }
            
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
                foreach (KeyValuePair<string, HashPrx> prx in _hashServices)
                {
                    Console.WriteLine("Ping {0}", prx.Key);
                    prx.Value.SrvKeepAlive();
                }
            }
        }
    }
}

