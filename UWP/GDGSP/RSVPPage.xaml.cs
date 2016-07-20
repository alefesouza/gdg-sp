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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using static GDGSP.Models.Event;

namespace GDGSP
{
    /// <summary>
    /// Página do aplicativo para o usuário fazer RSVP em um evento.
    /// </summary>
    public sealed partial class RSVPPage : Page
    {
        /// <summary>
        /// Lista de TextBox para poder pegar o texto de todas as TextBox criadas para responder as perguntas.
        /// </summary>
        List<TextBox> entries = new List<TextBox>();

        public RSVPPage()
        {
            this.InitializeComponent();

            CBTitle.Text = "RSVP";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Event _event = e.Parameter as Event;

            ResponseSwitch.OffContent = "Não";

            ResponseSwitch.IsOn = !_event.Response.Equals("no");

            if (_event.Response.Equals("yes") || _event.Yes_rsvp_count < _event.Rsvp_limit)
            {
                ResponseSwitch.OnContent = "Sim";
            }
            else if (_event.Yes_rsvp_count == _event.Rsvp_limit)
            {
                ResponseSwitch.OnContent = "Lista de espera";
            }

            for (int i = 0; i < _event.Survey_questions.Count; i++)
            {
                TextBlock label = new TextBlock();
                label.Text = _event.Survey_questions[i].Question;
                TextBox entry = new TextBox();
                entry.Margin = new Thickness(0, 5, 0, 5);

                entry.TextChanged += (sender, args) =>
                {
                    string _text = entry.Text;
                    if (_text.Length > 250)
                    {
                        _text = _text.Remove(_text.Length - 1);
                        entry.Text = _text;
                    }
                };

                if (_event.Answers != null)
                {
                    entry.Text = _event.Answers[i].Answer;
                }

                entries.Add(entry);

                QuestionsStack.Children.Add(label);
                QuestionsStack.Children.Add(entry);
            }

            Button button = new Button();
            button.Content = "Enviar";
            QuestionsStack.Children.Add(button);

            button.Click += async (s, ev) =>
            {
                try
                {
                    (s as Button).IsEnabled = false;

                    List<Questions> list = new List<Questions>();

                    for (int i = 0; i < entries.Count; i++)
                    {
                        list.Add(new Questions() { Id = _event.Survey_questions[i].Id, Question = _event.Survey_questions[i].Question, Answer = entries[i].Text });
                    }

                    ObjectToSend ob = new ObjectToSend() { Response = ResponseSwitch.IsOn ? "yes" : "no", Answers = list };

                    var uri = new Uri(Other.Other.GetRSVPUrl(_event.Id));
                    
                    var json = JsonConvert.SerializeObject(ob, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                    var postData = new List<KeyValuePair<string, string>>();
                    postData.Add(new KeyValuePair<string, string>("json", json));
                    postData.Add(new KeyValuePair<string, string>("refresh_token", Other.Other.localSettings.Values["refresh_token"].ToString()));

                    var content = new FormUrlEncodedContent(postData);

                    var client = new HttpClient();
                    client.MaxResponseContentBufferSize = 256000;
                    HttpResponseMessage response = await client.PostAsync(uri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();

                        string message = "";

                        switch(result)
                        {
                            case "yes":
                                message = "RSVP realizado com sucesso";
                                break;
                            case "waitlist":
                                message = "Você foi para a lista de espera";
                                break;
                            case "no":
                                message = "Você marcou que não irá";
                                break;
                        }

                        Other.Other.ShowMessage(message);

                        MainPage.openEvent = _event.Id;
                        EventsPage.events.GetEvents();
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

        private class ObjectToSend
        {
            public string Response { get; set; }
            public List<Questions> Answers { get; set; }
        }
    }
}
