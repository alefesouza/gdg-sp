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
using ImageCircle.Forms.Plugin.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.Linq;

namespace GDG_SP
{
    /// <summary>
    /// Página onde é exibido os links e informações de login do aplicativo.
    /// </summary>
    public partial class LinksPage : ContentPage
    {
        public ObservableCollection<Link> listLinks = new ObservableCollection<Link>();
        public CircleImage profileImage;
        public Label profileName, profileIntro;
        public Person member = null;

		private static LinksPage instance;

		public static LinksPage Instance
		{
			get
			{
				return instance;
			}
		}

        public LinksPage()
        {
            InitializeComponent();

            instance = this;

            profileImage = ProfileImage;
            profileName = ProfileName;
            profileIntro = ProfileIntro;

            if (Other.Other.GetSetting("member_profile").Length > 0)
            {
               member = JsonConvert.DeserializeObject<Person>(Other.Other.GetSetting("member_profile"));

                if (!member.Name.Equals(""))
                {
                    ProfileImage.Source = ImageSource.FromUri(new Uri(member.Photo.ToString()));
                    ProfileName.Text = member.Name.ToString();
                    ProfileIntro.Text = member.Intro.ToString();
                }
            }
            else
            {
                string image = "";

                image = Device.OnPlatform("Icon-72.png", "ic_launcher.png", "Assets/Square71x71Logo.scale-140.png");

                if(Device.Idiom == TargetIdiom.Desktop)
                {
                    image = "Assets/Square71x71Logo.scale-150.png";
                }
                else if((Device.Idiom == TargetIdiom.Tablet && Device.OS == TargetPlatform.Windows))
                {
                    image = "Assets/Square70x70Logo.scale-100.png";
                }

                ProfileImage.Source = ImageSource.FromFile(image);
                ProfileName.Text = "Fazer login";
            }
            
            listLinks.Add(new Link() { Title = "Site", Icon = Other.Other.GetImage("Site"), Value = AppResources.SiteUrl });
			listLinks.Add(new Link() { Title = "Meetups antigos", Icon = Other.Other.GetImage("YouTube"), Value = AppResources.OldMeetupsUrl });
            listLinks.Add(new Link() { Title = "Facebook", Icon = Other.Other.GetImage("Facebook"), Value = AppResources.FacebookUrl });
            listLinks.Add(new Link() { Title = "Google+", Icon = Other.Other.GetImage("Google"), Value = AppResources.GooglePlusUrl });
            listLinks.Add(new Link() { Title = "Instagram", Icon = Other.Other.GetImage("Instagram"), Value = AppResources.InstagramUrl });
            listLinks.Add(new Link() { Title = "Twitter", Icon = Other.Other.GetImage("Twitter"), Value = AppResources.TwitterUrl });
            listLinks.Add(new Link() { Title = "YouTube", Icon = Other.Other.GetImage("YouTube"), Value = AppResources.YouTubeUrl });
            listLinks.Add(new Link() { Title = "Contato", Icon = Other.Other.GetImage("Mail"), Value = AppResources.ContactMail });

            ListLinks.ItemsSource = listLinks;

            var login = new TapGestureRecognizer();
            login.Tapped += (s, e) => {
                if(member == null)
                {
					Navigation.PushAsync(new WebViewPage(Other.Other.GetLoginUrl(), true) { Title = "Login" });
                }
                else
                {
                    Other.Other.OpenSite("meetup.com/" + AppResources.MeetupId + "/member/" + member.Id, this, true, member.Name);
                }
            };
            ProfileGrid.GestureRecognizers.Add(login);
        }

		private async void ListLinks_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            Link item = e.SelectedItem as Link;

            if(item.Value.Equals("send_notification"))
            {
                await Navigation.PushAsync(new SendNotificationPage());
            }
            else if (item.Value.Equals("raffle_manager"))
			{
				string action = await DisplayActionSheet("Escolha o tipo de evento", "Cancelar", null, "Futuros", "Anteriores");

				int id = 0;
				string[] events;
				ObservableCollection<Event> lEv;

				if (action.Equals("Futuros"))
				{

					events = MainPage.Instance.listEvents.Select(_event => _event.Name).ToArray();
					string actionEvents = await DisplayActionSheet("Escolha o evento", "Cancelar", null, events);
					lEv = new ObservableCollection<Event>(MainPage.Instance.listEvents.Where(_event => _event.Name.Equals(actionEvents)));
				}
				else
				{
					events = PastEventsPage.Instance.listEvents.Select(_event => _event.Name).ToArray();
					string actionEvents = await DisplayActionSheet("Escolha o evento", "Cancelar", null, events);
					lEv = new ObservableCollection<Event>(PastEventsPage.Instance.listEvents.Where(_event => _event.Name.Equals(actionEvents)));
				}

				id = lEv[0].Id;

				await Navigation.PushAsync(new RaffleManagerPage(id));
			}
			else if(item.Title.Equals("Contato"))
            {
				Device.OpenUri(new Uri("mailto:" + AppResources.ContactMail));
            }
            else
            {
                Other.Other.OpenSite(item.Value, this, true, item.Title);
            }

            Device.StartTimer(TimeSpan.FromMilliseconds(50), () => {
                ((ListView)sender).SelectedItem = null;
                return false;
            });
        }
    }
}
