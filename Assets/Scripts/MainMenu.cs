using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject LoadingScreen;
    public void QuickMatch()
    {
        PhotonNetwork.ConnectUsingSettings();
        LoadingScreen.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
        gameObject.SetActive(false);
        LoadingScreen.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
