using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OpponentBoardDisplay : MonoBehaviour
{
    public Transform contentTransformForCardsInOpponentArea;
    public GameObject faceDownCardPrefab;
    public TextMeshProUGUI opponentCost;

    //this make sures cards are folded in opponent area 
    public void UpdateOpponentBoard(int cardCount, int opponentavailableCost)
    {
        opponentCost.text = opponentavailableCost.ToString();
        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = Instantiate(faceDownCardPrefab, contentTransformForCardsInOpponentArea);
            var cardUI = card.GetComponent<CardSelectHandler>();
            cardUI.costText.text = "?";
            cardUI.powerText.text = "?";
            cardUI.abilityText.text = "Hidden";

            card.GetComponent<Button>().interactable = false;
        }
    }

    public void RevealOpponentCards(List<CardData> cards)
    {
        
        List<GameObject> hiddenCards = new List<GameObject>();
        foreach (Transform child in contentTransformForCardsInOpponentArea)
        {
            var cardUI = child.GetComponent<CardSelectHandler>();
            if (cardUI != null && cardUI.costText.text == "?")
            {
                hiddenCards.Add(child.gameObject);
            }
        }
        // taking all hiddencards from hiddencards list and revealing them 
        for (int i = 0; i < cards.Count && i < hiddenCards.Count; i++)
        {
            var cardUI = hiddenCards[i].GetComponent<CardSelectHandler>();

            cardUI.costText.text = cards[i].cardCost.ToString();
            cardUI.powerText.text = cards[i].cardPower.ToString();
            cardUI.abilityText.text = cards[i].cardName;

            hiddenCards[i].GetComponent<Button>().interactable = false;
        }
    }
}
