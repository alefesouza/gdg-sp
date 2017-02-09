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
use GDGSP\Utils\Util;

//if($_POST["app_key"] == Utils::getAppKey()) {
  if(isset($_GET["refresh_token"]) && $_GET["refresh_token"] != "") {
    $event_id = $_GET["eventid"];

    $token = MeetupAPI::refreshMeetupToken($_GET["refresh_token"]);

    $api = new MeetupAPI($token);

    $member = $api->getMemberInfo();

    $member_id = $member->getId();
    
    if(isset($_GET["manager"])) {
      $is_admin = MeetupAPI::checkIsAdmin($member_id);

      if($is_admin) {
        if(isset($_GET["empty"]) && $_GET["empty"] == "true") {
          $db->clearRaffle($event_id);
        } else {
          $raffle_people = $db->getAllRaffle($event_id);

          echo json_encode($raffle_people);
        }
      } else {
        echo "invalid_user";
      }
    } else {
      $raffle_date = $_POST["raffle_date"];
      $seconds = $_POST["seconds"];

      if($db->addRaffle($member, $event_id, $raffle_date, $seconds)) {
        echo "success";
      } else {
        echo "error";
      }
    }
  }
// } else {
//   echo "invalid_key";
// }
?>