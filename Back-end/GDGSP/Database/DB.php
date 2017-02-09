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

namespace GDGSP\Database {
    use GDGSP\Model\{ Person, RafflePerson };

    class DB {
        private $db;
        private static $instance;

        // ID do Meetup atual
        public static $meetupId;

        // API do Meetup
        public static $meetupClientKey;
        public static $meetupSecretKey;

        // API do Twitter
        public static $twitterKey;
        public static $twitterSecret;
        public static $twitterSearch;
        
        // Notificações para One Signal
        public static $oneSignalAppId;
        public static $oneSignalRestKey;
        
        // Notificações para Windows 10
        public static $WNS_SId;
        public static $WNS_ClientSecret;

        // Capa dos aplicativos
        public static $headerImage;

        private function __construct() {
            $db_host = "";
            $db_login = "";
            $db_password = "";
            $db_table_name = "";
          
            $this->db = new \PDO("mysql:host=$db_host;dbname=$db_table_name;charset=utf8", $db_login, $db_password);
            $this->db->setAttribute(\PDO::ATTR_ERRMODE, \PDO::ERRMODE_EXCEPTION);
        }

        public static function init(string $meetupId) {
            self::$meetupId = $meetupId;

            $query = self::getInstance()->db->prepare("SELECT * FROM meetups WHERE meetupid=:meetup");
            $query->bindValue(':meetup', $meetupId, \PDO::PARAM_STR);
            $query->execute();
            
            while($row = $query->fetch(\PDO::FETCH_ASSOC)) {
                self::$meetupClientKey = $row["meetupapi_client"];
                self::$meetupSecretKey = $row["meetupapi_secret"];

                self::$oneSignalAppId = $row["onesignal_appid"];
                self::$oneSignalRestKey = $row["onesignal_restkey"];

                self::$WNS_SId = urlencode($row["wns_sid"]);
                self::$WNS_ClientSecret = urlencode($row["wns_clientsecret"]);

                self::$twitterKey = $row["twitter_key"];
                self::$twitterSecret = $row["twitter_secret"];
                self::$twitterSearch = $row["twitter_search"];

                self::$headerImage = $row["header_image"];
            }
        }

        public static function getInstance() {
            if (!isset(self::$instance)) {
                $object = __CLASS__;
                self::$instance = new $object;
            }
            return self::$instance;
        }

        public function addMember(Person $member) : bool {
            try{ 
                $query = $this->db->prepare("SELECT (id) FROM meetup_app_members WHERE member_id=:member_id");
                $query->bindValue(':member_id', $member->getId(), \PDO::PARAM_INT);
                $query->execute();
                $rowCount = $query->fetchColumn(0);
                
                if($rowCount == 0) {
                    $query = $this->db->prepare("INSERT INTO meetup_app_members (meetup_id, member_id, member_name, has_app) VALUES (:meetup_id, :member_id, :member_name, :has_app");

                    $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                    $query->bindValue(':member_id', $member->getId(), \PDO::PARAM_STR);
                    $query->bindValue(':member_name', $member->getName(), \PDO::PARAM_STR);
                    $query->bindValue(':has_app', 1, \PDO::PARAM_INT);

                    $query->execute();
                } else {
                    $query = $this->db->prepare("UPDATE meetup_app_members SET has_app=:has_app WHERE member_id=:member_id AND meetup_id=:meetup_id");

                    $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                    $query->bindValue(':member_id', $member->getId(), \PDO::PARAM_STR);
                    $query->bindValue(':has_app', 1, \PDO::PARAM_INT);
                    
                    $query->execute();
                }

                return true;
            } catch(\PDOException $e) {
                return false;
            }
        }

        public function addRaffle(Person $member, int $event_id, string $raffle_date, int $seconds) : bool {
            try{
                $query = $this->db->prepare("INSERT INTO meetup_raffle_manager (event_id, member_id, member_name, member_photo, meetup_id, raffle_date, post_date, seconds) VALUES (:event_id, :member_id, :member_name, :member_photo, :meetup_id, :raffle_date, now(), :seconds)");

                $query->bindValue(':event_id', $event_id, \PDO::PARAM_INT);
                $query->bindValue(':member_id', $member->getId(), \PDO::PARAM_STR);
                $query->bindValue(':member_name', $member->getName(), \PDO::PARAM_STR);
                $query->bindValue(':member_photo', $member->getPhoto(), \PDO::PARAM_STR);
                $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                $query->bindValue(':raffle_date', $raffle_date, \PDO::PARAM_STR);
                $query->bindValue(':seconds', $seconds, \PDO::PARAM_INT);

                $query->execute();
                return true;
            } catch(\PDOException $e) {
                return false;
            }
        }

        public function clearRaffle(int $event_id) : bool {
            try{
                $query = $this->db->prepare("DELETE FROM meetup_raffle_manager WHERE event_id=:event_id");
                $query->bindValue(':event_id', $event_id, \PDO::PARAM_INT);
                $query->execute();

                return true;
            } catch(\PDOException $e) {
                return false;
            }
        }

        public function getAllRaffle(int $event_id) : array {
            try {
                $query = $this->db->prepare("SELECT *, DATE_FORMAT(raffle_date,'%H:%i:%s %d/%m/%Y') AS raffle_date_format, DATE_FORMAT(post_date,'%H:%i:%s %d/%m/%Y') AS post_date_format FROM meetup_raffle_manager WHERE meetup_id=:meetup_id AND event_id=:event_id ORDER BY raffle_date");

                $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                $query->bindValue(':event_id', $event_id, \PDO::PARAM_INT);
                $query->execute();
                
                $raffle_people = array();
                
                while($row = $query->fetch(\PDO::FETCH_ASSOC)) {
                    $id = $row["member_id"];
                    $name = $row["member_name"];
                    $photo = $row["member_photo"];
                    $raffle_date = $row["raffle_date_format"];
                    $post_date = $row["post_date_format"];
                    $seconds = (string)$row["seconds"];

                    $raffle_people[] = new RafflePerson($id, $name, $photo, $raffle_date, $post_date, $seconds);
                }

                return $raffle_people;
            } catch(\PDOException $e) {
                var_dump($e);
                return array("error" => true);
            }
        }

        public function updateActivity(int $member_id) : bool {
            try {
                $query = $this->db->prepare("UPDATE meetup_app_members SET last_activity=now() WHERE member_id=:member_id");
                $query->bindValue(':member_id', $member_id, \PDO::PARAM_INT);
                $query->execute();
                return true;
            } catch(\PDOException $e) {
                return false;
            }
        }

        public function updateWindowsDevice(int $member_id, string $device) : bool {
            try {
                $query = $this->db->prepare("UPDATE meetup_wns_users SET member_id=:member_id WHERE device=:device");
                $query->bindValue(':member_id', $member_id, \PDO::PARAM_INT);
                $query->bindValue(':device', $device, \PDO::PARAM_STR);
                $query->execute();
                return true;
            } catch(\PDOException $e) {
                return false;
            }
        }

        public function getAllUsers() : array {
            try {
                $query = $this->db->prepare("SELECT member_id, member_name, faults FROM meetup_app_members WHERE meetup_id=:meetup_id AND faults > :number_faults ORDER BY faults DESC");
                $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                $query->bindValue(':number_faults', 0, \PDO::PARAM_INT);
                $query->execute();
                
                $people = array();

                while($row = $query->fetch(\PDO::FETCH_ASSOC)) {
                    $people[] = array(
                        "id" => (int)$row["member_id"],
                        "name" => (string)$row["member_name"],
                        "faults" => (int)$row["faults"]
                    );
                }

                var_dump($people);

                return $people;
            } catch(\PDOException $e) {
                return array("error" => true);
            }
        }

        public function getUsersWithApp() : array {
            try {
                $query = $this->db->prepare("SELECT member_id FROM meetup_app_members WHERE meetup_id=:meetup_id AND has_app=:has_app");
                $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                $query->bindValue(':has_app', 1, \PDO::PARAM_INT);
                $query->execute();
                
                $users_with_app = array();

                while($row = $query->fetch(\PDO::FETCH_ASSOC)) {
                    $users_with_app[] = $row["member_id"];
                }

                return $users_with_app;
            } catch(\PDOException $e) {
                return array("error" => true);
            }
        }

        public function deleteUser(int $member_id) : bool {
            try {
                $query = $this->db->prepare("UPDATE meetup_app_members SET has_app=:has_app WHERE member_id=:member_id");

                $query->bindValue(':member_id', $member_id, \PDO::PARAM_INT);
                $query->bindValue(':has_app', 0, \PDO::PARAM_INT);

                $query->execute();
                return true;
            } catch(\PDOException $e) {
                return false;
            }
        }

        public function getLastNotifiedEvents() : array {
            try {
                $query = $this->db->prepare("SELECT event_id FROM meetup_last_events WHERE meetup_id=:meetup_id");
                $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                $query->execute();

                $last_events = array();

                while($row = $query->fetch(\PDO::FETCH_ASSOC)) {
                    $last_events[] = $row["event_id"];
                }

                return $last_events;
            } catch(\PDOException $e) {
                return array("error" => true);
            }
        }

        public function addEventNotification(int $event_id) : bool {
            try {
                $query = $this->db->prepare("INSERT INTO meetup_last_events (meetup_id, event_id) VALUES (:meetup_id, :event_id)");
                $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                $query->bindValue(':event_id', $event_id, \PDO::PARAM_INT);
                $query->execute();
                return true;
            } catch(\PDOException $e) {
                return false;
            }
        }

        public function addWNSUsers(string $channel_uri, int $expire, string $platform, string $app_version) : bool {
            try {
                $query = $this->db->prepare("INSERT INTO meetup_wns_users (device, meetupid, expire, platform, app_version) VALUES (:channel_uri, :meetup_id, :expire, :platform, :app_version)");

                $query->bindValue(':channel_uri', $channel_uri, \PDO::PARAM_STR);
                $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                $query->bindValue(':expire', $expire, \PDO::PARAM_INT);
                $query->bindValue(':platform', $platform, \PDO::PARAM_STR);
                $query->bindValue(':app_version', $app_version, \PDO::PARAM_STR);

                $query->execute();

                return true;
            } catch(\PDOException $e) {
                return false;
            }
        }

        public function updateWNSUser(string $channel_uri, int $expire, string $channel) : bool {
            try {
                $query = $this->db->prepare("UPDATE meetup_wns_users SET device=:channel_uri, expire=:expire WHERE device=:channel AND meetupid=:meetup_id");

                $query->bindValue(':channel_uri', $channel_uri, \PDO::PARAM_STR);
                $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                $query->bindValue(':expire', $expire, \PDO::PARAM_INT);
                $query->bindValue(':channel', $channel, \PDO::PARAM_STR);

                $query->execute();
                return true;
            } catch(\PDOException $e) {
                return false;
            }
        }

        public function getSingleWNSUser(string $device) : array {
            try {
                $query = $this->db->prepare("SELECT device FROM meetup_wns_users WHERE device=:device AND meetupid=:meetup_id");
                $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                $query->bindValue(':device', $device, \PDO::PARAM_STR);
                $query->execute();

                $row = $query->fetch(\PDO::FETCH_ASSOC);

                $user = array("member_id" => $row["member_id"], "device" => $row["device"]);

                return $user;
            } catch(\PDOException $e) {
                return array("error" => true);
            }
        }

        public function getWNSUsers() : array {
            try {
                $query = $this->db->prepare("SELECT device, member_id FROM meetup_wns_users WHERE meetupid=:meetup_id");
                $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                $query->execute();

                $devices = array();

                while($row = $query->fetch(\PDO::FETCH_ASSOC)) {
                    $devices[] = array("member_id" => $row["member_id"], "device" => $row["device"]);
                }

                return $devices;
            } catch(\PDOException $e) {
                return array("error" => true);
            }
        }

        public function clearWnsUsers() : bool {
            $checkExpire = date("Ymd", time());
            
            try {
                $query = $this->db->prepare("DELETE FROM meetup_wns_users WHERE expire < :expire");
                $query->bindValue(':expire', $checkExpire, \PDO::PARAM_INT);
                $query->execute();
                return true;
            } catch(\PDOException $e) {
                return false;
            }
        }

        public function manageMeetupMember(int $member_id, string $member_name) : bool {
            try {
                $query = $this->db->prepare($dbi, "SELECT * FROM meetup_app_members WHERE meetup_id=:meetup_id AND member_id=:member_id");

                $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                $query->bindValue(':member_id', $member_id, \PDO::PARAM_INT);

                $query->execute();

                $rowCount = $query->fetchColumn(0);
                
                if($rowCount == 0) {
                    $query = $this->db->prepare($dbi, "INSERT INTO meetup_app_members (meetup_id, member_id, member_name, has_app, last_activity, faults) VALUES (:meetup_id, :member_id, :member_name, :has_app, :last_activity, :faults)");

                    $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                    $query->bindValue(':member_id', $member_id, \PDO::PARAM_INT);
                    $query->bindValue(':member_name', $member_name, \PDO::PARAM_STR);
                    $query->bindValue(':has_app', 0, \PDO::PARAM_INT);
                    $query->bindValue(':last_activity', "0", \PDO::PARAM_STR);
                    $query->bindValue(':faults', 1, \PDO::PARAM_INT);

                    $query->execute();
                } else {
                    $query = $this->db->prepare($dbi, "UPDATE meetup_app_members SET faults=(faults + 1) WHERE meetup_id=:meetup_id AND member_id=:member_id");

                    $query->bindValue(':meetup_id', self::$meetupId, \PDO::PARAM_STR);
                    $query->bindValue(':member_id', $member_id, \PDO::PARAM_INT);

                    $query->execute();
                }
                return true;
            } catch(\PDOException $e) {
                return false;
            }
        }
    }
}
?>