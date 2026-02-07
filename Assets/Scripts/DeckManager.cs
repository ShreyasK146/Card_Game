
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public List<CardData> allCardsData;

    public List<CardData> cardInDeck = new List<CardData>(); // these are in deck
    public List<CardData> cardInHand = new List<CardData>(); // these are in hand
    public List<CardData> selectedCards = new List<CardData>(); // these are selected card to play 
    public List<CardData> foldedCards = new List<CardData>(); 

    [SerializeField] GameObject cardGameObject;
    [SerializeField] private Transform contentTransformForCardsInDeck;
    [SerializeField] private Transform contentTransformForFoldedCards;
    public Button playCardButton;

    public int availableCost = 1;

    void Awake()
    {
        Instance = this; 
    }



    private void Start()
    {
        
        cardInDeck = new List<CardData>(allCardsData);

        RandomizeCardSelection(); // maybe need to remove this because what if all selected cards have cost > 1 at beginning?

        DrawCards(3); // initial 3 card drawing

        ShowCardsInHand();

        playCardButton.gameObject.SetActive(false);
        playCardButton.onClick.AddListener(PlayCardButtonClicked);
    }
    private void PlayCardButtonClicked()
    {
        for(int i = selectedCards.Count - 1; i >= 0; i--)
        {
            foldedCards.Add(selectedCards[i]);
            cardInHand.Remove(selectedCards[i]);
        }

        selectedCards.Clear();

        RefreshCardsInHand();
        RefreshCardsInPlayerCardArea();

        playCardButton.gameObject.SetActive(false);
    }

    private void RefreshCardsInPlayerCardArea()
    {
        foreach (Transform transform in contentTransformForFoldedCards)
        {
            Destroy(transform.gameObject);
        }
        foreach (var obj in foldedCards)
        {
            GameObject card = GameObject.Instantiate(cardGameObject, contentTransformForFoldedCards);
            var cardObj = card.GetComponent<CardSelectHandler>();

            cardObj.costText.text = "?";
            cardObj.powerText.text = "?";
            cardObj.abilityText.text = "Folded";

            cardObj.GetComponent<Button>().interactable = false;
        }
    }

    private void RefreshCardsInHand()
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

    private void ShowCardsInHand()
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

    private void DrawCards(int cardCount)
    {
        Debug.Log(cardInDeck.Count);
        for (int i = 0; i < cardCount; i++)
        {
            CardData card = cardInDeck[i];
            cardInHand.Add(card);
            cardInDeck.Remove(card);
        }
    }

    private void RandomizeCardSelection()
    {
        for (int i = 0; i < cardInDeck.Count; i++)
        {
            CardData temp = cardInDeck[i];
            int randomIndexToSwap = Random.Range(i,cardInDeck.Count);
            cardInDeck[i] = cardInDeck [randomIndexToSwap];
            cardInDeck[randomIndexToSwap] = temp;
        }   
    }
            
    public void SelectCard(CardData card)
    {
        selectedCards.Add(card);
        CheckPlayCardButton();
    }


    public void DeSelectCard(CardData card)
    {
        selectedCards.Remove(card);
        CheckPlayCardButton();
    }

    private void CheckPlayCardButton()
    {
        int totalCost = 0;
        foreach (var card in selectedCards)
        {
            totalCost += card.cardCost;
        }

        playCardButton.gameObject.SetActive(selectedCards.Count > 0 && totalCost <= availableCost);
        Debug.Log($"Selected: {selectedCards.Count} cards, Total cost: {totalCost}/{availableCost}");
    }


}
