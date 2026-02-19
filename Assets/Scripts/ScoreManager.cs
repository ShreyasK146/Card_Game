using TMPro;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;


public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] TextMeshProUGUI playerScoreText;

    [SerializeField] TextMeshProUGUI opponentName;
    [SerializeField] TextMeshProUGUI opponentScoreText;

    //[SerializeField] GameObject mainMenu;

    public int myScore = 0;
    public int opponentScore = 0;

    private string myPlayerId;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        //GameEvents.instance.OnScoreUpdated += HandleScoreUpdate;
        GameEvents.instance.OnGameStart += HandleGameStart;
        GameEvents.instance.OnGameEnd += AnnounceResult;
    }

    private void OnDisable()
    {
        if (GameEvents.instance != null)
        {
            // GameEvents.instance.OnScoreUpdated -= HandleScoreUpdate;
            GameEvents.instance.OnGameStart -= HandleGameStart;
            GameEvents.instance.OnGameEnd -= AnnounceResult;
        }
    }

    void HandleGameStart(List<string> playerIds)
    {
        myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
       
        if (PhotonNetwork.IsMasterClient)
        {
            playerName.text = "Player One";
            opponentName.text = "Player Two";
        }
        else
        {
            playerName.text = "Player Two";
            opponentName.text = "Player One";
        }

        UpdateScoreDisplay();
    }
  
    public void UpdateScoreDisplay()
    {
        playerScoreText.text = myScore.ToString();
        opponentScoreText.text = opponentScore.ToString() ;
    }
    public void AddScore(string playerId, int point,int opponentPoint)
    {
        //if (playerId == myPlayerId)
        //{
        //    myScore = points;
        //    if (myScore < 0) myScore = 0; 
        //}

        //UpdateScoreDisplay();
        if(playerId == myPlayerId)
        {
            Debug.Log("myScore = " + ScoreManager.Instance.myScore + "\t opponentScore = " + ScoreManager.Instance.opponentScore);
            ScoreUpdateMessage msg = new ScoreUpdateMessage
            {
                action = "scoreUpdate",
                pointsEarnedByPlayer = point,
                pointsForOpponent = opponentPoint,
                playerId = playerId
            };
            //ScoreUpdateMessage msgToUpdateOpponentScoreOnOpponentScene = new ScoreUpdateMessage
            //{
            //    action = "scoreUpdate",
            //    pointsEarned = points,
            //    playerId = playerId
            //};
            NetworkkManager.Instance.SendNetworkMessage(JsonUtility.ToJson(msg));
            //NetworkkManager.Instance.SendNetworkMessage(JsonUtility.ToJson(msgToUpdateOpponentScoreOnOpponentScene));
        }

    }

    
    public void AddScoreFromNetwork(string playerId, int opponnetPoint,int myPoint)
    {
        if (playerId != myPlayerId)
        {
            opponentScore = opponnetPoint;
            myScore = myPoint;
            Debug.Log("myScore = " + ScoreManager.Instance.myScore + "\t opponentScore = " + ScoreManager.Instance.opponentScore);
            //if (opponentScore < 0) opponentScore = 0;
            UpdateScoreDisplay();
        }


    }

    //game end message
    public void AnnounceResult()
    {
        Debug.Log("myscore = " + myScore + "opponent score = " + opponentScore);
        if (PhotonNetwork.IsMasterClient)
        {
            string myResultMessage = "";
            string opponentResultMessage = "";
            
            if (myScore > opponentScore)
            {
                myResultMessage = "YOU WIN! CONGRATULATIONS";
                opponentResultMessage = "YOU LOSE! BETTER LUCK NEXT TIME";
            }
            else if (opponentScore > myScore)
            {
                myResultMessage = "YOU LOSE! BETTER LUCK NEXT TIME";
                opponentResultMessage = "YOU WIN! CONGRATULATIONS";
            }
            else
            {
                myResultMessage = "IT'S A TIE";
                opponentResultMessage = "IT'S A TIE";
            }

            NetworkkManager.Instance.statusUI.gameObject.SetActive(true);
            NetworkkManager.Instance.statusText.text = myResultMessage;

            GameEndMessage message = new GameEndMessage
            {
                action = "gameEnd",
                message = opponentResultMessage

            };

            NetworkkManager.Instance.SendNetworkMessage(JsonUtility.ToJson(message));
        }

        
    }

    public void AnnounceResultFromNetwork(string action, string message)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            NetworkkManager.Instance.statusUI.gameObject.SetActive(true);
            NetworkkManager.Instance.statusText.text = message;

        }
    }
}
