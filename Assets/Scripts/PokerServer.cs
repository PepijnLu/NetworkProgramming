using System.Collections.Generic;
using UnityEngine;

public class PokerServer : MonoBehaviour
{
    [SerializeField] Poker _poker;
    [SerializeField] PokerHandEvaluator pokerHandEvaluator;
    Dictionary<int, PokerCard> pokerCardsDict;
    [SerializeField] List<PokerCard> pokerCards;
    [SerializeField] GetRequests getRequests;
    [SerializeField] ServerBehaviour serverBehaviour;

    void Start()
    {
        pokerCardsDict = _poker.GetCardDict();
        pokerCards = _poker.GetPokerCards();
    }
    public void EndPokerRound(PokerMatch pokerMatch, uint matchID)
    {
        Debug.Log("End poker round");

        List<PokerPlayer> remainingPlayers = pokerMatch.bettingPlayers;
        List<PokerPlayer> roundWinners = new();

        if(remainingPlayers.Count == 0)
        {
            Debug.LogError("Cant be 0 players remaining");
            return;
        }
        // else if (remainingPlayers.Count == 1)
        // {
        //     Debug.Log("Onlyt 1 player remaining");
        //     roundWinners.Add(remainingPlayers[0]);
        // }
        else
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
            int pool = 0;
            foreach(PokerPlayer _pokerPlayer in pokerMatch.playersInRound) 
            {
                pool += _pokerPlayer.betAmount;
            }
            int shareOfBet = pool / roundWinners.Count;

            foreach(PokerPlayer _pokerPlayer in roundWinners) 
            {
                Debug.Log($"Winner: {_pokerPlayer.userID} got {shareOfBet}");
                getRequests.AddPlayerChips(_pokerPlayer.userID, shareOfBet, _pokerPlayer.connectionID);
                //_pokerPlayer.userChips += shareOfBet;
            }
        }

        //Reset necessary values
        EndMatch(pokerMatch, matchID);

        Debug.Log("Round ended");

        
    }

    public List<PokerCard> GetShuffledCards(int _playerCount)
    {
        Debug.Log("Try get shuffled cards");

        List<PokerCard> deck = new();
        foreach (PokerCard pokerCard in pokerCards)
        {
            deck.Add(pokerCard);
        }

        Debug.Log("Whole deck created");

        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]); // swap
        }

        Debug.Log("Deck shuffled");

        int cardsToKeep = 5 + (_playerCount * 2);
        deck.RemoveRange(cardsToKeep, deck.Count - cardsToKeep);

        Debug.Log("Deck returned");

        return deck;
    }

    public async void EndMatch(PokerMatch _pokerMatch, uint matchID)
    {
        _pokerMatch.playersInRound = new();
        _pokerMatch.bettingPlayers = new();
        _pokerMatch.matchDeck = new();
        _pokerMatch.playersByUserID = new();
        _pokerMatch.currentTurnUserID = 0;
        _pokerMatch.currentBet = 0;
        _pokerMatch.lastRaiseUserID = 0;
        _pokerMatch.gameState = GAME_STATE.PRE_FLOP;
        _pokerMatch.bigBlindReraised = false;
        _pokerMatch.waitingForPlayersComplete = false;
        getRequests.setupMatches.Remove(matchID);

        // for(int i = _pokerMatch.connectedPlayers.Count - 1; i >= 0; i--)
        // {
        //     if((!_pokerMatch.connectedPlayers[i].joiningNextRound) || (_pokerMatch.connectedPlayers[i].userChips == 0))
        //     {
        //         _pokerMatch.connectedPlayers.RemoveAt(i);
        //         await getRequests.RunTask(0, "leaveMatch", new uint[2]{(uint)_pokerMatch.connectedPlayers[i].userID, matchID});
        //     }
        // }

        //if(_pokerMatch.connectedPlayers.Count <= 1)
        //{
            //Abandon match
            foreach(PokerPlayer _player in _pokerMatch.connectedPlayers)
            {
                serverBehaviour.SendDataToClient(_player.connectionID, "setLobbyStatus", 1, _stringData: new string[1]{ "toUserInfo" });
                await getRequests.RunTask(0, "leaveMatch", new uint[2]{(uint)_player.userID, matchID});
            }
            _pokerMatch.connectedPlayers.Clear();

            //Delete entry
            await getRequests.RunTask(0, "deleteMatch", new uint[1]{matchID});
        //}
        //else
        //{
            //StartMatch(matchID);
        //}

    }

    public async void StartMatch(uint matchID)
    {
        await getRequests.RunTask(0, "waitForPlayers", new uint[1]{matchID});
    }
}
