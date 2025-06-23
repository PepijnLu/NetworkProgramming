using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct PokerHandResult
{
    public int handValue;   // 1 = High Card, 10 = Royal Flush
    public int keyRank;     // Rank of the most relevant card
}

public class PokerHandEvaluator : MonoBehaviour
{
    public PokerHandResult EvaluateHand(List<PokerCard> allCards)
    {
        // Sort descending
        var ranks = allCards.Select(c => c.rank).ToList();
        var suits = allCards.Select(c => c.suit).ToList();

        var rankGroups = ranks.GroupBy(r => r).OrderByDescending(g => g.Count()).ThenByDescending(g => g.Key).ToList();
        var suitGroups = allCards.GroupBy(c => c.suit).ToList();

        bool IsStraight(List<int> sortedRanks, out int highCard)
        {
            var distinct = sortedRanks.Distinct().OrderByDescending(r => r).ToList();

            // Handle Ace as low (1) for A-2-3-4-5
            if (distinct.Contains(14)) distinct.Add(1);

            for (int i = 0; i <= distinct.Count - 5; i++)
            {
                bool straight = true;
                for (int j = 0; j < 4; j++)
                {
                    if (distinct[i + j] - 1 != distinct[i + j + 1])
                    {
                        straight = false;
                        break;
                    }
                }
                if (straight)
                {
                    highCard = distinct[i];
                    return true;
                }
            }

            highCard = 0;
            return false;
        }

        // Check for flush
        var flushSuit = suitGroups.FirstOrDefault(g => g.Count() >= 5);
        if (flushSuit != null)
        {
            var flushCards = flushSuit.OrderByDescending(c => c.rank).ToList();
            var flushRanks = flushCards.Select(c => c.rank).ToList();
            if (IsStraight(flushRanks, out int straightFlushHigh))
            {
                if (straightFlushHigh == 14)
                    return new PokerHandResult { handValue = 10, keyRank = 14 }; // Royal Flush
                else
                    return new PokerHandResult { handValue = 9, keyRank = straightFlushHigh }; // Straight Flush
            }
            return new PokerHandResult { handValue = 6, keyRank = flushRanks[0] }; // Flush
        }

        // Four of a kind
        if (rankGroups[0].Count() == 4)
        {
            return new PokerHandResult { handValue = 8, keyRank = rankGroups[0].Key };
        }

        // Full house
        if (rankGroups[0].Count() == 3 && rankGroups.Count > 1 && rankGroups[1].Count() >= 2)
        {
            return new PokerHandResult { handValue = 7, keyRank = rankGroups[0].Key };
        }

        // Straight
        if (IsStraight(ranks, out int straightHigh))
        {
            return new PokerHandResult { handValue = 5, keyRank = straightHigh };
        }

        // Three of a kind
        if (rankGroups[0].Count() == 3)
        {
            return new PokerHandResult { handValue = 4, keyRank = rankGroups[0].Key };
        }

        // Two pair
        if (rankGroups[0].Count() == 2 && rankGroups.Count > 1 && rankGroups[1].Count() == 2)
        {
            return new PokerHandResult { handValue = 3, keyRank = Mathf.Max(rankGroups[0].Key, rankGroups[1].Key) };
        }

        // One pair
        if (rankGroups[0].Count() == 2)
        {
            return new PokerHandResult { handValue = 2, keyRank = rankGroups[0].Key };
        }

        // High card
        return new PokerHandResult { handValue = 1, keyRank = rankGroups[0].Key };
    }
}
