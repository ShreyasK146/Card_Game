using UnityEngine;


public enum AbilityType
{
    BaseCard,
    GainPoints,
    StealPoints,
    DoublePower,
    RemoveOpponentScore,
    RemoveOpponentScorex2,
    RemoveOpponentScorex3,
    //DrawExtraCard,
    DiscardOpponentRandomCard,
    //DestroyOpponentCardInPlay,
    TriplePower,
    StealPointsx2,
    StealPointsx3
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