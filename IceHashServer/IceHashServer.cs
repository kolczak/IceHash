using System;

namespace IceHashServer
{
    public class IceHashServer : IceBox.Service
    {
        private Ice.ObjectAdapter _adapter;
        
        public void start(string name, Ice.Communicator communicator, string[] args)
        {
            _adapter = communicator.createObjectAdapter(name);
            _adapter.add(new HashModuleImpl(), Ice.Util.stringToIdentity("HashServer"));
            _adapter.activate();
            
            
                
        }
        
        public void stop()
        {
                _adapter.deactivate();
        }
    }
}

