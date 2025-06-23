<?php
include_once "db.php";

session_start();
header('Content-Type: application/json');


$behaviour = $_GET["behaviour"] ?? 0;

if ($behaviour == 1) 
{
    FetchCurrentChips();
}

if ($behaviour == 2) 
{
    UpdateUserChips();
}

if ($behaviour == 3) 
{
    LeaveMatch();
}

if ($behaviour == 4) 
{
    DeleteMatch();
}

function FetchCurrentChips()
{
    global $mysqli;
    $userID = $_GET["UserID"];

    $currentChipQuery = "SELECT TotalChips FROM player_info WHERE UserID = $userID";

    $playerChips = (int) $mysqli->query($currentChipQuery)->fetch_assoc()["TotalChips"];
    $output = [
        'value' => $playerChips,
    ];
    echo json_encode($output);
}

function UpdateUserChips()
{
    global $mysqli;
    $userID = $_GET["UserID"];
    $newAmount = $_GET["NewAmount"];

    $mysqli->query("UPDATE player_info SET TotalChips = '$newAmount' WHERE UserID = $userID");
    $output = [
        'value' => 1,
    ];
    echo json_encode($output);
}

function LeaveMatch()
{
    global $mysqli;

    $userID = $_GET["UserID"];
    $matchID = $_GET["MatchID"] ?? 0;

    $mysqli->query("DELETE FROM poker_players WHERE UserID = '$userID'");
    $mysqli->query("UPDATE poker_match SET PlayerCount = PlayerCount - 1 WHERE GameID = '$matchID'");
}

function DeleteMatch()
{
    global $mysqli;
    $matchID = $_GET["MatchID"];

    $mysqli->query("DELETE FROM poker_match WHERE GameID = '$matchID'");
}

?>