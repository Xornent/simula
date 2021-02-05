#define _CRT_NONSTDC_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS

#include <stdio.h>
#include <string.h>
#include <errno.h>
#include "syntax.tab.h"
#include "parse.h"
#include "syntaxtree.h"
#include "resources.h"

extern int yylex(void);
extern int yyparse(void);

extern FILE* yyin;
int line = 1;
syntaxTree* syntax;

int yywrap() {
	return 1;
}

void yyerror(const char* s) {
	printf("[error] %s at %d\n", s, line);
}

__declspec(dllexport) int entrypos(void) {
	yyparse();
	return 0;
}

__declspec(dllexport) syntaxTree* parseFromFile(const char* fileName) {
	yyin = fopen(fileName, "r");
	line = 1;

	if (yyin == NULL) {
		printf(res(fileAccessDenied));
		return defaultSyntaxTree();
	}

	syntaxTree* tree = defaultSyntaxTree();
	syntax = tree;
	yyparse();
	fclose(yyin);
	return tree;
}

void parseMessage(char* token) {
	printf("[parseMessage] %s", token);
}