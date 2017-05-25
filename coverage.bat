@echo off

 .\src\packages\OpenCover.Console.exe -register:user -target:".\src\packages\xunit.console.x86.exe" -targetargs:".\src\Eventus.Tests.Unit\bin\Debug\Eventus.Tests.Unit.dll -noshadow" -output:".\build\coverage.xml" -filter:"+[Eventus*]* -[Eventus]Eventus.Properties.* -[Eventus.Tests.Unit*]* -[Eventus.Samples.Core*]*"
if %ERRORLEVEL% == 0 goto :next

cmd /k
