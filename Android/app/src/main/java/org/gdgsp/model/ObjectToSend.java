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

import java.util.List;
import java.util.ArrayList;

/**
 * Objeto que é enviado ao servidor ao fazer RSVP.
 */
public class ObjectToSend {
	private String response;
	private List<Question> answers;

	public void setResponse(String response) {
		this.response = response;
	}

	public String getResponse() {
		return response;
	}

	public void setAnswers(List<Question> answers) {
		this.answers = answers;
	}

	public List<Question> getAnswers() {
		return answers;
	}
}
