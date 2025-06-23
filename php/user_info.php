<?php
include_once "db.php";
include_once "users.php";

header('Content-Type: application/json');
session_start();

$user = GetUserFromID($_SESSION["User_ID"]);
            $output = [
                'userID' => $user["ID"],
                'Username' => $user["Username"],
                'Email' => $user["Email"],
                'Country' => $user["Country"],
                'DateOfBirth' => date("j F Y", strtotime($user['DateOfBirth']))
                ];

echo json_encode($output);

?>