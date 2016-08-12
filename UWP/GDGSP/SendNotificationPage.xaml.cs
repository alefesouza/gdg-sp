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
using System.Collections.Generic;
using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GDGSP
{
    /// <summary>
    /// Página onde administradores podem enviar notificações.
    /// </summary>
    public sealed partial class SendNotificationPage : Page
    {
        public SendNotificationPage()
        {
            this.InitializeComponent();

            if (EventsPage.events.listEvents.Count > 0)
            {
                ToAll.Checked += (s, e) =>
                {
                    Events.Visibility = Visibility.Collapsed;
                };

                ToAll.Unchecked += (s, e) =>
                {
                    Events.Visibility = Visibility.Visible;
                };

                Events.ItemsSource = EventsPage.events.listEvents;
                Events.SelectedItem = EventsPage.events.listEvents[0];
            }
            else
            {
                ToAll.Visibility = Visibility.Collapsed;
                Events.Visibility = Visibility.Collapsed;
            }

            string value = "";

            Send.Click += async (s, e) =>
            {
                Message.Document.GetText(Windows.UI.Text.TextGetOptions.None, out value);

                Send.IsEnabled = false;

                try
                {
                    var postData = new List<KeyValuePair<string, string>>();
                    postData.Add(new KeyValuePair<string, string>("app_key", Other.Other.GetAppKey()));
                    postData.Add(new KeyValuePair<string, string>("title", Title.Text));
                    postData.Add(new KeyValuePair<string, string>("link", Link.Text));
                    postData.Add(new KeyValuePair<string, string>("image", Image.Text));
                    postData.Add(new KeyValuePair<string, string>("message", value));
                    if (!(bool)ToAll.IsChecked)
                    {
                        postData.Add(new KeyValuePair<string, string>("eventid", (Events.SelectedItem as Event).Id.ToString()));
                    }
                    postData.Add(new KeyValuePair<string, string>("refresh_token", Other.Other.localSettings.Values["refresh_token"].ToString()));

                    var content = new FormUrlEncodedContent(postData);

                    var client = new HttpClient();
                    client.MaxResponseContentBufferSize = 256000;
                    HttpResponseMessage response = await client.PostAsync(Other.Other.GetNotificationUrl(), content);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();

                        switch (result)
                        {
                            case "notification_send":
                                Other.Other.ShowMessage("Notificação enviada com sucesso");
                                MainPage.mainPage.headerList.SelectedIndex = 0;
                                break;
                            case "invalid_user":
                                Other.Other.ShowMessage("Usuário inválido");
                                Send.IsEnabled = true;
                                break;
                            case "try_again":
                                Other.Other.ShowMessage("Houve um erro, tente novamente");
                                Send.IsEnabled = true;
                                break;
                        }
                    }
                    else
                    {
                        (s as Button).IsEnabled = true;
                        Other.Other.ShowMessage("Houve um erro ao enviar os dados, tente novamente");
                    }
                }
                catch
                {
                    (s as Button).IsEnabled = true;
                    Other.Other.ShowMessage("Houve um erro ao enviar os dados, tente novamente");
                }
            };
        }
    }
}
