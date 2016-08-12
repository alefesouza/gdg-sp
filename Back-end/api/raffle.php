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
  if($_POST["refresh_token"] != "") {
    $eventid = $_GET["eventid"];

    $token = refreshMeetupToken($_POST["refresh_token"]);

    $memberdata = file_get_contents("https://api.meetup.com/$meetupid/members/self?access_token=$token");

    $member = json_decode($memberdata);

    $memberid = mysqli_real_escape_string($dbi, $member->id);
    
    if(isset($_POST["manager"])) {
      $isadmin = checkIsAdmin($memberid, $token);

      if($isadmin) {
        if($_POST["empty"] == "true") {
          $query = mysqli_query($dbi, "DELETE FROM meetup_raffle_manager WHERE event_id=$eventid");
        } else {
          $query = mysqli_query($dbi, "SELECT *, DATE_FORMAT(raffle_date,'%H:%i:%s %d/%m/%Y') AS raffle_date_format, DATE_FORMAT(post_date,'%H:%i:%s %d/%m/%Y') AS post_date_format FROM meetup_raffle_manager WHERE meetup_id='$meetupid' AND event_id=$eventid ORDER BY raffle_date");
          
          $raffle_people = array();
          
          while($row = mysqli_fetch_array($query)) {
            $raffle_people[] = array("id" => $row["member_id"], "name" => $row["member_name"], "photo" => $row["member_photo"], "raffle_date" => $row["raffle_date_format"], "post_date" => $row["post_date_format"], "seconds" => (string)$row["seconds"]);
          }

          echo json_encode($raffle_people);
        }
      } else {
        echo "invalid_user";
      }
    } else {
      $membername = mysqli_real_escape_string($dbi, $member->name);
      $memberphoto = mysqli_real_escape_string($dbi, $member->photo->photo_link);

      $raffle_date = $_POST["raffle_date"];
      $seconds = $_POST["seconds"];

      // Notei que meu servidor estava 14 segundos adiantado
      $add_seconds = 14;
      
      mysqli_query($dbi, "INSERT INTO meetup_raffle_manager (event_id, member_id, member_name, member_photo, meetup_id, raffle_date, post_date, seconds) VALUES ($eventid, $memberid, '$membername', '$memberphoto', '$meetupid', '$raffle_date', DATE_ADD(NOW(), INTERVAL $add_seconds SECOND), $seconds)") or die ("Error: ".mysqli_error($dbi));

      echo "success";
    }
  }
} else {
  echo "invalid_key";
}
?>