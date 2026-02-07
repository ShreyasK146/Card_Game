using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class NetworkkManager : MonoBehaviourPunCallbacks
{
    public static NetworkkManager Instance;

    [SerializeField] GameObject statusUI;
    [SerializeField] private GameObject MainScene;
    [SerializeField] private TextMeshProUGUI statusText;

    private int maxPlayers = 2;
    private bool isReconnecting = false;
    [HideInInspector]public bool gameStarted = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);  
    }

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.InRoom)
            return;

        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinedRoom()
    {
        if (isReconnecting)
        {
            Debug.Log("reconnected");
            isReconnecting = false;
            statusUI.SetActive(false);
            MainScene.SetActive(true);
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers-1)
        {
            StartGame();
        }
        else
        {
            //statusText.text = "Joined Room!";
            statusText.text = "Waiting For Player...";
        }
        
        //Debug.Log("CountOfRooms = " + PhotonNetwork.CountOfRooms);
        //Debug.Log("CountOfPlayersInRooms  = " + PhotonNetwork.CountOfPlayersInRooms);
        //Debug.Log("CountOfPlayers = " + PhotonNetwork.CountOfPlayers);
    }

    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers && !gameStarted)
        {
            statusText.text = "Starting the game...";
            StartGame();
        }
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        statusText.text = "Creating new room...";
        CreateRoom();
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;
        roomOptions.PlayerTtl = 20000;
        roomOptions.EmptyRoomTtl = 10000; // just in case
        PhotonNetwork.CreateRoom(null, roomOptions, null);
        //Debug.Log("Room Created?");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        statusUI.gameObject.SetActive(true);    
        if (cause != DisconnectCause.DisconnectByClientLogic)
        {
            Debug.Log("disconnected and tryign to join");
            isReconnecting = true;
            PhotonNetwork.ReconnectAndRejoin(); // to reconnect if ttl is not over yet
        }

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        statusUI.gameObject.SetActive(true);
        statusText.text = "Where did he go?";
        
    }

    private void StartGame()
    {
        statusText.text = "Starting game...";
        gameStarted = true;
        statusUI.SetActive(false);
        MainScene.SetActive(true);
        // start doing something ...

    }
}
