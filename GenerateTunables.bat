pushd "%~dp0"
for %%i in (dist/*.package) do (
    tools\TuningResourceGenerator\TuningResourceGenerator.exe "dist\%%i"
)
popd
