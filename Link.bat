@echo off
%1 mshta vbscript:createobject("shell.application").shellexecute("%~s0","::","","runas",1)(window.close)&exit
cd /d "%~dp0"
Link\Link.exe Link\Link.txt
