<?php
// Pesquisa de Tweets utilizando chave do aplicativo, código de https://github.com/lotsofcode/Twitter-Application-Only-Authentication-OAuth-PHP/blob/master/Oauth.php com algumas modificações por Alefe Souza <http://alefesouza.com>
include("../functions.php");

$bearer_token = get_bearer_token();

$json = json_decode(search_for_a_term($bearer_token, $twitter_search));

foreach($json->statuses as $tweet) {
	$medias = array();
	
	if(count($tweet->extended_entities->media) > 0) {
		foreach($tweet->extended_entities->media as $media) {
			$medias[] = array("url" => $media->media_url);
		}
	}
	
	$date = date("H:i d/m/Y", strtotime($tweet->created_at));
	
	$tweets[] = array(
		"id" => (string)$tweet->id,
		"date" => (string)$date,
		"link" => "http://twitter.com/".$tweet->user->screen_name."/status/".$tweet->id,
		"text" => (string)$tweet->text,
		"user_name" => (string)$tweet->user->name,
		"screen_name" => "@".$tweet->user->screen_name,
		"photo" => (string)$tweet->user->profile_image_url,
		"media" => $medias,
		"retweet_count" => $tweet->retweet_count,
		"favorite_count" => $tweet->favorite_count
	);
}

echo json_encode($tweets);

invalidate_bearer_token($bearer_token);

function get_bearer_token(){
	global $twitter_key;
	global $twitter_secret;
	
	$encoded_consumer_key = urlencode($twitter_key);
	$encoded_consumer_secret = urlencode($twitter_secret);
	$bearer_token = $encoded_consumer_key.':'.$encoded_consumer_secret;
	$base64_encoded_bearer_token = base64_encode($bearer_token);
  
	$url = "https://api.twitter.com/oauth2/token";
	$headers = array( 
		"POST /oauth2/token HTTP/1.1", 
		"Host: api.twitter.com",
		"Authorization: Basic ".$base64_encoded_bearer_token,
		"Content-Type: application/x-www-form-urlencoded;charset=UTF-8"
	); 
	$ch = curl_init();
	curl_setopt($ch, CURLOPT_URL,$url);
	curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
	curl_setopt($ch, CURLOPT_POST, 1);
	curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
	curl_setopt($ch, CURLOPT_POSTFIELDS, "grant_type=client_credentials");
	$header = curl_setopt($ch, CURLOPT_HEADER, 1);
	$httpcode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
	$retrievedhtml = curl_exec ($ch);
	curl_close($ch);
	$output = explode("\n", $retrievedhtml);
	$bearer_token = '';
	foreach($output as $line)
	{
		if($line === false)
		{
		}else{
			$bearer_token = $line;
		}
	}
	$bearer_token = json_decode($bearer_token);
	return $bearer_token->{'access_token'};
}

function invalidate_bearer_token($bearer_token){
	global $twitter_key;
	global $twitter_secret;
	
	$encoded_consumer_key = urlencode($twitter_key);
	$encoded_consumer_secret = urlencode($twitter_secret);
	$consumer_token = $encoded_consumer_key.':'.$encoded_consumer_secret;
	$base64_encoded_consumer_token = base64_encode($consumer_token);
  
	$url = "https://api.twitter.com/oauth2/invalidate_token";
	$headers = array( 
		"POST /oauth2/invalidate_token HTTP/1.1", 
		"Host: api.twitter.com",
		"Authorization: Basic ".$base64_encoded_consumer_token,
		"Accept: */*", 
		"Content-Type: application/x-www-form-urlencoded"
	);
	
	$ch = curl_init();
	curl_setopt($ch, CURLOPT_URL,$url);
	curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
	curl_setopt($ch, CURLOPT_POST, 1);
	curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
	curl_setopt($ch, CURLOPT_POSTFIELDS, "access_token=".$bearer_token."");
	$header = curl_setopt($ch, CURLOPT_HEADER, 1);
	$httpcode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
	$retrievedhtml = curl_exec ($ch);
	curl_close($ch);
	return $retrievedhtml;
}

function search_for_a_term($bearer_token, $query){
	$url = "https://api.twitter.com/1.1/search/tweets.json";
	$q = urlencode(trim($query));
	$formed_url = '?q='.$q.'&result_type=recent&count=15&include_entities=true';
  
	if(isset($_GET["max_id"])) {
		$formed_url .= "&max_id=".$_GET["max_id"];
	}
	
	$headers = array( 
		"GET /1.1/search/tweets.json".$formed_url." HTTP/1.1", 
		"Host: api.twitter.com",
		"Authorization: Bearer ".$bearer_token
	);
	$ch = curl_init();
	curl_setopt($ch, CURLOPT_URL,$url.$formed_url);
	curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
	curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
	$retrievedhtml = curl_exec ($ch);
	curl_close($ch);
	return $retrievedhtml;
}
?>