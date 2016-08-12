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

/**
 * Objeto que representa uma pessoa no aplicativo.
 */
public class Raffle {
	private int id;
	private String name, photo, raffle_date, post_date, seconds;

	public void setId(int id)
	{
		this.id = id;
	}

	public int getId()
	{
		return id;
	}

	public void setName(String name)
	{
		this.name = name;
	}

	public String getName()
	{
		return name;
	}

	public void setPhoto(String photo)
	{
		this.photo = photo;
	}

	public String getPhoto()
	{
		return photo;
	}

	public void setRaffle_date(String raffle_date)
	{
		this.raffle_date = raffle_date;
	}

	public String getRaffle_date()
	{
		return raffle_date;
	}

	public void setPost_date(String post_date)
	{
		this.post_date = post_date;
	}

	public String getPost_date()
	{
		return post_date;
	}

	public void setSeconds(String seconds)
	{
		this.seconds = seconds;
	}

	public String getSeconds()
	{
		return seconds;
	}}
