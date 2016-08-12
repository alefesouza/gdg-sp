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
Cron job de notificações
Coloque um cron job no seu servidor para rodar esse arquivo de minuto em minuto (ou o tempo que você quiser que cheque novos eventos e envie notificações).
Caso a API tenha um dado diferente do banco de dados, o arquivo enviará uma notificação de novo evento.
*/

// Supondo que estivesse suportando mais de um meetup, só incrementar esse array
$meetupids = array("GDG-SP");

include("../connect_db.php");
include("wns/wns.php");

foreach($meetupids as $meetupid) {
  include("../functions.php");

  $json = file_get_contents("http://api.meetup.com/$meetupid/events");

  $events = json_decode($json);
  
  $query = mysqli_query($dbi, "SELECT event_id FROM meetup_last_events WHERE meetup_id='$meetupid'");

  while($row = mysqli_fetch_array($query)) {
    $last_events[] = $row["event_id"];
  }

  for($i = 0; $i < count($events); $i++) {
    if(!in_array($events[$i]->id, $last_events)) {
      $event = $events[$i];
    }
  }

  // Se o evento for diferente de null
  if($event != null) {
    $id = $event->id;
    $name = $event->name;
    $description = $event->description;
    $place = $event->venue->name;
    $start = date("d/m/Y H:i", $event->time / 1000);

    $message = $start;
    if($place != "") {
      $message .= " ".$place;
    }
  
    mysqli_query($dbi, "INSERT INTO meetup_last_events (meetup_id, event_id) VALUES ('$meetupid', $id)");

    sendMessageOneSignal(count($events), $name, $message, getImage($name, $description), $id);

    // Já checa se tem algum token expirado na tabela dos usuários do Windows 10 e apaga
    $checkExpire = date("Ymd", time());
    mysqli_query($dbi, "DELETE FROM meetup_wns_users WHERE expire < $checkExpire");

    notify_wns_users(count($events), $name, $message, getImage($name, $description), $id);
  }
}
?>