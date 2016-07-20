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

foreach($meetupids as $meetupid) {
  include("../functions.php");
  include("wns/wns.php");

  $json = file_get_contents("http://api.meetup.com/$meetupid/events");

  $events = json_decode($json);

  // O evento com maior id é o último anunciado, isso é PHP, não reclame dessa gambiarra huehaheu
  $lastid = 0;
  for($i = 0; $i < count($events); $i++) {
    if($events[$i]->id > $lastid) {
      $lastid = $events[$i]->id;
      $lastevent = $i;
    }
  }
  
  $event = $events[$lastevent];

  // Se o evento for diferente de null
  if($event != null) {
    // E o ID dele for maior que o que estava no banco de dados, significa que é um novo evento, então enviará a notificação
    if($event->id > $last_event) {
      $id = $event->id;
      $name = $event->name;
      $description = $event->description;
      $place = $event->venue->name;
      $start = date("d/m/Y H:i", $event->time / 1000);

      $message = $start;
      if($place != "") {
        $message .= " ".$place;
      }

      // Após isso atualiza o banco de dados com esse ID sendo o último anunciado, quando surgir um evento com ID maior que esse vai refazer tudo isso
      mysqli_query($dbi, "UPDATE meetups SET last_event=$id WHERE meetupid='$meetupid'");
      
      sendMessageOneSignal(count($events), $name, $message, getImage($name, $description), $id);
      
      // Já checa se tem algum token expirado na tabela dos usuários do Windows 10 e apaga
      $checkExpire = date("Ymd", time());
      mysqli_query($dbi, "DELETE FROM meetup_wns_users WHERE expire < $checkExpire");

      notify_wns_users(count($events), $name, $message, getImage($name, $description), $id);
    }
    
    // Para testes
    //mysqli_query($dbi, "UPDATE meetups SET last_event=0 WHERE meetupid='$meetupid'");
  }
}
?>