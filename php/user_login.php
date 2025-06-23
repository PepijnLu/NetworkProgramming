<?php
include_once "db.php";
include_once "users.php";

StartUserSession();
header('Content-Type: application/json');
function StartUserSession()
{
    session_start();

    if(SessionExists())
    {
        //echo "<br>Session Found";

        // if(UserConnectedToSession())
        // {
        //     //Continue
        //     $hashed = $_GET["hashed"] ?? 1;
        //     $user = GetUserFromID($_SESSION["User_ID"]);
        //     if($hashed == 0)
        //     {
        //         $output = [
        //         'ID' => $user["ID"],
        //         'Username' => $user["Username"],
        //         'Email' => $user["Email"],
        //         'Country' => $user["Country"],
        //         'Date of Birth' => $user['DateOfBirth'],
        //         'SessionID' => session_id()
        //         ];
        //     }
        //     else
        //     {
        //         $output = [
        //             'success' => true,
        //             'message'=> 'Already logged in',
        //             'sessionId' => session_id()
        //         ];
        //     }
        //     echo json_encode($output);
        // }
        //else
        //{
            //Login Again
            LoginUser();
        //}
    }
    else
    {
        $output = [
            'success' => false,
            'message' => 'Session not found'
        ];
        echo json_encode($output);
    }
}

function LoginUser()
{
    $username = $_GET["User"];
    $pass = $_GET["pass"];
    //Hashed on the unity side, so manually input that its not hashed when logging in from web
    $hashed = $_GET["hashed"] ?? 1;

    $user = GetUserByUsername($username);

    if(ValidateUserLogin($user, $pass, $hashed))
    {
        if($hashed == 0)
        {
            $output = [
            'ID' => $user["ID"],
            'Username' => $user["Username"],
            'Email' => $user["Email"],
            'Country' => $user["Country"],
            'Date of Birth' => $user['DateOfBirth'],
            'SessionID' => session_id()
            ];
        }
        else
        {
            $output = [
                'success' => true,
                'message'=> 'Login success',
                'sessionId' => session_id()
            ];
        }

        $_SESSION["User_ID"] = $user["ID"];
        echo json_encode($output);
    }

    
}

function ValidateUserLogin($user, $pass, $hashed)
{
    if (!$user) 
    {
        $output = [
            'success' => false,
            'message' => 'User not found'
        ];
        echo json_encode($output);
        return false;
    }
    else
    {
        if($hashed == 0)
        {
            $pass = md5($pass);
        }
        $userPassword = $user["Pass"];

        if ($pass === $userPassword)
        {
            // $output = [
            //     'success' => true,
            //     'message' => 'Login successful'
            // ];
            // echo json_encode($output);
            return true;
        }
        else
        {
            $output = [
                'success' => false,
                'message' => 'Incorrect password'
            ];
            echo json_encode($output);
            return false;
        }
    }

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

function SessionExists()
{
    if (isset($_SESSION["Server_ID"])) 
    {
        return true;
    }   
    else
    {
        return false;
    }
}

function UserConnectedToSession()
{
    if (isset($_SESSION["User_ID"])) 
    {
        return true;
    }   
    else
    {
        return false;
    }
}

?>