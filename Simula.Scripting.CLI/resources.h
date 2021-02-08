#ifndef RESOURCES_H
#define RESOURCES_H

const char* stringTable[];

enum resourceId {
	fileAccessDenied = 1,

	insertSiblingToRoot
};

const char* res(int id);
void report(int id);

char* strcat_c(char* strDest, char* strSrc);

#endif