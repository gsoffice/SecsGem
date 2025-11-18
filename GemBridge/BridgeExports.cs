using System;
using System.Runtime.InteropServices;
using SecsGemLib;

public static class BridgeExports
{
    private static GemApi? _gem;

    [UnmanagedCallersOnly(EntryPoint = "Gem_Init")]
    public static int Gem_Init()
    {
        _gem = new GemApi();
        return 1;
    }

    [UnmanagedCallersOnly(EntryPoint = "Gem_Connect")]
    public static int Gem_Connect(IntPtr ipPtr, int port, bool passive)
    {
        string ip = Marshal.PtrToStringAnsi(ipPtr)!;

        var t = _gem!.ConnectAsync(ip, port, passive);
        t.Wait();

        return t.Result ? 1 : 0;
    }

    [UnmanagedCallersOnly(EntryPoint = "Gem_Disconnect")]
    public static void Gem_Disconnect()
    {
        _gem?.Disconnect();
    }
}
