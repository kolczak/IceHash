#!/bin/bash

for i in `cat logs/.node_pids`
do
	echo "Stopping $i"
	kill $i
done;

echo -ne "" > logs/.node_pids
