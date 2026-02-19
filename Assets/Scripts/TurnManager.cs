using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    public int currentTurn = 1;
    private int maxTurn = 6;
    [HideInInspector] public float timer = 30;
    [HideInInspector] public bool timerActive = false;
    

    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] GameObject gameScene;
    public TextMeshProUGUI turnText;
    [SerializeField] TextMeshProUGUI messageForOpponent;
    [SerializeField] TextMeshProUGUI messageForPlayer;
    [SerializeField] Button endturnButton;
    
    
    private bool playerEnded = false;
    private bool opponentEnded = false;

    [HideInInspector]public bool mastersTurn = true;
    [HideInInspector] public bool opponentTurn = false;
    //[HideInInspector] public bool lastTurnEnded = false;


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
        GameEvents.instance.OnGameStart += HandleGameStart;
        //GameEvents.instance.OnTurnStart += HandleTurnStart;
        //GameEvents.instance.OnTurnEnd += HandleTurnEnd;
        GameEvents.instance.OnPlayerEndedTurn += HandlePlayerEndedTurn;
    }



    private void OnDisable()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnGameStart -= HandleGameStart;
            //GameEvents.instance.OnTurnStart -= HandleTurnStart;
            //GameEvents.instance.OnTurnEnd -= HandleTurnEnd;
            GameEvents.instance.OnPlayerEndedTurn -= HandlePlayerEndedTurn;
        }
    }

    //private void HandleTurnStart(int obj)
    //{
        
    //}

    private void Start()
    {
        timerActive = true;
        endturnButton.gameObject.SetActive(true);
        endturnButton.onClick.AddListener(OnEndTurnClicked);
    }

    void Update()
    {
        if (timer > 0 && timerActive && gameScene.activeSelf)
        {
            timer -= Time.deltaTime;
            timerText.text = System.Math.Ceiling(timer).ToString() + "s";
        }
        else if (timer < 0 && timerActive)
        {
            TurnEnd();
        }
    }

    private void HandleGameStart(List<string> playerIds)
    {
        StartTurn(1);
    }

    private void StartTurn(int turnNumber)
    {
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    if (ScoreManager.Instance.myScore > ScoreManager.Instance.opponentScore)
        //    {
        //        Debug.Log("masters turn now");
        //        TurnManager.Instance.mastersTurn = true;
        //    }
        //    else if (ScoreManager.Instance.myScore < ScoreManager.Instance.opponentScore)
        //    {
        //        Debug.Log("opponents turn now");
        //        TurnManager.Instance.mastersTurn = false;
        //    }
        //    else
        //    {
        //        int i = Random.Range(0, 2);
        //        Debug.Log("score is tie picking random = " + i);
        //        TurnManager.Instance.mastersTurn = i == 1 ? true : false;
        //    }
        //}
        currentTurn = turnNumber;
        timer = 30f;
        timerActive = true;

        playerEnded = false;
        opponentEnded = false;

        messageForOpponent.text = "";
        messageForPlayer.text = "";

        if (turnNumber != 1 && DeckManager.Instance.cardInDeck.Count > 0)
        {
            turnText.text = turnNumber.ToString() + "/" + maxTurn.ToString();
        }
        
        endturnButton.gameObject.SetActive(true);
        endturnButton.interactable = true;

        GameEvents.instance.TurnStarted(currentTurn); 
    }

    public void StartTurnFromNetwork(int turnNumber, int count)
    {
        //DeckManager.Instance.countOfAllFoldedCardsPerRound = count;

        StartTurn(turnNumber);
    }

    public void OnEndTurnClicked()
    {
        if (playerEnded) return;
        playerEnded = true;
        endturnButton.interactable = false;
        timerActive = false;

        messageForPlayer.text = "Waiting for opponent to end turn...";

        string myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();

        EndTurnMessage msg = new EndTurnMessage
        {
            playerId = myPlayerId
        };

        string json = JsonUtility.ToJson(msg);
        NetworkkManager.Instance.SendNetworkMessage(json);
    }
    private void HandlePlayerEndedTurn(string playerid)
    {
        string myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
        if (playerid == myPlayerId)
        {
            playerEnded = true;
        }

        else
        {
            opponentEnded = true;
            messageForOpponent.text = "Opponent ended turn...";
        }

        CheckIfBothPlayersReady();
    }
    void TurnEnd()
    {
        if (timer <= 0 && !playerEnded) 
        {
            playerEnded = true;
            timerActive = false;
            string myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();

            EndTurnMessage msg = new EndTurnMessage
            {
                playerId = myPlayerId,
            };

            NetworkkManager.Instance.SendNetworkMessage(JsonUtility.ToJson(msg));
        }
    }

    
    void CheckIfBothPlayersReady()
    {
        if(playerEnded && opponentEnded)
        {

            messageForOpponent.text = "let the ability magic happen... going to next round...";
            messageForPlayer.text = "let the ability magic happen... going to next round...";

            GameEvents.instance.AllPlayersReady(true);
            if (PhotonNetwork.IsMasterClient)
            {
                if (currentTurn <= maxTurn)
                {
                    Invoke("GoToNextTurn", 4f);
                }
               
            }
        }
    }

    //private IEnumerator WaitForSomeTimeToCalculateTheWinner()
    //{
    //    yield return new WaitForSeconds(4f);
    //}

    public void UpdateOpponentMessageDisplay(string messageToDisplay)
    {
        messageForPlayer.text = messageToDisplay;
    }

    void GoToNextTurn()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int nextTurn = currentTurn + 1;
            if(nextTurn == 7)
            {
                GameEvents.instance.GameEnded();
            }
            else
            {
                TurnStartMessage msg = new TurnStartMessage
                {
                    turnNumber = nextTurn,
                    count = 0
                };

                string json = JsonUtility.ToJson(msg);
                NetworkkManager.Instance.SendNetworkMessage(json);
            }
                //DeckManager.Instance.countOfAllFoldedCardsPerRound = 0;

        }
        
    }


}
