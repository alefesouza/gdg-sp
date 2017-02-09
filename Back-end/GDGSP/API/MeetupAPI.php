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

namespace GDGSP\API {
    use GDGSP\Model\{ Person, Event, Result };
    use GDGSP\Database\DB;
    use GDGSP\Util\Utils;

    class MeetupAPI {
        private static $token;
        public static $more_past_events = false;

        public function __construct(string $token) {
            self::$token = $token;
        }

        public function setToken($token) {
            self::$token = $token;
        }

        public function getEvents(bool $past = false, int $page = 1, bool $modify = true) : Result {
            $logged = self::$token != "";

            if($past) {
                $url = 
                    "https://api.meetup.com/".DB::$meetupId.
                    "/events?status=past";

                if($logged) {
                    $url .= "&access_token=".self::$token;
                }

                $pastJson = self::makeCurl($url);

                $pastEvents = $pastJson->getResultAsArray();

                $plus = ($page - 1) * 10;

                for($i = 1; $i <= 10; $i++) {
                    $event = $pastEvents[count($pastEvents) - ($i + $plus)];
                    
                    if($event != null) {
                        $newPastEvents[] = $event;
                    }
                }

                if($pastEvents[count($pastEvents) - (11 + $plus)] != null) {
                    self::$more_past_events = true;
                }
                
                $modifiedEvents = Utils::generateEvents($newPastEvents, true);
                
                $pastJson->setResult($modifiedEvents);

                return $pastJson;
            } else {
                $url = 
                    "https://api.meetup.com/".DB::$meetupId.
                    "/events?fields=rsvpable";

                if($logged) {
                    $url .= ",survey_questions,self&access_token=".self::$token;
                }
                    
                $json = self::makeCurl($url);

                if($modify) {
                    $events = $json->getResultAsArray();

                    $modifiedEvents = Utils::generateEvents($events);
                
                    $json->setResult($modifiedEvents);
                }

                return $json;
            }
        }

        public function getRSVPs(int $event_id, bool $with_app = true) : string {
            $logged = self::$token != "";

            $url =
                "https://api.meetup.com/".DB::$meetupId.
                "/events/".$event_id.
                "/rsvps";

            if($logged) {
                $url .= "?access_token=".self::$token;
            }

            $json = self::makeCurl($url);
            
            $people = $json->getResultAsArray();

            $new_people = array();

            if($with_app) {
                $members_with_app = DB::getInstance()->getUsersWithApp();
            }

            foreach($people as $person) {
                $member = $person->member;

                $id = (int)$member->id;
                $name = (string)$member->name;
                $photo = isset($member->photo->photo_link) ? (string)$member->photo->photo_link : "";
                $intro = isset($member->bio) ? (string)$member->bio : "";
                $response = (string)$person->response;

                $person = new Person($id, $name, $photo, $intro);

                $person->setResponse($response);
                    
                if($with_app) {
                    $has_app = in_array($id, $members_with_app);
                    $person->setHasApp($has_app);
                }

                $new_people[] = $person;
            }

            return json_encode($new_people);
        }

        public static function getMemberInfo() : Person {
            $url =
                "https://api.meetup.com/".DB::$meetupId.
                "/members/self".
                "?access_token=".self::$token;

            $json = self::makeCurl($url);
            $member = $json->getResult();

            $member_id = $member->id;
            $member_name = $member->name;
            $member_photo = $member->photo->photo_link;
            $member_intro = $member->group_profile->intro;

            $person = new Person($member_id, $member_name, $member_photo, $member_intro);
            return $person;
        }

        public static function getAllUsers(string $api_key) : string {
            $meetup_id = DB::$meetupId;

            $url = 
                "https://api.meetup.com/".$meetup_id.
                "/members/self?key=".$api_key;

            $member_data = self::makeCurl($url);
            
            if($member_data->getHttpCode() == 200) {
                $member = $member_data->getResult();

                $member_id = $member->id;
                
                $is_admin = self::checkIsAdmin($member_id, $api_key);
                
                if($is_admin) {
                    $people = DB::getInstance()->getAllUsers();

                    return json_encode($people);
                } else {
                    return "invalid_user";
                }
            } else {
                return "error";
            }
        }

        public function doRSVP($event_id, $send_json) : Result {
            $token = self::$token;

            $json = json_decode($send_json);

            $response = $json->response;

            $url = "https://api.meetup.com/2/rsvp/";

            $query =
                "event_id=".$event_id.
                "&rsvp=".$response.
                "&access_token=".$token;

            foreach($json->answers as $answer) {
                $query .= "&answer_".$answer->id."=".urlencode($answer->answer);
            }

            $query = str_replace("+", "%20", $query);

            $result = self::makeCurl($url, $query);

            return $result;
        }

        public static function getLoginLocation() : string {
            return strtok("http://".$_SERVER['SERVER_NAME'].$_SERVER['REQUEST_URI'],'?')."?meetupid=".DB::$meetupId;
        }

        private static function getMemberStatus() : Result {
            $url = "https://api.meetup.com/".DB::$meetupId.
                "?access_token=".self::$token.
                "&fields=self";

            $result = self::makeCurl($url);

            return $result;
        }

        public static function doLogin(string $code) {
            $url = 'https://secure.meetup.com/oauth2/access';

            $query =
                "client_id=".DB::$meetupClientKey.
                "&client_secret=".DB::$meetupSecretKey.
                "&grant_type=authorization_code".
                "&redirect_uri=".self::getLoginLocation().
                "&code=".$code;

            $json = self::makeCurl($url, $query);
            
            $refresh_token = $json->getResult()->refresh_token;

            self::$token = self::refreshMeetupToken($refresh_token);

            $status = self::getMemberStatus();
            
            if($status->getHttpCode() == 200) {
                if($status->getResult()->self->status != "active") {
                    echo json_encode(
                        array(
                            "is_error" => true,
                            "description" => $status
                        )
                    );
                } else {
                    self::loginSuccess($refresh_token);
                }
            } else {
                echo json_encode(
                    array(
                        "is_error" => true,
                        "description" => "error"
                    )
                );
            }
        }

        private static function loginSuccess(string $refresh_token) {
            $db = DB::getInstance();
            
            $member = self::getMemberInfo();

            $db = $db->getInstance();
            $db->addMember($member);
            
            $qr = self::generateQR($member->getId());
            
            echo json_encode(
                array(
                    "is_error" => false,
                    "refresh_token" => $refresh_token,
                    "qr_code" => $qr
                )
            );
        }

        private static function generateQR(string $member_id) : string {
            $content = file_get_contents(
                "https://api.qrserver.com/v1/create-qr-code/?size=500x500&data=".
                Utils::qrCode.
                $member_id);

            return base64_encode($content);
        }

        public static function refreshMeetupToken(string $refresh_token) : string {
            $url = "https://secure.meetup.com/oauth2/access";

            $query =
                "client_id=".DB::$meetupClientKey.
                "&client_secret=".DB::$meetupSecretKey.
                "&grant_type=refresh_token".
                "&refresh_token=".$refresh_token;

            $data = self::makeCurl($url, $query);
            
            return $data->getResult()->access_token;
        }
        
        public static function checkIsAdmin(int $member_id, string $key = "") : bool {
            $meetup_id = DB::$meetupId;
            $token = self::$token;

            $url1 = "https://api.meetup.com/$meetup_id";
            $url2 = "https://api.meetup.com/$meetup_id/members?filter=stepup_eligible";

            if($token != "") {
                $url1 .= "?access_token=$token";
                $url2 .= "&access_token=$token";
            }

            if($key != "") {
                $url1 .= "?key=$key";
                $url2 .= "&key=$key";
            }

            $get_organizers = self::makeCurl($url1);

            $organizers_ids[] = $get_organizers->getResult()->organizer->id;

            $get_coorganizers = self::makeCurl($url2);

            $coorganizers = $get_coorganizers->getResultAsArray();

            foreach($coorganizers as $coorganizer) {
                $organizers_ids[] = $coorganizer->id;
            }

            // Ids do Meetup de usuários que podem enviar notificações mas não são organizadores
            $extra_members = array(193513345);

            if(in_array($member_id, $extra_members) || in_array($member_id, $organizers_ids)) {
                return true;
            }
            
            return false;
        }

        private static function makeCurl(string $url, string $query = "") : Result {
            $ch = curl_init();

            curl_setopt($ch, CURLOPT_URL, $url);

            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);

            if($query != "") {
                $headers = array(
                    "Content-Type: application/x-www-form-urlencoded"
                );
                
                curl_setopt($ch, CURLOPT_HTTPHEADER, $headers);
                curl_setopt($ch, CURLOPT_POST, true);
                curl_setopt($ch, CURLOPT_POSTFIELDS, $query);
            }
            
            $result = curl_exec($ch);

            if (!curl_errno($ch)) {
                $http_code = curl_getinfo($ch, CURLINFO_HTTP_CODE);
            } else {
                $http_code = 404;
            }

            curl_close($ch);

            $request = new Result($http_code, $result);

            return $request;
        }
    }
}
?>