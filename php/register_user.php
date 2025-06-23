<?php
include_once "db.php";

header('Content-Type: application/json');
RegisterUser();

function RegisterUser()
{
    $username = $_GET["User"];
    $email = $_GET["Email"];
    $pass = $_GET["Pass"];
    $country = $_GET["Country"];

    if(ValidateCredentials($username, $email))
    {
        InsertUser($username, $email, $pass, $country, $dob);
        $output = [
            'success' => true,
            'message' => 'User registered'
        ];
        echo json_encode($output);
    }
}

function ValidateCredentials($username, $email)
{
    //Check if username is unique
    $user = GetUserByUsername($username);
    if($user) 
    {
        $output = [
            'success' => false,
            'message' => 'Username already in use'
        ];
        echo json_encode($output);
        return false;
    }

    //Check if email is unique
    $user = GetUserByUsername($email);
    if($user) 
    {
        $output = [
            'success' => false,
            'message' => 'Email already in use'
        ];
        echo json_encode($output);
        return false;
    }

    return true;
}

function InsertUser($username, $email, $pass, $country, $dob)
{
    global $mysqli;
    $phpTimeStamp = (strtotime($dob));
    $insertUser = "INSERT INTO Users(Username, Email, Pass, Country) VALUES ('$username','$email', MD5('$pass'), '$country')"; 

    if (!($result = $mysqli->query($insertUser)))
    showerror($mysqli->errno,$mysqli->error);
}

function GetUserByUsername($_username)
{
    global $mysqli;
    $user = "SELECT * FROM Users WHERE Username = '$_username' OR Email = '$_username'";
    if (!($result = $mysqli->query($user)))
    showerror($mysqli->errno,$mysqli->error);

    $user = $result->fetch_assoc();
    return $user;
}

?>