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

if($_POST["app_key"] == Utils::getAppKey()) {
	$api_key = $_POST["api_key"];

	$member_data = file_get_contents("https://api.meetup.com/".DB::$meetupId."/members/self?key=".$api_key);
  
	if(strpos($http_response_header[0], "200") !== false) {
		$member = json_decode($member_data);

		$memberId = $member->id;
		
		$is_admin = MeetupAPI::checkIsAdmin($memberId, $api_key);
		
    if($is_admin) {
      $json = json_decode($_POST["json"]);
			
      foreach($json as $member) {
				$member_id = $member->id;
				$member_name = $member->name;

				DB::getInstance()->manageMeetupMember($member_id, $member_name);
      }
      
      echo "success";
    } else {
      echo "invalid_user";
    }
	} else {
    echo "error";
  }
} else {
  echo "invalid_key";
}
?>