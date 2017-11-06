@echo off
cd Data
setlocal EnableDelayedExpansion

REM Backup data
for /f "tokens=1-4 delims=/ " %%a in ('date /t') do (set cdate=%%c-%%b-%%a)
set search=./%cdate%*
set filenames=
for /d %%f in (%search%) do set filenames=!filenames! %%f
echo %filenames%
if defined filenames (7z a -r \\DiskStation\SurprisingMinds\SurprisingMinds_%cdate%.zip %filenames%)

REM Delete data from one week ago
for /f "delims=" %%d in ('"powershell [DateTime]::Now.AddDays(-7).ToString('yyyy-MM-dd')"') do set ddate=%%d
echo %ddate%
for /D %%f in ("./%ddate%*") do @(
rmdir /S /Q "%%f"
)

REM Go back to working directory
cd ..