using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        roomOptions.PlayerTtl = 30000;
        roomOptions.EmptyRoomTtl = 10000;
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
            //case "abilityExecute":
            //    HandleAbilityExecuteMessage(jsonMessage);
            //    break;

        }
    }

    //private void HandleAbilityExecuteMessage(string jsonMessage)
    //{
    //    AbilityExecuteMessage msg = JsonUtility.FromJson<AbilityExecuteMessage>(jsonMessage);

    //    // Only execute on the client that didn't send it
      
    //    AbilityManager.Instance.ExecuteAbilityFromNetwork(msg.playerId, msg.abilityName, msg.abilityValue, msg.cardPower);
        
    //}

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
            scoreManager.AddScoreFromNetwork(msg.playerId, msg.pointsEarned);
        }
    }

    private void HandleTurnStartMessage(string jsonMessage)
    {
        TurnStartMessage msg = JsonUtility.FromJson<TurnStartMessage>(jsonMessage);
        TurnManager.Instance.StartTurnFromNetwork(msg.turnNumber);
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
}

[System.Serializable]

public class ScoreUpdateMessage
{
    public string action = "scoreUpdate";
    public int pointsEarned;
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
//[System.Serializable]
//public class AbilityExecuteMessage
//{
//    public string action = "abilityExecute";
//    public string playerId;
//    public string abilityName;
//    public int abilityValue;
//    public int cardPower;
//}
