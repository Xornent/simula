d:
cd D:\projects\xornent\simula\Simula.Scripting.CLI\

D:\projects\tools\gnu\mix\bin\flex.exe tokens.l

cd D:\projects\tools\gnu\mix\bin
bison.exe -d D:\projects\xornent\simula\Simula.Scripting.CLI\syntax.y
copy syntax.tab.c D:\projects\xornent\simula\Simula.Scripting.CLI\
copy syntax.tab.h D:\projects\xornent\simula\Simula.Scripting.CLI\