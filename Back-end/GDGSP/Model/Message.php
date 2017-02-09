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
    class Message extends JsonCommon {
        protected $app_id, $included_segments, $contents, $big_picture, $headings, $android_accent_color, $android_led_color, $small_icon, $url, $tags;

        public function __construct(string $app_id, array $included_segments, array $contents, string $big_picture, array $headings, string $android_accent_color, string $android_led_color, string $small_icon, string $url, array $tags) {
            $this->app_id = $app_id;
            $this->included_segments = $included_segments;
            $this->contents = $contents;
            $this->big_picture = $big_picture;
            $this->headings = $headings;
            $this->android_accent_color = $android_accent_color;
            $this->android_led_color = $android_led_color;
            $this->small_icon = $small_icon;
            $this->url = $url;
        }

        public function setApp_id(string $app_id) { $this->app_id = $app_id; }
        public function getApp_id() : string { return $this->app_id; }
        public function setIncluded_segments(array $included_segments) { $this->included_segments = $included_segments; }
        public function getIncluded_segments() : array { return $this->included_segments; }
        public function setContents(array $contents) { $this->contents = $contents; }
        public function getContents() : array { return $this->contents; }
        public function setBig_picture(string $big_picture) { $this->big_picture = $big_picture; }
        public function getBig_picture() : string { return $this->big_picture; }
        public function setHeadings(array $headings) { $this->headings = $headings; }
        public function getHeadings() : array { return $this->headings; }
        public function setAndroid_accent_color(string $android_accent_color) { $this->android_accent_color = $android_accent_color; }
        public function getAndroid_accent_color() : string { return $this->android_accent_color; }
        public function setAndroid_led_color(string $android_led_color) { $this->android_led_color = $android_led_color; }
        public function getAndroid_led_color() : string { return $this->android_led_color; }
        public function setSmall_icon(string $small_icon) { $this->small_icon = $small_icon; }
        public function getSmall_icon() : string { return $this->small_icon; }
        public function setUrl(string $url) { $this->url = $url; }
        public function getUrl() : string { return $this->url; }
        public function setTags(string $tags) { $this->tags = $tags; }
        public function getTags() : string { return $this->tags; }
    }
}
?>