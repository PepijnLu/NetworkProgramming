<?php
include "db.php";
include "users.php";

if (isset($_GET['PHPSESSID'])) 
{ //is the session id in the url?
    $sid=htmlspecialchars($_GET['PHPSESSID']); //sanitize session id from url
    session_id($sid); //set session id for this session to what came from url
    echo "<br>set session ID";
}
else
{
    echo "<br>session id not found in url";
}
  
session_start();
  
if (isset($_SESSION["Server_ID"]) && $_SESSION["Server_ID"]!=0) 
{
    //perform score insert
    echo "<br>session id found in session";
    echo "<br>perform score insert";
    AddScoreToDatabase($_GET["Score"], $_SESSION["User_ID"], $_GET["GameID"] ?? 1, $_SESSION["Server_ID"]);
} 
else 
{
    echo "<br>0"; //(error to unity server that it needs to log in again)
}


    
//echo $_GET["ID"];


//Expecting one result
/*
$row = $result->fetch_assoc(); //get info from "knitting"

echo json_encode($row); //show json
*/

//Expecting multiple results
/*
$my_json = "{\"users\":["; //create variable that will contain the entire json and initial padding of json
$row = $result->fetch_assoc(); //get first row from "knit"

do { //begin loop to get all rows from result
  $my_json .= json_encode($row); //convert row to json and add to variable
} while ($row = $result->fetch_assoc()); //end loop

$my_json .= "]}"; //closing json
echo $my_json; //show json
*/

function AddScoreToDatabase(int $_score, int $_userID, int $_gameID = 1, int $_serverID): void
{
    global $mysqli;
    
    $query = "INSERT INTO Scores(Score, UserID, ScoredAt, GameID, ServerID) VALUES ($_score, $_userID, FROM_UNIXTIME(UNIX_TIMESTAMP()), $_gameID, $_serverID)"; //query
    $check = 0;

    $user = GetUserFromID($_userID);
    $username = $user["Username"];

    if($_score == 0) $check = 1;
    if($username == null) $check = 1;
    if($_gameID <= 0) $check = 1;

    if($check == 1) 
    {
        echo "error"; 
        //See query errors
        print_r(mysqli_error_list($mysqli));
        return;
    }

    if (!($result = $mysqli->query($query))) // apply query
    showerror($mysqli->errno,$mysqli->error); // if apply fails show error

    //header("Location: index.php");
}

function AddUserToDatabase(string $_username, string $_email, string $_pass, string $_country): void
{
    global $mysqli;

    if(strlen($_username) <= 3) $check = 1;
    if($_email == "" || !str_contains($_email, "@")) $check = 1;
    if(strlen($_pass) <= 7) $check = 1;
    $check = 0;

    if($check == 1) 
    {
        echo "error"; 
        return;
    }

    $_email = filter_var($_email, FILTER_SANITIZE_EMAIL);

    $query = "INSERT INTO Users(Username, Email, Pass, Country) VALUES ($_username, $_email, MD5($_pass), $_country)"; //query



    if (!($result = $mysqli->query($query))) // apply query
    showerror($mysqli->errno,$mysqli->error); // if apply fails show error
}

?>