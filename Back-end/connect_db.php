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

$db_host = "db_host";
$db_login = "db_login";
$db_password = "db_password";
$db_table_name = "db_table_name";

$dbi = mysqli_connect($db_host, $db_login, $db_password, $db_table_name);
$dbi -> set_charset("utf8");

mysqli_query($dbi, "CREATE TABLE IF NOT EXISTS meetups (id int auto_increment primary key, meetupid varchar(100), meetupapi_client varchar(50), meetupapi_secret varchar(50), onesignal_appid varchar(50), onesignal_restkey varchar(50), wns_sid varchar(100), wns_clientsecret varchar(50), cover_image text)") or die("ERROR: ".mysqli_error($dbi));

//mysqli_query($dbi, "INSERT INTO meetups (meetupid, meetupapi_client, meetupapi_secret, onesignal_appid, onesignal_restkey, wns_sid, wns_clientsecret, cover_image, last_event) VALUES ('GDG-SP', 'meetupapi_client', 'meetupapi_secret', 'onesignal_appid', 'onesignal_restkey', 'wns_sid', 'wns_clientsecret', 'http:// cover_image')") or die("ERROR: ".mysqli_error($dbi));

mysqli_query($dbi, "CREATE TABLE IF NOT EXISTS meetup_app_members (id int auto_increment primary key, meetup_id varchar(50), member_id int, last_activity int, faults int)") or die("ERROR: ".mysqli_error($dbi));

mysqli_query($dbi, "CREATE TABLE IF NOT EXISTS meetup_last_events (id int auto_increment primary key, meetup_id varchar(50), event_id int)") or die("ERROR: ".mysqli_error($dbi));

// Até a criação desse aplicativo o OneSignal não suportava Windows 10, por isso precisa de um banco de dados para armazenar todos os usuários de Windows 10 e enviar notificações para eles
mysqli_query($dbi, "CREATE TABLE IF NOT EXISTS meetup_wns_users (id int auto_increment primary key, device varchar(255), meetupid varchar(50), expire int, member_id int, platform varchar(9), app_version varchar(10))") or die("ERROR: ".mysqli_error($dbi));
?>