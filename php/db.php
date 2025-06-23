<?php
 $db_user = 'pepijnluchtmeije';
 $db_pass = 'aePi7ehuShoh';
 $db_host = 'localhost';
 $db_name = 'pepijnluchtmeije';

/* Open a connection */
$mysqli = new mysqli("$db_host","$db_user","$db_pass","$db_name");

/* check connection */
if ($mysqli->connect_errno) {
 echo "Failed to connect to MySQL: (" . $mysqli->connect_errno . ") " . $mysqli->connect_error;
 exit();

}
function showerror($error,$errornr) {
   die("Error (" . $errornr . ") " . $error);
   }

function GetJsonFromQuery($_query, $_singleResult, $name)
{
  global $mysqli;
  $query = $_query;
  $output = [];

  if (!($result = $mysqli->query($query)))
  showerror($mysqli->errno,$mysqli->error);

  if(!$_singleResult)
  {

    $rows = $result->fetch_all(MYSQLI_ASSOC);
    foreach ($rows as $row) {
    $output[] = $row;
    }
  }
  else
  {
    $highScore = $result->fetch_assoc();
    $output[] = $highScore;
  }
  
  if($output) 
  {
   return array($name => $output);
  }
}
?>