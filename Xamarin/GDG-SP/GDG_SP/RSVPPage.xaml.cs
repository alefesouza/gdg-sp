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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Forms;
using static GDG_SP.Model.Event;

namespace GDG_SP
{
    /// <summary>
    /// Página do aplicativo para o usuário fazer RSVP em um evento.
    /// </summary>
    public partial class RSVPPage : ContentPage
    {
        /// <summary>
        /// Lista de Entry para poder pegar o texto de todas as Entry criadas para responder as perguntas.
        /// </summary>
        List<Entry> entries = new List<Entry>();

        public RSVPPage(Event _event)
        {
            InitializeComponent();

            Title = "RSVP";

            if (_event.Response == null)
            {
                _event.Response = "no";
            }

            ResponseSwitch.IsToggled = !_event.Response.Equals("no");

            switch (_event.Response)
            {
                case "yes":
                    State.Text = "Sim";
                    break;
                case "waitlist":
                    State.Text = "Lista de espera";
                    break;
                default:
                    State.Text = "Não";
                    break;
            }

            QuestionsStack.IsVisible = ResponseSwitch.IsToggled;

            ResponseSwitch.Toggled += (s, e) =>
            {
                if (e.Value)
                {
                    if (_event.Yes_rsvp_count == _event.Rsvp_limit && !_event.Response.Equals("yes"))
                    {
                        State.Text = "Lista de espera";
                    }
                    else
                    {
                        State.Text = "Sim";
                    }
                }
                else
                {
                    State.Text = "Não";
                }

                QuestionsStack.IsVisible = e.Value;
            };

            for (int i = 0; i < _event.Survey_questions.Count; i++)
            {
                Label label = new Label();
                label.Text = _event.Survey_questions[i].Question;
                Entry entry = new Entry();

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

            Send.Clicked += async (s, e) =>
            {
                try
                {
                    (s as Button).IsEnabled = false;

                    List<Questions> list = new List<Questions>();

                    for (int i = 0; i < entries.Count; i++)
                    {
                        Questions q = _event.Survey_questions[i];
                        q.Answer = entries[i].Text;
                        list.Add(q);
                    }

                    ObjectToSend ob = new ObjectToSend() { Response = ResponseSwitch.IsToggled ? "yes" : "no", Answers = list };

                    var uri = new Uri(Other.Other.GetRSVPUrl(_event.Id));

                    var json = JsonConvert.SerializeObject(ob, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                    var postData = new List<KeyValuePair<string, string>>();
                    postData.Add(new KeyValuePair<string, string>("json", json));
                    postData.Add(new KeyValuePair<string, string>("refresh_token", Other.Other.GetSetting("refresh_token")));

                    var content = new FormUrlEncodedContent(postData);

                    var client = new HttpClient();
                    client.MaxResponseContentBufferSize = 256000;
                    HttpResponseMessage response = await client.PostAsync(uri, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string result = await response.Content.ReadAsStringAsync();

                        string message = "";

                        switch (result)
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

                        await DisplayAlert("", message, "OK");

                        MainPage.openEvent = _event.Id;
						MainPage.Instance.GetEvents();
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

        private class ObjectToSend
        {
            public string Response { get; set; }
            public List<Questions> Answers { get; set; }
        }
    }
}