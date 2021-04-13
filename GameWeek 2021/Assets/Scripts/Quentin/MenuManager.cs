using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        StartButtons();
        GUILayout.EndArea();
    }

    static void StartButtons()
    {
        if (GUILayout.Button("Host"))
        {
            PlayerData.IsHost = true;
            SceneManager.LoadScene("LobbyScene");
        }

        if (GUILayout.Button("Client"))
        {
            PlayerData.IsHost = false;
            SceneManager.LoadScene("LobbyScene");
        }

        PlayerData.ConnectAddress = GUILayout.TextField(PlayerData.ConnectAddress, 30);
        PlayerData.Name = GUILayout.TextField(PlayerData.Name, 10);
    }
}