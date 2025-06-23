<?php
include_once "db.php";
include_once "user_login.php";
include_once "users.php";
    
ChangeCredentials();

function ChangeCredentials()
{
    if(UserConnectedToSession())
    {
        $newUsername = $_GET["User"] ?? "";
        $newPassword = $_GET["Pass"] ?? "";
        $newEmail = $_GET["Email"] ?? "";
        $newCountry = $_GET["Country"] ?? "";
        $newDoB = $_GET["DoB"] ?? "";

        $user = GetUserFromID($_SESSION["User_ID"]);

        if($newUsername != "" && $newUsername != $user["Username"]) 
        {
            UpdateUserInfo($_SESSION["User_ID"], "Username", $newUsername);
            echo "<br>Changed Username";
        }
        if($newPassword != "" && md5($newPassword) != $user["Pass"]) 
        {
            UpdateUserInfo($_SESSION["User_ID"], "Pass", md5($newPassword));
            echo "<br>Changed Pass";
        }
        else
        {
            echo "Pass the same: ". $newPassword. " vs " . $user["Pass"];
        }
        if($newEmail != "" && $newEmail != $user["Email"]) 
        {
            UpdateUserInfo($_SESSION["User_ID"], "Email", $newEmail);
            echo "<br>Changed Email";
        }
        if($newCountry != "" && $newCountry != $user["Country"]) 
        {
            UpdateUserInfo($_SESSION["User_ID"], "Country", $newCountry);
            echo "<br>Changed Country";
        }   
        
        if($newDoB != "" && strtotime($newDoB) != strtotime(strval($user["DateOfBirth"])))
        {
            $phpTimeStamp = (strtotime($newDoB));
            UpdateUserInfo($_SESSION["User_ID"], "DateOfBirth", $phpTimeStamp);
            echo "<br>Changed Date of Birth";
        }  
        
    }   
    else
    {
        echo "login first";
    }
}

function UpdateUserInfo($userID, $dataEntry, $value)
{
    global $mysqli;

    if ($dataEntry === "DateOfBirth") {
        $updateUserInfo = "UPDATE `Users` SET `$dataEntry` = FROM_UNIXTIME('$value') WHERE ID = '$userID'";
    } else {
        $updateUserInfo = "UPDATE `Users` SET `$dataEntry` = '$value' WHERE ID = '$userID'";
    }

    if (!($result = $mysqli->query($updateUserInfo))) {
        showerror($mysqli->errno, $mysqli->error);
    }

    echo "<br>Updated ".$dataEntry;
}

?>