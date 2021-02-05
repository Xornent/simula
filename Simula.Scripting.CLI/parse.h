#ifndef PARSE_H
#define PARSE_H
#include "syntaxtree.h"

__declspec(dllexport) int entrypos(void);

// open a external text file of source, and using the parser to parse the text.
// it returns 
__declspec(dllexport) syntaxTree* parseFromFile(const char* fileName);

#endif