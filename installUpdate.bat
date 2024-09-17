@echo

Taskkill /f /im /CherryMerryGramDesktop.exe
set "%PATH%=%CD%"
:exit
msiexec.exe /i "%PATH%\installer.msi" /qn /quiet
del "installer.msi";
cd %PATH%
start CherryMerryGramDesktop.exe