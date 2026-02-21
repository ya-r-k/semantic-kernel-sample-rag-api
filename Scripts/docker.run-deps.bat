@echo off
REM Запуск Qdrant

docker run -d --name qdrant -p 6333:6333 -p 6334:6334 -v "D:\Development Data\Docker Volumes\qdrant_storage":/qdrant/storage --cpus=1 --memory=1g --memory-swap=1g qdrant/qdrant:v1.14.1

REM Запуск MongoDB

docker run -d --name mongodb -p 27017:27017 -v "D:\Development Data\Docker Volumes\mongodb_storage":/data/db --cpus=1 --memory=1g --memory-swap=1g mongo:8.0.10 

REM Запуск Ollama

docker run -d --gpus=all  --name ollama -p 11434:11434 -v "D:\Development Data\Docker Volumes\ai-models-files":/root/.ollama --cpus=8 --cpu-shares=1024 ollama/ollama:0.15.6
docker update --cpu-shares=512 mongodb qdrant

@echo Скрипт завершен. Проверьте контейнеры командой: docker ps 
set /p DUMMY=Press Enter to continue...