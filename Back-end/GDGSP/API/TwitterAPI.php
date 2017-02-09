<?php
// Pesquisa de Tweets utilizando chave do aplicativo, código de https://gist.github.com/lgladdy/5141615 com modificações por Alefe Souza <contact@alefesouza.com>

namespace GDGSP\API {
    use GDGSP\Database\DB;
    use GDGSP\Model\Tweet;

    class TwitterAPI {
        private $token;

        const api_base = "https://api.twitter.com/";

        public function __construct($token) {
            $this->token = $token;
        }

        public static function getToken() : string {
            $bearer_token_creds = base64_encode(DB::$twitterKey.":".DB::$twitterSecret);

            //Get a bearer token.
            $opts = array(
                "http" => array(
                    "method" => "POST",
                    "header" => "Authorization: Basic ".$bearer_token_creds."\r\n".
                            "Content-Type: application/x-www-form-urlencoded;charset=UTF-8",
                    "content" => "grant_type=client_credentials"
                )
            );

            $context = stream_context_create($opts);
            $json = file_get_contents(self::api_base."oauth2/token", false, $context);
            $result = json_decode($json,true);

            return $result["access_token"];
        }

        public function getTweets(string $max_id) : string {
            $opts = array(
                "http" => array(
                    "method" => "GET",
                    "header" => "Authorization: Bearer ".$this->token
                )
            );

            if($max_id != "") {
                $max_id = "&max_id=".$max_id;
            }

            $context = stream_context_create($opts);

            $get_url =
                self::api_base."1.1/search/tweets.json?".
                    "count=30".
                    "&q=".urlencode(DB::$twitterSearch).
                    "&result_type=recent".
                    "&include_entities=true".
                    $max_id;

            $json = file_get_contents($get_url, false, $context);

            return $json;
        }

        public function makeAppJson(string $json) : string {
            $twitter_json = json_decode($json);

            foreach($twitter_json->statuses as $tweet) {
                $medias = array();
            
                $text = (string)$tweet->text;
                
                if(isset($tweet->extended_entities)) {
                    if(count($tweet->extended_entities->media) > 0) {
                        foreach($tweet->extended_entities->media as $media) {
                            $medias[] = array("url" => $media->media_url);
                            $text = str_replace($media->url, $media->display_url, $text);
                        }
                    }
                }
                
                if(count($tweet->entities->urls) > 0) {
                    foreach($tweet->entities->urls as $media) {
                        $text = str_replace($media->url, $media->display_url, $text);
                    }
                }
                
                $date = date("H:i d/m/Y", strtotime($tweet->created_at));

                $id = (string)$tweet->id_str;
                $date = (string)$date;
                $link = "http://twitter.com/".$tweet->user->screen_name."/status/".$tweet->id_str;
                $user_name = (string)$tweet->user->name;
                $screen_name = "@".$tweet->user->screen_name;
                $photo = str_replace("normal", "bigger", (string)$tweet->user->profile_image_url);
                $media = $medias;
                $retweet_count = (int)$tweet->retweet_count;
                $favorite_count = (int)$tweet->favorite_count;
                
                $new_tweet = new Tweet($id, $date, $link, $text, $user_name, $screen_name, $photo, $media, $retweet_count, $favorite_count);

                $tweets[] = $new_tweet;
            }

            $more_tweets = isset($tweets[10]);

            $max_id = isset($tweets[10]) ? $tweets[10]->getId() : "0";

            $returnTweets = array_slice($tweets, 0, 10);

            $returnJson = array(
                "more_tweets" => (bool)$more_tweets,
                "max_id" => $max_id,
                "tweets" => $returnTweets
            );

            return json_encode($returnJson);
        }
    }
}
?>