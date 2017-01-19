#!/bin/bash
for i in `seq 1 100`;
do
	echo $i > hardaanhetwerk.txt
	git add .
	git commit -m "Hard aan het werk $i"
	echo $i
done