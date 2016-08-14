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
      $query = mysqli_query($dbi, "SELECT * FROM meetup_app_members WHERE meetup_id='$meetupid' AND faults > 0 ORDER BY faults DESC");

      $people = array();

      while($row = mysqli_fetch_array($query)) {
        $people[] = array("id" => (int)$row["member_id"], "name" => (string)$row["member_name"], "faults" => (int)$row["faults"]);
      }

      echo json_encode($people);
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