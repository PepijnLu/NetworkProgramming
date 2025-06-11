using System;
using System.Collections.Generic;
using UnityEngine;
public class Poker : MonoBehaviour
{
    public static Poker instance;
    [SerializeField] List<PokerCard> pokerCards;
    Dictionary<int, PokerCard> pokerCardsDict = new();
    [SerializeField] Transform deckTransform1, deckTransform2, deckTransform3, deckTransform4, deckTransform5, pokerHand1Transform, pokerHand2Transform;
    void Awake()
    {
        instance = this;

        foreach(PokerCard _pokerCard in pokerCards)
        {
            pokerCardsDict.Add(_pokerCard.cardID, _pokerCard);
        }
    }
    // Update is called once per frame
    void Update()
    {
        
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
}
