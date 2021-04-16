using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewMenuManager : MonoBehaviour
{
    public TMPro.TMP_InputField ipInputField;
    public TMPro.TMP_InputField pseudoInputField;
    public TMPro.TMP_Dropdown classDropdown;

    public GameObject mainMenu;
    public GameObject creditMenu;

    private void Start()
    {
        ipInputField.text = PlayerData.ConnectAddress;
        ipInputField.onEndEdit.AddListener(SetIp);

        pseudoInputField.text = PlayerData.Name;
        pseudoInputField.onEndEdit.AddListener(SetPseudo);

        classDropdown.onValueChanged.AddListener(SetClass);

        mainMenu.SetActive(true);
        creditMenu.SetActive(false);
    }

    public void SetClass(int classId)
    {
        switch(classId)
        {
            case 0: PlayerData.Class = enPlayerClass.GD;
                break;

            case 1:
                PlayerData.Class = enPlayerClass.GP;
                break;

            case 2:
                PlayerData.Class = enPlayerClass.ART;
                break;

            default:
                PlayerData.Class = enPlayerClass.GD;
                break;
        }
    }

    public void SetPseudo(string pseudo)
    {
        PlayerData.Name = pseudo;
    }

    public void SetIp(string ip)
    {
        PlayerData.ConnectAddress = ip;
    }

    public void Host()
    {
        PlayerData.IsHost = true;
        SceneManager.LoadScene("LobbyScene");
    }

    public void Client()
    {
        PlayerData.IsHost = false;
        SceneManager.LoadScene("LobbyScene");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void CreditMenu()
    {
        mainMenu.SetActive(false);
        creditMenu.SetActive(true);
    }

    public void MainMenu()
    {
        mainMenu.SetActive(true);
        creditMenu.SetActive(false);
    }
}
