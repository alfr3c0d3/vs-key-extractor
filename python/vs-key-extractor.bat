@echo off
set "index_path=index.py"

if exist "%index_path%" (
    python index.py
) else (
    echo Error: index.py not found!
)

pause