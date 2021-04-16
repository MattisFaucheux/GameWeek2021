using UnityEngine;

public static class PlayerData
{
    private static bool _isHost = false;

    private static string _connectAddress = "127.0.0.1";

    private static string _name = "Name";
    
    private static enPlayerClass _class = enPlayerClass.GD;

    public static bool IsHost
    {
        get => _isHost;
        set => _isHost = value;
    }

    public static string ConnectAddress
    {
        get => _connectAddress;
        set => _connectAddress = value;
    }

    public static string Name
    {
        get => _name;
        set => _name = value;
    }

    public static enPlayerClass Class
    {
        get => _class;
        set => _class = value;
    }
}