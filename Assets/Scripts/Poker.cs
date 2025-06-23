using System;
using System.Collections.Generic;
using UnityEngine;
public enum GAME_STATE
{
    PRE_FLOP,
    FLOP,
    TURN,
    RIVER,
    SHOWDOWN
}
public class PokerPlayer
{
    public int userID;
    public int connectionID;
    public int handCard1;
    public int handCard2;
    public int betAmount;
    public int orderInTurn;
    public int userChips;
    public PokerHandResult pokerHandResult;
}

public class PokerMatch
{
    public Dictionary<uint, PokerPlayer> playersByUserID = new();
    public List<PokerPlayer> connectedPlayers = new();
    public List<PokerPlayer> playersInRound = new();
    public List<PokerPlayer> bettingPlayers = new();
    public List<PokerCard> matchDeck = new();
    public int currentTurnUserID;
    public int currentBet;
    public int lastRaiseUserID;
    public GAME_STATE gameState;
    public bool bigBlindReraised;
    public bool waitingForPlayersComplete;
}
public class Poker : MonoBehaviour
{
    [Header("Used By Server")]
    [SerializeField] PokerHandEvaluator pokerHandEvaluator;
    [Header("Used By Client")]
    [SerializeField] ClientDataProcess clientDataProcess;
    [SerializeField] GameObject placeholderRiver;
    [SerializeField] Transform pokerHandC1Transform;
    [SerializeField] Transform pokerHandC2Transform;
    List<Transform> sharedCardTransforms;
    public Dictionary<uint, PokerPlayer> playersByUserID = new();
    public int userMatchID;
    public bool isYourTurn;
    public int currentMatchBet;
    public int userChips;
    public int userBet;
    public int raiseBetAmount;
    [Header("Used By Both")]
    public static Poker instance;
    [SerializeField] List<PokerCard> pokerCards;
    Dictionary<int, PokerCard> pokerCardsDict = new();
    void Awake()
    {
        instance = this;

        foreach (PokerCard _pokerCard in pokerCards)
        {
            pokerCardsDict.Add(_pokerCard.cardID, _pokerCard);
        }
    }

    //Called from server
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

    public List<PokerCard> GetShuffledCards(int _playerCount)
    {
        List<PokerCard> deck = new();

        foreach (PokerCard pokerCard in pokerCards)
        {
            deck.Add(pokerCard);
        }

        for (int i = deck.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]); // swap
        }
        int cardsToKeep = 5 + (_playerCount * 2);
        deck.RemoveRange(cardsToKeep, deck.Count - cardsToKeep);

        return deck;
    }

    public void GenerateHand(int card1ID, int card2ID)
    {
        Instantiate(pokerCardsDict[card1ID], pokerHandC1Transform);
        Instantiate(pokerCardsDict[card2ID], pokerHandC2Transform);
    }

    public void GenerateSharedCard(int _cardNumber, int _cardID)
    {
        Debug.Log($"Generate Shared Card {_cardNumber}, count: {sharedCardTransforms.Count}");
        Transform cardTransform = sharedCardTransforms[_cardNumber - 1];
        Destroy(cardTransform.GetChild(0).gameObject);
        PokerCard newCard = Instantiate(pokerCardsDict[_cardID], cardTransform);
        newCard.transform.localRotation = Quaternion.Euler(0, 180, 0);
        Debug.Log($"Generated Shared Card {_cardNumber}"); 
    }

    //1 = fold, 2 is call, 3 = raise
    public void PlayTurn(int _action)
    {
        if(!isYourTurn) return;

        int betAmount = 0;

        if(_action == 2)
        {
            betAmount = currentMatchBet - userBet;
        }
        else if (_action == 3)
        {
            betAmount = raiseBetAmount;
        }

        if(betAmount > userChips)
        {
            Debug.Log("Cannot bet more than you have");
            return;
        }

        if((betAmount < currentMatchBet + 20) && (_action == 3))
        {
            Debug.Log("Cannot bet less than current bet + minimal raise amount of 20");
            return;
        }

        int userID = clientDataProcess.userInfo.userID;
        int matchID = userMatchID;

        userBet = betAmount;
        userChips -= betAmount;

        UIManager.instance.GetTextElementFromDict("YourChips").text = $"Chips: {userChips}";
        EndPlayerTurn();

        ClientBehaviour.instance.SendInt(new uint[3]{(uint)userID, (uint)matchID, (uint)userChips}, "setUserChips");
        ClientBehaviour.instance.SendInt(new uint[4]{(uint)userID, (uint)matchID, (uint)_action, (uint)betAmount}, "playTurn");
    }

    public void StartPokerRound()
    {
        GameObject _river = Instantiate(placeholderRiver);
        sharedCardTransforms = new();
        for(int i = 0; i < 5; i++)
        {
            sharedCardTransforms.Add(_river.transform.GetChild(i));
        }
    }

    public void EndPlayerTurn()
    {
        isYourTurn = false;
        UIManager.instance.GetUIElementFromDict("BetButtons").SetActive(false);
    }

    public void ChangeRaiseAmount(bool _increment)
    {
        if(_increment)
        {
            if((raiseBetAmount + 20) <= userChips) 
            {
                raiseBetAmount += 20;
            }
            else 
            {
                Debug.LogWarning("User doesnt have enough chips to increment bet");
            }
        }
        else
        {
            if((raiseBetAmount - 20) >= 0) raiseBetAmount -= 20;
        }

        UIManager.instance.GetTextElementFromDict("RaiseBetAmount").text = $"{raiseBetAmount}";
    }

    //Runs on client
    public void ResetMatchClient(bool _disconnect)
    {

    }
    //Runs on server
    public void ResetMatchServer(PokerMatch pokerMatch)
    {

    }
}
