@echo off
setlocal
rem Relative to this BAT file.
set "HTML_REL=."
set "SERVER_REL=Internal\uml_preview_server.py"
set "PORT=8765"

for %%I in ("%~dp0%HTML_REL%") do set "HTML_DIR=%%~fI"
for %%I in ("%~dp0%SERVER_REL%") do set "SERVER_PY=%%~fI"
echo [INFO] HTML_DIR: "%HTML_DIR%"
echo [INFO] SERVER_PY: "%SERVER_PY%"
if not exist "%HTML_DIR%\index.html" (
  echo [ERROR] index.html not found.
  pause
  exit /b 1
)
if not exist "%SERVER_PY%" (
  echo [ERROR] Python server script not found.
  pause
  exit /b 1
)
where python >nul 2>&1
if errorlevel 1 (
  echo [ERROR] Python not found in PATH.
  pause
  exit /b 1
)
echo [INFO] Opening browser...
start "" "http://127.0.0.1:%PORT%/index.html"
echo [INFO] Starting server. Leave this window open. Press Ctrl+C to stop.
python -u "%SERVER_PY%" "%HTML_DIR%" --port %PORT%
echo [ERROR] Server exited. Check the message above.
pause
