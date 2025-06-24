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
    public int userID;
    public string Username;
    public string Email;
    public string Country;
    public string DateOfBirth;
    public int connectionID;
}

public class SingleInt
{
    public int value = 0;
    public int value2 = 0;
    public int value3 = 0;
    public int value4 = 0;
    public int value5 = 0;
}

public class SingleString
{
    public string result;
}

