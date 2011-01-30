using System;
using HashModule;

namespace IceHashServer
{
    public class HashModuleImpl : HashDisp_
    {
        public int ID 
        {
            get;
            set;
        }
        
        #region implemented abstract members of HashModule.HashDisp_
        public override Status Push (string key, string value, Ice.Current current__)
        {
            throw new System.NotImplementedException();
        }
        
        
        public override string Get (string key, Ice.Current current__)
        {
            throw new System.NotImplementedException();
        }
        
        
        public override Status Delete (string key, Ice.Current current__)
        {
            throw new System.NotImplementedException();
        }
        
        
        public override Range SrvRegister (Ice.Current current__)
        {
            throw new System.NotImplementedException();
        }
        
        
        public override int SrvGetNodeId (Ice.Current current__)
        {
            return ID;
        }
        
        
        public override int SrvPing (Ice.Current current__)
        {
            Console.WriteLine("Dostalem Pinga");
            return 1;
        }
        
        
        public override Connector SrvLookup (int key, Ice.Current current__)
        {
            throw new System.NotImplementedException();
        }
        
        
        public override string SrvGet (int key, Ice.Current current__)
        {
            throw new System.NotImplementedException();
        }
        
        #endregion
        public HashModuleImpl ()
        {
        }
    }
}

