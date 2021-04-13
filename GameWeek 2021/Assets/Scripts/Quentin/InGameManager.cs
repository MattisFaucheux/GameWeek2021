using MLAPI;
using MLAPI.SceneManagement;
using MLAPI.Transports.UNET;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviour
{
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
            if (GUILayout.Button("Stop the game")) NetworkSceneManager.SwitchScene("LobbyScene");
            GUILayout.Label("You're hosting the room.");
        }
        else
        {
            if (GUILayout.Button("Disconnect")) NetworkManager.Singleton.StopClient();
            if (NetworkManager.Singleton.IsConnectedClient) GUILayout.Label("Connected to: " + PlayerData.ConnectAddress);
            else GUILayout.Label("Trying to connect " + PlayerData.ConnectAddress + "...");
        }
    }
}