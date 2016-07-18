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

using GDGSP.Models;
using System.Collections.ObjectModel;

namespace GDGSP.ViewModels
{
    /// <summary>
    /// ViewModel que representa a barra lateral do aplicativo
    /// </summary>
    public class LeftPanelViewModel
    {
        public LeftPanelViewModel()
        {
            string imageLocation = "ms-appx:///Assets/Images/";

            string[] titles = { "Site", "Meetus antigos", "Facebook", "Google+", "Instagram", "Twitter", "YouTube" };

            string[] icons = { "Site.png", "Youtube.png", "Facebook.png", "GooglePlus.png", "Instagram.png", "Twitter.png", "Youtube.png" };

            string[] links = { Other.Other.resourceLoader.GetString("SiteUrl"), Other.Other.resourceLoader.GetString("OldMeetupUrl"), Other.Other.resourceLoader.GetString("FacebookUrl"), Other.Other.resourceLoader.GetString("GooglePlusUrl"), Other.Other.resourceLoader.GetString("InstagramUrl"), Other.Other.resourceLoader.GetString("TwitterUrl"), Other.Other.resourceLoader.GetString("YouTubeUrl") };

            for (int i = 0; i < titles.Length; i++)
            {
                var item = new Link() { Title = titles[i], Value = "http://" + links[i], Image = imageLocation + icons[i] };
                Items.Add(item);
            }
        }

        public ObservableCollection<Link> Items { get; private set; } = new ObservableCollection<Link>();
    }
}
