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

// Arquivo para enviar notificações com login com o Meetup, apenas organizadores e usuários selecionados podem enviar notificações

include("../functions.php");

$token = refreshMeetupToken($_POST["refresh_token"]);

$memberdata = @file_get_contents("https://api.meetup.com/members/self?access_token=$token");
$member = json_decode($memberdata);

$memberid = $member->id;

if(strpos($http_response_header[0], "200") !== false) {
	if(checkIsAdmin($memberid, $token)) {
		echo "notification_send";
		
		include("wns/wns.php");

		$title = $_POST["title"];
		$message = $_POST["message"];
		$image = $_POST["image"];
		$link = $_POST["link"];
		$eventid = isset($_POST["eventid"]) || $_POST["eventid"] == "" ? $_POST["eventid"] : "";
		
		sendMessageOneSignal2($title, $message, $image, $link, $eventid);
		notify_wns_users2($title, $message, $image, $link, $eventid);
	} else {
		echo "invalid_user";
	}
} else {
	echo "try_again";
} ?>