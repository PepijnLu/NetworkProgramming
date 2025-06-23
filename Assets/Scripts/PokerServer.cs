using System.Collections.Generic;
using UnityEngine;

public class PokerServer : MonoBehaviour
{
    [SerializeField] PokerHandEvaluator pokerHandEvaluator;
    public void EndPokerRound(PokerMatch pokerMatch)
    {
        Debug.Log("End poker round");

        List<PokerPlayer> remainingPlayers = pokerMatch.bettingPlayers;
        List<PokerPlayer> roundWinners = new();

        if(remainingPlayers.Count == 0)
        {
            Debug.LogError("Cant be 0 players remaining");
            return;
        }
        else if (remainingPlayers.Count == 1)
        {
            Debug.Log("Onlyt 1 player remaining");
            roundWinners.Add(remainingPlayers[0]);
        }
        else if(remainingPlayers.Count > 1)
        {
            //SHOWDOWN
            Debug.Log("More than one player remaining");
            int highestHand = 0;
            int highestRelevantRank = 0;
            List<PokerCard> sharedCards = pokerMatch.matchDeck;
            foreach(PokerPlayer _player in remainingPlayers)
            {
                List<PokerCard> playerPlusSharedCards = new();
                foreach(PokerCard _card in sharedCards) playerPlusSharedCards.Add(_card);
                playerPlusSharedCards.Add(pokerCardsDict[_player.handCard1]);
                playerPlusSharedCards.Add(pokerCardsDict[_player.handCard2]);

                Debug.Log("Hand check setup complete");

                PokerHandResult _result = pokerHandEvaluator.EvaluateHand(playerPlusSharedCards);
                _player.pokerHandResult = _result;

                Debug.Log("Hand check result fetched");

                if(_result.handValue == highestHand)
                {
                    if(_result.keyRank > highestRelevantRank)
                    {
                        roundWinners.Clear();
                        highestRelevantRank = _result.keyRank;
                        roundWinners.Add(_player);
                    }
                    else if(_result.keyRank == highestRelevantRank)
                    {
                        roundWinners.Add(_player);
                    }
                }
                else if(_result.handValue >= highestHand)
                {
                    roundWinners.Clear();
                    highestRelevantRank = _result.keyRank;
                    highestHand = _result.handValue;
                    roundWinners.Add(_player);
                }
                
            }

            Debug.Log("Hand check complete");

            //Distribute chips under winners
            int pool = 0;
            foreach(PokerPlayer _pokerPlayer in pokerMatch.playersInRound) 
            {
                pool += _pokerPlayer.betAmount;
            }
            int shareOfBet = pool / roundWinners.Count;

            foreach(PokerPlayer _pokerPlayer in roundWinners) 
            {
                Debug.Log($"Winner: {_pokerPlayer.userID} got {shareOfBet}");
                _pokerPlayer.userChips += shareOfBet;
            }

            Debug.Log("Chips distrubuted");

            //Update remaining player chips in database
            foreach(PokerPlayer _pokerPlayer in pokerMatch.bettingPlayers)
            {
                GetRequests.instance.SetPlayerChips(_pokerPlayer.userID, _pokerPlayer.userChips);
            }

            Debug.Log("Updated chips in databse");

            //Reset necessary values
            ResetMatchServer(pokerMatch);

            //Start new round
        }
        Debug.Log("Round ended");
    }
}
