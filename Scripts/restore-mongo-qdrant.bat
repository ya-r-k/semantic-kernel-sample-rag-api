@echo off
setlocal

set "SCRIPT_DIR=%~dp0"
set "BACKUP_DIR=%SCRIPT_DIR%..\backups"

if not "%~1"=="" (
  set "MONGO_BACKUP=%~1"
) else (
  for /f "delims=" %%f in ('dir /b /o-d "%BACKUP_DIR%\mongo_backup_*.archive" 2^>nul') do (
    set "MONGO_BACKUP=%BACKUP_DIR%\%%f"
    goto :mongo_selected
  )
)
:mongo_selected

if not "%~2"=="" (
  set "QDRANT_BACKUP=%~2"
) else (
  for /f "delims=" %%f in ('dir /b /o-d "%BACKUP_DIR%\qdrant_storage_*.tar.gz" 2^>nul') do (
    set "QDRANT_BACKUP=%BACKUP_DIR%\%%f"
    goto :qdrant_selected
  )
  if exist "%BACKUP_DIR%\qdrant_storage_backup.tar.gz" set "QDRANT_BACKUP=%BACKUP_DIR%\qdrant_storage_backup.tar.gz"
)
:qdrant_selected

if not exist "%MONGO_BACKUP%" (
  echo Mongo backup file not found: "%MONGO_BACKUP%"
  exit /b 1
)

if not exist "%QDRANT_BACKUP%" (
  echo Qdrant backup file not found: "%QDRANT_BACKUP%"
  exit /b 1
)

call :ensure_started mongodb
if errorlevel 1 exit /b 1
call :ensure_started qdrant
if errorlevel 1 exit /b 1

echo [MongoDB] Restoring from "%MONGO_BACKUP%"...
docker cp "%MONGO_BACKUP%" mongodb:/tmp/mongo_backup.archive
docker exec mongodb sh -c "mongorestore --archive=/tmp/mongo_backup.archive --gzip --drop"
docker exec mongodb sh -c "rm -f /tmp/mongo_backup.archive"

echo [Qdrant] Restoring from "%QDRANT_BACKUP%"...
docker cp "%QDRANT_BACKUP%" qdrant:/tmp/qdrant_storage_backup.tar.gz
docker exec qdrant sh -c "rm -rf /qdrant/storage/* && tar -xzf /tmp/qdrant_storage_backup.tar.gz -C /qdrant"
docker exec qdrant sh -c "rm -f /tmp/qdrant_storage_backup.tar.gz"

echo Restore finished from: %BACKUP_DIR%
exit /b 0

:ensure_started
set "NAME=%~1"
for /f %%i in ('docker ps -a -q -f "name=^%NAME%^$"') do set "EXISTS=%%i"
if not defined EXISTS (
  echo Container "%NAME%" was not found. Start dependencies first using docker.run-deps.bat
  exit /b 1
)
docker start "%NAME%" >nul 2>&1
set "EXISTS="
exit /b 0
