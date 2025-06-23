<?php
include_once "db.php";
session_start();
header('Content-Type: application/json');

$behaviour = $_GET["behaviour"] ?? 0;

if($behaviour == 0) 
{
    QuitMatchmaking();
}
else if($behaviour == 1)
{
    WaitForPlayers();
}
else if($behaviour == 2)
{
    FindOrCreateMatch();
}
else if($behaviour == 3)
{
    StartMatch();
}

// function StartMatchmacking()
// {
//     global $mysqli;
//     $userID = $_SESSION["User_ID"];
//     $_SESSION['IsHost'] = false;

//     $startSearching = "INSERT INTO Matchmaking VALUES ($userID)";

//     if (!($result = $mysqli->query($startSearching)))
//     showerror($mysqli->errno,$mysqli->error);

//     $output = [
//         'value' => 1,
//     ];

//     echo json_encode($output);
// }

//Gets called bt unity until it returns something that isnt 0
function FindOrCreateMatch()
{
    global $mysqli;

    $userID = $_GET["UserID"];
    //$_SESSION['IsHost'] = false;
    
    //Try find an existing match
    $findMatch = "SELECT * FROM poker_match WHERE PlayerCount < 4 ORDER BY RAND() LIMIT 1";
    $findResult = $mysqli->query($findMatch);

    //Existing match found
    if ($findResult && $findResult->num_rows > 0) 
    {
        $row = $findResult->fetch_assoc();
        $matchID = $row['GameID'];

        $getPlayerCount = "SELECT PlayerCount FROM poker_match WHERE GameID = $matchID";
        $playerCountResult = $mysqli->query($getPlayerCount);
        $playerCountRow = $playerCountResult->fetch_assoc();
        $playerCount = $playerCountRow['PlayerCount'];
        $playerID = $playerCount + 1;

        //Add yourself to match 
        $addYourselfToMatch = "INSERT INTO poker_players VALUES ('$userID', '$matchID', 1)"; 
        if (!($result = $mysqli->query($addYourselfToMatch)))
        showerror($mysqli->errno,$mysqli->error);

        //Increase match player count
        $increasePlayerCount = "UPDATE poker_match SET PlayerCount = PlayerCount + 1 WHERE GameID = $matchID";
        if (!($result = $mysqli->query($increasePlayerCount)))
        showerror($mysqli->errno,$mysqli->error);

        $output = [
                    'value' => $matchID
                ];
        echo json_encode($output);
    }
    //No match found, create one
    else
    {
        //How many current matches exist?
        $countQuery = "SELECT MAX(GameID) FROM poker_match";
        $countResult = $mysqli->query($countQuery);
        if ($countResult) 
        {
            $row = $countResult->fetch_assoc();
            $newMatchID = ($row['GameID'] ?? 0) + 1;
        }

        //Create new match
        $createMatch = "INSERT INTO poker_match VALUES ('$newMatchID', 1)"; 
        if (!($result = $mysqli->query($createMatch)))
        showerror($mysqli->errno,$mysqli->error);

        //Add yourself to match 
        $addYourselfToMatch = "INSERT INTO poker_players VALUES ('$userID', '$newMatchID', 1)"; 
        if (!($result = $mysqli->query($addYourselfToMatch)))
        showerror($mysqli->errno,$mysqli->error);
        $_SESSION['IsHost'] = true;

        //Set session match ID
        $_SESSION['MatchID'] = $newMatchID;

        $output = [
            'value' => $newMatchID
        ];
        echo json_encode($output);
    }
}

function StartMatch()
{
    global $mysqli;
    //$matchID = $_SESSION['MatchID'];
    //$userID = $_SESSION["User_ID"];
    $userID = $_GET["UserID"];
    $matchID = $_GET['MatchID'];
    
    $getGameState = "SELECT * FROM poker_match WHERE GameID = $matchID";
    $gameStateResult = $mysqli->query($getGameState);
    $gameStateRow = $gameStateResult->fetch_assoc();
    $gameState = $gameStateRow['GameState'];

    if($gameState === 'Ongoing')
    {
        $getPlayerState = "SELECT * FROM poker_players WHERE MatchID = $matchID AND UserID = $userID";
        $getPlayerStateResult = $mysqli->query($getPlayerState);
        $playerStateRow = $getPlayerStateResult->fetch_assoc();

        $handCard1 = $playerStateRow['Card1ID'];
        $handCard2 = $playerStateRow['Card2ID'];
        // $deckCard1 = $gameStateRow['Card1ID'];
        // $deckCard2 = $gameStateRow['Card2ID'];
        // $deckCard3 = $gameStateRow['Card3ID'];
        // $deckCard4 = $gameStateRow['Card4ID'];
        // $deckCard5 = $gameStateRow['Card5ID'];

        if($handCard1 == null || $handCard2 == null)
        {
            $output = [
                'handCard1' => 0
            ];
            echo json_encode($output);
            return;
        }
        else
        {   
            $output = [
                'handCard1' => $handCard1,
                'handCard2'=> $handCard2,
                // 'deckCard1' => $deckCard1,
                // 'deckCard2' => $deckCard2,
                // 'deckCard3' => $deckCard3,
                // 'deckCard4' => $deckCard4,
                // 'deckCard5' => $deckCard5,
            ];
            echo json_encode($output);
            return;
        }
    }
    else if($_SESSION['IsHost'])
    {
        $getPlayerCount = "SELECT PlayerCount FROM poker_match WHERE GameID = $matchID";
        $playerCountResult = $mysqli->query($getPlayerCount);
        $playerCountRow = $playerCountResult->fetch_assoc();
        $playerCount = $playerCountRow['PlayerCount'];

        //Host sets a random player to start
        if($playerCount > 1)
        {
            //Set players to not waiting
            $updatePlayerState = "UPDATE poker_players SET Waiting = 0 WHERE MatchID = $matchID";
            if (!($result = $mysqli->query($updatePlayerState)))
            showerror($mysqli->errno,$mysqli->error);

            //Set player turn
            $randomNumber = random_int(1, $playerCount);
            $updatePlayerTurn = "UPDATE poker_match SET PlayerTurn = '$randomNumber' WHERE GameID = $matchID";
            if (!($result = $mysqli->query($updatePlayerTurn)))
            showerror($mysqli->errno,$mysqli->error);

            $excluded = []; // initially excluded numbers
            
            //Handout cards
            for($i = 1; $i < $playerCount + 1; $i++)
            {
                $firstCardID = GetRandomNumber($excluded);
                $excluded[] = $firstCardID;

                $secondCardID = GetRandomNumber($excluded);
                $excluded[] = $secondCardID;

                $mysqli->query("UPDATE poker_players SET Card1ID = '$firstCardID' WHERE MatchID = $matchID AND PlayerID = $i");
                $mysqli->query("UPDATE poker_players SET Card2ID = '$secondCardID' WHERE MatchID = $matchID AND PlayerID = $i");
            }

            //Make shared cards
            /*
            $firstCardID = GetRandomNumber($excluded);
            $excluded[] = $firstCardID;

            $secondCardID = GetRandomNumber($excluded);
            $excluded[] = $secondCardID;

            $thirdCardID = GetRandomNumber($excluded);
            $excluded[] = $thirdCardID;

            $fourthCardID = GetRandomNumber($excluded);
            $excluded[] = $fourthCardID;

            $fifthCardID = GetRandomNumber($excluded);
            $excluded[] = $fifthCardID;

            $mysqli->query("UPDATE poker_match SET Card1ID = '$firstCardID' WHERE GameID = $matchID");
            $mysqli->query("UPDATE poker_match SET Card2ID = '$secondCardID' WHERE GameID = $matchID");
            $mysqli->query("UPDATE poker_match SET Card3ID = '$thirdCardID' WHERE GameID = $matchID");
            $mysqli->query("UPDATE poker_match SET Card4ID = '$fourthCardID' WHERE GameID = $matchID");
            $mysqli->query("UPDATE poker_match SET Card5ID = '$fifthCardID' WHERE GameID = $matchID");
            */
            

            //Set game state
            $updateMatchState = "UPDATE poker_match SET GameState = 'Ongoing' WHERE GameID = $matchID";
            if (!($result = $mysqli->query($updateMatchState)))
            showerror($mysqli->errno,$mysqli->error);

            $output = [
                'handCard1' => 0
            ];
            echo json_encode($output);
            return;

        }
        else
        {
            $output = [
                'handCard1' => 0
            ];
            echo json_encode($output);
            return;
        }
    }

    $output = [
        'handCard1' => 0
    ];
    echo json_encode($output);
}

function WaitForPlayers()
{
    global $mysqli;

    $matchID = $_GET["MatchID"];
    $currentConnectedPlayers = $_GET["connPlayers"];

    $connectedPlayersQuery = "SELECT COUNT(*) FROM poker_players WHERE MatchID = '$matchID'";
    $totalPlayersQuery = "SELECT PlayerCount FROM poker_match WHERE GameID = '$matchID'";

    $connectedPlayers = (int) $mysqli->query($connectedPlayersQuery)->fetch_assoc()["COUNT(*)"];
    $totalPlayers = (int) $mysqli->query($totalPlayersQuery)->fetch_assoc()["PlayerCount"];

    if($connectedPlayers < $totalPlayers)
    {
        //Wait longer, not all players have connected
        $output = [
            'value' => 1,
            'value2' => $connectedPlayers,
            'value3' => $totalPlayers
        ];
    }
    else if($totalPlayers < 4)
    {
        //Wait 10 seconds to let more players join
        $output = [
            'value' => 2,
            'value2' => $connectedPlayers
        ];
    }
    else
    {
        //Start in 3 seconds
        $output = [
            'value' => 3,
            'value2' => $connectedPlayers
        ];
    }
    echo json_encode($output);
}

function GetRandomNumber($_excluded)
{
    // Pick 1 random number not in $excluded, and add it to $excluded
    $validNumbers = array_diff(range(1, 52), $_excluded);
    $validNumbers = array_values($validNumbers); // reindex array

    $randomIndex = random_int(0, count($validNumbers) - 1);
    $randomNumber = $validNumbers[$randomIndex];
    return $randomNumber;
}

function QuitMatchmaking()
{
    global $mysqli;
    $userID = $_SESSION["User_ID"];

    $findMatch = "DELETE FROM Matchmaking WHERE UserID = $userID";

    if (!($result = $mysqli->query($findMatch)))
    showerror($mysqli->errno,$mysqli->error);

    //For testing purposes
    $deletMatchData = "DELETE FROM MatchData";
    if (!($result = $mysqli->query($deletMatchData)))
    showerror($mysqli->errno,$mysqli->error);

    
}

function CreateGame()
{
    global $mysqli;

}
?>