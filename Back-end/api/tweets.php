<?php
// Pesquisa de Tweets utilizando chave do aplicativo, código de https://gist.github.com/lgladdy/5141615 com algumas modificações por Alefe Souza <http://alefesouza.com>
include("../functions.php");

$api_base = 'https://api.twitter.com/';
$bearer_token_creds = base64_encode($twitter_key.':'.$twitter_secret);
//Get a bearer token.
$opts = array(
  'http'=>array(
    'method' => 'POST',
    'header' => 'Authorization: Basic '.$bearer_token_creds."\r\n".
               'Content-Type: application/x-www-form-urlencoded;charset=UTF-8',
    'content' => 'grant_type=client_credentials'
  )
);

$context = stream_context_create($opts);
$json = file_get_contents($api_base.'oauth2/token',false,$context);
$result = json_decode($json,true);

$bearer_token = $result['access_token'];

$opts = array(
  'http'=>array(
    'method' => 'GET',
    'header' => 'Authorization: Bearer '.$bearer_token
  )
);

if(isset($_GET["max_id"])) {
	$maxid = "&max_id=".$_GET["max_id"];
}

$context = stream_context_create($opts);

$json = file_get_contents($api_base."1.1/search/tweets.json?count=30&q=".urlencode($twitter_search)."&result_type=recent&include_entities=true$maxid", false, $context);
$json = json_decode($json);

foreach($json->statuses as $tweet) {
	$medias = array();
  
  $text = (string)$tweet->text;
	
	if(count($tweet->extended_entities->media) > 0) {
		foreach($tweet->extended_entities->media as $media) {
			$medias[] = array("url" => $media->media_url);
      $text = str_replace($media->url, $media->display_url, $text);
		}
	}
	
	if(count($tweet->entities->urls) > 0) {
		foreach($tweet->entities->urls as $media) {
      $text = str_replace($media->url, $media->display_url, $text);
		}
	}
	
	$date = date("H:i d/m/Y", strtotime($tweet->created_at));
	
	$tweets[] = array(
		"id" => (string)$tweet->id,
		"date" => (string)$date,
		"link" => "http://twitter.com/".$tweet->user->screen_name."/status/".$tweet->id,
		"text" => $text,
		"user_name" => (string)$tweet->user->name,
		"screen_name" => "@".$tweet->user->screen_name,
		"photo" => str_replace("normal", "bigger", (string)$tweet->user->profile_image_url),
		"media" => $medias,
		"retweet_count" => $tweet->retweet_count,
		"favorite_count" => $tweet->favorite_count
	);
}

$more_tweets = $tweets[10] != null;

$max_id = $tweets[10] != null ? $tweets[10]["id"] : "0";

$returnTweets = array_slice($tweets, 0, 10);

$returnJson = array("more_tweets" => (bool)$more_tweets, "max_id" => $max_id, "tweets" => $returnTweets);

echo json_encode($returnJson);
?>