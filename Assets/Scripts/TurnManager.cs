using Photon.Pun;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public int currentTurn = 1;
    private int maxTurn = 6;
    private float timer = 60;
    private bool timerActive = false;

    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] GameObject gameScene;
    public TextMeshProUGUI turnText;
    [SerializeField] Button endturnButton;

    private bool playerEnded = false;
    private bool opponentEnded = false;

    public static TurnManager Instance;
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

        //GameEvents.instance.OnTurnStart += HandleTurnStart;
        GameEvents.instance.OnGameStart += HandleGameStart;
        GameEvents.instance.OnPlayerEndedTurn += HandlePlayerEndedTurn;
    }


    private void OnDisable()
    {
        if (GameEvents.instance != null)
        {
            //GameEvents.instance.OnTurnStart -= HandleTurnStart;
            GameEvents.instance.OnGameStart -= HandleGameStart;
            GameEvents.instance.OnPlayerEndedTurn -= HandlePlayerEndedTurn;
        }
    }

    private void HandleGameStart(List<string> playerIds)
    {
        StartTurn(1);
    }

    private void Start()
    {
        timerActive = true;
        endturnButton.gameObject.SetActive(true);
        endturnButton.onClick.AddListener(OnEndTurnClicked);
    }
    private void StartTurn(int turnNumber)
    {
        currentTurn = turnNumber;
        timer = 60f;
        timerActive = true;

        playerEnded = false;
        opponentEnded = false;
        if (turnNumber != 1 && DeckManager.Instance.cardInDeck.Count > 0)
        {
            turnText.text = turnNumber.ToString() + "/" + maxTurn.ToString();
        }
        

        endturnButton.gameObject.SetActive(true);
        endturnButton.interactable = true;

        GameEvents.instance.TurnStarted(currentTurn);
    }
    void Update()
    {

        if (timer > 0 && timerActive && gameScene.activeSelf )
        {
            timer -= Time.deltaTime;
            timerText.text = Math.Ceiling(timer).ToString() +"s";
        }
        else if(timer < 0 && timerActive)
        {
            TurnEnd();
        }
    }

    public void OnEndTurnClicked()
    {
        if (playerEnded) return;
        playerEnded = true;
        endturnButton.interactable = false;
        timerActive = false;

        string myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();

        EndTurnMessage msg = new EndTurnMessage
        {
            playerId = myPlayerId
        };

        string json = JsonUtility.ToJson(msg);
        NetworkkManager.Instance.SendNetworkMessage(json);
    }

    void TurnEnd()
    {
        if (timer <= 0 && !playerEnded) 
        {
            playerEnded = true;
            timerActive = false;
            //GameEvents.instance.PlayerEndedTurn("Player1");
            string myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();

            EndTurnMessage msg = new EndTurnMessage
            {
                playerId = myPlayerId
            };

            string json = JsonUtility.ToJson(msg);
            NetworkkManager.Instance.SendNetworkMessage(json);
            //Invoke("SimulateOpponentEndTurn", 2f);
        }
    }

    private void HandlePlayerEndedTurn(string playerid)
    {
        string myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
        if (playerid == myPlayerId)
        {
            
            playerEnded = true;
        }
            
        else
            opponentEnded = true;
        CheckIfBothPlayersReady();
    }

    //void SimulateOpponentEndTurn()
    //{
    //    Debug.Log("Opponent ended turn (simulated)");
    //    GameEvents.instance.PlayerEndedTurn("Player2");
    //}

    void CheckIfBothPlayersReady()
    {
        if(playerEnded && opponentEnded)
        {
            
            GameEvents.instance.AllPlayersReady(true);
            if (PhotonNetwork.IsMasterClient)
            {
                if (currentTurn < maxTurn)
                {
                    Invoke("GoToNextTurn", 4f);
                }
                else
                {
                    GameEvents.instance.GameEnded(true);
                }
            }
        }
    }
    public void StartTurnFromNetwork(int turnNumber)
    {
        StartTurn(turnNumber);
    }

    void GoToNextTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int nextTurn = currentTurn + 1;

            // Send network message to start turn for both players
            TurnStartMessage msg = new TurnStartMessage
            {
                turnNumber = nextTurn
            };

            string json = JsonUtility.ToJson(msg);
            NetworkkManager.Instance.SendNetworkMessage(json);
        }
    }
}
