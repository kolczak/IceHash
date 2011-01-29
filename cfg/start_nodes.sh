#!/bin/bash

if [ -d logs ]
then
	echo "Directory logs exists"
else
	echo "Directory logs not exists - creating..."
	mkdir logs
fi

./stop_nodes.sh
echo -ne "" > logs/.node_pids
for i in `seq 1 10`
do
	echo "Starting node $i..."
	icegridnode --Ice.Config="icegridnode$i.cfg" 2>&1 1>>"logs/icegridnode$i.log" &
	echo $! >> logs/.node_pids
done
