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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using Xamarin.Forms;

namespace GDG_SP
{
    public partial class SendNotificationPage : ContentPage
    {
        public SendNotificationPage()
        {
            InitializeComponent();

            Title = "Enviar notificação";

            ObservableCollection<Event> listEvents = MainPage.main.listEvents;

            if (listEvents.Count > 0)
            {
                Dictionary<string, Event> events = new Dictionary<string, Event>();

                foreach (Event _event in listEvents)
                {
                    events.Add(_event.Name, _event);
                }

                foreach (string name in events.Keys)
                {
                    Events.Items.Add(name);
                }

                Events.SelectedIndex = 0;

                ToAll.Toggled += (s, e) =>
                {
                    StackPicker.IsVisible = !ToAll.IsToggled;
                };
            }
            else
            {
                ToAllGrid.IsVisible = false;
                StackPicker.IsVisible = false;
            }

            Send.Clicked += async (s, ev) =>
            {
                Send.IsEnabled = false;

                try
                {
                    var postData = new List<KeyValuePair<string, string>>();
                    postData.Add(new KeyValuePair<string, string>("title", TextTitle.Text));
                    postData.Add(new KeyValuePair<string, string>("link", Link.Text));
                    postData.Add(new KeyValuePair<string, string>("image", Image.Text));
                    postData.Add(new KeyValuePair<string, string>("message", Message.Text));
                    if (!ToAll.IsToggled)
                    {
                        postData.Add(new KeyValuePair<string, string>("eventid", listEvents[Events.SelectedIndex].Id.ToString()));
                    }
                    postData.Add(new KeyValuePair<string, string>("refresh_token", Other.Other.GetSetting("refresh_token")));

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
                                Other.Other.ShowMessage("Notificação enviada com sucesso", this);
                                await Navigation.PopAsync();
                                break;
                            case "invalid_user":
                                Other.Other.ShowMessage("Usuário inválido", this);
                                (s as Button).IsEnabled = true;
                                break;
                            case "try_again":
                                Other.Other.ShowMessage("Houve um erro, tente novamente", this);
                                (s as Button).IsEnabled = true;
                                break;
                        }
                    }
                    else
                    {
                        (s as Button).IsEnabled = true;
                        Other.Other.ShowMessage("Houve um erro ao enviar os dados, tente novamente", this);
                    }
                }
                catch
                {
                    (s as Button).IsEnabled = true;
                    Other.Other.ShowMessage("Houve um erro ao enviar os dados, tente novamente", this);
                }
            };
        }
    }
}