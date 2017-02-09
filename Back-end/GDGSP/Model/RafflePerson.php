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

namespace GDGSP\Model {
    class RafflePerson extends Person {
        protected $raffle_date, $post_date, $seconds;

        public function __construct(int $id, string $name, string $photo, string $raffle_date, string $post_date, string $seconds) {
            parent::__construct($id, $name, $photo, "", "");

            $this->raffle_date = $raffle_date;
            $this->post_date = $post_date;
            $this->seconds = $seconds;
        }

        function setRaffle_date(string $raffle_date) { $this->raffle_date = $raffle_date; }
        function getRaffle_date() : string { return $this->raffle_date; }
        function setPost_date(string $post_date) { $this->post_date = $post_date; }
        function getPost_date() : string { return $this->post_date; }
        function setSeconds(string $seconds) { $this->seconds = $seconds; }
        function getSeconds() : string { return $this->seconds; }
    }
}
?>