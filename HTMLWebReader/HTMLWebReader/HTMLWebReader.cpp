#define _WINSOCK_DEPRECATED_NO_WARNINGS
#define _CRT_SECURE_NO_WARNINGS

#define DEG 0x26646567
#define PPP 0x3C2F703E

#define STOP 0x3E3C2F68746D6C3E

#include <iostream>
#include <winsock2.h>
#include <WS2tcpip.h>
#include <List>
#include <openssl/ssl.h>
#include <openssl/err.h>

using namespace std;

int sock;
SSL* ssl;
FILE* f = fopen("D:\\data.html", "wb");
list<BYTE> HTTPpacket;
string curr_temp = "";
string curr_sky = "";
 
string RusUTF8ToChar(const char* data, int size)
{
    int offset = 0;
    string res = "";
    while (offset < size)
    {
        uint8_t fggh = data[offset];
        if ((uint8_t)data[offset] == 0xD0 || (uint8_t)data[offset] == 0xD1)
        {
            uint16_t ch = (uint8_t)data[offset];
            ch = (ch << 8) + (uint8_t)data[offset + 1];
            offset += 2;
            uint8_t bt;
            if (ch <= 0xD0BF)
            {
                bt = ch - 0xCFD0;
            }
            else
            {
                bt = ch - 0xD090;
            }
            if (ch == 0xD001)
            {
                bt = 168;
            }
            if (ch == 0xD191)
            {
                bt = 184;
            }
            res += bt;
        }
        else
        {
            res += (uint8_t)data[offset];
            offset++;
        }
    }
    return res;
}

inline bool InitializeSockets()
{
    WSADATA WsaData;
    return WSAStartup(MAKEWORD(2, 2), &WsaData) == NO_ERROR;
}

inline void ShutdownSockets()
{
    WSACleanup();
}



void GetWheather()
{
    curr_temp = "";
    curr_sky = "";
    uint32_t buff32 = 0x0;
    list<BYTE>::iterator buff8 = HTTPpacket.begin();
    while (buff8 != HTTPpacket.end())
    {
        buff32 <<= 8;
        buff32 += *buff8;

        if (buff32 == DEG)
        {
            buff8--;
            buff8--;
            buff8--;
            buff8--;
            break;
        }

        buff8++;
    }
    if (buff8 == HTTPpacket.end())
    {
        printf("ERROR: temperature not found!");
        curr_temp = "ERR";
        curr_sky = "ERROR";
        return;
    }
    while (*buff8 != '>')
    {
        char tmp = (char)*buff8;
        curr_temp = tmp + curr_temp;
        buff8--;
    }
    cout << curr_temp << "\n";

    buff32 = 0x0;

    while (buff8 != HTTPpacket.end())
    {
        buff32 <<= 8;
        buff32 += *buff8;

        if (buff32 == PPP)
        {
            buff8--;
            buff8--;
            buff8--;
            buff8--;
            break;
        }

        buff8++;
    }
    if (buff8 == HTTPpacket.end())
    {
        printf("ERROR: sky not found!");
        curr_temp = "ERR";
        curr_sky = "ERROR";
        return;
    }
    while (*buff8 != '>')
    {
        char tmp = (char)*buff8;
        curr_sky = tmp + curr_sky;
        buff8--;
    }
    curr_sky = RusUTF8ToChar(curr_sky.c_str(), strlen(curr_sky.c_str()));
    cout << curr_sky << "\n";
}

int RecvPacket()
{
    int len = 1;
    uint64_t stop_point = 0x0;
    char buf;
    do {
        len = SSL_read(ssl, &buf, 1);
        stop_point <<= 8;
        stop_point += buf;
        HTTPpacket.push_back(buf);
        fwrite(&buf, 1, 1, f);
        fflush(f);
    } while (len > 0 && stop_point != STOP);
    if (len < 0) {
        int err = SSL_get_error(ssl, len);
        if (err == SSL_ERROR_WANT_READ)
            return 0;
        if (err == SSL_ERROR_WANT_WRITE)
            return 0;
        if (err == SSL_ERROR_ZERO_RETURN || err == SSL_ERROR_SYSCALL || err == SSL_ERROR_SSL)
            return -1;
    }
    return 1;
}

int SendPacket(const char* buf)
{
    int len = SSL_write(ssl, buf, strlen(buf));
    if (len < 0) {
        int err = SSL_get_error(ssl, len);
        switch (err) {
        case SSL_ERROR_WANT_WRITE:
            return 0;
        case SSL_ERROR_WANT_READ:
            return 0;
        case SSL_ERROR_ZERO_RETURN:
        case SSL_ERROR_SYSCALL:
        case SSL_ERROR_SSL:
        default:
            return -1;
        }
    }
    return 1;
}

void log_ssl()
{
    int err;
    while (err = ERR_get_error()) {
        char* str = ERR_error_string(err, 0);
        if (!str)
            return;
        printf(str);
        printf("\n");
        fflush(stdout);
    }
}

int main()
{
    setlocale(LC_ALL, "Russian");
    InitializeSockets();
    int handle = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

    if (handle < 0)
    {
        printf("failed to create socket\n");
        system("pause");
        return false;
    }

    typedef int socklen_t;

    struct hostent* remoteHost;
    remoteHost = gethostbyname("pogoda.vtomske.ru");

    char* ip_addr = new char[16];
    memset(ip_addr, 0, 16);
    snprintf(ip_addr, 15, "%d.%d.%d.%d", 
        (BYTE)(remoteHost->h_addr[0]),
        (BYTE)(remoteHost->h_addr[1]),
        (BYTE)(remoteHost->h_addr[2]),
        (BYTE)(remoteHost->h_addr[3]));
    printf("%s\n", ip_addr);

    struct sockaddr_in sa;
    memset(&sa, 0, sizeof(sa));
    sa.sin_family = AF_INET;
    sa.sin_addr.s_addr = inet_addr(ip_addr); // 173.230.129.147 //  address
    sa.sin_port = htons(443);
    socklen_t socklen = sizeof(sa);
    if (connect(handle, (struct sockaddr*) & sa, socklen))
    {
        printf("Error connecting to server.\n");
        system("pause");
        return -1;
    }

    SSL_library_init();
    SSLeay_add_ssl_algorithms();
    SSL_load_error_strings();
    const SSL_METHOD* meth = TLSv1_2_client_method();
    SSL_CTX* ctx = SSL_CTX_new(meth);
    ssl = SSL_new(ctx);
    if (!ssl) {
        printf("Error creating SSL.\n");
        log_ssl();
        system("pause");
        return -1;
    }

    sock = SSL_get_fd(ssl);
    SSL_set_fd(ssl, handle);

    int err = SSL_connect(ssl);
    if (err <= 0) {
        printf("Error creating SSL connection.  err=%x\n", err);
        log_ssl();
        fflush(stdout);
        system("pause");
        return -1;
    }
    printf("SSL connection using %s\n", SSL_get_cipher(ssl));
    
    const char* request = "GET /tomsk HTTP/1.1\r\nHost: pogoda.vtomske.ru \r\n\r\n";

    SendPacket(request);
    RecvPacket();
    /*list<BYTE>::iterator i = HTTPpacket.begin();
    for (; i != HTTPpacket.end(); i++)
       printf("%c", *i);*/
    GetWheather();
    printf("\nsuccess!\n");
}