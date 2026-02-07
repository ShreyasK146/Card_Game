using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardSelectHandler : MonoBehaviour
{
    public TextMeshProUGUI costText;
    public TextMeshProUGUI powerText;
    public TextMeshProUGUI abilityText;

    public CardData cardData;

    private bool selected = false;
    RectTransform rect;


    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        gameObject.GetComponent<Button>().onClick.AddListener(CardSelect);
    }

    private void CardSelect()
    {
        if (!selected)
        {
            rect.localScale = Vector3.one * 1.05f;
            
            selected = true;

            DeckManager.Instance.SelectCard(cardData);
        }

        else if (selected)
        {
            rect.localScale = Vector3.one;
           
            selected = false;

            DeckManager.Instance.DeSelectCard(cardData);
        }

    }
}
