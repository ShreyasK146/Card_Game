using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
public class NetworkkManager : MonoBehaviourPunCallbacks
{
    public static NetworkkManager Instance;


    public GameObject statusUI;
    public GameObject MainScene;
    public TextMeshProUGUI statusText;
    [HideInInspector] public bool gameStarted = false;

    private int maxPlayers = 2;
    private bool isReconnecting = false;

    private PhotonView photonView;
    private Coroutine disconnectCoroutine;
    private string roomNameToRejoin;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (statusUI != null)
            {
                DontDestroyOnLoad(statusUI);
            }
        }
        else
        {
            Destroy(gameObject);
        }
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        Application.runInBackground = true;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        TurnManager.Instance.timerActive = false;
        if (this.CanRecoverFromDisconnect(cause))
        {
            this.Recover();
        }
    }

    private bool CanRecoverFromDisconnect(DisconnectCause cause)
    {
        switch (cause)
        {
            // the list here may be non exhaustive and is subject to review

            case DisconnectCause.Exception:
            case DisconnectCause.ExceptionOnConnect:
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.ClientTimeout:
            case DisconnectCause.DisconnectByServerLogic:
            case DisconnectCause.DisconnectByServerReasonUnknown:
                return true;
        }
        Debug.Log("why iam here?");
        return false;
    }
    //private void Update()
    //{
    //    if(PhotonNetwork.CurrentRoom.PlayerCount != 2 && TurnManager.Instance.timerActive)
            
    //}
    private void Recover()
    {
        Debug.Log("I got disconnected lets see if i can join");
        statusUI.gameObject.SetActive(true);
        statusText.text = "Disconnected trying to rejoin. Match will automatically abandoned if failed to join";
        isReconnecting = true;
        if (!PhotonNetwork.ReconnectAndRejoin())
        {
            Debug.Log("ReconnectAndRejoin failed, trying Reconnect");
            if (!PhotonNetwork.Reconnect())
            {
                Debug.Log("Reconnect failed, trying ConnectUsingSettings");
                if (!PhotonNetwork.ConnectUsingSettings())
                {
                    Debug.Log("ConnectUsingSettings failed");
                }
                else
                {
                    Debug.Log("I'm here 3");
                }
            }
            else
            {
                Debug.Log("I'm here 2");
            }
        }
        else
        {
            Debug.Log("I'm here ");
        }
    }

    public override void OnConnectedToMaster()
    {
        if (PhotonNetwork.InRoom)
            return;
        if (isReconnecting)
        {
            PhotonNetwork.RejoinRoom(roomNameToRejoin); // rejoin specific room
            return;
        }
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinedRoom()
    {
        Debug.Log($"OnJoinedRoom - isReconnecting:{isReconnecting} playerCount:{PhotonNetwork.CurrentRoom.PlayerCount}");
        if (isReconnecting)
        {
            photonView.RPC("SyncTimerOnReconnect", RpcTarget.Others, TurnManager.Instance.timer);
            Debug.Log("reconnected");
            isReconnecting = false;
            statusUI.SetActive(false);
            TurnManager.Instance.timerActive = true;
            if (!MainScene.activeSelf)
                MainScene.SetActive(true);

            return;
        }
        if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers)
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
    [PunRPC]
    void SyncTimerOnReconnect(float opponentTimer)
    {
        // Use whichever timer is higher (more fair)
        TurnManager.Instance.timer = Mathf.Max(TurnManager.Instance.timer, opponentTimer);
        TurnManager.Instance.timerActive = true;
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {

        if (disconnectCoroutine != null)
        {
            
            StopCoroutine(disconnectCoroutine);
            disconnectCoroutine = null;
            statusUI.SetActive(false);
            //Time.timeScale = 1f;
            statusText.text = "Opponent reconnected!";
            TurnManager.Instance.timerActive = true;
        }
        else if (PhotonNetwork.CurrentRoom.PlayerCount == maxPlayers && !gameStarted)
        {
            StartGame();
        }
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"JoinRoomFailed: {returnCode} - {message}");
        statusText.text = "Creating new room...";
        CreateRoom();
    }
    
    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayers;
        roomOptions.PlayerTtl = 30000;
        roomOptions.EmptyRoomTtl = 10000;
        PhotonNetwork.CreateRoom(null, roomOptions, null);
        //Debug.Log("Room Created?");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //TurnManager.Instance.timerActive = false;
        statusText.text = "Opponent disconnected. Waiting 15s...";
        statusUI.SetActive(true);
        //Time.timeScale = 0f;
        disconnectCoroutine = StartCoroutine(DisconnectCountdown());
    }
    private IEnumerator DisconnectCountdown()
    {
        for (int i = 15; i > 0; i--)
        {
            statusText.text = $"Opponent disconnected. Waiting {i}s...";
            yield return new WaitForSecondsRealtime(1f); // realtime because timeScale is 0
        }
        //Time.timeScale = 1f;
        // end the game however you handle it
        Debug.Log("Opponent did not reconnect, ending game");
    }

    private void StartGame()
    {
        roomNameToRejoin = PhotonNetwork.CurrentRoom.Name;  
        Debug.Log("im in start game");
        statusText.text = "Starting game...";
        gameStarted = true;
        statusUI.gameObject.SetActive(false);
        MainScene.SetActive(true);
        //string localPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
        List<string> playerIds = new List<string> { "Player1", "Player2" };
        GameEvents.instance.GameStarted(playerIds);
    }

    public void SendNetworkMessage(string jsonMessage)
    {
        photonView.RPC("ReceiveNetworkMessage",RpcTarget.All, jsonMessage);
    }

    [PunRPC]
    void ReceiveNetworkMessage(string jsonMessage)
    {
        NetworkMessage message = JsonUtility.FromJson<NetworkMessage>(jsonMessage);

        switch (message.action)
        {
            case "turnStart":
                HandleTurnStartMessage(jsonMessage);
                break;
            case "syncBoard":
                HandleSyncBoardMessage(jsonMessage);
                break;
            case "revealCards":
                HandleRevealCardsMessage(jsonMessage);
                break;
            case "endTurn":
                HandleEndTurnMessage(jsonMessage); 
                break;
            case "scoreUpdate":
                HandleScoreUpdateMessage(jsonMessage);
                break;
            case "gameEnd":
                HandleEndGameMessage(jsonMessage);
                break;
            case "whoseTurn":
                HandleWhoseTurnMessage(jsonMessage);
                break;
            case "counterUpdate":
                HandleCounterUpdateMessage(jsonMessage);
                break;
            case "handleRemainingReveals":
                HandleRemainingRevealMessage(jsonMessage);
                break;


        }
    }

    private void HandleRemainingRevealMessage(string jsonMessage)
    {
        HandleRemainingMessage msg = JsonUtility.FromJson<HandleRemainingMessage>(jsonMessage);
        RevealManager.Instance.Method3(msg.counter);
    }

    private void HandleWhoseTurnMessage(string jsonMessage)
    {
        WhoseTurnMessage msg = JsonUtility.FromJson<WhoseTurnMessage>(jsonMessage);
        RevealManager.Instance.MessageAndCounterUpdatesFromNetwork(msg.playerId, msg.isMastersTurn, msg.counter);
    }

    private void HandleCounterUpdateMessage(string jsonMessage)
    {
        CounterUpdateMessage msg = JsonUtility.FromJson<CounterUpdateMessage>(jsonMessage);
        DeckManager.Instance.CounterUpdatesFromNetwork(msg.playerId,msg.counter);
    }


    private void HandleEndGameMessage(string jsonMessage)
    {
        GameEndMessage message = JsonUtility.FromJson<GameEndMessage>(jsonMessage);
        //ScoreManager.Instance.AnnounceResultFromNetwork(message.action, message.message);
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();

        if (scoreManager != null)
        {
            scoreManager.AnnounceResultFromNetwork(message.action, message.message);
        }
    }

    private void HandleScoreUpdateMessage(string jsonMessage)
    {
        ScoreUpdateMessage msg = JsonUtility.FromJson<ScoreUpdateMessage>(jsonMessage);
        //ScoreManager.Instance.AddScoreFromNetwork(msg.playerId, msg.pointsEarned);
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();

        if(scoreManager != null)
        {
            scoreManager.AddScoreFromNetwork(msg.playerId, msg.pointsEarnedByPlayer, msg.pointsForOpponent);
        }
    }

    private void HandleTurnStartMessage(string jsonMessage)
    {
        TurnStartMessage msg = JsonUtility.FromJson<TurnStartMessage>(jsonMessage);
        TurnManager.Instance.StartTurnFromNetwork(msg.turnNumber,msg.count);
    }

    private void HandleSyncBoardMessage(string jsonMessage)
    {
        
        SyncBoardMessage msg = JsonUtility.FromJson<SyncBoardMessage>(jsonMessage);
        if(msg.senderActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
        {
            OpponentBoardDisplay opponentBoard = FindFirstObjectByType<OpponentBoardDisplay>();
            
            if (opponentBoard != null)
            {
                opponentBoard.UpdateOpponentBoard(msg.cardCount, msg.availableCost);
            }
        }
    }
    private void HandleRevealCardsMessage(string jsonMessage)
    {
        RevealCardsMessage msg = JsonUtility.FromJson<RevealCardsMessage>(jsonMessage);
        Debug.Log($"Received reveal cards from {msg.playerId}");    

        RevealManager.Instance.ReceiveOpponentCards(msg.playerId, msg.cardIds);
    }
    private void HandleEndTurnMessage(string jsonMessage)
    {
        EndTurnMessage msg = JsonUtility.FromJson<EndTurnMessage>(jsonMessage);
        GameEvents.instance.PlayerEndedTurn(msg.playerId);

        // Update message display (only if it's opponent's message)
        string myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
        if (msg.playerId != myPlayerId)
        {
            TurnManager.Instance.UpdateOpponentMessageDisplay(msg.messageToDisplay);
        }
    }
}

[System.Serializable]
public class NetworkMessage
{
    public string action;
}

[System.Serializable]

public class EndTurnMessage
{
    public string action = "endTurn";
    public string playerId;
    public string messageToDisplay;
}

[System.Serializable]
public class SyncBoardMessage
{
    public string action = "syncBoard";
    public int cardCount;
    public int senderActorNumber;
    public int availableCost;
}

[System.Serializable]
public class TurnStartMessage
{
    public string action = "turnStart";
    public int turnNumber;
    public int count;
}

[System.Serializable]

public class ScoreUpdateMessage
{
    public string action = "scoreUpdate";
    public int pointsEarnedByPlayer;
    public int pointsForOpponent;
    public string playerId;
}

[System.Serializable]
public class RevealCardsMessage
{
    public string action = "revealCards";
    public string playerId;
    public List<int> cardIds;
}

[System.Serializable]
public class GameEndMessage
{
    public string action = "gameEnd";
    public string message;
}

[System.Serializable]
public class WhoseTurnMessage
{
    public string playerId;
    public string action = "whoseTurn";
    public bool isMastersTurn = true;
    public int counter = 0;
}
//[System.Serializable]
//public class WhoseTurnMessage2
//{
//    public string action = "whoseTurn2";
//    public bool isMastersTurn = true;
//}



[System.Serializable]
public class CounterUpdateMessage
{
    public string action = "counterUpdate";
    public string playerId;
    public int counter = 0;
}

[System.Serializable]
public class HandleRemainingMessage
{
    public string action = "handleRemainingReveals";
    public string playerId;
    public int counter = 0;
}

//[System.Serializable]
//public class AbilityExecuteMessage
//{
//    public string action = "abilityExecute";
//    public string playerId;
//    public string abilityName;
//    public int abilityValue;
//    public int cardPower;
//}
