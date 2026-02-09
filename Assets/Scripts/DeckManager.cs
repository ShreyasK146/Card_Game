using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Photon.Pun;


public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public List<CardData> allCardsData;

    public List<CardData> cardInDeck = new List<CardData>(); // these are in deck
    public List<CardData> cardInHand = new List<CardData>(); // these are in hand
    public List<CardData> selectedCards = new List<CardData>(); // these are selected card to play 
    public List<CardData> foldedCards = new List<CardData>();
    //public List<CardData> newCardsThisTurn = new List<CardData>();

    [SerializeField] GameObject cardGameObject;
    [SerializeField] private Transform contentTransformForCardsInDeck;
    [SerializeField] private Transform contentTransformForFoldedCards;
    public Button playCardButton;
    public Button endTurnButton;
    private int totalCardCostUsedInCurrentRound = 0;
    private int selectedCardCost = 0;


    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
    private void OnEnable()
    {
        GameEvents.instance.OnGameStart += HandleGameStart;
        GameEvents.instance.OnTurnStart += HandleTurnStart;
    }
    private void OnDisable()
    {
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnGameStart -= HandleGameStart;
            GameEvents.instance.OnTurnStart -= HandleTurnStart;
        }
    }
    private void Start()
    {
        playCardButton.gameObject.SetActive(false);
        playCardButton.onClick.AddListener(PlayCardButtonClicked);
        //GameEvents.instance.GameStarted(new List<string> { "Player1", "Player2" });
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    string myId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
        //    if(ScoreManager.Instance != null)
        //        ScoreManager.Instance.AddScore(myId, 5);
        //}
    }
    public void HandleGameStart(List<string> playerids)
    {
        TurnManager.Instance.turnText.text = "1/6";
        
        cardInDeck = new List<CardData>(allCardsData);

        RandomizeCardSelection(); // maybe need to remove this because what if all selected cards have cost > 1 at beginning?

        DrawCards(3); // initial 3 card drawing

        ShowCardsInHand();
    }
    public void HandleTurnStart(int turnNumber)
    {
        if (turnNumber > 1 && cardInDeck.Count > 0)
        {
            totalCardCostUsedInCurrentRound = 0;
            //foldedCards.Clear();
            //newCardsThisTurn.Clear();
            DrawCards(5); // maxdrawable cards (i dont think we will ever be able to draw 5 card at a time but let it be)
            RefreshCardsInHand();
            //RefreshCardsInPlayerCardArea();
        }
    }



    public void PlayCardButtonClicked()
    {
        //newCardsThisTurn.Clear();
        for (int i = selectedCards.Count - 1; i >= 0; i--)
        {
            foldedCards.Add(selectedCards[i]);
            //newCardsThisTurn.Add(selectedCards[i]);
            cardInHand.Remove(selectedCards[i]);
        }

        selectedCards.Clear();

        RefreshCardsInHand();
        RefreshCardsInPlayerCardArea();
        totalCardCostUsedInCurrentRound = selectedCardCost;
        playCardButton.gameObject.SetActive(false);

        GameEvents.instance.CardFolded(foldedCards);
        SyncBoardMessage msg = new SyncBoardMessage
        {
            action = "syncBoard",
            cardCount = foldedCards.Count,
            senderActorNumber = PhotonNetwork.LocalPlayer.ActorNumber
        };

        string json = JsonUtility.ToJson(msg);
        NetworkkManager.Instance.SendNetworkMessage(json);
        endTurnButton.interactable =true;
    }

    public void RefreshCardsInPlayerCardArea()
    {
        //foreach (Transform transform in contentTransformForFoldedCards)
        //{
        //    Destroy(transform.gameObject);
        //}
        foreach (var obj in foldedCards)
        {
            GameObject card = GameObject.Instantiate(cardGameObject, contentTransformForFoldedCards);
            var cardObj = card.GetComponent<CardSelectHandler>();

            cardObj.costText.text = obj.cardCost.ToString();
            cardObj.powerText.text = obj.cardPower.ToString();
            cardObj.abilityText.text = obj.cardName;

            cardObj.GetComponent<Button>().interactable = false;
        }
        
    }

    public void RefreshCardsInHand()
    {
        foreach (Transform transform in contentTransformForCardsInDeck)
        {
            Destroy(transform.gameObject);
        }
        foreach (var obj in cardInHand)
        {
            GameObject card = GameObject.Instantiate(cardGameObject, contentTransformForCardsInDeck);
            var cardObj = card.GetComponent<CardSelectHandler>();
            cardObj.costText.text = obj.cardCost.ToString();
            cardObj.powerText.text = obj.cardPower.ToString();
            cardObj.abilityText.text = obj.cardName;
            cardObj.cardData = obj;
        }
    }

    public void ShowCardsInHand()
    {
        foreach (var obj in cardInHand)
        {
            GameObject card = GameObject.Instantiate(cardGameObject, contentTransformForCardsInDeck);
            var cardObj = card.GetComponent<CardSelectHandler>();
            cardObj.costText.text = obj.cardCost.ToString();
            cardObj.powerText.text = obj.cardPower.ToString();
            cardObj.abilityText.text = obj.cardName;
            cardObj.cardData = obj;
        }
    }

    public void DrawCards(int cardCount)
    {
        Debug.Log(cardInDeck.Count);
        int i = 0;
        while(i < cardCount && i < cardInDeck.Count && cardInHand.Count < 5)
        {
            CardData card = cardInDeck[i];
            cardInHand.Add(card);
            cardInDeck.Remove(card);
            i++;
        }
        GameEvents.instance.CardDrawn(cardCount);
    }

    private void RandomizeCardSelection()
    {
        for (int i = 0; i < cardInDeck.Count; i++)
        {
            CardData temp = cardInDeck[i];
            int randomIndexToSwap = Random.Range(i, cardInDeck.Count);
            cardInDeck[i] = cardInDeck[randomIndexToSwap];
            cardInDeck[randomIndexToSwap] = temp;
        }
    }

    public void SelectCard(CardData card)
    {
        selectedCards.Add(card);
        endTurnButton.interactable = false;
        CheckPlayCardButton();
        GameEvents.instance.CardSelected(card);
    }


    public void DeSelectCard(CardData card)
    {
        selectedCards.Remove(card);
        if (selectedCards.Count == 0)
            endTurnButton.interactable = true;
        CheckPlayCardButton();
        GameEvents.instance.CardDeSelected(card);
    }

    public void CheckPlayCardButton()
    {
        
        selectedCardCost = totalCardCostUsedInCurrentRound;
        foreach (var card in selectedCards)
        {
            selectedCardCost += card.cardCost;
        }

        playCardButton.gameObject.SetActive(selectedCards.Count > 0 && selectedCardCost+totalCardCostUsedInCurrentRound <= TurnManager.Instance.currentTurn);        
    }


}