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
 * Objeto que representa um tweet.
 */
public class Tweet {
	private String id, date, link, text, user_name, screen_name, photo;
	private int retweet_count, favorite_count;

	public void setId(String id)
	{
		this.id = id;
	}

	public String getId()
	{
		return id;
	}

	public void setDate(String date)
	{
		this.date = date;
	}

	public String getDate()
	{
		return date;
	}

	public void setLink(String link)
	{
		this.link = link;
	}

	public String getLink()
	{
		return link;
	}

	public void setText(String text)
	{
		this.text = text;
	}

	public String getText()
	{
		return text;
	}

	public void setUser_name(String user_name)
	{
		this.user_name = user_name;
	}

	public String getUser_name()
	{
		return user_name;
	}

	public void setScreen_name(String screen_name)
	{
		this.screen_name = screen_name;
	}

	public String getScreen_name()
	{
		return screen_name;
	}

	public void setPhoto(String photo)
	{
		this.photo = photo;
	}

	public String getPhoto()
	{
		return photo;
	}

	public void setRetweet_count(int retweet_count)
	{
		this.retweet_count = retweet_count;
	}

	public int getRetweet_count()
	{
		return retweet_count;
	}

	public void setFavorite_count(int favorite_count)
	{
		this.favorite_count = favorite_count;
	}

	public int getFavorite_count()
	{
		return favorite_count;
	}
}
