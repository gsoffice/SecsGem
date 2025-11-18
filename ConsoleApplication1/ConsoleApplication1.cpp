#include <windows.h>
#include <iostream>

typedef int(__cdecl* GEM_INIT)();
typedef int(__cdecl* GEM_CONNECT)(const char*, int, bool);
typedef void(__cdecl* GEM_DISCONNECT)();

int main()
{
    HMODULE lib = LoadLibraryA("GemBridge.dll");
    if (!lib)
    {
        std::cout << "DLL load failed. Error: " << GetLastError() << std::endl;
        return 1;
    }
    std::cout << "DLL loaded." << std::endl;

    GEM_INIT Gem_Init = (GEM_INIT)GetProcAddress(lib, "Gem_Init");
    GEM_CONNECT Gem_Connect = (GEM_CONNECT)GetProcAddress(lib, "Gem_Connect");
    GEM_DISCONNECT Gem_Disconnect = (GEM_DISCONNECT)GetProcAddress(lib, "Gem_Disconnect");

    if (!Gem_Init || !Gem_Connect || !Gem_Disconnect)
    {
        std::cout << "Failed to get exported functions." << std::endl;
        return 1;
    }

    std::cout << "Gem_Init()" << std::endl;
    int ret = Gem_Init();
    std::cout << "Gem_Init returned: " << ret << std::endl;

    std::cout << "Gem_Connect(\"127.0.0.1\", 5000, true)" << std::endl;
    int conn = Gem_Connect("127.0.0.1", 5000, true    );
    std::cout << "Gem_Connect returned: " << conn << std::endl;

    //std::cout << "Gem_Disconnect()" << std::endl;
    //Gem_Disconnect();
    getchar();

    FreeLibrary(lib);

    return 0;
}
