#pragma once
#include <windows.h>
#include <string>

// C# → C++ 콜백 함수 타입
typedef void(__cdecl* GEM_CALLBACK)(unsigned char* data, int size, long long msgId);

// DLL Export 함수 타입들
typedef int(__cdecl* GEM_INIT)();
typedef int(__cdecl* GEM_CONNECT)(const char*, int, bool);
typedef void(__cdecl* GEM_DISCONNECT)();
typedef void(__cdecl* GEM_REGISTER_CALLBACK)(GEM_CALLBACK cb);

class SecsGem
{
public:
    SecsGem();
    ~SecsGem();

    bool Load(const char* dllPath);
    bool Init();
    bool Connect(const char* ip, int port, bool passive);
    void RegisterCallback(GEM_CALLBACK callback);
    void Disconnect();

private:
    HMODULE hDll = nullptr;

    GEM_INIT              Gem_Init = nullptr;
    GEM_CONNECT           Gem_Connect = nullptr;
    GEM_DISCONNECT        Gem_Disconnect = nullptr;
    GEM_REGISTER_CALLBACK Gem_RegisterCallback = nullptr;
};