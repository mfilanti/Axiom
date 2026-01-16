@echo off
echo Eliminazione file .meta...
for /r %%f in (*.meta) do del "%%f"

echo Eliminazione cartelle bin...
for /d /r %%d in (bin) do rd /s /q "%%d"

echo Eliminazione cartelle obj...
for /d /r %%d in (obj) do rd /s /q "%%d"

echo Operazione completata.
pause
