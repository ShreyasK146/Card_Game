using UnityEngine;


public enum AbilityType
{
    GainPoints,
    StealPoints,
    DoublePower,
    DrawExtraCard,
    DiscardOpponentRandomCard,
    DestroyOpponentCardInPlay
}

[CreateAssetMenu(fileName = "Card_",menuName = "CreateObject/CardObject")]
public class CardData : ScriptableObject
{
    public int id;
    public string cardName;
    public int cardCost;
    public int cardPower;
    public AbilityData ability;
}
[System.Serializable]
public class AbilityData
{
    public AbilityType abilityName;
    public int abilityValue;
}