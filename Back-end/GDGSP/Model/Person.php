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
    class Person extends JsonCommon {
        protected $id, $name, $photo, $intro, $response, $has_app, $is_admin;

        public function __construct(int $id, string $name, string $photo, string $intro) {
            $this->id = $id;
            $this->name = $name;
            $this->photo = $photo;
            $this->intro = $intro;
        }

        public function setId(int $id) { $this->id = $id; }
        public function getId() : int { return $this->id; }
        public function setName(string $name) { $this->name = $name; }
        public function getName() : string { return $this->name; }
        public function setPhoto(string $photo) { $this->photo = $photo; }
        public function getPhoto() : string { return $this->photo; }
        public function setIntro(string $intro) { $this->intro = $intro; }
        public function getIntro() : string { return $this->intro; }
        public function setStatus(string $status) { $this->status = $status; }
        public function getStatus() : string { return $this->status; }
        public function setResponse(string $response) { $this->response = $response; }
        public function getResponse() : string { return $this->response; }
        public function setHasApp(bool $has_app) { $this->has_app = $has_app; }
        public function getHasApp() : bool { return $this->has_app; }
        public function setIsAdmin(bool $is_admin) { $this->is_admin = $is_admin; }
        public function isAdmin() : bool { return $this->is_admin; }
    }
}
?>