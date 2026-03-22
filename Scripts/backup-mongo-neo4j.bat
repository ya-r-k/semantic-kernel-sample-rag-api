REM @echo off
REM Папка для бэкапов
set BACKUP_DIR=../backups
if not exist "%BACKUP_DIR%" mkdir "%BACKUP_DIR%"

REM Бэкап MongoDB
echo [MongoDB] Dump...
docker exec mongodb mongodump --archive=/data/db/backup.archive --gzip
docker cp mongodb:/data/db/backup.archive "%BACKUP_DIR%\mongo_backup.archive"
docker exec mongodb rm /data/db/backup.archive

REM Бэкап Neo4j
echo [Neo4j] Dump...
docker exec neo4j mkdir -p /var/lib/neo4j/backups
docker exec neo4j neo4j-admin database dump neo4j --to-path=/var/lib/neo4j/backups --overwrite-destination=true
docker cp neo4j:/var/lib/neo4j/backups/neo4j.dump "%BACKUP_DIR%\neo4j_backup.dump"
docker exec neo4j rm /data/backup/neo4j.dump

echo Бэкапы сохранены в %BACKUP_DIR% 

set /p DUMMY=Press Enter to continue...
