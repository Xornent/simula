#include "resources.h"

const char* stringTable[] = {
	"",
	"the access of the given source file is denied.",
	"a sibling cannot be added to root node. a root can only have childrens."
};

const char* res(int id) { return stringTable[id]; }