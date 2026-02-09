using Photon.Pun;
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
    private bool hasRevealedCards = false;

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
        //if (hasRevealedCards)
        //{
        //    return;
        //}
        //hasRevealedCards = true;
        int myTotalPower = 0;
        foreach (var card in DeckManager.Instance.foldedCards)
        {
            myTotalPower += card.cardPower;
        }
        if (myTotalPower > 0)
        {
            ScoreManager.Instance.AddScore(myPlayerId, myTotalPower);
          
        }
        Invoke(nameof(FinishRevealPhase), 3f);
    }

    void FinishRevealPhase()
    {
        DeckManager.Instance.foldedCards.Clear();
    }
}