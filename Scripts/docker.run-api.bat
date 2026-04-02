@echo off
chcp 65001 >nul
set "NO_PAUSE="
if "%1"=="--no-pause" set "NO_PAUSE=1" & shift

REM Сначала запустить зависимости
echo.
echo --------------------------------------------------------------
echo [DEBUG] Запуск скрипта docker.run-deps
echo --------------------------------------------------------------
call "%~dp0docker.run-deps.bat" --no-pause
echo.
echo --------------------------------------------------------------
echo [DEBUG] docker.run-deps завершён с кодом: %errorlevel%
echo --------------------------------------------------------------
if errorlevel 1 exit /b 1

echo Запуск SampleRag.API в Docker...

REM Создать docker-сеть, если не существует
docker network inspect samplerag-net >nul 2>&1
if errorlevel 1 docker network create samplerag-net >nul 2>&1

REM Подключить зависимости к сети (если не подключены)
for %%C in (mongodb qdrant ollama) do docker network connect samplerag-net %%C 2>nul

REM Запустить SampleRag.API
REM Укажите путь к вашему Dockerfile, если он не в корне проекта
REM Пример: docker build -t sampleragapi ../SampleRag.API
docker build -t sampleragapi -f ../SampleRag.API/Dockerfile ..

REM Запустить контейнер API
echo.
echo Запуск контейнера SampleRag.API...
docker ps -a --format "{{.Names}}" | findstr /R /C:"^sampleragapi$" >nul
if %errorlevel%==0 (
  docker start sampleragapi >nul 2>&1
) else (
  docker run -d --name sampleragapi --network samplerag-net -p 5234:8080 ^
    -v "%cd%\..\SampleRag.API\wwwroot\assets:/app/wwwroot/assets" ^
    -e ASPNETCORE_ENVIRONMENT=Development ^
    -e ASPNETCORE_URLS=http://+:8080 ^
    -e DOTNET_USE_POLLING_FILE_WATCHER=true ^
    -e DOTNET_WATCH_RELOAD_ON_CHANGE=true ^
    -e ASPNETCORE_DETAILEDERRORS=true ^
    -e DbSettings__ConnectionString=mongodb://mongodb:27017 ^
    -e VectorDbSettings__Url=http://qdrant:6334 ^
    -e GenAiProviderSettings__Url=http://ollama:11434 ^
    sampleragapi
)

REM Умная пауза в конце скрипта
if defined NO_PAUSE goto :skip_pause
echo.
echo.
echo.
echo [DEBUG] Список запущенных контейнеров:
docker ps
echo.
echo.
echo.
echo.
echo [DEBUG] Скрипт завершен. Контейнеры можно проверить командой: docker ps
set /p DUMMY=Press Enter to continue...
:skip_pause
