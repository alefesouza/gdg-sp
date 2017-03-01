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
    class Event extends JsonCommon {
        protected $id, $name, $image, $description, $link, $who, $place, $address, $city, $lat, $lon, $start, $end, $yes_rsvp_count, $rsvp_limit, $waitlist_count, $response, $survey_questions, $answers, $rsvpable, $how_to_find_us, $image_width, $image_height;

        public function __construct(int $id, string $name, string $image, string $description, string $link, string $who, string $place, string $address, string $city, float $lat, float $lon, string $start, string $end, int $yes_rsvp_count, int $rsvp_limit, int $waitlist_count, string $response, array $survey_questions, $answers, bool $rsvpable, string $how_to_find_us) {
            $this->id = $id;
            $this->name = $name;
            $this->image = $image;
            $this->description = $description;
            $this->link = $link;
            $this->who = $who;
            $this->place = $place;
            $this->address = $address;
            $this->city = $city;
            $this->lat = $lat;
            $this->lon = $lon;
            $this->start = $start;
            $this->end = $end;
            $this->yes_rsvp_count = $yes_rsvp_count;
            $this->rsvp_limit = $rsvp_limit;
            $this->waitlist_count = $waitlist_count;
            $this->response = $response;
            $this->survey_questions = $survey_questions;
            $this->answers = $answers;
            $this->rsvpable = $rsvpable;
            $this->how_to_find_us = $how_to_find_us;
        }

        public function setId(int $id) { $this->id = $id; }
        public function getId() : int { return $this->id; }
        public function setName(string $name) { $this->name = $name; }
        public function getName() : string { return $this->name; }
        public function setImage(string $image) { $this->image = $image; }
        public function getImage() : string { return $this->image; }
        public function setDescription(string $description) { $this->description = $description; }
        public function getDescription() : string { return $this->description; }
        public function setLink(string $link) { $this->link = $link; }
        public function getLink() : string { return $this->link; }
        public function setWho(string $who) { $this->who = $who; }
        public function getWho() : string { return $this->who; }
        public function setPlace(string $place) { $this->place = $place; }
        public function getPlace() : string { return $this->place; }
        public function setAddress(string $address) { $this->address = $address; }
        public function getAddress() : string { return $this->address; }
        public function setCity(string $city) { $this->city = $city; }
        public function getCity() : string { return $this->city; }
        public function setLat(float $lat) { $this->lat = $lat; }
        public function getLat() : float { return $this->lat; }
        public function setLon(float $lon) { $this->lon = $lon; }
        public function getLon() : float { return $this->lon; }
        public function setStart(string $start) { $this->start = $start; }
        public function getStart() : string { return $this->start; }
        public function setEnd(string $end) { $this->end = $end; }
        public function getEnd() : string { return $this->end; }
        public function setYes_rsvp_count(int $yes_rsvp_count) { $this->yes_rsvp_count = $yes_rsvp_count; }
        public function getYes_rsvp_count() : int { return $this->yes_rsvp_count; }
        public function setRsvp_limit(int $rsvp_limit) { $this->rsvp_limit = $rsvp_limit; }
        public function getRsvp_limit() : int { return $this->rsvp_limit; }
        public function setWaitlist_count(int $waitlist_count) { $this->waitlist_count = $waitlist_count; }
        public function getWaitlist_count() : int { return $this->waitlist_count; }
        public function setResponse(string $response) { $this->response = $response; }
        public function getResponse() : string { return $this->response; }
        public function setSurvey_questions(array $survey_questions) { $this->survey_questions = $survey_questions; }
        public function getSurvey_questions() : array { return $this->survey_questions; }
        public function setAnswers($answers) { $this->answers = $answers; }
        public function getAnswers() { return $this->answers; }
        public function setRsvpable(bool $rsvpable) { $this->rsvpable = $rsvpable; }
        public function getRsvpable() : bool { return $this->rsvpable; }
        public function setHow_to_find_us(string $how_to_find_us) { $this->how_to_find_us = $how_to_find_us; }
        public function getHow_to_find_us() : string { return $this->how_to_find_us; }
        public function setImage_width(int $image_width) { $this->image_width = $image_width; }
        public function getImage_width() : int { return $this->image_width; }
        public function setImage_height(int $image_height) { $this->image_height = $image_height; }
        public function getImage_height() : int { return $this->image_height; }
    }
}
?>