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

// Arquivo para enviar notificações com login com o Meetup, apenas organizadores e usuários selecionados podem enviar notificações

include("../index.php");

use GDGSP\API\OneSignalAPI;
use GDGSP\Notification\WNS;
use GDGSP\Util\Utils;

if($_POST["app_key"] == Utils::getAppKey()) {
	$token = MeetupAPI::refreshMeetupToken($_POST["refresh_token"]);

	$api = new MeetupAPI($token);

	$member = $api->getMemberInfo();

	$member_id = $member->getId();

	if(strpos($http_response_header[0], "200") !== false) {
		if(MeetupAPI::checkIsAdmin($member_id)) {
			$title = $_POST["title"];
			$message = $_POST["message"];
			$image = $_POST["image"];
			$link = $_POST["link"];
			$eventid = isset($_POST["eventid"]) || $_POST["eventid"] == "" ? $_POST["eventid"] : "";

			OneSignalAPI::sendMessage($title, $message, $image, $link, $eventid);
			WNS::notifyUsersEvent($title, $message, $image, $link, $eventid);
			
			echo "notification_send";
		} else {
			echo "invalid_user";
		}
	} else {
		echo "try_again";
	}
} else {
	echo "invalid_key";
} ?>