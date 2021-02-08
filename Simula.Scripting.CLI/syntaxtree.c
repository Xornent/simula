#include "syntaxtree.h"
#include "resources.h"
#include <stdlib.h>
#include <string.h>

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
	if (obj == NULL) return;
    if(dest->parent == NULL) {
		report(insertSiblingToRoot);
		return;
	}

	dest->parent->childrenCount++;

	// the destination is the first children. 
	if (dest->previousSibling == NULL) {
		obj->indention = dest->indention;
		obj->firstSibling = obj;
		obj->lastSibling = dest->lastSibling;
		obj->nextSibling = dest;
		obj->previousSibling = NULL;
		obj->parent = dest->parent;
		
		syntaxTree* nex = obj->nextSibling;
		while (nex != NULL) {
			nex->firstSibling = obj;
			nex = nex->nextSibling;
		}

		dest->previousSibling = obj;
		
		dest->parent->children = obj;

	} else {
		obj->indention = dest->indention;
		obj->firstSibling = dest->firstSibling;
		obj->lastSibling = dest->lastSibling;
		obj->nextSibling = dest;
		obj->previousSibling = dest->previousSibling;
		obj->parent = dest->parent;

		dest->previousSibling = obj;
	}
}

void insertSiblingAfter(syntaxTree* obj, syntaxTree* dest) {
	if (obj == NULL) return;
	if (dest->parent == NULL) {
		report(insertSiblingToRoot);
		return;
	}

	dest->parent->childrenCount++;

	// the destination is the last children. 
	if (dest->nextSibling == NULL) {
		obj->indention = dest->indention;
		obj->firstSibling = dest->firstSibling;
		obj->lastSibling = obj;
		obj->nextSibling = NULL;
		obj->previousSibling = dest;
		obj->parent = dest->parent;

		syntaxTree* prev = obj->previousSibling;
		while (prev != NULL) {
			prev->lastSibling = obj;
			prev = prev->previousSibling;
		}

		dest->nextSibling = obj;

	} else {
		obj->indention = dest->indention;
		obj->firstSibling = dest->firstSibling;
		obj->lastSibling = dest->lastSibling;
		obj->nextSibling = dest->nextSibling;
		obj->previousSibling = dest;
		obj->parent = dest->parent;

		dest->nextSibling = obj;
	}
}

// insert a child node into the head of the children list.
// if the children is null, this will create a children list for parental nodes.
void insertChild(syntaxTree* obj, syntaxTree* parent) {
	if (obj == NULL) return;
	if (parent->children == NULL) {
		parent->childrenCount = 1;
		parent->children = obj;

		obj->firstSibling = obj;
		obj->lastSibling = obj;
		obj->previousSibling = NULL;
		obj->nextSibling = NULL;
		obj->parent = parent;
		obj->indention = parent->indention + 1;
		return;
	}

	parent->childrenCount++;
	
	obj->previousSibling = NULL;
	obj->nextSibling = parent->children;
	parent->children->previousSibling = obj;

	syntaxTree* nex = obj;
	while (nex != NULL) {
		nex->firstSibling = obj;
		nex = nex->nextSibling;
	}
	obj->lastSibling = parent->children->lastSibling;

	obj->parent = parent;
	obj->indention = parent->indention + 1;
	parent->children = obj;
}

// insert a child node into the end of the children list.
// if the children is null, this will create a children list for parental nodes.
// avoid using this method whenever you can use insertChild, this function forced 
// to enumerate all of the children for a second time.
void appendChild(syntaxTree* obj, syntaxTree* parent) {
	if (obj == NULL) return;
	if (parent->children == NULL) {
		parent->childrenCount = 1;
		parent->children = obj;

		obj->firstSibling = obj;
		obj->lastSibling = obj;
		obj->previousSibling = NULL;
		obj->nextSibling = NULL;
		obj->parent = parent;
		obj->indention = parent->indention + 1;
		return;
	}

	parent->childrenCount++;

	obj->previousSibling = parent->children->lastSibling;
	parent->children->lastSibling->nextSibling = obj;
	obj->nextSibling = NULL;

	syntaxTree* prev = obj;
	while (prev != NULL) {
		prev->lastSibling = obj;
		prev = prev->previousSibling;
	}
	obj->lastSibling = obj;

	obj->parent = parent; 
	obj->indention = parent->indention + 1;
}

syntaxTree* createSyntaxTree(syntaxNodeType type) {
	syntaxTree* def = defaultSyntaxTree();
	def->type = type;
	return def;
}

#ifdef c_plusplus
syntaxTree* createLiteral(std::string val) {
	syntaxTree* tree = createSyntaxTree(literals);
	tree->literalValue.str = val;
	return tree;
}
#endif

syntaxTree* createLiteral(char* val) {
	syntaxTree* tree = createSyntaxTree(literals);
	tree->literalString = val;
	return tree;
}