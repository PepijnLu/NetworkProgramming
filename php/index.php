<?php
include "db.php";
include "users.php";

header('Content-Type: application/json; charset=utf-8');

// $top5Scores = GetLast5Scores();
// $userScores = GetTimesPlayersPlayedGame();
// $recentHighScores = GetHighestScoreLast7Days();
$lastMonthSameDay = GetLastMonthSameDay();

$last5Scores = GetJsonFromQuery("SELECT _score.Score, _score.ScoredAt, _user.Username FROM Scores _score INNER JOIN Users _user ON (_score.UserID = _user.ID) ORDER BY _score.ScoredAt DESC LIMIT 5", false, "Last5Scores");

$userScores = GetJsonFromQuery("SELECT _user.Username, COUNT(*) AS Count, AVG(_score.Score) AS AverageScore FROM Scores _score INNER JOIN Users _user ON (_score.UserID = _user.ID) WHERE _score.ScoredAt > $lastMonthSameDay GROUP BY _user.ID, _user.Username ORDER BY AVG(_score.Score) DESC", false, "TopUserGoldLastMonth");

$weekAgo = date('Y-m-d', strtotime('-7 days'));
$recentHighScore = GetJsonFromQuery("SELECT MAX(_score.Score) AS HighScore, _score.ScoredAt, _user.Username FROM Scores _score INNER JOIN Users _user ON (_score.UserID = _user.ID) WHERE _score.ScoredAt > $weekAgo", true, "RecentHighScore");

$recentGames = GetJsonFromQuery("SELECT COUNT(*) AS NumberOfGames FROM Scores _score INNER JOIN Users _user ON (_score.UserID = _user.ID) WHERE _score.ScoredAt > $lastMonthSameDay", true, "RecentGames");
$recentPlayer1Games = GetJsonFromQuery("SELECT _user.Username, COUNT(*) AS NumberOfGames FROM Scores _score INNER JOIN Users _user ON (_score.UserID = _user.ID) WHERE _score.ScoredAt > $lastMonthSameDay AND _user.ID = 1", true, "RecentP1Games");

$date1 = $_GET["Date1"] ?? "";
$date2 = $_GET["Date2"] ?? "";
$getScoresBetweenDates = array();
if($date1 != "" && $date2 != "") 
{

  $date1 = date_format(date_create($date1), 'Y/m/d');
  $date2 = date_format(date_create($date2), 'Y/m/d');
  //echo "<br>. date 1: ".$date1."<br>date 2: ".$date2;
  $getScoresBetweenDates = GetJsonFromQuery("SELECT _score.Score, _score.ScoredAt, _user.Username FROM Scores _score INNER JOIN Users _user ON (_score.UserID = _user.ID) WHERE _score.ScoredAt BETWEEN '$date1' AND '$date2'", false, "ScoresBetweenDates");
  
}

echo json_encode(array_merge($userScores, $recentGames, $recentHighScore, $last5Scores, $recentPlayer1Games, (array)$getScoresBetweenDates));

function GetLastMonthSameDay()
{
  $today = new DateTime();
  $lastMonthSameDay = (clone $today)->modify('-1 month');
  if ($lastMonthSameDay->format('d') !== $today->format('d')) 
  {
    // Adjust for months with fewer days
    $lastMonthSameDay = $lastMonthSameDay->modify('last day of last month');
  }
  return $lastMonthSameDay->format('Y-m-d');
}

?>