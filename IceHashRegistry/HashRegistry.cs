using System;
using System.Threading;
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
        private Thread _clientThread;
        
        public HashRegistryImpl()
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

