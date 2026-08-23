#!/bin/bash
cd "${0%/*}"
rm ../resources/*.stbl
for i in ../strings/*.yaml; do
    mono ../tools/STBLize+/STBLize+.exe $i -d ../resources -nu
done
