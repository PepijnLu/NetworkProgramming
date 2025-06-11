using System.Collections.Generic;
using UnityEngine;
public class RootData 
{
    public List<ScoreEntry> Last5Scores;
    public List<UserScoreSummary> UserScores;
    public List<HighScoreEntry> RecentHighScore;
    public List<ScoreEntry> ScoresBetweenDates;
    public List<RecentGameEntry> RecentP1Games;
}
public class ScoreEntry 
{
    public int Score;
    public string ScoredAt;
    public string Username;
}

public class UserScoreSummary 
{
    public string Username;
    public int Count;
    public float AverageScore;
}

public class HighScoreEntry 
{
    public int HighScore;
    public string ScoredAt;
    public string Username;
}

public class RecentGameEntry 
{
    public string Username;
    public int NumberOfGames;
}

public class LoginResponse
{
    public bool success;
    public string message;
    public string sessionId; // Optional: will be null if not successful
}

public class UserInfo
{
    public string Username;
    public string Email;
    public string Country;
    public string DateOfBirth;
}

public class PokerMatchCardInfo
{
    public int handCard1;
    public int handCard2;
    public int deckCard1;
    public int deckCard2;
    public int deckCard3;
    public int deckCard4;
    public int deckCard5;
}

public class PokerMatchPlayerInfo
{
    public int playerID;
    public int betAmount;
    public int remainingChips;
    public string chosenAction;
    public int waiting;
}

public class SingleInt
{
    public int value = 0;
}

public class SingleString
{
    public string result;
}

