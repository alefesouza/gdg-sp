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

$location = strtok("http://".$_SERVER['SERVER_NAME'].$_SERVER['REQUEST_URI'],'?');

if(isset($_POST["code"])) {
  $url = 'https://secure.meetup.com/oauth2/access';
  
  $code = $_POST["code"];
  $query = "client_id=$meetup_client_key&client_secret=$meetup_secret_key&grant_type=authorization_code&redirect_uri=$location?meetupid=$meetupid&code=$code";

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
  
	$token = refreshMeetupToken($json->refresh_token);
	
	$ismember = @file_get_contents("https://api.meetup.com/$meetupid?access_token=$token&fields=self");
  $json2 = json_decode($ismember);
  
  $status = $json2->self->status;
  
	if(strpos($http_response_header[0], "200") !== false) {
		if($status != "active") {
			echo json_encode(array("is_error" => true, "description" => $status));
		} else {
			$memberdata = @file_get_contents("https://api.meetup.com/$meetupid/members/self?access_token=$token");
			$memberjson = json_decode($memberdata);
			$memberid = $memberjson->id;

			$query = mysqli_query($dbi, "SELECT * FROM meetup_app_members WHERE member_id=$memberid");

			if(mysqli_num_rows($query) == 0) {
				$last_activity = date("Ymd", time());
				mysqli_query($dbi, "INSERT INTO meetup_app_members (member_id, last_activity) VALUES ($memberid, $last_activity)");
			}
			
			$qr = file_get_contents("https://api.qrserver.com/v1/create-qr-code/?size=500x500&data=".$qrcode.$memberid);
			
			echo json_encode(array("is_error" => false, "refresh_token" => $json->refresh_token, "qr_code" => base64_encode($qr)));
		}
	} else {
		echo json_encode(array("is_error" => true, "description" => "error"));
	}
} else if(isset($_GET["code"])) {
} else {
  header("location:https://secure.meetup.com/oauth2/authorize?client_id=$meetup_client_key&response_type=code&redirect_uri=$location?meetupid=$meetupid&scope=rsvp+event_management");
}
?>