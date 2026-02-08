@echo off
REM TC Agro Analytics - Docker Compose Shutdown Script (Windows)
REM Stops all infrastructure services

echo Stopping TC Agro Analytics infrastructure...

REM Navigate to scripts directory
cd /d "%~dp0"

REM Stop services
docker-compose down

echo.
echo Services stopped successfully!
echo.
echo To also remove volumes (data will be lost): docker-compose down -v
