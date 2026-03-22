@echo off
dotnet format ../SampleRag.API.slnx --verbosity diagnostic

echo Запуск завершен. Проверьте контейнеры командой: docker ps 
set /p DUMMY=Press Enter to continue...
