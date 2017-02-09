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
    class Result {
        private $http_code, $result;

        public function __construct(int $http_code, string $result) {
            $this->http_code = $http_code;
            $this->result = $result;
        }

        public function setHttpCode(int $http_code) { $this->http_code = $http_code; }
        public function getHttpCode() : int { return $this->http_code; }
        public function setResult(string $result) { $this->result = $result; }
        public function getResult() : \stdClass { return json_decode($this->result); }
        public function getResultAsArray() : array { return json_decode($this->result); }
    }
}
?>