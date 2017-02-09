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

declare(strict_types = 1);

date_default_timezone_set("America/Sao_Paulo");

header('Content-Type: application/json; charset=utf-8');

spl_autoload_register(function ($name) {
    require str_replace("\\", "/", $name).".php";
});

use GDGSP\Database\DB;
use GDGSP\Util\Utils;

if(isset($_GET["platform"])) {
    Utils::$platform = $_GET["platform"];
}

if(isset($_GET["via"])) {
    Utils::$via = $_GET["via"];
}

$db = DB::getInstance();

if(isset($_GET["meetupid"])) {
    $meetup_id = $_GET["meetupid"];
    DB::init($meetup_id);
}
?>