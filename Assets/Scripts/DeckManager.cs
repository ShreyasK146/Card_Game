using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public List<CardData> allCardsData;

    public List<CardData> cardInDeck = new List<CardData>(); // these are in deck(12 cards for each)
    public List<CardData> cardInHand = new List<CardData>(); // these are in hand
    public List<CardData> selectedCards = new List<CardData>(); // these are selected card to play 
    public List<CardData> foldedCards = new List<CardData>(); // folded cards that will be revealed after both end turn
    public List<CardData> allPlayedCards = new List<CardData>(); // this is used to make sure cards are accumulated correctly after each round

    [SerializeField] GameObject cardGameObject;
    [SerializeField] private Transform contentTransformForCardsInHand; 
    [SerializeField] private Transform contentTransformForCardsInPlayerArea; 
    [SerializeField] TextMeshProUGUI availableCostText;
    public TextMeshProUGUI opponentavailableCostText;
    public Button playCardButton;
    public Button endTurnButton;

    private int totalCardCostUsedInCurrentRound = 0; 
    private int selectedCardCost = 0;
    //public int displayedCardCount = 0;
    int foldedCardCount = 0;
    int randomizeCalled = 0;


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
   
    }

    public void HandleGameStart(List<string> playerids)
    {
        //displayedCardCount = 0;
        TurnManager.Instance.turnText.text = "1/6";
        cardInDeck = new List<CardData>(allCardsData);

        RandomizeCardSelection(); // only first 5 cards are randomized initially 
        DrawCards(3); // initial 3 card drawing
        RandomizeCardSelection(); // randomizing cards from 4 to 12

        availableCostText.text = "1";
        opponentavailableCostText.text = "1";
        ShowCardsInHand();
    }
    public void HandleTurnStart(int turnNumber)
    {
        if (turnNumber > 1 && cardInDeck.Count > 0)
        {
            totalCardCostUsedInCurrentRound = 0;
            availableCostText.text = turnNumber.ToString();
            opponentavailableCostText.text = turnNumber.ToString();
            DrawCards(1); // Drwaing 1 card per turn
            RefreshCardsInHand();

        }
    }

    public void PlayCardButtonClicked()
    {
        //normal foreach would give error because of how enumeration works so had to use normal one with reverse. maybe there's way
        for (int i = selectedCards.Count - 1; i >= 0; i--)
        {
            foldedCards.Add(selectedCards[i]);
            allPlayedCards.Add(selectedCards[i]);
            cardInHand.Remove(selectedCards[i]);
        }

        totalCardCostUsedInCurrentRound += selectedCardCost; 
        selectedCards.Clear();
        RefreshCardsInHand();
        RefreshCardsInPlayerCardArea();
        
        playCardButton.gameObject.SetActive(false);
        availableCostText.text = (TurnManager.Instance.currentTurn - totalCardCostUsedInCurrentRound).ToString();

        GameEvents.instance.CardFolded(foldedCards);
        SyncBoardMessage msg = new SyncBoardMessage
        {
            action = "syncBoard",
            cardCount = foldedCardCount,
            availableCost = TurnManager.Instance.currentTurn - totalCardCostUsedInCurrentRound,
            senderActorNumber = PhotonNetwork.LocalPlayer.ActorNumber
        };

        NetworkkManager.Instance.SendNetworkMessage(JsonUtility.ToJson(msg));
        endTurnButton.interactable =true;
    }

    public void RefreshCardsInPlayerCardArea() 
    {
        int currentDisplayCount = contentTransformForCardsInPlayerArea.childCount;
        foldedCardCount = 0;
       
        for (int i = currentDisplayCount; i < allPlayedCards.Count; i++)
        {
            GameObject card = GameObject.Instantiate(cardGameObject, contentTransformForCardsInPlayerArea);
            var cardObj = card.GetComponent<CardSelectHandler>();
            //player card shuld be visible in player area
            cardObj.costText.text = allPlayedCards[i].cardCost.ToString();
            cardObj.powerText.text = allPlayedCards[i].cardPower.ToString();
            cardObj.abilityText.text = allPlayedCards[i].cardName;

            cardObj.GetComponent<Button>().interactable = false;
            foldedCardCount++;
        }
    }

    public void RefreshCardsInHand()
    {
        foreach (Transform transform in contentTransformForCardsInHand)
        {
            Destroy(transform.gameObject);
        }
        foreach (var obj in cardInHand)
        {
            GameObject card = GameObject.Instantiate(cardGameObject, contentTransformForCardsInHand);
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
            GameObject card = GameObject.Instantiate(cardGameObject, contentTransformForCardsInHand);
            var cardObj = card.GetComponent<CardSelectHandler>();
            cardObj.costText.text = obj.cardCost.ToString();
            cardObj.powerText.text = obj.cardPower.ToString();
            cardObj.abilityText.text = obj.cardName;
            cardObj.cardData = obj;
        }
    }

    public void DrawCards(int cardCount)
    {
        int i = 0;
        while(i < cardCount && i < cardInDeck.Count && cardInHand.Count < 5) // can make adjustment in calls easily if we want to maintain 5 cards always in hand
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
        if(randomizeCalled == 0)
        {
            for (int i = 0; i < 5; i++) 
            {
                CardData temp = cardInDeck[i];
                int randomIndexToSwap = Random.Range(i, 5);// for now swap only first 5
                cardInDeck[i] = cardInDeck[randomIndexToSwap];
                cardInDeck[randomIndexToSwap] = temp;
            }
            randomizeCalled = 1;
        }
        else
        {
            for (int i = 3; i < cardInDeck.Count; i++) //swap from 4 to 12
            {
                CardData temp = cardInDeck[i];
                int randomIndexToSwap = Random.Range(i, cardInDeck.Count);
                cardInDeck[i] = cardInDeck[randomIndexToSwap];
                cardInDeck[randomIndexToSwap] = temp;
            }
        }
        
    }

    public void SelectCard(CardData card)
    {
        selectedCards.Add(card);
        endTurnButton.interactable = false; // had to disable endturnbutton otherwise lot of bugs if we select card and end the turn
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
        selectedCardCost = 0;
        foreach (var card in selectedCards)
        {
            selectedCardCost += card.cardCost;
        }
        // we need totalcardcostusedincurrentround variable to pass cases such as 2 cards in played one at a time in same round
        playCardButton.gameObject.SetActive(selectedCards.Count > 0 && selectedCardCost+totalCardCostUsedInCurrentRound <= TurnManager.Instance.currentTurn);        
    }


}