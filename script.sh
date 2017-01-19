#!/bin/bash
for i in `seq 420 1337`;
do
	echo $i > hardaanhetwerk.txt
	git add .
	git commit -m "Hard aan het werk $i"
	echo $i
done