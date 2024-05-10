@echo off
set "index_path=index.js"

if exist "%index_path%" (
    node index.js
) else (
    echo Error: index.js not found!
)

pause
