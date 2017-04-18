echo off

nuget.exe restore "NecroBot-Private for Pokemon GO.sln"
"C:\Program Files (x86)\Microsoft Visual Studio\2017\BuildTools\MSBuild\15.0\Bin\MsBuild.exe" "NecroBot-Private for Pokemon GO.sln" /property:Configuration=Release /property:Platform=x86
