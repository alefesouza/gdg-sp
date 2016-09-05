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

using System.Collections.Generic;

namespace GDG_SP.Model
{
	public class Tweet
	{
		public string Id { get; set; }
		public string Date { get; set; }
		public string Link { get; set; }
		public string Text { get; set; }
		public string User_name { get; set; }
		public string Screen_name { get; set; }
		public string Photo { get; set; }
		public List<Media> media { get; set; }
		public int Retweet_count { get; set; }
		public int Favorite_count { get; set; }

		public class Media
		{
			public string Url { get; set; }
		}
	}
}