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

if($_POST["refresh_token"] != "") {
	$token = refreshMeetupToken($_POST["refresh_token"]);
	
	$json = @file_get_contents("https://api.meetup.com/$meetupid/events?access_token=$token&fields=survey_questions,self,rsvpable");
	
	if(strpos($http_response_header[0], "200") !== false) {
		$memberdata = file_get_contents("https://api.meetup.com/$meetupid/members/self?access_token=$token");
		
		$member = json_decode($memberdata);

		$memberid = $member->id;
		$membername = $member->name;
		$memberphoto = $member->photo->photo_link;
		$memberintro = $member->group_profile->intro;
		
		$isadmin = checkIsAdmin($memberid, $token);
		
		$last_activity = date("Ymd", time());
		mysqli_query($dbi, "UPDATE meetup_app_members SET last_activity=$last_activity WHERE member_id=$memberid");
		
		if(($platform == "windows" || $platform == "windows81") && $_POST["ChannelUri"] != "") {
			mysqli_query($dbi, "UPDATE meetup_wns_users SET member_id=$memberid WHERE device='".$_POST["ChannelUri"]."'");
		}
	} else {
		$json = file_get_contents("https://api.meetup.com/$meetupid/events?fields=rsvpable");
	}
} else {
	$json = file_get_contents("https://api.meetup.com/$meetupid/events?fields=rsvpable");
}

$events = json_decode($json);

$newevents = array();

foreach($events as $event) {
  $name = $event->name;
  $image = getImage($name, $event->description);
  $who = $event->group->who;
  $place = $event->venue->name;
  $address = $event->venue->address_1;
  $lat = $event->venue->lat;
  $lon = $event->venue->lon;
  $start = date("d/m/Y H:i", $event->time / 1000);
  $end = date("H:i", ($event->time + $event->duration) / 1000);
  $yes_rsvp_count = (int)$event->yes_rsvp_count;
  
  $description = getHtml($name, $event->description, $lat, $lon, $place, $address, $start, $end, $yes_rsvp_count, $who, $event->how_to_find_us);
  
  $newevents[] = array(
    "id" => (int)$event->id,
    "name" => $name,
    "image" => $image,
    "description" => $description,
    "link" => $event->link,
    "who" => $who,
    "place" => (string)$place,
    "address" => (string)$address,
    "city" => (string)$event->venue->city,
    "lat" => (double)$lat,
    "lon" => (double)$lon,
    "start" => (string)$start,
    "end" => (string)$end,
    "yes_rsvp_count" => (int)$yes_rsvp_count,
    "rsvp_limit" => (int)$event->rsvp_limit,
    "waitlist_count" => (int)$event->waitlist_count,
    "response" => $event->self->rsvp->response,
    "survey_questions" => $event->survey_questions,
    "answers" => $event->self->rsvp->answers,
    "rsvpable" => $event->rsvpable
	);
	
	if($_GET["via"] == "xamarin") {
		list($width, $height, $type, $attr) = getimagesize($image);
		$newevents[count($newevents) - 1]["image_width"] = $width;
		$newevents[count($newevents) - 1]["image_height"] = $height;
	}
}

$newjson = array("member" => array("id" => (int)$memberid, "name" => (string)$membername, "photo" => (string)$memberphoto, "intro" => (string)$memberintro, "is_admin" => (boolean)$isadmin), "header" => $header_image, "events" => $newevents);

if($_GET["via"] == "xamarin") {
	list($width, $height, $type, $attr) = getimagesize($header_image);
	$newjson["header_width"] = $width;
	$newjson["header_height"] = $height;
}

echo json_encode($newjson);

function getHtml($title, $description, $lat, $lon, $place, $address, $start, $end, $yes_rsvp_count, $who, $how_to_find_us) {
  global $location;
  global $platform;
	global $mapbox_token;
  
	if($platform == "windows" || $platform == "windows81" || $platform == "wp") {
		$geotag = "bingmaps:?cp=$lat~$lon&lvl=10&where=".urlencode("$place, $address");
	} else if($platform == "ios") {
		$geotag = "maps://maps.apple.com/?ll=$lat,$lon&q=".urlencode("$place, $address");
	} else {
    $geotag = "geo:$lat, $lon?q=".urlencode("$place, $address");
  }

	$end = strpos($start, $end) !== false ? "" : " - $end";
  $date = str_replace(" ", "<br>", $start).$end;
  
  $html = '<!DOCTYPE html>
<html>
<head>
	<meta charset="utf-8" />
	<meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no">
	<link rel="stylesheet" href="'.$location.'/css/style.css" />
	<link rel="stylesheet" href="'.$location.'/css/leaflet-0.7.7.css" />
  <link href="https://fonts.googleapis.com/css?family=Roboto:400,700,700italic,400italic" rel="stylesheet" type="text/css">
</head>
<body>
  <h2 class="margin">'.$title.'</h2>
  <table class="margin"><tr><td>'.$date.'</td><td>'.$yes_rsvp_count.' '.$who.' vão</td></table>';
	
	if($place == "") {
		$html .= '<p class="margin"><a href="http://do_login">Faça login para visualizar a localização</a></p>';
	} else {
		$html .= '<div id="map"></div><p class="margin" style="text-align: center;">'.$place.' - '.$address.'<br>'.$how_to_find_us.'</p>';
	}
	
  $html .= '<div class="margin">'.$description.'</div>';

	if($place != "") {
	$html .= '<script src="'.$location.'/js/leaflet-0.7.7.js"></script>
	<script>

		var mymap = L.map("map").setView(['.$lat.', '.$lon.'], 15);

		L.tileLayer("https://api.tiles.mapbox.com/v4/{id}/{z}/{x}/{y}.png?access_token='.$mapbox_token.'", {
			maxZoom: 18,
			attribution: "Map data &copy; <a href=\"http://openstreetmap.org\">OpenStreetMap</a> contributors, " +
				"<a href=\"http://creativecommons.org/licenses/by-sa/2.0/\">CC-BY-SA</a>, " +
				"Imagery © <a href=\"http://mapbox.com\">Mapbox</a>",
			id: "mapbox.streets"
		}).addTo(mymap);
		
		L.marker(['.$lat.', '.$lon.']).addTo(mymap)
			.bindPopup("<b>'.$place.'</b>").openPopup();

		var popup = L.popup();

		function onMapClick(e) {
		  window.location.replace("'.$geotag.'");
		}

		mymap.on("click", onMapClick);

	</script>';
	}
	
	$html .=	'</body>
	</html>';
  return $html;
}
?>