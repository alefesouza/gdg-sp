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

/*
Cron job de notificações
Coloque um cron job no seu servidor para rodar esse arquivo de minuto em minuto (ou o tempo que você quiser que cheque novos eventos e envie notificações).
Caso a API tenha um dado diferente do banco de dados, o arquivo enviará uma notificação de novo evento.
*/

include("../index.php");

use GDGSP\API\{ MeetupAPI, OneSignalAPI };
use GDGSP\Database\DB;
use GDGSP\Notification\WNS;
use GDGSP\Util\Utils;

// Supondo que estivesse suportando mais de um meetup, só incrementar esse array
$meetup_ids = array("GDG-SP");

foreach($meetup_ids as $meetup_id) {
  DB::init($meetup_id);

  $api = new MeetupAPI("");
  $result = $api->getEvents(false, 0, false);

  $events = $result->getResultAsArray();

  $last_events = $db->getLastNotifiedEvents();

  for($i = 0; $i < count($events); $i++) {
    if(!in_array($events[$i]->id, $last_events)) {
      $event = $events[$i];
    }
  }

  if(isset($event)) {
    $id = $event->id;
    $name = $event->name;
    $description = $event->description;
    
    if(isset($event->venue)) {
      $place = isset($event->venue->name) ? $event->venue->name : "";
    }

    $start = date("d/m/Y H:i", $event->time / 1000);

    $image = Utils::getImage($name, $description);

    $message = $start;

    if(isset($place)) {
      $message .= " - ".$place;
    }
  
    $db->addEventNotification($id);

    OneSignalAPI::sendNotification(count($events), $name, $message, $image, $id);

    // Já checa se tem algum token expirado na tabela dos usuários do Windows 10 e apaga
    $db->clearWnsUsers();

    WNS::notifyUsers(count($events), $name, $message, $image, $id);
  }
}
?>