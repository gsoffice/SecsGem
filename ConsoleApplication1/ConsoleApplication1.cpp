#include "SecsGem.h"
#include <iostream>

// 이 콜백은 C#에서 메시지 들어올 때 호출됨
void __cdecl OnGemMessage(unsigned char* data, int size, long long msgId)
{
    std::string s((char*)data, size);

    std::cout << "[C++] Callback Received: "
        << s
        << " | MsgID = " << msgId
        << std::endl;
}

int main()
{
    SecsGem gem;

    // DLL 로딩
    if (!gem.Load("GemBridge.dll"))
        return 1;

    // 초기화
    gem.Init();

    // 콜백 등록
    gem.RegisterCallback(&OnGemMessage);

    // 연결
    gem.Connect("127.0.0.1", 5000, true);

    std::cout << "Press ENTER to exit..." << std::endl;
    getchar();

    return 0;
}
