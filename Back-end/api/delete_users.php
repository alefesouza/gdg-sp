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

/*
Cron job de atualização de usuários que desinstalaram o aplicativo
Esse cron job verifica usuários que desinstalaram o aplicativo usando a API do One Signal e atualiza eles no banco de dados para não aparecerem na seção "Pessoas com o app" dos aplicativos.
*/

$meetupids = array("GDG-SP");

foreach($meetupids as $meetupid) {
  include("../functions.php");

  $return = json_decode(getDevices());
  
  foreach($return->players as $player) {
    if($player->invalid_identifier) {
      $memberid = $player->tags->member_id;
      
      if($player->tags->member_id != "") {
			  $query = mysqli_query($dbi, "UPDATE meetup_app_members SET has_app=0 WHERE member_id=$memberid");
      }
    }
  }
}

function getDevices(){
  global $one_signal_appid;
  global $one_signal_restkey;
  
  $ch = curl_init(); 
  curl_setopt($ch, CURLOPT_URL, "https://onesignal.com/api/v1/players?app_id=".$one_signal_appid); 
  curl_setopt($ch, CURLOPT_HTTPHEADER, array('Content-Type: application/json', 
                                             'Authorization: Basic '.$one_signal_restkey)); 
  curl_setopt($ch, CURLOPT_RETURNTRANSFER, TRUE); 
  curl_setopt($ch, CURLOPT_HEADER, FALSE);
  curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, FALSE);
  $response = curl_exec($ch); 
  curl_close($ch); 
  return $response; 
}
?>