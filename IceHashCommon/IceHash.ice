module HashModule {
	struct Range {
		int startRange;
		int endRange;
	};

	struct Connector {
		int address;
	};

	enum Status { Correct, Exists, Error, NotExist };

	interface Hash {
		Status Push(string key, string value);
		string Get(string key);
		Status Delete(string key);
		Range SrvRegister(int nodeId);
		Range SrvGetRange();
		int SrvGetNodeId();
		int SrvPing();
		//Connector SrvLookup(int key);
		string SrvLookup(int key); //returns service name
		string SrvGet(int key);
		void SrvKeepAlive();
	};

	sequence<string> StringSeq;

    interface HashRegister {
		string getHashName();
        StringSeq getIceHashNames(int count);
        void register(string name, Hash *proxy);
		int getIceHashNodesCount();
    };
};

