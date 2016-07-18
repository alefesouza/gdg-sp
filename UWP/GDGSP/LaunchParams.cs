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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GDGSP
{
    public partial class MainPage : Page
    {
        private string launchParam;
        public string LaunchParam
        {
            get
            {
                return launchParam;
            }

            set
            {
                launchParam = value;
                if (value.StartsWith("opensite"))
                {
                    Other.Other.OpenSite("http://meetup.com/" + Other.Other.resourceLoader.GetString("MeetupId"));
                    if (value.Contains("0"))
                    {
                        Application.Current.Exit();
                    }
                }
                else if (value.StartsWith("jumplink") || value.StartsWith("url"))
                {
                    string[] split = value.Split('|');

                    Link info = new Link() { Title = split[1], Value = split[2] };

                    ToWebView(info);
                }
                else if (value.StartsWith("event"))
                {
                    string[] split = value.Split('|');
                    openEvent = int.Parse(split[1]);
                }
                else
                {
                    headerList.SelectedIndex = 0;
                }
            }
        }
    }
}
