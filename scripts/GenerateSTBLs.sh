#!/bin/bash
cd "${0%/*}"
for i in ../strings/*.yaml; do
    ../tools/STBLize+/STBLize+ $i -d ../resources -nu
done
