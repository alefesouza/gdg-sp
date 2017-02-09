<?php
/*
 * Copyright (C) 2017 Alefe Souza <contact@alefesouza.com>
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
include("../index.php");

use GDGSP\API\MeetupAPI;

$event_id = $_GET["eventid"];

$token = MeetupAPI::refreshMeetupToken($_POST["refresh_token"]);

$api = new MeetupAPI($token);

$json = $api->doRSVP($event_id, $_POST["json"]);

$response = $json->getResult()->response;

switch($response) {
  case "yes": case "waitlist": case "no":
    echo $response;
    break;
  default:
    http_response_code(401);
}
?>