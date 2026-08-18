#!/bin/bash
cd "${0%/*}"
for i in dist/*.package; do
    mono tools/TuningResourceGenerator/TuningResourceGenerator.exe $i
done
