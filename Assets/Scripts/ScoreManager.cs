using System;
using TMPro;
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [SerializeField] TextMeshProUGUI playerName;
    [SerializeField] TextMeshProUGUI playerScoreText;

    [SerializeField] TextMeshProUGUI opponentName;
    [SerializeField] TextMeshProUGUI opponentScoreText;

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
    private void OnEnable()
    {
        //GameEvents.instance.OnScoreUpdated += HandleScoreUpdate;
        GameEvents.instance.OnGameStart += HandleGameStart; 
    }

    private void OnDisable()
    {
        if (GameEvents.instance != null)
        {
           // GameEvents.instance.OnScoreUpdated -= HandleScoreUpdate;
            GameEvents.instance.OnGameStart -= HandleGameStart;
        }
    }
    //public void HandleScoreUpdate(string playerId, int points)
    //{
    //    if (playerId == myPlayerId)
    //        myScore += points;
    //    else
    //        opponentScore += points;
    //    UpdateScoreDisplay();
    //}
    private void UpdateScoreDisplay()
    {
        playerScoreText.text = myScore.ToString();
        opponentScoreText.text = opponentScore.ToString() ;
    }
    public void AddScore(string playerId, int points)
    {

        if (playerId == myPlayerId)
        {
            myScore += points;
        }

        UpdateScoreDisplay();
        ScoreUpdateMessage msg = new ScoreUpdateMessage
        {
            action = "scoreUpdate",
            pointsEarned = points,
            playerId = playerId
        };
       
        NetworkkManager.Instance.SendNetworkMessage(JsonUtility.ToJson(msg));
    }

    public void AddScoreFromNetwork(string playerId, int points)
    {

        if (playerId != myPlayerId)
        {
            opponentScore += points;
            UpdateScoreDisplay();
        }

        //GameEvents.instance.ScoreUpdated(playerId, playerId == myPlayerId ? myScore : opponentScore);
    }

    public string GetWinner()
    {
        if (myScore > opponentScore)
            return "You Win!";
        else if (opponentScore > myScore)
            return "You Lose! Better Luck Next Time";
        else
            return "It's a Tie";
    }


}
