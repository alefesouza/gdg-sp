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

package org.gdgsp.model;

import java.io.Serializable;
import java.util.List;

/**
 * Objeto que representa um evento no aplicativo.
 */
public class Event implements Serializable {
	private int id, yes_rsvp_count, rsvp_limit, waitlist_count;
	private double lat, lon;
	private String name, image, description, link, who, place, address, city, start, end, response;
	private boolean rsvpable;
	private List<Question> survey_questions, answers;

	public int getId() {
		return id;
	}

	public void setId(int id) {
		this.id = id;
	}

	public int getYes_rsvp_count() {
		return yes_rsvp_count;
	}

	public void setYes_rsvp_count(int yes_rsvp_count) {
		this.yes_rsvp_count = yes_rsvp_count;
	}

	public int getRsvp_limit() {
		return rsvp_limit;
	}

	public void setRsvp_limit(int rsvp_limit) {
		this.rsvp_limit = rsvp_limit;
	}

	public int getWaitlist_count() {
		return waitlist_count;
	}

	public void setWaitlist_count(int waitlist_count) {
		this.waitlist_count = waitlist_count;
	}

	public double getLat() {
		return lat;
	}

	public void setLat(double lat) {
		this.lat = lat;
	}

	public double getLon() {
		return lon;
	}

	public void setLon(double lon) {
		this.lon = lon;
	}

	public String getName() {
		return name;
	}

	public void setName(String name) {
		this.name = name;
	}

	public String getImage() {
		return image;
	}

	public void setImage(String image) {
		this.image = image;
	}

	public String getDescription() {
		return description;
	}

	public void setDescription(String description) {
		this.description = description;
	}

	public String getLink() {
		return link;
	}

	public void setLink(String link) {
		this.link = link;
	}

	public String getWho() {
		return who;
	}

	public void setWho(String who) {
		this.who = who;
	}

	public String getPlace() {
		return place;
	}

	public void setPlace(String place) {
		this.place = place;
	}

	public String getAddress() {
		return address;
	}

	public void setAddress(String address) {
		this.address = address;
	}

	public String getCity() {
		return city;
	}

	public void setCity(String city) {
		this.city = city;
	}

	public String getStart() {
		return start;
	}

	public void setStart(String start) {
		this.start = start;
	}

	public String getEnd() {
		return end;
	}

	public void setEnd(String end) {
		this.end = end;
	}

	public String getResponse() {
		return response;
	}

	public void setResponse(String response) {
		this.response = response;
	}

	public boolean isRsvpable() {
		return rsvpable;
	}

	public void setRsvpable(boolean rsvpable) {
		this.rsvpable = rsvpable;
	}

	public List<Question> getSurvey_questions() {
		return survey_questions;
	}

	public void setSurvey_questions(List<Question> survey_questions) {
		this.survey_questions = survey_questions;
	}

	public List<Question> getAnswers() {
		return answers;
	}

	public void setAnswers(List<Question> answers) {
		this.answers = answers;
	}
}
