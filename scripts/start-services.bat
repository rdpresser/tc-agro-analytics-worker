@echo off
REM TC Agro Analytics - Docker Compose Startup Script (Windows)
REM Starts all infrastructure services

echo Starting TC Agro Analytics infrastructure...

REM Navigate to scripts directory
cd /d "%~dp0"

REM Start services
docker-compose up -d

REM Wait for services to be healthy
echo Waiting for services to be healthy...
timeout /t 5 /nobreak >nul

REM Check service status
docker-compose ps

echo.
echo Services started successfully!
echo.
echo Service URLs:
echo   - PostgreSQL: localhost:5432 (user: postgres, password: postgres)
echo   - RabbitMQ Management: http://localhost:15672 (user: guest, password: guest)
echo   - RabbitMQ AMQP: localhost:5672
echo   - Redis: localhost:6379
echo   - Grafana: http://localhost:3000 (user: admin, password: admin)
echo   - pgAdmin: http://localhost:5050 (user: admin@tcagro.com, password: admin)
echo.
echo To view logs: docker-compose logs -f
echo To stop services: docker-compose down
echo To remove volumes: docker-compose down -v
