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

using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using GDG_SP.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace GDG_SP
{
	public partial class TweetsPage : ContentPage
	{
		ObservableCollection<Tweet> listTweets = new ObservableCollection<Tweet>();
		bool itemsAdded, isLoading;
		string max_id;

		public TweetsPage()
		{
			InitializeComponent();

			var recognizer = new TapGestureRecognizer();

			recognizer.Tapped += (s, e) =>
			{
				TweetsLoading.IsVisible = true;
				TweetsError.IsVisible = false;
				GetTweets(max_id);
			};

			TweetsError.GestureRecognizers.Add(recognizer);

			GetTweets();
		}

		/// <summary>
		/// Método que solicita a lista de tweets e preenche a lista.
		/// </summary>
		public async void GetTweets(string max_id = "")
		{
			if (max_id.Equals(""))
			{
				Loading.IsVisible = true;
				ListTweets.IsVisible = false;
			}

			TweetsError.IsVisible = false;
			TweetsLoading.IsVisible = true;

			string jsonString = "";

			try
			{
				var client = new HttpClient();
				client.MaxResponseContentBufferSize = 256000;
				HttpResponseMessage response = await client.GetAsync(Other.Other.GetTweetsUrl(max_id));

				if (response.IsSuccessStatusCode)
				{
					string result = await response.Content.ReadAsStringAsync();

					jsonString = result;
				}
				else
				{
					throw new Exception();
				}
			}
			catch
			{
				TweetsLoading.IsVisible = false;
				TweetsError.IsVisible = true;
				await DisplayAlert("Tweets", "Verifique sua conexão de internet", "OK");
				return;
			}

			JObject root = null;

			try
			{
				if (max_id.Equals(""))
				{
					root = JObject.Parse(jsonString);
					this.max_id = root["max_id"].ToString();
					string tweets = root["tweets"].ToString();
					listTweets = JsonConvert.DeserializeObject<ObservableCollection<Tweet>>(tweets);
					ListTweets.ItemsSource = listTweets;
				}
				else
				{
					root = JObject.Parse(jsonString);
					this.max_id = root["max_id"].ToString();
					string tweets = root["tweets"].ToString();
					ObservableCollection<Tweet> newTweets = JsonConvert.DeserializeObject<ObservableCollection<Tweet>>(tweets);
					foreach (Tweet tweet in newTweets)
					{
						listTweets.Add(tweet);
					}
				}
			}
			catch
			{
				await DisplayAlert("", "Houve um erro ao receber a lista de tweets", "OK");
				return;
			}

			if (!itemsAdded)
			{
				ToolbarItem refreshButton = new ToolbarItem("Atualizar", Other.Other.GetImage("Refresh"), () =>
				{
					GetTweets();
				});

				ToolbarItems.Add(refreshButton);

				itemsAdded = true;

				ListTweets.ItemAppearing += (sender, e) =>
					{
						if (isLoading || listTweets.Count == 0)
							return;

						if ((e.Item as Tweet).Equals(listTweets[listTweets.Count - 1]))
						{
							GetTweets(this.max_id);
							isLoading = true;
						}
					};
			}

			if ((bool)root["more_tweets"])
			{
				isLoading = false;
				TweetsLoading.IsVisible = true;
				TweetsMessage.IsVisible = false;
			}
			else
			{
				isLoading = true;
				TweetsLoading.IsVisible = false;
				TweetsMessage.IsVisible = true;
			}

			Loading.IsVisible = false;
			ListTweets.IsVisible = true;
		}

		private void ListPerson_ItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem == null) return;
			Tweet tweet = e.SelectedItem as Tweet;

			Other.Other.OpenSite(tweet.Link, this, true, "#GDGSP");

			Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
			{
				((ListView)sender).SelectedItem = null;
				return false;
			});
		}
	}
}
