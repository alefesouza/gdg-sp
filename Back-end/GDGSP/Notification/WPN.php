<?php
// https://github.com/brianantonelli/Windows-8-PHP-Notifications/blob/master/wpn.php
namespace GDGSP\Notification {
    class WPNTypesEnum{       
        const Toast = 'wns/toast';
        const Badge = 'wns/badge';
        const Tile  = 'wns/tile';
        const Raw   = 'wns/raw';
    }

    class WPNResponse {
        public $message = '';
        public $error = false;
        public $httpCode = '';
        
        function __construct($message, $httpCode, $error = false){
            $this->message = $message;
            $this->httpCode = $httpCode;
            $this->error = $error;
        }
    }

    class WPN{            
        private $access_token = '';
        private $sid = '';
        private $secret = '';
            
        function __construct($sid, $secret){
            $this->sid = $sid;
            $this->secret = $secret;
        }
        
        private function get_access_token(){
            if($this->access_token != ''){
                return;
            }
            $str = "grant_type=client_credentials&client_id=$this->sid&client_secret=$this->secret&scope=notify.windows.com";
                
            $url = "https://login.live.com/accesstoken.srf";
            $ch = curl_init($url);
            curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, 0);
            curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, 0);
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_HTTPHEADER, array('Content-Type: application/x-www-form-urlencoded'));
            curl_setopt($ch, CURLOPT_POSTFIELDS, "$str");
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
            $output = curl_exec($ch);
            curl_close($ch);                       
            $output = json_decode($output);
            if(isset($output->error)){
                            echo $str;
                throw new Exception($output->error_description);
            }
            $this->access_token = $output->access_token;
        }
        
        public function post_tile($uri, $xml_data, $type = WPNTypesEnum::Tile, $tileTag = ''){
            if($this->access_token == ''){
                $this->get_access_token();
            }
            $headers = array('Content-Type: text/xml', "Content-Length: " . strlen($xml_data), "X-WNS-Type: $type", "Authorization: Bearer $this->access_token");
            if($tileTag != ''){
                array_push($headers, "X-WNS-Tag: $tileTag");
            }
            $ch = curl_init($uri);
            # Tiles: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868263.aspx
            # http://msdn.microsoft.com/en-us/library/windows/apps/hh465435.aspx
            curl_setopt($ch, CURLOPT_SSL_VERIFYHOST, 0);
            curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, 0);
            curl_setopt($ch, CURLOPT_POST, 1);
            curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
            curl_setopt($ch, CURLOPT_POSTFIELDS, "$xml_data");
            curl_setopt($ch, CURLOPT_VERBOSE, 1);
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
            $output = curl_exec($ch);
            $response = curl_getinfo( $ch );
            curl_close($ch);
        
            $code = $response['http_code'];
            if($code == 200){
                return new WPNResponse('Successfully sent message', $code);
            }
            else if($code == 401){
                $this->access_token = '';
                return $this->post_tile($uri, $xml_data, $type, $tileTag);
            }
            else if($code == 410 || $code == 404){
                return new WPNResponse('Expired or invalid URI', $code, true);
            }
            else{
                return new WPNResponse('Unknown error while sending message', $code, true);
            }
        }
    }
}
?>