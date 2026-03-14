@echo off
REM Создать docker-сеть, если не существует
docker network inspect samplerag-net >nul 2>&1
if errorlevel 1 docker network create samplerag-net

REM Подключить зависимости к сети (если не подключены)
for %%C in (mongodb qdrant ollama) do docker network connect samplerag-net %%C 2>nul

REM Запустить SampleRag.API
REM Укажите путь к вашему Dockerfile, если он не в корне проекта
REM Пример: docker build -t sampleragapi ../SampleRag.API
docker build -t sampleragapi -f ../SampleRag.API/Dockerfile ..

REM Запустить контейнер API
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

echo Запуск завершен. Проверьте контейнеры командой: docker ps 
set /p DUMMY=Press Enter to continue...
