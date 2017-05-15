@echo off
src\packages\OpenCover.4.6.519\tools\OpenCover.Console.exe -register:user -target:"src\packages\xunit.runner.console.2.2.0\tools\xunit.console.x86.exe" -targetargs:"src\EventSourcing.Tests.Unit\bin\Debug\EventSourcing.Tests.Unit.dll -noshadow" -output:".\build\coverage.xml" -filter:"+[EventSourcing*]* -[EventSourcing.Tests.Unit*]* -[EventSourcing.Sampes.Core]*" 
if %ERRORLEVEL% == 0 goto :next

:quit
exit /b %ERRORLEVEL%

:next
@echo.
@echo %date%
@echo %time%
@echo.