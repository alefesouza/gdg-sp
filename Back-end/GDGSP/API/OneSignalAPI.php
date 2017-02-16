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
    use GDGSP\Database\DB;
    use GDGSP\Model\{ Message, Notification };

    class OneSignalAPI {
        public static function sendNotification(int $count, string $title, string $message, string $image, int $event_id) : \stdClass {
            $headings = array(
                "en" => $title
            );

            $content = array(
                "en" => $message
            );

            $app_id = (string)DB::$oneSignalAppId;
            $included_segments = array('All');
            $data = array("eventid" => (int)$event_id);
            $contents = $content;
            $big_picture = $image;
            $headings = $headings;
            $android_accent_color = 'FF008bf8';
            $android_led_color = 'FF008bf8';
            $small_icon = 'ic_notification';
            $ios_badgeType = 'SetTo';
            $ios_badgeCount = $count;

            $notification = new Notification($app_id, $included_segments, $data, $contents, $big_picture, $headings, $android_accent_color, $android_led_color, $small_icon, $ios_badgeType, $ios_badgeCount);

            $fields = json_encode($notification);

            $url = "https://onesignal.com/api/v1/notifications";

            $response = self::makeCurl($url, $fields);

            return $response;
        }

        public static function sendMessage(string $title, string $message, string $image, string $url, int $event_id) : \stdClass {
            $headings = array(
                "en" => $title
            );

            $content = array(
                "en" => $message
            );

            $app_id = DB::$oneSignalAppId;
            $included_segments = array('All');
            $contents = $content;
            $headings = $headings;
            $android_accent_color = 'FF008bf8';
            $android_led_color = 'FF008bf8';
            $small_icon = 'ic_notification';
            $big_picture = $image;
            
            if($event_id != "") {
                $members = MeetupAPI::getRSVPs($event_id);

                foreach($members as $member) {
                    if($member->has_app) {
                        $tags[] = array(
                            "key" => "meetup_id",
                            "relation" => "=",
                            "value" => $member->id
                        );
                    }
                }
            }

            $message = new Message($app_id, $included_segments, $contents, $big_picture, $headings, $android_accent_color, $android_led_color, $small_icon, $url, $tags);
            
            $fields = json_encode($message);

            $url = "https://onesignal.com/api/v1/notifications";

            $response = self::makeCurl($url, $fields);

            return $response;
        }

        public static function getDevices() : \stdClass {
            $url = "https://onesignal.com/api/v1/players?app_id=".DB::$oneSignalAppId;

            $content = self::makeCurl($url);

            return $content;
        }

        private static function makeCurl(string $url, string $fields = "") : \stdClass {
            $ch = curl_init(); 
            curl_setopt($ch, CURLOPT_URL, $url); 
            curl_setopt($ch, CURLOPT_HTTPHEADER, array('Content-Type: application/json', 
                                                        'Authorization: Basic '.DB::$oneSignalRestKey)); 
            curl_setopt($ch, CURLOPT_RETURNTRANSFER, true); 
            curl_setopt($ch, CURLOPT_HEADER, false);

            if($fields != "") {
                curl_setopt($ch, CURLOPT_POST, true);
                curl_setopt($ch, CURLOPT_POSTFIELDS, $fields);
            }

            $response = curl_exec($ch); 
            curl_close($ch);

            return json_decode($response);
        }
    }
}
?>