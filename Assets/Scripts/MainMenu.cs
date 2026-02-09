using Photon.Pun;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] GameObject networkManager;
    [SerializeField] GameObject statusUI;
    //[SerializeField] GameObject scoreManager;
    public void QuickMatch()
    {
        statusUI.SetActive(true);
        networkManager.SetActive(true);
        //scoreManager.SetActive(true);
        gameObject.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
                EditorApplication.isPlaying = false;
        #endif
    }
}
