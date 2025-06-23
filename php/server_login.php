<?php
include "db.php";

header('Content-Type: application/json');
StartServerSession();
function StartServerSession()
{
    $serverID = $_GET["Server_ID"];
    $serverPass = $_GET["Server_pass"];

    //Hashed on the unity side, so manually input that its not hashed when logging in from web
    $hashedPass = $_GET["hashed"] ?? 1;

    $server = GetServer($serverID);

    if(!$server)
    {
        AddServerToTable($serverID, $serverPass);
    }

    $result = ValidateServerLogin( $serverID, $serverPass, $hashedPass);
    echo json_encode($result);
}

function GetServer($serverID)
{
    global $mysqli;
    $getServer = "SELECT * FROM Servers WHERE ID = $serverID";
    if (!($result = $mysqli->query($getServer)))
    showerror($mysqli->errno,$mysqli->error);
    $row = $result->fetch_assoc();
    return $row;
}

function AddServerToTable($serverID, $serverPass)
{
    global $mysqli;
    echo "<br>Adding Server to Table: ". $serverID. ", ". $serverPass;
    $addServer = "INSERT INTO Servers(ID, Pass) VALUES ($serverID, MD5('$serverPass'))"; 
    
    if (!($result = $mysqli->query($addServer)))
    showerror($mysqli->errno,$mysqli->error);
}

function ValidateServerLogin($serverID, $serverPass, $hashedPass)
{
    $server = GetServer($serverID);

    if($hashedPass == 0)
    {
        $serverPass = md5($serverPass);
    }

    if ($server)
    {
        if ($server["Pass"] === $serverPass)
        {
            session_start();
            $deleteOldSession = $_GET["delOld"] ?? false;
            if($deleteOldSession)
            {
                session_regenerate_id($deleteOldSession);
            }

            if (!isset($_SESSION["Server_ID"])) 
            {
                $_SESSION["Server_ID"] = $serverID;
            }

            $output = [
                'success' => true,
                'message' => 'Login successful',
                'sessionId' => session_id()
            ];
        }
        else
        {
            $output = [
                'success' => false,
                'message' => 'Incorrect password'
            ];
        }
    }
    else
    {
        $output = [
            'success' => false,
            'message' => 'Server ID not found'
        ];
    }

    return $output;
}
?>