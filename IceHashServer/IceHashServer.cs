using System;

namespace IceHashServer
{
    public class IceHashServer : IceBox.Service
    {
        private Ice.ObjectAdapter _adapter;
        
        public void start(string name, Ice.Communicator communicator, string[] args)
        {
            Console.WriteLine("Wystartowalem serwer");
            _adapter = communicator.createObjectAdapter(name);
            _adapter.add(new HashModuleImpl(), Ice.Util.stringToIdentity("IIceHashService"));
            _adapter.activate();
            
            
                
        }
        
        public void stop()
        {
                _adapter.deactivate();
        }
    }
}

