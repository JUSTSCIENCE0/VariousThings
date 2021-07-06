#include "pch.h"

#include "ARCFOUR.h"

BYTE* S = new BYTE[256];
UINT8 i = 0;
UINT8 j = 0;

void InitGenerator(BYTE* Key, UINT16 length)
{
    for (int it = 0; it < 256; it++)
    {
        S[it] = it;
    }
    j = 0;
    for (int it = 0; it < 256; it++)
    {
        j = (j + S[it] + Key[it % length]) % 256;
        std::swap(S[it], S[j]);
    }
    i = 0;
    j = 0;
}

BYTE NextBYTE()
{
    i = (i + 1) % 256;
    j = (j + S[i]) % 256;
    std::swap(S[i], S[j]);
    UINT8 t = (S[i] + S[j]) % 256;
    return S[t];
}