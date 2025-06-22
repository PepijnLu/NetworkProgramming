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
}

public class PokerMatch
{
    public Dictionary<uint, PokerPlayer> playersByUserID = new();
    public List<PokerPlayer> playersInMatch = new();
    public List<PokerPlayer> bettingPlayers = new();
    public List<PokerCard> matchDeck = new();
    public int currentTurnUserID;
    public int currentBet;
    public int lastRaiseUserID;
    public GAME_STATE gameState;
}
public class Poker : MonoBehaviour
{
    public static Poker instance;
    [SerializeField] ClientDataProcess clientDataProcess;
    [SerializeField] List<PokerCard> pokerCards;
    public int userMatchID;
    public int currentMatchBet;
    public Dictionary<uint, PokerPlayer> playersByUserID = new();
    Dictionary<int, PokerCard> pokerCardsDict = new();
    [SerializeField] Transform deckTransform1, deckTransform2, deckTransform3, deckTransform4, deckTransform5, pokerHand1Transform, pokerHand2Transform;
    void Awake()
    {
        instance = this;

        foreach (PokerCard _pokerCard in pokerCards)
        {
            pokerCardsDict.Add(_pokerCard.cardID, _pokerCard);
        }
    }
    // Update is called once per frame
    void Update()
    {

    }

    public void EndPokerRound()
    {
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
        PokerCard newCard1 = Instantiate(pokerCardsDict[card1ID], pokerHand1Transform);
        PokerCard newCard2 = Instantiate(pokerCardsDict[card2ID], pokerHand2Transform);

        // PokerCard newDeckCard1 = Instantiate(pokerCardsDict[deckCard1], deckTransform1);
        // PokerCard newDeckCard2 = Instantiate(pokerCardsDict[deckCard2], deckTransform2);
        // PokerCard newDeckCard3 = Instantiate(pokerCardsDict[deckCard3], deckTransform3);
        // PokerCard newDeckCard4 = Instantiate(pokerCardsDict[deckCard4], deckTransform4);
        // PokerCard newDeckCard5 = Instantiate(pokerCardsDict[deckCard5], deckTransform5);
    }

    //1 = fold, 2 is call, 3 = raise
    public void PlayTurn(int _action, int betAmount)
    {
        if((betAmount < currentMatchBet) && (_action == 2))
        {
            Debug.Log("Cannot check with bet less than current bet");
            return;
        }
        else if((betAmount < currentMatchBet + 20) && (_action == 3))
        {
            Debug.Log("Cannot bet less than current bet + minimal raise amount of 20");
            return;
        }

        int userID = clientDataProcess.userInfo.userID;
        int matchID = userMatchID;

        ClientBehaviour.instance.SendInt(new uint[4]{(uint)userID, (uint)matchID, (uint)_action, (uint)betAmount}, "playTurn");
    }
}
