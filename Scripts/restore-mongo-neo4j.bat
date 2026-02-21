@echo off
REM Папка с бэкапами
set BACKUP_DIR=/backups

REM Восстановление MongoDB
echo [MongoDB] Restore...
docker cp "%BACKUP_DIR%\mongo_backup.archive" mongodb:/data/db/backup.archive
docker exec mongodb mongorestore --archive=/data/db/backup.archive --gzip --drop
docker exec mongodb rm /data/db/backup.archive

REM Восстановление Neo4j
echo [Neo4j] Restore...
docker cp "%BACKUP_DIR%\neo4j_backup.dump" neo4j:/data/backup/neo4j.dump
docker exec neo4j neo4j-admin database load neo4j --from-path=/data/backup --overwrite-destination=true --force
docker exec neo4j rm /data/backup/neo4j.dump

echo Восстановление завершено из %BACKUP_DIR% 
