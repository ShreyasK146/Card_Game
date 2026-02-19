using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;
    static int count = 0;


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
            //// No ability, just add base card power
            //if (playerId != GetOpponentID())
            //{
            //    ScoreManager.Instance.myScore = ScoreManager.Instance.myScore + card.cardPower;
            //    ScoreManager.Instance.AddScore(playerId, ScoreManager.Instance.myScore, ScoreManager.Instance.opponentScore);
            //}
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
            case AbilityType.StealPoints:
                StealPoints(playerId, card.cardPower, card.ability.abilityValue);
                break;
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
                if(playerId != GetOpponentID())
                {
                    ScoreManager.Instance.myScore = ScoreManager.Instance.myScore + card.cardPower;
                    ScoreManager.Instance.UpdateScoreDisplay();
                    Debug.Log("myScore = " + ScoreManager.Instance.myScore + "\t opponentScore = " + ScoreManager.Instance.opponentScore);
                    ScoreManager.Instance.AddScore(playerId, ScoreManager.Instance.myScore, ScoreManager.Instance.opponentScore);
                }
                    
                break;
        }
    }

    private string GetOpponentID()
    {
        foreach(var player in PhotonNetwork.PlayerList)
        {
            if (player != PhotonNetwork.LocalPlayer)
            {
                Debug.Log("we found opponent");
                return player.ActorNumber.ToString();
            }    
                
        }
        return "";
    }

    private void StealPoints(string playerId, int cardPower, int stealPoints)
    {
        string myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
        if (myPlayerId == playerId)
        {
            //int totalPoints = ;
            ScoreManager.Instance.myScore = ScoreManager.Instance.myScore + cardPower + stealPoints;
            ScoreManager.Instance.opponentScore = ScoreManager.Instance.opponentScore - stealPoints;
            ScoreManager.Instance.UpdateScoreDisplay();
            Debug.Log("myScore = " + ScoreManager.Instance.myScore + "\t opponentScore = " + ScoreManager.Instance.opponentScore);
            ScoreManager.Instance.AddScore(playerId, ScoreManager.Instance.myScore, ScoreManager.Instance.opponentScore);

            Debug.Log("iam player right ? ");
        }

       
    }
    void GainPoints(string playerId, int cardPower, int bonusPoints)
    {
        string myPlayerId = PhotonNetwork.LocalPlayer.ActorNumber.ToString();
        if (myPlayerId == playerId)
        {
            ScoreManager.Instance.myScore = ScoreManager.Instance.myScore + cardPower + bonusPoints;
            ScoreManager.Instance.UpdateScoreDisplay();
            Debug.Log("myScore = " + ScoreManager.Instance.myScore + "\t opponentScore = " + ScoreManager.Instance.opponentScore);
            ScoreManager.Instance.AddScore(playerId, ScoreManager.Instance.myScore,ScoreManager.Instance.opponentScore);
        }

    }


    


   
}


