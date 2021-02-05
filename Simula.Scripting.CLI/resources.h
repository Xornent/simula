#ifndef RESOURCES_H
#define RESOURCES_H

const char* stringTable[];

enum resourceId {
	fileAccessDenied = 1,

	insertSiblingToRoot
};

const char* res(int id);

#endif