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

include("../functions.php");

$eventid = $_GET["eventid"];

if($_POST["refresh_token"] != "") {
	$token = refreshMeetupToken($_POST["refresh_token"]);
	
	$json = file_get_contents("https://api.meetup.com/$meetupid/events/$eventid/rsvps?access_token=$token");
} else {
	$json = file_get_contents("http://api.meetup.com/$meetupid/events/$eventid/rsvps");
}

$query = mysqli_query($dbi, "SELECT member_id FROM meetup_app_members WHERE meetup_id='$meetupid'");

while($row = mysqli_fetch_array($query)) {
	$members_with_app[] = $row["member_id"];
}

$people = json_decode($json);

foreach($people as $person) {
  $id = $person->member->id;
  $name = $person->member->name;
  $photo = $person->member->photo->photo_link;
  $bio = $person->member->bio;
  $response = $person->response;
	
	$has_app = in_array($id, $members_with_app);
  
  $newpeople[] = array(
    "id" => (int)$id,
    "name" => $name,
    "photo" => (string)$photo,
    "intro" => (string)$bio,
		"response" => $response,
		"has_app" => (boolean)$has_app
	);
}

echo json_encode($newpeople);
?>