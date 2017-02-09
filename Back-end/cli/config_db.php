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

$db_host = "db_host";
$db_login = "db_login";
$db_password = "db_password";
$db_table_name = "db_table_name";

$db = new \PDO("mysql:host=$db_host;dbname=$db_table_name;charset=utf8", $db_login, $db_password);
$db->setAttribute(\PDO::ATTR_ERRMODE, \PDO::ERRMODE_EXCEPTION);

$db->query("CREATE TABLE IF NOT EXISTS meetups (id int auto_increment primary key, meetupid varchar(100), meetupapi_client varchar(50), meetupapi_secret varchar(50), onesignal_appid varchar(50), onesignal_restkey varchar(50), wns_sid varchar(100), wns_clientsecret varchar(50), twitter_key varchar(255), twitter_secret varchar(255), twitter_search varchar(255), cover_image text)");

$db->query("CREATE TABLE IF NOT EXISTS meetup_app_members (id int auto_increment primary key, meetup_id varchar(50), member_id int, member_name varchar(255), has_app boolean, last_activity datetime, faults int)");

$db->query("CREATE TABLE IF NOT EXISTS meetup_last_events (id int auto_increment primary key, meetup_id varchar(50), event_id int)");

// Até a criação desse aplicativo o OneSignal não suportava Windows 10, por isso precisa de um banco de dados para armazenar todos os usuários de Windows 10 e enviar notificações para eles
$db->query("CREATE TABLE IF NOT EXISTS meetup_wns_users (id int auto_increment primary key, device varchar(255), meetupid varchar(50), expire int, member_id int, platform varchar(9), app_version varchar(10))");

// Cadastrar as informações de um meetup, preencha os bindValue
$query = $db->prepare("INSERT INTO meetups (meetupid, meetupapi_client, meetupapi_secret, onesignal_appid, onesignal_restkey, wns_sid, wns_clientsecret, cover_image, last_event) VALUES (:meetup_id, :meetupapi_client, :meetupapi_secret, :onesignal_appid, :onesignal_restkey, :wns_sid, :wns_clientsecret, :twitter_key, :twitter_secret, :twitter_search, :cover_image)");

$query->bindValue(":meetup_id", "", \PDO::PARAM_STR);
$query->bindValue(":meetupapi_client", "", \PDO::PARAM_STR);
$query->bindValue(":meetupapi_secret", "", \PDO::PARAM_STR);
$query->bindValue(":onesignal_appid", "", \PDO::PARAM_STR);
$query->bindValue(":onesignal_restkey", "", \PDO::PARAM_STR);
$query->bindValue(":wns_sid", "", \PDO::PARAM_STR);
$query->bindValue(":wns_clientsecret", "", \PDO::PARAM_STR);
$query->bindValue(":twitter_key", "", \PDO::PARAM_STR);
$query->bindValue(":twitter_secret", "", \PDO::PARAM_STR);
$query->bindValue(":twitter_search", "", \PDO::PARAM_STR);
$query->bindValue(":cover_image", "", \PDO::PARAM_STR);

$query->execute();
?>