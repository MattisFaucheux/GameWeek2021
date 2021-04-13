using MLAPI;
using MLAPI.SceneManagement;
using MLAPI.Transports.UNET;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    private static UNetTransport _net;

    void Start()
    {
        _net = GameObject.Find("NetworkManager").GetComponent<UNetTransport>();
        _net.ConnectAddress = PlayerData.ConnectAddress;
        if (PlayerData.IsHost && !NetworkManager.Singleton.IsHost) NetworkManager.Singleton.StartHost();
        else if (!NetworkManager.Singleton.IsClient) NetworkManager.Singleton.StartClient();
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        StatusLabels();
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsHost) SceneManager.LoadScene("MenuScene");
        GUILayout.EndArea();
    }

    static void StatusLabels()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            if (GUILayout.Button("Close this room")) NetworkManager.Singleton.StopHost();
            if (GUILayout.Button("Launch the game")) NetworkSceneManager.SwitchScene("GameScene");
            GUILayout.Label("You're hosting the room.");
        }
        else
        {
            if (GUILayout.Button("Leave this room")) NetworkManager.Singleton.StopClient();
            if (NetworkManager.Singleton.IsConnectedClient) GUILayout.Label("Connected to: " + _net.ConnectAddress);
            else GUILayout.Label("Trying to connect " + _net.ConnectAddress + "...");
        }
    }
}