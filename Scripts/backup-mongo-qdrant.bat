@echo off
REM Папка для бэкапов
set BACKUP_DIR=../backups
if not exist "%BACKUP_DIR%" mkdir "%BACKUP_DIR%"

REM Бэкап MongoDB
echo [MongoDB] Dump...
docker start mongodb >nul 2>&1
docker exec mongodb mongodump --db=samplerag --archive=/data/db/backup.archive --gzip
docker cp mongodb:/data/db/backup.archive "%BACKUP_DIR%\mongo_backup.archive"
docker exec mongodb rm /data/db/backup.archive

REM Бэкап Qdrant
echo [Qdrant] Dump...
docker start qdrant >nul 2>&1
docker exec qdrant sh -c "tar -czf /tmp/qdrant_storage_backup.tar.gz -C /qdrant storage"
docker cp qdrant:/tmp/qdrant_storage_backup.tar.gz "%BACKUP_DIR%\qdrant_storage_backup.tar.gz"
docker exec qdrant sh -c "rm -f /tmp/qdrant_storage_backup.tar.gz"

echo Бэкапы сохранены в %BACKUP_DIR%

set /p DUMMY=Press Enter to continue...
