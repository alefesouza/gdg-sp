<?php
// Código de https://arjunkr.quora.com/How-to-Windows-10-WNS-Windows-Notification-Service-via-PHP com modificações por Alefe Souza <http://alefesouza.com>

namespace GDGSP\Notification {
    use GDGSP\API\MeetupAPI;
    use GDGSP\Database\DB;
    use GDGSP\Util\Utils;

    include("WPN.php");

    class WNS {
        public static $channelUri;
        private static $channel = "";

        public static function checkURI() {
            $db = DB::getInstance();
            $user = $db->getSingleWNSUser(self::$channelUri);
            
            $channel = $user["device"];

            if($user["device"] != "") {
                $data = array("uri_exists"=>true);

                echo json_encode($data);
            } else {
                $data = array("uri_exists"=>false);

                echo json_encode($data);

                self::register();
            }
        }

        private static function register() {
            // Set POST request variable
            $url = 'https://login.live.com/accesstoken.srf';
    
            $fields = array(
                'grant_type' => urlencode('client_credentials'),
                'client_id' => urlencode(DB::$WNS_SId),
                'client_secret' => urlencode(DB::$WNS_ClientSecret),
                'scope' => urlencode('notify.windows.com')
            );
        
            //url-ify the data for the POST
            foreach($fields as $key=>$value) {
                $fields_string .= $key.'='.$value.'&';
            }
            
            rtrim($fields_string, '&');

            $headers = array(
                'Content-Type: application/x-www-form-urlencoded'
            );

            // Open connection
            $ch = curl_init();

            // Set the url, number of POST vars, POST data
            curl_setopt($ch, CURLOPT_URL, $url);
    
            curl_setopt($ch, CURLOPT_POST, true);
            curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
    
            // disable SSL certificate support
            curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
    
            curl_setopt($ch, CURLOPT_POSTFIELDS, $fields_string);
            
            // execute post
            $result = curl_exec($ch);

            if ($result === FALSE) {
                die('Curl failed: ' . curl_error($ch));
            }

            $obj = json_decode($result);

            $access_token = $obj->{'access_token'};
            $token_type = $obj->{'token_type'};

            // Close connection
            curl_close($ch);

            //echo $result;
            $expire = date('Ymd', strtotime("+31 days"));

            $db = DB::getInstance();
          
            if(self::$channel == '') {
                $platform = $_GET["platform"];
                $app_version = $_GET["appversion"];
                
                $db->addWNSUsers(self::$channelUri, $expire, $platform, $app_version);
            } else {
                $db->updateWNSUser(self::$channelUri, $expire, self::$channel);
            }
        }

        public static function notifyUsers($count, $title, $description, $image, $id) {
            ob_start(); ?>
<?xml version="1.0" encoding="utf-8"?>
<toast activationType="foreground" scenario="reminder" duration="short" launch="event|<?php echo $id; ?>">
    <visual>
        <binding template="ToastGeneric">
            <text><?php echo $title; ?></text>
            <text><?php echo $description; ?></text>
            <image placement="inline" src="<?php echo $image; ?>" />
        </binding>
    </visual>
</toast>
            <?php

            $xml_toast = ob_get_clean();

            ob_start(); ?>
<?xml version="1.0" encoding="utf-8"?>
<badge value="<?php echo $count; ?>" />
            <?php

            $xml_badge = ob_get_clean();

            $url =
                Utils::getLocation().
                "/tiles/tile.php?tile=0&meetupid=".
                DB::$meetupId;

            $xml_tile = file_get_contents($url);
            
            $users = DB::getInstance()->getWNSUsers();
            
            foreach($users as $user) {
                $device = $user["device"];

                $obj = new WPN(DB::$WNS_SId, DB::$WNS_ClientSecret);

                $obj->post_tile($device, $xml_toast, $type = WPNTypesEnum::Toast, $tileTag = '');
                $obj->post_tile($device, $xml_badge, $type = WPNTypesEnum::Badge, $tileTag = '');
                $obj->post_tile($device, $xml_tile, $type = WPNTypesEnum::Tile, $tileTag = '');
            }
        }

        public static function notifyUsersEvent($title, $message, $image, $url, $eventid)
        {
            $args = $url != "" ? " launch=\"url|$title|$url\"" : "";
            $image_element = $image != "" ? '<image placement="inline" src="'.$image.'" />' : "";
            
            ob_start(); ?>
<?xml version="1.0" encoding="utf-8"?>
<toast activationType="foreground" scenario="reminder" duration="short"<?php echo $args; ?>>
    <visual>
        <binding template="ToastGeneric">
            <text><?php echo $title; ?></text>
            <text><?php echo $message; ?></text>
            <?php echo $image_element; ?>
        </binding>
    </visual>
</toast>
<?php
            $xml_toast = ob_get_clean();
            
            $devices = DB::getInstance()->getWNSUsers();
            
            if($eventid == "") {
                foreach($users as $user) {
                    $device = $user["device"];

                    $obj = new WPN(DB::$WNS_SId, DB::$WNS_ClientSecret);

                    $obj->post_tile($device, $xml_toast, $type = WPNTypesEnum::Toast, $tileTag = '');
                }
            } else {
                $api = new MeetupAPI("");
                $people = $api->getRSVPs($event_id);

                foreach($people as $person) {
                    $ids[] = $person->id;
                }

                foreach($users as $user) {
                    if(in_array($user['member_id'], $ids)) {
                        $obj = new WPN($wns_sId,$wns_client_secret);
                        $obj->post_tile($user["device"], $xml_toast, $type = WPNTypesEnum::Toast, $tileTag = '');
                    }
                }
            }
        }
    }
}
?>