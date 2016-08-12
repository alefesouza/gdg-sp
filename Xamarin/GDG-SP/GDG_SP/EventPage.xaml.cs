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
using Plugin.Share;
using Xamarin.Forms;

namespace GDG_SP
{
    /// <summary>
    /// Página onde é exibido o evento selecionado.
    /// </summary>
    public partial class EventPage : ContentPage
    {
        public EventPage(Event _event)
        {
            InitializeComponent();

            Title = Device.OnPlatform("", null, AppResources.AppName);

            if (Device.OS == TargetPlatform.Windows)
            {
                ToolbarItems.Add(new ToolbarItem("Copiar link", null, async () =>
                {
                    await CrossShare.Current.SetClipboardText(_event.Link);
                }, ToolbarItemOrder.Secondary));
            }

            ToolbarItems.Add(new ToolbarItem("RSVP", Other.Other.GetImage("Rsvp"), async () =>
            {
                if (Other.Other.GetSetting("refresh_token").Length > 0)
                {
                    if (_event.Rsvpable)
                    {
                        await Navigation.PushAsync(new RSVPPage(_event));
                    }
                    else
                    {
                        await DisplayAlert("", "Não é possível fazer RSVP nesse evento", "OK");
                    }
                }
                else
                {
                    bool alert = await DisplayAlert("RSVP", "Você precisa fazer login para fazer RSVP, deseja fazer login agora?", "Sim", "Não");
                    if (alert)
                    {
                        await Navigation.PushAsync(new WebViewPage(Other.Other.GetLoginUrl(), true, _event.Id) { Title = "Login" });
                    }
                }
            }));

            ToolbarItems.Add(new ToolbarItem(_event.Who, Other.Other.GetImage("People"), () =>
                {
                    Navigation.PushAsync(new TabbedPeoplePage(_event.Id) { Title = _event.Who });
                }));

            ToolbarItems.Add(new ToolbarItem("Compartilhar", Other.Other.GetImage("Share"), () =>
            {
                Other.Other.ShareLink(_event.Name, _event.Link);
            }));

            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = _event.Description;
            WView.Source = htmlSource;

            WView.Navigating += (sender, e) =>
            {
                if (e.Url.StartsWith("http://do_login"))
                {
                    Navigation.PushAsync(new WebViewPage(Other.Other.GetLoginUrl(), true, _event.Id) { Title = "Login" });
                    e.Cancel = true;
                }
                else
                {
                    Other.Other.OpenSite(e.Url, this);
                    e.Cancel = true;
                }
            };
        }
    }
}

