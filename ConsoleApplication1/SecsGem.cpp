#include "SecsGem.h"
#include <iostream>

SecsGem::SecsGem()
{
}

SecsGem::~SecsGem()
{
    if (hDll)
        FreeLibrary(hDll);
}

bool SecsGem::Load(const char* dllPath)
{
    hDll = LoadLibraryA(dllPath);
    if (!hDll)
    {
        std::cout << "[SecsGem] DLL Load Failed. Error=" << GetLastError() << std::endl;
        return false;
    }

    Gem_Init = (GEM_INIT)GetProcAddress(hDll, "Gem_Init");
    Gem_Connect = (GEM_CONNECT)GetProcAddress(hDll, "Gem_Connect");
    Gem_Disconnect = (GEM_DISCONNECT)GetProcAddress(hDll, "Gem_Disconnect");
    Gem_RegisterCallback = (GEM_REGISTER_CALLBACK)GetProcAddress(hDll, "Gem_RegisterCallback");

    if (!Gem_Init || !Gem_Connect || !Gem_Disconnect || !Gem_RegisterCallback)
    {
        std::cout << "[SecsGem] Failed to load required DLL exports." << std::endl;
        return false;
    }

    return true;
}

bool SecsGem::Init()
{
    if (!Gem_Init)
        return false;

    int ret = Gem_Init();
    std::cout << "[SecsGem] Gem_Init returned: " << ret << std::endl;
    return ret == 1;
}

void SecsGem::RegisterCallback(GEM_CALLBACK callback)
{
    if (!Gem_RegisterCallback)
        return;

    Gem_RegisterCallback(callback);
    std::cout << "[SecsGem] Callback registered." << std::endl;
}

bool SecsGem::Connect(const char* ip, int port, bool passive)
{
    if (!Gem_Connect)
        return false;

    int ret = Gem_Connect(ip, port, passive);
    std::cout << "[SecsGem] Gem_Connect returned: " << ret << std::endl;

    return ret == 1;
}

void SecsGem::Disconnect()
{
    if (Gem_Disconnect)
        Gem_Disconnect();
}
