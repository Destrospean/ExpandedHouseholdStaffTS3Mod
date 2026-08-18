@echo off
pushd "%~dp0"
for %%i in (..\strings\*.yaml) do (
    ..\tools\STBLize+\STBLize+.exe "..\strings\%%i" -d ..\resources -nu
)
popd
