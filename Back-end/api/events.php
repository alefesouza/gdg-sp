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
use GDGSP\Database\DB;
use GDGSP\Util\Utils;

$page = $_GET["page"] ?: 1;

if(isset($_POST["refresh_token"]) && $_POST["refresh_token"] != "") {
	$token = MeetupAPI::refreshMeetupToken($_POST["refresh_token"]);

	$api = new MeetupAPI($token);
	
	$past_events = $api->getEvents(true, $page);
	
	if(!isset($_GET["page"])) {
		$events = $api->getEvents();
	}
	
	if($past_events->getHttpCode() == 200) {
		$member = $api->getMemberInfo();
		
		$is_admin = MeetupAPI::checkIsAdmin($member->getId());
		$member->setIsAdmin($is_admin);
		
		$db->updateActivity($member->getId());

		$platform = Utils::$platform;
		
		if(($platform == "windows" || $platform == "windows81") && $_POST["ChannelUri"] != "") {
			$db->updateWindowsDevice($member->getId(), $_POST["ChannelUri"]);
		}
	} else {
		set_not_logged_structure();
	}
} else {
	set_not_logged_structure();
}

function set_not_logged_structure() {
	global $page;
	
	$GLOBALS["api"] = new MeetupAPI("");

	$GLOBALS["past_events"] = $GLOBALS['api']->getEvents(true, $page);
	
	if(!isset($_GET["page"])) {
		$GLOBALS["events"] = $GLOBALS['api']->getEvents();
	}

	$GLOBALS["member"] = array(
		"id" => 0,
		"name" => "",
		"photo" => "",
		"intro" => "",
		"is_admin" => false
	);
}

$new_json = array(
	"member" => $member,
	"header" => (string)DB::$headerImage,
	"more_past_events" => MeetupAPI::$more_past_events,
	"past_events" => $past_events->getResultAsArray()
);

if(!isset($_GET["page"])) {
	$new_json["events"] = $events->getResultAsArray();
}

echo json_encode($new_json);
?>