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
    class Notification extends Message {
        protected $data, $ios_badgeType, $ios_badgeCount;
      
        public function __construct(string $app_id, array $included_segments, array $data, array $contents, string $big_picture, array $headings, string $android_accent_color, string $android_led_color, string $small_icon, string $ios_badgeType, int $ios_badgeCount) {
            parent::__construct($app_id, $included_segments, $contents, $big_picture, $headings, $android_accent_color, $android_led_color, $small_icon, "", array());

            $this->data = $data;
            $this->ios_badgeType = $ios_badgeType;
            $this->ios_badgeCount = $ios_badgeCount;
        }

        public function setData(array $data) { $this->data = $data; }
        public function getData() : array { return $this->data; }
        public function setIos_badgeType(string $ios_badgeType) { $this->ios_badgeType = $ios_badgeType; }
        public function getIos_badgeType() : string { return $this->ios_badgeType; }
        public function setIos_badgeCount(string $ios_badgeCount) { $this->ios_badgeCount = $ios_badgeCount; }
        public function getIos_badgeCount() : string { return $this->ios_badgeCount; }
    }
}
?>