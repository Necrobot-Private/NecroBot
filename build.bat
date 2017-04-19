@echo off
if EXIST "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community" goto VS2017_Community
if EXIST "C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise" goto VS2017_Enterprise

:VS2017_Community
echo Building under Visual Studio 2017 Community as %processor_architecture%
nuget.exe restore "NecroBot-Private for Pokemon GO.sln"
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" "NecroBot-Private for Pokemon GO.sln" /property:Configuration=Release /property:Platform="Any CPU"

:VS2017_Enterprise
echo Building under Visual Studio 2017 Enterprise as %processor_architecture%
nuget.exe restore "NecroBot-Private for Pokemon GO.sln"
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe" "NecroBot-Private for Pokemon GO.sln" /property:Configuration=Release /property:Platform="Any CPU"