#include "resources.h"
#include <stdio.h>
#include <cassert>
#include <stdlib.h>

const char* stringTable[] = {
	"",
	"the access of the given source file is denied.",
	"a sibling cannot be added to root node. a root can only have childrens."
};

const char* res(int id) { return stringTable[id]; }
void report(int id) {
	printf(stringTable[id]);
}

char* strcat_c(char* strDest, char* strSrc) {
    assert((strDest != NULL) && (strSrc != NULL));
    char* address = (char*)malloc((strlen(strDest) + strlen(strSrc) + 1) * sizeof(char));
    char* tmp = address;
    assert(address != NULL);
    while (*strDest != '\0') {
        *address = *strDest;
        strDest++;
        address++;
    }
    while (*strSrc != '\0') {
        *address = *strSrc;
        strSrc++;
        address++;
    }
    *address = '\0';
    return tmp;
}