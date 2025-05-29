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

