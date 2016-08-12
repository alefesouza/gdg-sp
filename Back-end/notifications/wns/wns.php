<?php
// Código de https://arjunkr.quora.com/How-to-Windows-10-WNS-Windows-Notification-Service-via-PHP com algumas modificações por Alefe Souza <http://alefesouza.com>
include_once 'wpn.php';
$channel = '';
$ChannelUri = $_POST['ChannelUri'];

if(!isset($dbi)) {
	include("../../connect_db.php");
	if(!isset($meetupid)) {
		$meetupid = $_GET["meetupid"];
	}
}

if($_POST['check_uri'] == "true")
{	
	check_uri();
}

function check_uri()
{
	global $dbi;
	global $meetupid;
	global $ChannelUri;
	$sql = mysqli_query($dbi, "select device from meetup_wns_users where device='$ChannelUri' and meetupid='$meetupid'") or die ("ERROR: ".mysqli_error($dbi));
	$info = mysqli_fetch_array($sql);
	$channel = $info['device'];
	$uricheck = mysqli_num_rows($sql);;
	if($uricheck == 1)
	{
		$data = array("uri_exists"=>true);
		echo json_encode($data);
	}
	else
	{
		$data = array("uri_exists"=>false);
		echo json_encode($data);
		register_wns();
	}
}

function register_wns() 
{
		global $dbi;
		global $meetupid;
		global $ChannelUri;
		global $channel;
		global $wns_sid;
		global $wns_client_secret;
       
        // Set POST request variable
        $url = 'https://login.live.com/accesstoken.srf';
 
        $fields = array(
            'grant_type' => urlencode('client_credentials'),
            'client_id' => urlencode($wns_sid),
            'client_secret' => urlencode($wns_client_secret),
            'scope' => urlencode('notify.windows.com')
        );
	
		//url-ify the data for the POST
		foreach($fields as $key=>$value) { $fields_string .= $key.'='.$value.'&'; }
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
        //curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, false);
 
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
	    if($channel == '')
		{
			$platform = $_GET["platform"];
			$appversion = $_GET["appversion"];
			
			mysqli_query($dbi, "insert into meetup_wns_users (device, meetupid, expire, platform, app_version) values ('$ChannelUri', '$meetupid', $expire, '$platform', '$appversion')") or die ("ERROR: ".mysqli_error($dbi));
		}
		else
		{
			mysqli_query($dbi, "update meetup_wns_users set device='$ChannelUri', expire=$expire where device='$channel' and meetupid='$meetupid'") or die ("ERROR: ".mysqli_error($dbi));
		}
}

function notify_wns_users($count, $title, $description, $image, $id)
{
	global $dbi;
	global $meetupid;
	global $wns_sid;
	global $wns_client_secret;
	global $location;
	
	$xml_toast = '<?xml version="1.0" encoding="utf-8"?><toast activationType="foreground" scenario="reminder" duration="short" launch="event|'.$id.'"><visual><binding template="ToastGeneric"><text>'.$title.'</text><text>'.$description.'</text><image placement="inline" src="'.$image.'" /></binding></visual></toast>';
	$xml_badge = '<?xml version="1.0" encoding="utf-8"?><badge value="'.$count.'" />';
	$xml_tile = file_get_contents($location."/tiles/tile.php?tile=0&meetupid=$meetupid");
	
	$sql = mysqli_query($dbi, "select * from meetup_wns_users where meetupid='$meetupid'") or die ("ERROR: ".mysqli_error($dbi));
	
	while($row = mysqli_fetch_array($sql))
	{
		$uri = $row['device'];
		
		$obj = new WPN($wns_sid,$wns_client_secret);
		$obj->post_tile($uri, $xml_toast, $type = WPNTypesEnum::Toast, $tileTag = '');
		$obj->post_tile($uri, $xml_badge, $type = WPNTypesEnum::Badge, $tileTag = '');
		$obj->post_tile($uri, $xml_tile, $type = WPNTypesEnum::Tile, $tileTag = '');
	}
}

function notify_wns_users2($title, $message, $image, $url, $eventid)
{
	global $dbi;
	global $wns_sid;
	global $wns_client_secret;
	global $meetupid;
	
	$xml_toast = '<?xml version="1.0" encoding="utf-8"?><toast activationType="foreground" scenario="reminder" duration="short"';
	if($url != "") {
	 $xml_toast .= ' launch="url|'.$title.'|'.$url.'"';
	}
	$xml_toast .= '><visual><binding template="ToastGeneric"><text>'.$title.'</text><text>'.$message.'</text>';
	if($image != "") $xml_toast .= '<image placement="inline" src="'.$image.'" />';
	$xml_toast .= '</binding></visual></toast>';
	
	$sql = mysqli_query($dbi, "select * from meetup_wns_users where meetupid='$meetupid'") or die ("ERROR: ".mysqli_error($dbi));
	
	if($eventid == "") {
		while($row = mysqli_fetch_array($sql))
		{
			$uri = $row['device'];

			$obj = new WPN($wns_sid,$wns_client_secret);
			$obj->post_tile($uri, $xml_toast, $type = WPNTypesEnum::Toast, $tileTag = '');
		}
	} else {
		$json = file_get_contents("http://api.meetup.com/$meetupid/events/$eventid/rsvps");
		$people = json_decode($json);
		foreach($people as $person) {
			$ids[] = $person->member->id;
		}

		while($row = mysqli_fetch_array($sql))
		{
			if(in_array($row['member_id'], $ids)) {
				$uri = $row['device'];

				$obj = new WPN($wns_sid,$wns_client_secret);
				$obj->post_tile($uri, $xml_toast, $type = WPNTypesEnum::Toast, $tileTag = '');
			}
		}
	}
}
?>