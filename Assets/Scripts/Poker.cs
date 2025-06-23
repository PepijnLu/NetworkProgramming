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
    public bool joiningNextRound;
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
    [SerializeField] List<PokerCard> pokerCards;
    Dictionary<int, PokerCard> pokerCardsDict = new();
    void Awake()
    {
        foreach (PokerCard _pokerCard in pokerCards)
        {
            pokerCardsDict.Add(_pokerCard.cardID, _pokerCard);
        }
    }

    public Dictionary<int, PokerCard> GetCardDict()
    {
        return pokerCardsDict;
    }

    public List<PokerCard> GetPokerCards()
    {
        return pokerCards;
    }
}
