using System.Collections.Generic;
using System;
using UnityEngine;


public class GameEvents : MonoBehaviour
{
    public static GameEvents instance;

    [SerializeField] GameObject deckManager;
    [SerializeField] GameObject turnManager;
    [SerializeField] GameObject scoreManager;
    [SerializeField] GameObject revealManager;
    [SerializeField] GameObject abilityManager;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
        
    }
    private void Start()
    {
        //calling here otherwise null exception
        deckManager.gameObject.SetActive(true);
        turnManager.gameObject.SetActive(true);
        scoreManager.gameObject.SetActive(true);  
        revealManager.gameObject.SetActive(true);
        abilityManager.gameObject.SetActive(true);
    }
    public event Action<List<string>> OnGameStart;
    public event Action<int> OnTurnStart;
    public event Action<string> OnPlayerEndedTurn;
    public event Action<bool> OnAllPlayersReady;
    public event Action<List<CardData>> OnRevealCard;
    public event Action<string,int> OnScoreUpdated;
    public event Action<int> OnTurnEnd;
    public event Action OnGameEnd;


    public event Action<CardData> OnCardsSelected;
    public event Action<CardData> OnCardsDeSelected;
    public event Action<List<CardData>> OnCardsFolded;
    public event Action<int> OnCardsDrawn;


    public event Action OnOpponentDisconnected;
    public event Action OnOpponentReconnected;
    public event Action OnPlayerReconnected;
    public event Action OnOpponentForfeited;
    public event Action OnMatchAbandoned;

    //...............................................................//
    public void GameStarted(List<string> playerids)
    {
        OnGameStart?.Invoke(playerids);
    }

    public void TurnStarted(int turnNumber)
    {
        OnTurnStart?.Invoke(turnNumber);
    }

    public void PlayerEndedTurn(string playerid)
    {
        OnPlayerEndedTurn?.Invoke(playerid);
    }

    public void AllPlayersReady(bool ready)
    {
        OnAllPlayersReady?.Invoke(ready);
    }


    public void RevealCard(List<CardData> cards)
    {
        OnRevealCard?.Invoke(cards);
    }

    public void ScoreUpdated(string playerid, int score)
    {
        OnScoreUpdated?.Invoke(playerid,score);
    }

    public void TurnEnded(int turnNumber)
    {
        OnTurnEnd?.Invoke(turnNumber);
    }

    public void GameEnded()
    {
        OnGameEnd?.Invoke();
    }

    //...............................................................//
    public void CardDrawn(int numberOfCards)
    {
        OnCardsDrawn?.Invoke(numberOfCards);
    }
    public void CardSelected(CardData card)
    {
        OnCardsSelected?.Invoke(card);
    }
    public void CardDeSelected(CardData card)
    {
        OnCardsDeSelected?.Invoke(card);
    }
    public void CardFolded(List<CardData> cards)
    {
        OnCardsFolded?.Invoke(cards);
    }

    //...............................................................//
}
