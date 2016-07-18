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

include("../../functions.php");

$json = file_get_contents("https://api.meetup.com/$meetupid/events");
$events = json_decode($json);

$event = $events[$_GET["tile"]];

if($event != null) {
  $name = $event->name;
	$description = $event->description;
  $place = $event->venue->name;
  $address = $event->venue->address_1;
  $start = date("d/m/Y H:i", $event->time / 1000);
	$end = date("H:i", ($event->time + $event->duration) / 1000);
	$end = strpos($start, $end) !== false ? "" : " - $end";
	$image = getImage($name, $description);
	
  $tile = new SimpleXMLElement('<tile>
  <visual lang="pt-BR" version="2">
  <binding template="TileSquare150x150PeekImageAndText01" fallback="TileSquarePeekImageAndText01">
  <image id="1" src="'.$image.'" alt="alt text"/>
  <text id="1">'.$name.'</text>
  <text id="2">'.$place.'</text>
  <text id="3">'.date("d/m/Y", $event->time / 1000).'</text>
  <text id="4">'.date("H:i", $event->time / 1000).$end.'</text>
  </binding> 
  <binding template="TileWide310x150PeekImage02" fallback="TileWidePeekImage02" branding="name'.$wp.'">
  <image id="1" src="'.$image.'" alt="alt text"/>
  <text id="1">'.$name.'</text>
  <text id="2">'.$place.'</text>
  <text id="3">'.$event->venue->address_1.'</text>
  <text id="4">'.date("d/m/Y", $event->time / 1000).' ~ '.date("H:i", $event->time / 1000).$end.'</text>
  <text id="5"> </text>
  </binding>
  <binding template="TileSquare310x310BlockAndText01">
  <text id="1">'.$name.'</text>
  <text id="2">'.$place.'</text>
  <text id="3">'.$event->venue->address_1.'</text>
  <text id="4">'.date("H:i", $event->time / 1000).$end.'</text>
  <text id="5"> </text>
  <text id="6"> </text>
  <text id="7"> </text>
  <text id="8">'.date("d", $event->time / 1000).'</text>
  <text id="9">'.date("m/Y", $event->time / 1000).'</text>
  </binding>
  </visual>
  </tile>');

  Header('Content-type: text/xml');
  print($tile->asXML());
}
?>