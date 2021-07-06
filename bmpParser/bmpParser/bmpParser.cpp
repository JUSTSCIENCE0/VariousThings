#define _CRT_SECURE_NO_WARNINGS
#define X_OFFSET 0
#include <iostream>

int32_t size;
int32_t data_start;
int32_t widht;
int32_t y = 0;
int offset = 0;
int n=0;

uint8_t buff8;
uint16_t buff16;
uint32_t buff32;
uint64_t buff64;

struct Point
{
    int x, y;
};

void calc_line(FILE* f)
{
    bool continuously = false;
    //Point pnt;
    for (int x = X_OFFSET; x < widht + X_OFFSET; x++)
    {
        fread(&buff8, 1, 1, f);
        fread(&buff16, 2, 1, f);
        offset += 3;
        if (buff16 == 0x0 && buff8 == 0x0)
        {
            if (!continuously)
            {
                n++;
                continuously = true;
                printf("{ %d, %d },\t", x, y);
                if (n % 8 == 0)
                    printf("\n");
            }
        }
        else if (buff16 == 0xFFFF && buff8 == 0xFF)
        {
            if (continuously)
            {
                n++;
                continuously = false;
                printf("{ %d, %d },\t", x - 1, y);
                if (n % 8 == 0)
                    printf("\n");
            }
        }
    }
    if (continuously)
    {
        n++;
        continuously = false;
        printf("{ %d, %d },\t", widht + X_OFFSET - 1, y);
        if (n % 8 == 0)
            printf("\n");
    }
    y++;
}

int main()
{
    FILE* f = fopen("img.bmp", "rb");
    if (!f)
    {
        std::cout << "Couldn't find file!\n";
        return -1;
    }

    fread(&buff16, 2, 1, f);
    offset += 2;
    if (buff16 != 0x4d42)
    {
        std::cout << "Invalid format\n";
        return -1;
    }

    fread(&size, 4, 1, f);
    offset += 4;

    if (size < 0)
    {
        std::cout << "Invalid size\n";
        return -1;
    }
    std::cout << "size = " << size << "\n";

    fread(&buff32, 4, 1, f);
    offset += 4;

    fread(&data_start, 4, 1, f);
    offset += 4;
    if (data_start < 0)
    {
        std::cout << "Invalid data start\n";
        return -1;
    }
    std::cout << "data_start = " << data_start << "\n";

    fread(&buff32, 4, 1, f);
    offset += 4;

    fread(&widht, 4, 1, f);
    offset += 4;
    if (widht < 1)
    {
        std::cout << "Invalid widht\n";
        return -1;
    }
    std::cout << "widht = " << widht << "\n";

    while (offset < data_start)
    {
        fread(&buff8, 1, 1, f);
        offset += 1;
    }

    printf("struct XPoint[] = {\n");
    while(offset < size)
    {
        calc_line(f);
    }
    printf("};\n n = %d\n", n);

    std::cout << "Succsess!\n";
}