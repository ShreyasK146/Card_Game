using Photon.Pun;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // In AbilityManager.cs
    public void ExecuteAbilityLocally(CardData card, string playerId)
    {
        if (card.ability == null)
        {
            // No ability, just add base card power
            ScoreManager.Instance.AddScore(playerId, card.cardPower);
            return;
        }
        // WELP:(  - I TRIED DOING DIFFERENT ABILITY BUT THE SYNCING WAS.. SO LEFT IT OUT 
        switch (card.ability.abilityName)
        {
            case AbilityType.GainPoints:
                GainPoints(playerId, card.cardPower, card.ability.abilityValue);
                break;
            //case AbilityType.RemoveOpponentScore:
            //    RemoveTheOpponentScore(playerId, card.cardPower, card.ability.abilityValue);
            //    break;
            //case AbilityType.RemoveOpponentScorex2:
            //    RemoveTheOpponentScore(playerId, card.cardPower, card.ability.abilityValue*2);
            //    break;
            //case AbilityType.RemoveOpponentScorex3:
            //    RemoveTheOpponentScore(playerId, card.cardPower, card.ability.abilityValue*3);
            //    break;
            //case AbilityType.StealPoints:
            //    StealPoints(playerId, card.cardPower, card.ability.abilityValue);
            //    break;
            case AbilityType.DoublePower:
                GainPoints(playerId, card.cardPower * 2, 0); 
                break;
            case AbilityType.TriplePower:
                GainPoints(playerId, card.cardPower * 3, 0); 
                break;
            //case AbilityType.StealPointsx2:
            //    StealPoints(playerId, card.cardPower, card.ability.abilityValue * 2);
            //    break;
            //case AbilityType.StealPointsx3:
            //    StealPoints(playerId, card.cardPower, card.ability.abilityValue * 3);
            //    break;
            //case AbilityType.DiscardOpponentRandomCard:
                //DiscardOpponentCard(playerId, card.ability.abilityValue);
                //break;
            default:
                ScoreManager.Instance.AddScore(playerId, card.cardPower);
                break;
        }
    }
    void GainPoints(string playerId, int cardPower, int bonusPoints)
    {
        string myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
        if(myPlayerId == playerId)
        {
            int totalPoints = cardPower + bonusPoints;
            ScoreManager.Instance.AddScore(playerId, totalPoints);
        }

    }


   
}


