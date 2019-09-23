rem Run in Administrator mode!

@echo off

rem Set current directory (required for Vista and higher)
@setlocal enableextensions
@cd /d "%~dp0"

set regasm64="c:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe"
set regasm32="c:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe"

%regasm64% "bin\Debug_DEMO\Bytescout.PDF.dll" /tlb /codebase
%regasm32% "bin\Debug_DEMO\Bytescout.PDF.dll" /tlb /codebase

pause