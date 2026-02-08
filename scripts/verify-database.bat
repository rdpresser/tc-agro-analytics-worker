@echo off
REM TC Agro Analytics - Database Setup Verification (Windows)
REM Verifies if the database was created correctly

echo Checking PostgreSQL connection...

REM Check if container is running
docker ps | findstr tc-agro-postgres >nul
if errorlevel 1 (
    echo Error: PostgreSQL container is not running!
    echo Run: docker-compose up -d tc-agro-postgres
    exit /b 1
)

echo PostgreSQL container is running

REM Check database existence
echo.
echo Checking if database 'tc_agro_analytics' exists...

docker exec tc-agro-postgres psql -U postgres -tAc "SELECT 1 FROM pg_database WHERE datname='tc_agro_analytics'" > temp.txt
set /p DB_EXISTS=<temp.txt
del temp.txt

if "%DB_EXISTS%"=="1" (
    echo Database 'tc_agro_analytics' exists!
) else (
    echo Database 'tc_agro_analytics' does NOT exist!
    echo.
    echo Creating database...
    docker exec tc-agro-postgres psql -U postgres -c "CREATE DATABASE tc_agro_analytics;"
    echo Database created!
)

REM Show database info
echo.
echo Database Information:
docker exec tc-agro-postgres psql -U postgres -c "\l tc_agro_analytics"

REM Show extensions
echo.
echo Installed Extensions:
docker exec tc-agro-postgres psql -U postgres -d tc_agro_analytics -c "\dx"

REM Test connection
echo.
echo Testing connection...
docker exec tc-agro-postgres psql -U postgres -d tc_agro_analytics -c "SELECT version();"

echo.
echo Database setup verification complete!
echo.
echo Connection String:
echo Host=localhost;Port=5432;Database=tc_agro_analytics;Username=postgres;Password=postgres
