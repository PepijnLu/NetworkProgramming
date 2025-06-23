<?php
function GetUserFromID(int $userID) 
{
    global $mysqli;
    $query = "SELECT * FROM Users WHERE ID = $userID";

    if (!($result = $mysqli->query($query)))
    showerror($mysqli->errno,$mysqli->error);

    $row = $result->fetch_assoc();
    return $row;
}
?>