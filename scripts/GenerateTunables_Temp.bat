@echo off
pushd "%~dp0"
..\tools\TuningResourceGenerator\TuningResourceGenerator.exe $(GetPackagePath)
popd
