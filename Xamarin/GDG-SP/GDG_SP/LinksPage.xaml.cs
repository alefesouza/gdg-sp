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

namespace GDG_SP
{
    /// <summary>
    /// Página onde é exibido os links e informações de login do aplicativo.
    /// </summary>
    public partial class LinksPage : ContentPage
    {
        ObservableCollection<Link> listLinks = new ObservableCollection<Link>();
        public static LinksPage linksPage;
        public CircleImage profileImage;
        public Label profileName, profileIntro;
        public static Person member = null;

        public LinksPage()
        {
            InitializeComponent();

            linksPage = this;

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
                ProfileImage.Source = ImageSource.FromFile(Device.OnPlatform("Icon-72.png", null, "Assets/Square71x71Logo.scale-140.png"));
                ProfileName.Text = "Fazer login";
            }

            string directory = "";

            if (Device.OS == TargetPlatform.Windows)
            {
                directory = "Assets/Images/";
            }

            listLinks.Add(new Link() { Title = "Site", Icon = directory + "Site.png", Value = AppResources.SiteUrl });
			listLinks.Add(new Link() { Title = "Meetups antigos", Icon = directory + "YouTube.png", Value = AppResources.OldMeetupsUrl });
            listLinks.Add(new Link() { Title = "Facebook", Icon = directory + "Facebook.png", Value = AppResources.FacebookUrl });
            listLinks.Add(new Link() { Title = "Google+", Icon = directory + "Google.png", Value = AppResources.GooglePlusUrl });
            listLinks.Add(new Link() { Title = "Instagram", Icon = directory + "Instagram.png", Value = AppResources.InstagramUrl });
            listLinks.Add(new Link() { Title = "Twitter", Icon = directory + "Twitter.png", Value = AppResources.TwitterUrl });
            listLinks.Add(new Link() { Title = "YouTube", Icon = directory + "YouTube.png", Value = AppResources.YouTubeUrl });
            listLinks.Add(new Link() { Title = "Contato", Icon = directory + "Mail.png", Value = AppResources.ContactMail });

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

        private void ListLinks_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;

            Link item = e.SelectedItem as Link;

            if(item.Title.Equals("Contato"))
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
