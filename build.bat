echo off
if exists "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE" goto VS2015_Community
if exists "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community" goto VS2017_Community

:VS2017_Community
echo Building under Visual Studio 2017 Community as %processor_architecture%
if "%processor_architecture%"=="AMD64" (
nuget.exe restore "NecroBot-Private for Pokemon GO.sln"
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" "NecroBot-Private for Pokemon GO.sln" /property:Configuration=Release /property:Platform=x64
) ELSE (
nuget.exe restore "NecroBot-Private for Pokemon GO.sln"
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\MSBuild.exe" "NecroBot-Private for Pokemon GO.sln" /property:Configuration=Release /property:Platform=x86
)

:VS2015_Community
echo Building under Visual Studio 2015 Community as %processor_architecture%
if "%processor_architecture%"=="AMD64" (
nuget.exe restore "NecroBot-Private for Pokemon GO.sln"
"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" "NecroBot-Private for Pokemon GO.sln" /property:Configuration=Release /property:Platform=x64
) ELSE (
nuget.exe restore "NecroBot-Private for Pokemon GO.sln"
"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe" "NecroBot-Private for Pokemon GO.sln" /property:Configuration=Release /property:Platform=x86
)