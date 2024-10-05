@echo

Taskkill /f /im /sakuragram.exe
set "%PATH%=%CD%"
:exit
msiexec.exe /i "%PATH%\sakuragram_Release_x64.msi" /qn /quiet
del "installer.msi";
cd %PATH%
start sakuragram.exe