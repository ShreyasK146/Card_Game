using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevealManager : MonoBehaviour
{
    public static RevealManager Instance;

    private List<CardData> myFoldedCards;
    private List<CardData> opponentFoldedCards;
    private string myPlayerId;
    private bool myCardsReceived = false;
    private bool opponentCardsReceived = false;


    private void Awake()
    {
        if (Instance == null)
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
        GameEvents.instance.OnAllPlayersReady += StartRevealPhase;
    }

    private void OnDisable()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnAllPlayersReady -= StartRevealPhase;
        }
    }

    void StartRevealPhase(bool ready)
    {
        myCardsReceived = false;
        opponentCardsReceived = false;
 
        myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();

        myFoldedCards = new List<CardData>(DeckManager.Instance.foldedCards);
        myCardsReceived = true;

        SendMyFoldedCards();
    }

    void SendMyFoldedCards()
    {
        List<int> cardIds = new List<int>();
        foreach (var card in myFoldedCards)
        {
            cardIds.Add(card.id);
        }

        RevealCardsMessage msg = new RevealCardsMessage
        {
            playerId = myPlayerId,
            cardIds = cardIds
        };

        string json = JsonUtility.ToJson(msg);
        NetworkkManager.Instance.SendNetworkMessage(json);
    }

    public void ReceiveOpponentCards(string playerId, List<int> cardIds)
    {
        if (playerId == myPlayerId)
        {
            return;
        }
        opponentFoldedCards = new List<CardData>();

        foreach (int cardId in cardIds)
        {
            CardData card = FindCardById(cardId);
            if (card != null)
            {
                opponentFoldedCards.Add(card);
            }
        }
        opponentCardsReceived = true;
       
        if (myCardsReceived && opponentCardsReceived)
        {
            RevealAllCards();
        }

        
    }

    CardData FindCardById(int cardId)
    {
        foreach (var card in DeckManager.Instance.allCardsData)
        {
            if (card.id == cardId)
            {
                return card;
            }
        }
        
        return null;
    }
    //public void MessageAndCounterUpdatesFromNetwork2(bool isMasterTurn)
    //{
    //    TurnManager.Instance.mastersTurn = isMasterTurn;
    //}
    void RevealAllCards()
    {
        List<CardData> allCards = new List<CardData>();
        allCards.AddRange(opponentFoldedCards);
        allCards.AddRange(myFoldedCards);
        GameEvents.instance.RevealCard(allCards);
        OpponentBoardDisplay opponentBoard = FindFirstObjectByType<OpponentBoardDisplay>();
        if (opponentBoard != null && opponentFoldedCards.Count > 0)
        {
            opponentBoard.RevealOpponentCards(opponentFoldedCards);
        }
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    if (ScoreManager.Instance.myScore > ScoreManager.Instance.opponentScore)
        //        TurnManager.Instance.mastersTurn = true;
        //    else if (ScoreManager.Instance.myScore < ScoreManager.Instance.opponentScore)
        //        TurnManager.Instance.mastersTurn = false;
        //    else
        //        TurnManager.Instance.mastersTurn = UnityEngine.Random.Range(0, 2) == 1;
        //    WhoseTurnMessage2 msg = new WhoseTurnMessage2
        //    {
        //        isMastersTurn = TurnManager.Instance.mastersTurn
        //    };
        //    NetworkkManager.Instance.SendNetworkMessage(JsonUtility.ToJson(msg));
        //}

        if (PhotonNetwork.IsMasterClient && TurnManager.Instance.mastersTurn && DeckManager.Instance.countOfAllFoldedCardsInPlayerArea > 0)
            Method1();
        else if (!PhotonNetwork.IsMasterClient && !TurnManager.Instance.mastersTurn && DeckManager.Instance.countOfAllFoldedCardsInOpponentArea > 0)
            Method2();
        else
            MessageAndCounterUpdatesFromNetwork
            (
                PhotonNetwork.LocalPlayer.ActorNumber.ToString(), TurnManager.Instance.mastersTurn, 
                PhotonNetwork.IsMasterClient == true? DeckManager.Instance.countOfAllFoldedCardsInPlayerArea : DeckManager.Instance.countOfAllFoldedCardsInOpponentArea
            );
    }
    private void Method1()
    {
        Debug.Log("iam in method 1 " + PhotonNetwork.IsMasterClient);
        if(DeckManager.Instance.countOfAllFoldedCardsInPlayerArea > 0)
        {
            AbilityManager.Instance.ExecuteAbilityLocally(DeckManager.Instance.foldedCards[0], myPlayerId);
            DeckManager.Instance.foldedCards.RemoveAt(0);
            DeckManager.Instance.countOfAllFoldedCardsInPlayerArea--;
            TurnManager.Instance.mastersTurn = false;
            
            WhoseTurnMessage msg = new WhoseTurnMessage
            {
                action = "whoseTurn",
                counter = DeckManager.Instance.countOfAllFoldedCardsInPlayerArea,
                playerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString(),
                isMastersTurn = false
            };
            StartCoroutine(AddingVisibleDelay(msg));
        }

    }
    private void Method2()
    {
        Debug.Log("iam in method 2 " + PhotonNetwork.IsMasterClient);
        if (DeckManager.Instance.countOfAllFoldedCardsInOpponentArea > 0)
        {
            AbilityManager.Instance.ExecuteAbilityLocally(DeckManager.Instance.foldedCards[0], myPlayerId);
            DeckManager.Instance.foldedCards.RemoveAt(0);
            DeckManager.Instance.countOfAllFoldedCardsInOpponentArea--;
            TurnManager.Instance.mastersTurn = true;

            WhoseTurnMessage msg = new WhoseTurnMessage
            {
                action = "whoseTurn",
                counter = DeckManager.Instance.countOfAllFoldedCardsInOpponentArea,
                playerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString(),
                isMastersTurn = true
            };
            StartCoroutine(AddingVisibleDelay(msg));
            
        }

    }


    private IEnumerator AddingVisibleDelay(WhoseTurnMessage msg)
    {
        yield return new WaitForSeconds(0.25f);
        NetworkkManager.Instance.SendNetworkMessage(JsonUtility.ToJson(msg));
    }
    public void MessageAndCounterUpdatesFromNetwork(string playerId, bool isMasterTurn, int counter)
    {
        string myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
        if(myPlayerId != playerId)
        {
            if(PhotonNetwork.IsMasterClient)
            {
                DeckManager.Instance.countOfAllFoldedCardsInOpponentArea = counter;
            }
            else
            {
                DeckManager.Instance.countOfAllFoldedCardsInPlayerArea = counter;
            }
        }

        Debug.Log("counter when switching turns = " + DeckManager.Instance.countOfAllFoldedCardsInPlayerArea + "\t" + DeckManager.Instance.countOfAllFoldedCardsInOpponentArea);
        TurnManager.Instance.mastersTurn = isMasterTurn;
        if (DeckManager.Instance.countOfAllFoldedCardsInPlayerArea <= 0 && DeckManager.Instance.countOfAllFoldedCardsInOpponentArea <= 0)
        {
            Debug.Log("executing method 3 ");
            Method4();
        }
        else if (DeckManager.Instance.countOfAllFoldedCardsInPlayerArea <= 0)
        {
            Debug.Log("executing method 4-1 ");
            //Method3(playerId);
            HandleRemainingMessage msg = new HandleRemainingMessage
            {
                action = "handleRemainingReveals",
                counter = DeckManager.Instance.countOfAllFoldedCardsInOpponentArea
            };
            NetworkkManager.Instance.SendNetworkMessage(JsonUtility.ToJson(msg));
            
        }
        else if (DeckManager.Instance.countOfAllFoldedCardsInOpponentArea <= 0)
        {
            Debug.Log("executing method 4-2 ");
            HandleRemainingMessage msg = new HandleRemainingMessage
            {
                action = "handleRemainingReveals",
                counter = DeckManager.Instance.countOfAllFoldedCardsInPlayerArea
            };
            NetworkkManager.Instance.SendNetworkMessage(JsonUtility.ToJson(msg));   
            //Method3(playerId);
        }
        else
        {
            if (isMasterTurn && PhotonNetwork.IsMasterClient)
            {
                Debug.Log("executing method 1");
                Method1();
            }

            else if (!isMasterTurn && !PhotonNetwork.IsMasterClient)
            {
                Debug.Log("executing method 2");
                Method2();
            }
        }
    }

    public void Method3(int counter)
    {
        if (PhotonNetwork.IsMasterClient && DeckManager.Instance.countOfAllFoldedCardsInPlayerArea > 0)
        {
            myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
            while(counter > 0 && DeckManager.Instance.countOfAllFoldedCardsInPlayerArea > 0)
            {
                Debug.Log("counter = " + counter + "\t foldedcardsinplayerarea = " + DeckManager.Instance.countOfAllFoldedCardsInPlayerArea);
                AbilityManager.Instance.ExecuteAbilityLocally(DeckManager.Instance.foldedCards[0], myPlayerId);
                DeckManager.Instance.foldedCards.RemoveAt(0);
                DeckManager.Instance.countOfAllFoldedCardsInPlayerArea--;
                counter--;
            }
            
        }
        else if(!PhotonNetwork.IsMasterClient && DeckManager.Instance.countOfAllFoldedCardsInOpponentArea > 0)
        {
            myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
            while (counter > 0 && DeckManager.Instance.countOfAllFoldedCardsInOpponentArea > 0)
            {
                Debug.Log("counter = " + counter + "\t foldedcardsinopponentarea = " + DeckManager.Instance.countOfAllFoldedCardsInOpponentArea);
                AbilityManager.Instance.ExecuteAbilityLocally(DeckManager.Instance.foldedCards[0], myPlayerId);
                DeckManager.Instance.foldedCards.RemoveAt(0);
                DeckManager.Instance.countOfAllFoldedCardsInOpponentArea--;
                counter--;
            }
        }
        Method4();
    }
    private void Method4()
    {
        Invoke(nameof(FinishRevealPhase), 3f);
    }
    void FinishRevealPhase()
    {
        Debug.Log("at finish reveal = " + DeckManager.Instance.countOfAllFoldedCardsInPlayerArea + "\t" + DeckManager.Instance.countOfAllFoldedCardsInOpponentArea);
        DeckManager.Instance.foldedCards.Clear();
    }

}