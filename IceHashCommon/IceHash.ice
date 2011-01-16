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
		Range SrvRegister();
		int SrvGetNodeId();
		int SrvPing();
		Connector SrvLookup(int key);
		string SrvGet(int key);
	};
};

