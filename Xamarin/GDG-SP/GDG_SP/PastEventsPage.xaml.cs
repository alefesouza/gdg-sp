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

using GDG_SP.Model;
using GDG_SP.Resx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Share;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Diagnostics;
using System.Net.Http;

namespace GDG_SP
{
	/// <summary>
	/// Página onde é exibido os eventos anteriores.
	/// </summary>
	public partial class PastEventsPage : ContentPage
	{
		public ObservableCollection<Event> listEvents;
		Event _event;
		public static int openEvent = 0;
		double imageWidth = 0;
		ToolbarItem more;
		bool tabletMenu, isLoading;
		int page = 2;

		private static PastEventsPage instance;

		public static PastEventsPage Instance
		{
			get
			{
				return instance;
			}
		}

		public PastEventsPage()
		{
			InitializeComponent();

			instance = this;

			if (Device.Idiom == TargetIdiom.Desktop)
			{
				Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
				{
					Column1.Width = 350;
					Column2.Width = new GridLength(1, GridUnitType.Star);
					return false;
				});
			}

			if (Device.Idiom == TargetIdiom.Tablet || Device.Idiom == TargetIdiom.Desktop)
			{
				imageWidth = 350;
			}

			if (Device.Idiom == TargetIdiom.Phone)
			{
				MainGrid.Children.Remove(EventWebView);
				MainGrid.ColumnDefinitions.Remove(Column2);
			}

			more = new ToolbarItem("Atualizar", Other.Other.GetImage("Refresh"), () =>
			{
				if (!tabletMenu)
				{
					ForceLayout();
					//MainPage.Instance.GetEvents(true);
				}
			});

			ToolbarItems.Add(more);

			TryAgain.Clicked += (s, e) =>
			{
				MainPage.Instance.GetEvents();
			};

			ListEvents.ItemAppearing += (sender, e) =>
				{
					if (isLoading || listEvents.Count == 0)
						return;

					if ((e.Item as Event).Equals(listEvents[listEvents.Count - 1]))
					{
						GetEvents(this.page);
						isLoading = true;
					}
				};

			var recognizer = new TapGestureRecognizer();

			recognizer.Tapped += (s, e) =>
			{
				EventsLoading.IsVisible = true;
				EventsError.IsVisible = false;
				GetEvents(page);
			};

			EventsError.GestureRecognizers.Add(recognizer);
		}

		public void GetEvents(string jsonString, bool refresh = false)
		{
			ErrorScreen.IsVisible = false;
			ListEvents.IsVisible = false;
			Loading.IsVisible = true;

			if (jsonString.Equals(""))
			{
				return;
			}

			listEvents = new ObservableCollection<Event>();

			JObject root = null;

			try
			{
				root = JObject.Parse(jsonString);
				listEvents = JsonConvert.DeserializeObject<ObservableCollection<Event>>(root["past_events"].ToString());
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.StackTrace);
				ErrorMessage.Text = "Houve um erro ao receber os eventos";
				ErrorScreen.IsVisible = true;
				Loading.IsVisible = false;
				return;
			}

			HeaderImage.Source = ImageSource.FromUri(new Uri(root["header"].ToString()));

			if (listEvents.Count > 0)
			{
				if (imageWidth > 0)
				{
					for (int i = 0; i < listEvents.Count; i++)
					{
						listEvents[i].HeightRequest = Other.Other.GetHeightImage(imageWidth - 20, listEvents[i].Image_width, listEvents[i].Image_height);
					}
				}

				HeaderImage.SizeChanged += (s, e) =>
				{
					ForceLayout();

					if (imageWidth == 0)
					{
						imageWidth = HeaderImage.Width;
					}

					HeaderImage.HeightRequest = Other.Other.GetHeightImage(imageWidth, (double)root["header_width"], (double)root["header_height"]);

					if (imageWidth > 0)
					{
						for (int i = 0; i < listEvents.Count; i++)
						{
							listEvents[i].HeightRequest = Other.Other.GetHeightImage(imageWidth - 20, listEvents[i].Image_width, listEvents[i].Image_height);
						}
					}

					ListEvents.ItemsSource = listEvents;
					ForceLayout();
				};

				ListEvents.ItemsSource = listEvents;

				if ((Device.Idiom == TargetIdiom.Desktop || Device.Idiom == TargetIdiom.Tablet || Device.Idiom == TargetIdiom.Desktop) && !refresh)
				{
					_event = listEvents[0];

					if (!tabletMenu)
					{
						ToolbarItems.Remove(more);

						ToolbarItems.Add(new ToolbarItem(_event.Who, Other.Other.GetImage("People"), () =>
						{
							Navigation.PushAsync(new TabbedPeoplePage(_event));
						}));

						ToolbarItems.Add(new ToolbarItem("Compartilhar", Other.Other.GetImage("Share"), () =>
						{
							Other.Other.ShareLink(_event.Name, _event.Link);
						}));

						more.Command = new Command(async () =>
						{
							string action = await DisplayActionSheet("Menu", "Cancelar", null, "Atualizar", "Abrir meetup", "Abrir no navegador");

							if (action != null)
							{
								if (action.Equals("Atualizar"))
								{
									MainPage.Instance.GetEvents(true);
								}
								else if (action.Equals("Abrir no navegador"))
								{
									Other.Other.OpenSite(_event.Link, this);
								}
							}
						});

						ToolbarItems.Add(more);

						tabletMenu = true;
					}
				}

				if (openEvent != 0)
				{
					foreach (Event e in listEvents)
					{
						if (e.Id == openEvent)
						{
							ListEvents.SelectedItem = e;
						}
					}
					openEvent = 0;
				}
				else if (Device.Idiom == TargetIdiom.Desktop || Device.Idiom == TargetIdiom.Tablet)
				{
					_event = listEvents[0];
					TabletPost();
				}

				ListEvents.IsVisible = true;
			}
			else
			{
				ErrorMessage.Text = "Não há eventos futuros";
				ErrorScreen.IsVisible = true;
			}

			Loading.IsVisible = false;
		}

		public async void GetEvents(int page)
		{
			EventsError.IsVisible = false;
			EventsLoading.IsVisible = true;

			string jsonString = "";

			try
			{
				var client = new HttpClient();
				client.MaxResponseContentBufferSize = 256000;
				HttpResponseMessage response = await client.GetAsync(Other.Other.GetEventsUrl(page));

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
				EventsLoading.IsVisible = false;
				EventsError.IsVisible = true;
				await DisplayAlert("Tweets", "Verifique sua conexão de internet", "OK");
				return;
			}

			JObject root = null;

			try
			{
				root = JObject.Parse(jsonString);
				this.page++;
				string events = root["past_events"].ToString();
				ObservableCollection<Event> newEvents = JsonConvert.DeserializeObject<ObservableCollection<Event>>(events);
				foreach (Event _event in newEvents)
				{
					_event.HeightRequest = Other.Other.GetHeightImage(imageWidth - 20, _event.Image_width, _event.Image_height);
					listEvents.Add(_event);
				}
			}
			catch
			{
				await DisplayAlert("", "Houve um erro ao receber a lista de tweets", "OK");
				return;
			}

			if ((bool)root["more_past_events"])
			{
				isLoading = false;
				EventsLoading.IsVisible = true;
				EventsMessage.IsVisible = false;
			}
			else
			{
				isLoading = true;
				EventsLoading.IsVisible = false;
				EventsMessage.IsVisible = true;
			}

			Loading.IsVisible = false;
			ListEvents.IsVisible = true;
		}

		private void TabletPost()
		{
			EventWebView.Navigating -= WView_Navigating;
			string content = _event.Description;
			var htmlSource = new HtmlWebViewSource();
			htmlSource.Html = content;

			EventWebView.Source = htmlSource;
			EventWebView.Navigating += WView_Navigating;
		}

		private void WView_Navigating(object sender, WebNavigatingEventArgs e)
		{
			if (e.Url.StartsWith("http://do_login"))
			{
				Navigation.PushAsync(new WebViewPage(Other.Other.GetLoginUrl(), true) { Title = "Login" });
				e.Cancel = true;
			}
			else if (e.Url.StartsWith("http"))
			{
				Other.Other.OpenSite(e.Url, this);
				e.Cancel = true;
			}
		}

		// Menu ao pressionar um evento na lista no Windows Phone ou arrastar no iOS
		private async void HandleEventItem(object sender, EventArgs e)
		{
			Event _event = (sender as MenuItem).BindingContext as Event;

			if (_event == null)
				return;

			string action;

			if (Device.OS == TargetPlatform.Windows)
			{
				action = await DisplayActionSheet(_event.Name, "Cancelar", null, "Copiar link", "Compartilhar", "Abrir no navegador");
			}
			else
			{
				// O compartilhar do iOS já tem um copiar link
				action = await DisplayActionSheet(_event.Name, "Cancelar", null, "Compartilhar", "Abrir no navegador");
			}

			if (action != null)
			{
				if (action.Equals("Copiar link"))
				{
					await CrossShare.Current.SetClipboardText(_event.Link);
					Other.Other.ShowMessage("Link copiado com sucesso", this);
				}
				else if (action.Equals("Compartilhar"))
				{
					await CrossShare.Current.ShareLink(_event.Link, _event.Name, _event.Name);
				}
				else if (action.Equals("Abrir no navegador"))
				{
					Other.Other.OpenSite(_event.Link, this);
				}
			}
		}

		private async void ListEvents_ItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem == null) return;
			Event _event = e.SelectedItem as Event;

			if (Device.Idiom == TargetIdiom.Desktop || Device.Idiom == TargetIdiom.Tablet)
			{
				this._event = _event;
				TabletPost();
			}
			else
			{
				await Navigation.PushAsync(new EventPage(_event, true));
			}

			Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
			{
				((ListView)sender).SelectedItem = null;
				return false;
			});
		}
	}
}