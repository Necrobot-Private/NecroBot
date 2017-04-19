@echo off
nuget.exe restore "NecroBot-Private for Pokemon GO.sln"
for /f "delims=" %%i in ('dir /s /b /a-d "%programfiles(x86)%\MSBuild.exe"') do (set necrobuilder="%%i")
%necrobuilder% "NecroBot-Private for Pokemon GO.sln" /property:Configuration="Release" /property:Platform="Any CPU"
set necrobuilder=
