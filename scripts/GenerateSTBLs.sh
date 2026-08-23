#!/bin/bash
cd "${0%/*}"
for i in ../strings/*.yaml; do
    mono ../tools/STBLize+/STBLize+.exe $i -d ../resources -nu
done
