<?php
include_once "db.php";
include_once "find_match.php";
header('Content-Type: application/json');

session_start();
$sessionID = session_id();

$mysqli->query("TRUNCATE TABLE poker_players");
$mysqli->query("TRUNCATE TABLE poker_match");


session_unset();
session_destroy();

$output = [
    'success' => true,
    'message' => 'Session destroyed',
    'sessionId' => $sessionID
];

echo json_encode($output);

?>