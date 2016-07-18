<?php
/*
 * Copyright (C) 2016 Alefe Souza <http://alefesouza.com>
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

// Pelos aplicativos, o usuário pode dar um post aqui com a resposta dele se irá e as perguntas respondidas após ter feito login
include("../functions.php");

$eventid = $_GET["eventid"];
$token = refreshMeetupToken($_POST["refresh_token"]);

$json = json_decode($_POST["json"]);
$response = $json->response;

$query = "event_id=$eventid&rsvp=$response&access_token=$token";

foreach($json->answers as $answer) {
  $query .= "&answer_".$answer->id."=".urlencode($answer->answer);
}

$query = str_replace("+", "%20", $query);

$url = "https://api.meetup.com/2/rsvp/";

$headers = array(
    'Content-Type: application/x-www-form-urlencoded'
);

$ch = curl_init();

curl_setopt($ch, CURLOPT_URL, $url);

curl_setopt($ch, CURLOPT_POST, true);
curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);

curl_setopt($ch, CURLOPT_POSTFIELDS, $query);

$result = curl_exec($ch);

$json = json_decode($result);

//Para testes
// $myfile = fopen("teste.json", "w") or die("Unable to open file!");
// $txt = $result." ".$json->response;
// fwrite($myfile, $txt);
// fclose($myfile);

switch($json->response) {
  case "yes": case "waitlist": case "no":
    echo $json->response;
    break;
  default:
    http_response_code(401);
}
?>