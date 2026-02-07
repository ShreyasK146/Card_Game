using UnityEngine;

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
    public string abilityName;
    public int abilityValue;
}