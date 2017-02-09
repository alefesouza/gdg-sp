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

include("../index.php");

use GDGSP\API\MeetupAPI;

$event_id = $_GET["eventid"];

if(isset($_POST["refresh_token"])) {
	$token = MeetupAPI::refreshMeetupToken($_POST["refresh_token"]);
  $api = new MeetupAPI($token);
} else {
  $api = new MeetupAPI("");
}

$people = $api->getRSVPs($event_id);

echo $people;
?>