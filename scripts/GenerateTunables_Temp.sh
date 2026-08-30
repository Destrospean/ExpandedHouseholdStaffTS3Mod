#!/bin/bash
cd "${0%/*}"
filename=ts3buildtool.log
last_line=$(tail -n 1 $filename)
path="${last_line:52}"
mono ../tools/TuningResourceGenerator/TuningResourceGenerator.exe "$path"
rm $filename
