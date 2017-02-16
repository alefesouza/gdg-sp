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

namespace GDGSP\Windows {
    use GDGSP\API\MeetupAPI;

    class Tile {
        public function getTile(int $pos) : string {
            $api = new MeetupAPI("");
            $result = $api->getEvents(false, 0, false);
          
            $events = $result->getResultAsArray();

            $event = $events[$pos];

            if($_GET["platform"] != "wp") {
                $wp = "AndLogo";
            } else {
                $wp = "";
            }

            if($event != null) {
                $name = $event->name;
                $description = $event->description;

                if(isset($event->venue)) {
                    $place = $event->venue->name ?? "";
                    $address = $event->venue->address_1 ?? "";
                } else {
                    $place = "";
                    $address = "";
                }

                $start = date("d/m/Y H:i", $event->time / 1000);
                $start_day = date("d/m/Y", $event->time / 1000);
                $start_hour = date("H:i", $event->time / 1000);

                if(isset($event->duration)) {
                    $end = date("H:i", ($event->time + $event->duration) / 1000);

                    $end = $start_hour != $end ? "" : " - $end";
                } else {
                    $end = "";
                }
                
                ob_start(); ?>
<?xml version="1.0" encoding="utf-8"?>
<tile>
    <visual lang="pt-BR" version="2">
        <binding template="TileSquare150x150Text03" fallback="TileSquareText03" branding="name<?php echo $wp; ?>">
            <text id="1"><?php echo $name; ?></text>
            <text id="2"><?php echo $place; ?></text>
            <text id="3"><?php echo $start_hour; ?></text>
            <text id="4"><?php echo $start_hour.$end; ?></text>
        </binding> 
        <binding template="TileWide310x150Text05" fallback="TileWideText05" branding="name<?php echo $wp; ?>">
            <text id="1"><?php echo $name; ?></text>
            <text id="2"><?php echo $place; ?></text>
            <text id="3"><?php echo $address; ?></text>
            <text id="4"><?php echo $start_day.' ~ '.$start_hour.$end; ?></text>
            <text id="5"> </text>
        </binding>
        <binding template="TileSquare310x310BlockAndText01" branding="name<?php echo $wp; ?>">
            <text id="1"><?php echo $name; ?></text>
            <text id="2"><?php echo $place; ?></text>
            <text id="3"><?php echo $address; ?></text>
            <text id="4"><?php echo $start_hour.$end; ?></text>
            <text id="5"> </text>
            <text id="6"> </text>
            <text id="7"> </text>
            <text id="8"><?php echo date("d", $event->time / 1000); ?></text>
            <text id="9"><?php echo date("m/Y", $event->time / 1000); ?></text>
        </binding>
    </visual>
</tile>
                <?php $content = ob_get_clean();

                $tile = new \SimpleXMLElement($content);
              
                header('Content-type: text/xml');
            } else {
                return "";
            }

            return $tile->asXML();
        }
    }
}
?>