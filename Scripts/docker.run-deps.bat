@echo off
chcp 65001 >nul
set "NO_PAUSE="
if "%1"=="--no-pause" set "NO_PAUSE=1" & shift

REM Показать запущенные/остановленные контейнеры
echo [INFO] Проверяем контейнеры для перезапуска:
echo.
docker ps -a ^
  --filter "name=qdrant" ^
  --filter "name=mongodb" ^
  --filter "name=ollama" ^
  --format "table {{.Names}}\t{{.Status}}\t{{.Image}}"
echo.

REM Спрашиваем подтверждение (Y/N)
choice /c YN /n /m "Для запуска зависимостей необходимо удалить и пересоздать эти контейнеры? [Y/N]: "
if errorlevel 2 (
    echo [INFO] Отменено. Контейнеры не удалены и не перезапущены.
    exit /b 0
)

REM Удаляем + запускаем
for %%c in (qdrant mongodb ollama) do (
    echo.
    echo [DEBUG] Удаление существующих контейнеров %%c...
    docker rm -f %%c
)

REM Запустить Qdrant
echo.
echo [DEBUG] Запуск контейнера Qdrant...
docker run -d --name qdrant ^
  -p 6333:6333 -p 6334:6334 ^
  -v "D:\Development Data\Docker Volumes\qdrant_storage:/qdrant/storage" ^
  --cpus=1 --memory=1g --memory-swap=1g ^
  qdrant/qdrant:v1.14.1
echo [DEBUG] Qdrant запущен.

REM Запуск MongoDB
echo.
echo [DEBUG] Запуск контейнера MongoDB...
docker run -d --name mongodb ^
  -p 27017:27017 ^
  -v "D:\Development Data\Docker Volumes\mongodb_storage":/data/db ^
  --cpus=1 --memory=1g --memory-swap=1g ^
  mongo:8.0.10
echo [DEBUG] MongoDB запущен.

REM Запуск Ollama
echo.
echo [DEBUG] Запуск контейнера Ollama...
docker run -d --gpus=all ^
  --name ollama -p 11434:11434 ^
  -v "D:\Development Data\Docker Volumes\ai-models-files:/root/.ollama" ^
  --cpus=8 --cpu-shares=1024 ^
  ollama/ollama:0.17.6
echo [DEBUG] Ollama запущен.

REM === Проверка всех ===
echo.
echo [DEBUG] Проверка контейнеров...
for %%c in (qdrant mongodb ollama) do (
    docker ps --filter "name=%%c" --format "{{.Names}}" | findstr /C:"%%c" >nul 2>&1
    if %errorlevel%==0 (
        echo [OK] Docker Контейнер %%c запущен
    ) else (
        echo [ERROR] Docker Контейнер %%c НЕ запущен!
    )
)

docker update --cpu-shares=512 mongodb qdrant >nul 2>&1

echo.
echo [DEBUG] Загрузка моделей в Ollama...
for %%c in (qwen3.5:0.8b qwen3:4b mxbai-embed-large:335m) do (
    echo [DEBUG] Загрузка модели %%c в Ollama...
    docker exec -it ollama ollama pull %%c
    echo.
    echo.
    echo.
)

echo.
echo.
echo.
echo [DEBUG] Загрузка моделей в Ollama завершена.
echo [DEBUG] Список загруженных ИИ-моделей в Ollama:
docker exec -it ollama ollama list

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
