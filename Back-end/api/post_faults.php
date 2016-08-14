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

if($_POST["app_key"] == $app_key) {
	$memberdata = file_get_contents("https://api.meetup.com/$meetupid/members/self?key=".$_POST["api_key"]);
  
	if(strpos($http_response_header[0], "200") !== false) {
		$member = json_decode($memberdata);

		$memberid = $member->id;
		
		$isadmin = checkIsAdmin($memberid);
		
    if($isadmin) {
      $json = json_decode($_POST["json"]);
			
      foreach($json as $person) {
				$personid = $person->id;
				$personname = mysqli_real_escape_string($dbi, $person->name);
				
				$query = mysqli_query($dbi, "SELECT * FROM meetup_app_members WHERE member_id=$personid");
			
				if(mysqli_num_rows($query) == 0) {
					mysqli_query($dbi, "INSERT INTO meetup_app_members (meetup_id, member_id, member_name, has_app, last_activity, faults) VALUES ('$meetupid', $personid, '$personname', 0, 0, 1)");
				} else {
					mysqli_query($dbi, "UPDATE meetup_app_members SET faults=(faults + 1) WHERE meetup_id='$meetupid' AND member_id=".$personid);
				}
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