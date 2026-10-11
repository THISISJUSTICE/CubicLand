@echo off
cd /d "%~dp0"

echo [CHECK] Current directory: %CD%
echo [CHECK] Python script:
dir "Tools\UMLSync\sync.py"

echo.
echo [RUN]
python "Internal\sync.py"

echo.
pause