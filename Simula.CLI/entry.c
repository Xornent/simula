#include <windows.h>
#include <stdio.h>
#include <stdlib.h>

int main()
{
    int (*yyparse)(void) = NULL;
    HMODULE mod = LoadLibraryA("SimulaScriptingCLI.dll");

    if (mod != NULL) {
        yyparse = (int(*)(void))(GetProcAddress(mod, "entrypos"));
        if (yyparse != NULL)
            yyparse();
        else printf("unable to find function entrypos");
    } else printf("unable to load module");

    return 0;
}