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
    class Tweet extends JsonCommon {
        protected $id, $date, $link, $text, $user_name, $screen_name, $photo, $media, $retweet_count, $favorite_count;

        public function __construct(string $id, string $date, string $link, string $text, string $user_name, string $screen_name, string $photo, array $media, int $retweet_count, int $favorite_count) {
            $this->id = $id;
            $this->date = $date;
            $this->link = $link;
            $this->text = $text;
            $this->user_name = $user_name;
            $this->screen_name = $screen_name;
            $this->photo = $photo;
            $this->media = $media;
            $this->retweet_count = $retweet_count;
            $this->favorite_count = $favorite_count;
        }

        public function setId(string $id) { $this->id = $id; }
        public function getId() : string { return $this->id; }
        public function setDate(string $date) { $this->date = $date; }
        public function getDate() : string { return $this->date; }
        public function setLink(string $link) { $this->link = $link; }
        public function getLink() : string { return $this->link; }
        public function setText(string $text) { $this->text = $text; }
        public function getText() : string { return $this->text; }
        public function setUser_name(string $user_name) { $this->user_name = $user_name; }
        public function getUser_name() : string { return $this->user_name; }
        public function setScreen_name(string $screen_name) { $this->screen_name = $screen_name; }
        public function getScreen_name() : string { return $this->screen_name; }
        public function setPhoto(string $photo) { $this->photo = $photo; }
        public function getPhoto() : string { return $this->photo; }
        public function setMedia(array $media) { $this->media = $media; }
        public function getMedia() : array { return $this->media; }
        public function setRetweet_count(int $retweet_count) { $this->retweet_count = $retweet_count; }
        public function getRetweet_count() : int { return $this->retweet_count; }
        public function setFavorite_count(int $favorite_count) { $this->favorite_count = $favorite_count; }
        public function getFavorite_count() : int { return $this->favorite_count; }
    }
}
?>