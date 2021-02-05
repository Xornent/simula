#ifndef DYNAMIC_LINKS_H
#define DYNAMIC_LINKS_H

#include "winheader.h"

// a method to link the function from an external dynamic link library.
// 
// the declaration statements in the dll code is 
//     __declspec(dllexport) void method(int a) { ... }
//
// and the caller to load the dll into the application is by calling as follows, which is wrapped up in the
// macro to be called easier.
//     void (*method) (int) = NULL;
//     HMODULE module = LoadLibraryA("dll.dll")
//     if(module != null) 
//         method = (void(*)(int)) GetProcessAddress(module, "method")

#endif