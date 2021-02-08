#ifndef SYNTAXTREE_H
#define SYNTAXTREE_H

#ifdef c_plusplus
#include <string>
#endif

// types of syntax tree nodes.
typedef enum {
	root = 0,
	
	functionDecl,
	annonymousFunctionDecl,
	classDecl,
	propertyDecl,
	datatypeDecl,

	ifblock,
	elseifblock,
	elseblock,
	whileblock,
	iterateblock,
	conditionalblock,

	contentblock,

	// the evaluation nodes are generic nodes that directly came from the parsing process. the operators,
	// assignments, and other compound statements are not parsed into the tree.
	//
	// in the following process after parsing, we separate the evaluation types down to basic evaluation,
	// their node types is listed below.
	evaluation,

	binaryOperator,
	unaryLeftOperator,
	unaryRightOperator,
	parenthesisOperator,
	bracketOperator,
	braceOperator,
	indexOperator,
	callOperator,
	literals,

	useStmt,
	optionStmt,
	moduleStmt,
	breakstmt,
	continuestmt,
	returnstmt,
	passstmt,

	general

} syntaxNodeType;

// represent a node in the syntax tree. (or the root of a syntax tree)
// the basic structure of the tree is a linked list, with a reference to its siblings and children.
typedef struct syntaxTreeNode {

	// the type of the syntax tree, a integer value among the syntaxTreeNode enumeration. it is set
	// when the syntax tree is generated.
	int type;

	// the depth of the current node down the root.
	// for root node, the indention is set to 0.
	int indention;
	
	struct syntaxTreeNode* children;
	int childrenCount;

	struct syntaxTreeNode* previousSibling;
	struct syntaxTreeNode* nextSibling;
	struct syntaxTreeNode* firstSibling;
	struct syntaxTreeNode* lastSibling;
	struct syntaxTreeNode* parent;
	
	const char* literalString;

} syntaxTree;

syntaxTree* defaultSyntaxTree();
void insertSiblingBefore(syntaxTree* obj, syntaxTree* dest);
void insertSiblingAfter(syntaxTree* obj, syntaxTree* dest);
void insertChild(syntaxTree* obj, syntaxTree* parent);
void appendChild(syntaxTree* obj, syntaxTree* dest);

syntaxTree* createSyntaxTree(syntaxNodeType type);
syntaxTree* createLiteral(char* val);
#ifdef c_plusplus
syntaxTree* createLiteral(std::string val);
#endif

#endif