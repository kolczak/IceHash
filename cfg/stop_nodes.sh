#!/bin/bash

for i in `cat logs/.node_pids`
do
	kill $i
done;
