using System;
using HashModule;

namespace IceHashServer
{
    public class ConnectedNode
    {
        public HashPrx Proxy {get; set;}
        public Range HashTableRange {get; set;}
        
        public ConnectedNode (HashPrx prx)
        {
            Proxy = prx;
            HashTableRange = new Range(-1, -1);
        }
        
        public ConnectedNode (HashPrx prx, Range rng)
        {
            Proxy = prx;
            HashTableRange = rng;
        }
    }
}

