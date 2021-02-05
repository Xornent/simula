#include "syntaxtree.h"
#include "resources.h"
#include <stdlib.h>

syntaxTree* defaultSyntaxTree() {
	syntaxTree* treeRoot = (syntaxTree*)(malloc(sizeof(syntaxTree)));
	
	treeRoot->childrenCount = 0;
	treeRoot->children = NULL;
	treeRoot->firstSibling = treeRoot;
	treeRoot->lastSibling = treeRoot;

	treeRoot->previousSibling = NULL;
	treeRoot->nextSibling = NULL;
	treeRoot->parent = NULL;
	treeRoot->indention = 0;
	treeRoot->type = root;

	return treeRoot;
}

void insertSiblingBefore(syntaxTree* obj, syntaxTree* dest) {
    
}

void insertSiblingAfter(syntaxTree* obj, syntaxTree* dest) {}
void insertChild(syntaxTree* obj, syntaxTree* parent) {}
void appendChild(syntaxTree* obj, syntaxTree* dest) {}