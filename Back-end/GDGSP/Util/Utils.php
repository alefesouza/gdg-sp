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

namespace GDGSP\Util {
    use GDGSP\Database\DB;
    use GDGSP\Model\Event;

    class Utils {
        const qrCode = "http://gdgsp.org/qrcode/";
        const mapBoxToken = "";

        public static $platform;
        public static $via;

        // Caminho do arquivo atual
        public static function getLocation() {
            return "http://".dirname($_SERVER['SERVER_NAME'].$_SERVER['PHP_SELF']);
        }

        public static function getAppKey() {
            return md5("");
        }

        public static function setPlatform($platform) {
            self::$platform = $platform;
        }

        public static function getImage($name, $description) {
            preg_match_all('~<img.*?src=["\']+(.*?)["\']+~', $description, $images);
            $image = $images[1][0] ?? "";
            if($image == "") {
                return self::getImage2($name);
            } else {
                return $image;
            }
        }

        // Retorna imagem baseada na trilha caso a primeira função não encontre uma imagem
        public static function getImage2($name) {
            global $location;
            global $header_image;

            $trilhas = array(
                array(
                    "name" => "html5 study group",
                    "image" => "html5_study_group.jpg"
                ),
                array(
                    "name" => "html5sg",
                    "image" => "html5_study_group.jpg"
                ),
                array(
                    "name" => "android meetup",
                    "image" => "android_meetup.jpg"
                ),
                array(
                    "name" => "women techmakers",
                    "image" => "women_techmakers.jpg"
                ),
                array(
                    "name" => "android studyjams",
                    "image" => "android_studyjams.jpg"
                ),
                array(
                    "name" => "coding dojo",
                    "image" => "coding_dojo.jpg"
                )
            );

            foreach($trilhas as $trilha) {
                if(stripos($name, $trilha["name"]) !== false) {
                    return str_replace("api", "images", $location)."/".$trilha["image"];
                }
            }

            return DB::$headerImage;
        }

        public static function generateEvents(array $events, bool $isPast = false) {
            $new_events = array();

            foreach($events as $event) {
                $id = $event->id;

                $name = $event->name;
                $description = $event->description;
                $image = self::getImage($name, $description);
                $link = $event->link;
                $who = $event->group->who;

                if(isset($event->venue)) {
                    $place = $event->venue->name ?? "";
                    $address = $event->venue->address_1 ?? "";
                    $city = (string)$event->venue->city ?? "";
                    $lat = (float)$event->venue->lat ?? 0;
                    $lon = (float)$event->venue->lon ?? 0;
                } else {
                    $place = "";
                    $address = "";
                    $city = "";
                    $lat = 0;
                    $lon = 0;
                }

                $start = date("d/m/Y H:i", $event->time / 1000);

                if(isset($event->duration)) {
                    $end = date("H:i", ($event->time + $event->duration) / 1000);
                } else {
                    $end = "";
                }

                $yes_rsvp_count = (int)$event->yes_rsvp_count ?? 0;
                $rsvp_limit = isset($event->rsvp_limit) ? (int)$event->rsvp_limit : 0;
                $waitlist_count = (int)$event->waitlist_count ?? 0;

                $response = $event->self->rsvp->response ?? "";
                $survey_questions = $event->survey_questions ?? array();
                $answers = $event->self->rsvp->answers ?? array();

                $rsvpable = $event->rsvpable ?? false;
                $how_to_find_us = $event->how_to_find_us ?? "";
                
                $new_event = new Event($id, $name, $image, $description, $link, $who, $place, $address, $city, $lat, $lon, $start, $end, $yes_rsvp_count, $rsvp_limit, $waitlist_count, $response, $survey_questions, $answers, $rsvpable, $how_to_find_us);

                // Não perder o tempo necessário pra carregar o tamanho das imagens do Xamarin em outras plataformas, até o momento do desenvolvimento dos aplicativos o Xamarin não se estendia as imagens verticalmente, por isso fiz um cálculo dentro dos aplicativos para "arrumar" esse erro.
                if(self::$via == "xamarin") {
                    list($width, $height, $type, $attr) = getimagesize($image);

                    $new_event->setImage_width($width);
                    $new_event->setImage_height($height);
                }

                $new_event->setDescription(self::getHtml($new_event, $isPast));

                $new_events[] = $new_event;
            }
            
            return json_encode($new_events);
        }

        public static function getHtml(Event $event, bool $pastEvent) {
            $platform = self::$platform;

            $place = urlencode($event->getPlace().", ".$event->getAddress());

            if($platform == "windows" || $platform == "windows81" || $platform == "wp") {
                $geotag =
                    "bingmaps:?cp=".$event->getLat()."~".$event->getLon().
                    "&lvl=8&where=".$place;
            } else if($platform == "ios") {
                $geotag =
                    "maps://maps.apple.com/?ll=".$event->getLat().",".$event->getLon().
                    "&q=".$place;
            } else {
                $geotag =
                    "geo:".$event->getLat().", ".$event->getLon().
                    "?q=".$place;
            }

            $end = $event->getEnd() == "" || strpos($event->getStart(), $event->getEnd()) !== false ? "" : " - ".$event->getEnd();
            $date = str_replace(" ", "<br>", $event->getStart()).$event->getEnd();
           
            if($pastEvent) {
                $go = "foram";
            } else {
                $go = "vão";
            }
 
            // Espaço para o FAB de se inscrever no evento
            if($platform == "android" && !$pastEvent) {
                $padding_bottom = 70;
            }

            ob_start(); ?>
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
    <link rel="stylesheet" href="<?php echo self::getLocation(); ?>/css/style.css" />
    <link rel="stylesheet" href="<?php echo self::getLocation(); ?>/css/leaflet-0.7.7.css" />
    <link href="https://fonts.googleapis.com/css?family=Roboto:400,700,700italic,400italic" rel="stylesheet" type="text/css">

    <style>
        body {
            padding-bottom: <?php echo $padding_bottom; ?>px;
        }
    </style>
</head>
<body>
    <h2 class="margin"><?php echo $event->getName(); ?></h2>

    <table class="margin">
        <tr>
            <td><?php echo $date; ?></td>
            <td><?php echo $event->getYes_rsvp_count()." ".$event->getWho()." ".$go; ?></td>
        </tr>
    </table>

    <?php if($event->getPlace() == "") { ?>
        <p class="margin"><a href="http://do_login">Faça login para visualizar a localização</a></p>
    <?php } else { ?>
        <div id="map"></div><p class="margin" style="text-align: center;"><?php echo $event->getPlace(); ?> - <?php echo $event->getAddress(); ?><br><?php echo $event->getHow_to_find_us(); ?></p>
    <?php } ?>

    <div class="margin"><?php echo $event->getDescription(); ?></div>

    <?php if($event->getPlace() != "") { ?>
        <script src="<?php echo self::getLocation(); ?>/js/leaflet-0.7.7.js"></script>
        <script>

            var mymap = L.map("map").setView([<?php echo $event->getLat().", ".$event->getLon(); ?>], 15);

            L.tileLayer("https://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token=<?php echo self::mapBoxToken; ?>", {
                maxZoom: 18,
                attribution: "Map data &copy; <a href=\"http://openstreetmap.org\">OpenStreetMap</a> contributors, " +
                    "<a href=\"http://creativecommons.org/licenses/by-sa/2.0/\">CC-BY-SA</a>, " +
                    "Imagery © <a href=\"http://mapbox.com\">Mapbox</a>",
                id: "mapbox.streets"
            }).addTo(mymap);

            L.marker([<?php echo $event->getLat().", ".$event->getLon(); ?>]).addTo(mymap)
                .bindPopup("<b><?php echo $event->getPlace(); ?></b>").openPopup();

            var popup = L.popup();

            function onMapClick(e) {
                window.location.replace("<?php echo $geotag; ?>");
            }

            mymap.on("click", onMapClick);

        </script>
    <?php } ?>
</body>
</html>
            <?php
            $html = ob_get_clean();

            return $html;
        }
    }
}
?>