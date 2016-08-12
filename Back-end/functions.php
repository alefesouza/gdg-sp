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

date_default_timezone_set("America/Sao_Paulo");

// Um UTF-8 a mais nunca é demais
header('Content-Type: text/html; charset=utf-8');

// Caminho do arquivo atual
$location = "http://".dirname($_SERVER['SERVER_NAME'].$_SERVER['PHP_SELF']);
$qrcode = "http://gdgsp.org/qrcode/";

// Caso o cron de notificações passe por aqui, o $meetupid já terá valor
if(!isset($meetupid)) {
	$meetupid = $_GET["meetupid"];
}
$platform = $_GET["platform"];

$mapbox_token = "mapbox_token";

if($dbi == null) {
	include("connect_db.php");
}

$query = mysqli_query($dbi, "SELECT * FROM meetups WHERE meetupid='$meetupid'");
$dados = mysqli_fetch_array($query);

// Usado no login de usuários com o meetup
$meetup_client_key = $dados["meetupapi_client"];
$meetup_secret_key = $dados["meetupapi_secret"];

// Notificações para One Signal
$one_signal_appid = $dados["onesignal_appid"];
$one_signal_restkey = $dados["onesignal_restkey"];

// Notificações para Windows 10
$wns_sid = urlencode($dados["wns_sid"]);
$wns_client_secret = urlencode($dados["wns_clientsecret"]);

$last_event = $dados["last_event"];
$header_image = $dados["header_image"];

// Caso o array meetupids do cron.php tenha mais de um item, não redeclarar as funções
if(!function_exists('getImage')) {
	// Retorna url da primeira imagem na descrição de um evento
	function getImage($name, $description) {
		preg_match_all('~<img.*?src=["\']+(.*?)["\']+~', $description, $images);
		$image = $images[1][0];
		if($image == "") {
			return getImage2($name, $size);
		} else {
			return $image;
		}
	}

	// Retorna imagem baseada na trilha caso a primeira função não encontre uma imagem
	function getImage2($name) {
		global $location;
		global $header_image;

		$trilhas = array(
			array(
				"name" => "html5 study group html5sg",
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

		return $header_image;
	}

	function refreshMeetupToken($refresh_token) {
		global $meetup_client_key;
		global $meetup_secret_key;

		$url = "https://secure.meetup.com/oauth2/access";

		$query = "client_id=$meetup_client_key&client_secret=$meetup_secret_key&grant_type=refresh_token&refresh_token=$refresh_token";

		$headers = array(
				"Content-Type: application/x-www-form-urlencoded"
		);

		$ch = curl_init();

		curl_setopt($ch, CURLOPT_URL, $url);

		curl_setopt($ch, CURLOPT_POST, true);
		curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);

		curl_setopt($ch, CURLOPT_POSTFIELDS, $query);

		$result = curl_exec($ch);
		$dados = json_decode($result);
		
		return $dados->access_token;
	}

	function sendMessageOneSignal($count, $title, $message, $image, $eventid) {
		global $one_signal_appid;
		global $one_signal_restkey;

		$headings = array(
			"en" => $title
		 );

		$content = array(
			"en" => $message
		 );

		$fields = array(
			'app_id' => $one_signal_appid,
			'included_segments' => array('All'),
			'data' => array("eventid" => (int)$eventid),
			'contents' => $content,
			'big_picture' => $image,
			'headings' => $headings,
			'android_accent_color' => 'FF008bf8',
			'android_led_color' => 'FF008bf8',
			'small_icon' => 'ic_notification',
			'ios_badgeType' => 'SetTo',
			'ios_badgeCount' => $count
		);

		$fields = json_encode($fields);
		print("\nJSON sent:\n");
		print($fields);

		$ch = curl_init();
		curl_setopt($ch, CURLOPT_URL, "https://onesignal.com/api/v1/notifications");
		curl_setopt($ch, CURLOPT_HTTPHEADER, array('Content-Type: application/json',
													 'Authorization: Basic '.$one_signal_restkey));
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, TRUE);
		curl_setopt($ch, CURLOPT_HEADER, FALSE);
		curl_setopt($ch, CURLOPT_POST, TRUE);
		curl_setopt($ch, CURLOPT_POSTFIELDS, $fields);
		curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, FALSE);

		$response = curl_exec($ch);
		curl_close($ch);

		return $response;
	}

	function sendMessageOneSignal2($title, $message, $image, $url, $eventid) {
		global $one_signal_appid;
		global $one_signal_restkey;
		global $meetupid;
		global $dbi;

		$headings = array(
			"en" => $title
		 );

		$content = array(
			"en" => $message
		 );

		$fields = array(
			'app_id' => $one_signal_appid,
			'included_segments' => array('All'),
			'contents' => $content,
			'headings' => $headings,
			'android_accent_color' => 'FF008bf8',
			'android_led_color' => 'FF008bf8',
			'small_icon' => 'ic_notification',
		);
		
		if($image != "") {
			$fields["big_picture"] = $image;
		}
		
		if($url != "") {
			$fields["url"] = $url;
		}
		
		if($eventid != "") {
			// "Não é gambiarra, é a sintaxe normal do PHP"
			// Aqui eu pego todos os ids que vão para o evento, chego quais tem o app e monto um array para enviar ao OneSignal
			$json = file_get_contents("http://api.meetup.com/$meetupid/events/$eventid/rsvps");
			$people = json_decode($json);
			foreach($people as $person) {
				$ids[] = $person->member->id;
			}
			
			$query = mysqli_query($dbi, "SELECT member_id FROM meetup_app_members WHERE meetup_id='$meetupid'");

			while($row = mysqli_fetch_array($query)) {
				$members_with_app[] = $row["member_id"];
			}

			foreach($members_with_app as $member_with_app) {
				if(in_array($member_with_app, $ids)) {
					$tags[] = array("key" => "meetup_id", "relation" => "=", "value" => $member_with_app);
				}
			}
			
			$fields["tags"] = $tags;
		}
		
		$fields = json_encode($fields);
		
		$ch = curl_init();
		curl_setopt($ch, CURLOPT_URL, "https://onesignal.com/api/v1/notifications");
		curl_setopt($ch, CURLOPT_HTTPHEADER, array('Content-Type: application/json',
													 'Authorization: Basic '.$one_signal_restkey));
		curl_setopt($ch, CURLOPT_RETURNTRANSFER, TRUE);
		curl_setopt($ch, CURLOPT_HEADER, FALSE);
		curl_setopt($ch, CURLOPT_POST, TRUE);
		curl_setopt($ch, CURLOPT_POSTFIELDS, $fields);
		curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, FALSE);

		$response = curl_exec($ch);
		curl_close($ch);

		return $response;
	}
	
	function checkIsAdmin($memberid, $token) {
		global $meetupid;
		
		$getorganizers = @file_get_contents("https://api.meetup.com/$meetupid?access_token=$token");
		$organizersids[] = json_decode($getorganizers)->organizer->id;

		$getcoorganizers = @file_get_contents("https://api.meetup.com/$meetupid/members?access_token=$token&filter=stepup_eligible");
		$coorganizers = json_decode($getcoorganizers);

		foreach($coorganizers as $coorganizer) {
			$organizersids[] = $coorganizer->id;
		}

		// Ids do Meetup de usuários que podem enviar notificações mas não são organizadores
		$extramembers = array(193513345);

		if(in_array($memberid, $extramembers) || in_array($memberid, $organizersids)) {
			return true;
		}
		
		return false;
	}
}
?>