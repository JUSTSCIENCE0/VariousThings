#pragma once

#ifdef ARCFOUR_EXPORTS
#define ARCFOUR_API __declspec(dllexport)
#else
#define ARCFOUR_API __declspec(dllimport)
#endif

extern "C" ARCFOUR_API void InitGenerator(BYTE * Key, UINT16 length);
extern "C" ARCFOUR_API BYTE NextBYTE();

