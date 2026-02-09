using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class OpponentBoardDisplay : MonoBehaviour
{
    public Transform opponentBoardContainer;
    public GameObject faceDownCardPrefab;

    public void UpdateOpponentBoard(int cardCount)
    {
        for (int i = 0; i < cardCount; i++)
        {
            GameObject card = Instantiate(faceDownCardPrefab, opponentBoardContainer);
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
        foreach (Transform child in opponentBoardContainer)
        {
            var cardUI = child.GetComponent<CardSelectHandler>();
            if (cardUI != null && cardUI.costText.text == "?")
            {
                hiddenCards.Add(child.gameObject);
            }
        }

   
        for (int i = 0; i < cards.Count && i < hiddenCards.Count; i++)
        {
            var cardUI = hiddenCards[i].GetComponent<CardSelectHandler>();

            cardUI.costText.text = cards[i].cardCost.ToString();
            cardUI.powerText.text = cards[i].cardPower.ToString();
            cardUI.abilityText.text = cards[i].cardName;

            hiddenCards[i].GetComponent<Button>().interactable = false;
        }

        Debug.Log($"Revealed {cards.Count} opponent cards");
    }
}
