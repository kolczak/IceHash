module HashModule {
	class Range {
		int startRange;
		int endRange;
	};

	struct Connector {
		int address;
	};

	enum Status { Correct, Exists, Error, NotExist };

	dictionary<int, string> ValuesDictionary; 

	class RegisterResponse {
		Range keysRange;
		ValuesDictionary values;
	};

	interface Hash {
		Status Push(int key, string value);
		string Get(int key);
		Status Delete(int key);
		RegisterResponse SrvRegister(int nodeId);
		Range SrvGetRange();
		int SrvGetNodeId();
		int SrvPing();
		//Connector SrvLookup(int key);
		Hash *SrvLookup(int key); //returns service name
		string SrvGet(int key);
		void SrvKeepAlive();
	};

	enum NodeType { Predecessor, Successor };

	struct NodeInfo {
		int id;
		string endpoint;
		NodeType type;
	};

	sequence<NodeInfo> NodeInfoSeq;

    interface HashRegister {
		int getHashId(string endpoint);
        NodeInfoSeq getIceHashNodesInfo(int id, int count);
        void register(string name, Hash *proxy);
		int getIceHashNodesCount();
    };
};

