@echo off
echo Build Preparation....
cd %CD%
echo Select your Platform: (10 Seconds = Any CPU)
echo.
echo 1 = Any CPU 
echo 2 = x86
echo 3 = x64
CHOICE /C 123 /N /T 10 /D 1 /M "Select your Build:"
IF ERRORLEVEL 1 SET Platform=Any CPU
IF ERRORLEVEL 2 SET Platform=x86
IF ERRORLEVEL 3 SET Platform=x64
ECHO %Platform% Selected as Build Type!
echo.
echo Select your Configuration: (10 Seconds = Release)
echo.
echo 1 = Release
echo 2 = Debug
CHOICE /C 12 /N /T 10 /D 1 /M "Select your Build Configuration:"
IF ERRORLEVEL 1 SET Release=Release
IF ERRORLEVEL 2 SET Release=Debug
echo %Release% Selected as Build Configuration!
echo.
echo Build Starting... (%Release% - %Platform%)
echo.
nuget.exe restore "NecroBot-Private for Pokemon GO.sln"
for /f "delims=" %%i in ('dir /s /b /a-d "%programfiles(x86)%\MSBuild.exe"') do (set necrobuilder="%%i")
%necrobuilder% "NecroBot-Private for Pokemon GO.sln" /property:Configuration="%Release%" /property:Platform="%Platform%"
set necrobuilder=
pause
