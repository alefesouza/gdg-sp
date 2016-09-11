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

using GDGSPCheckIn.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GDGSPCheckIn
{
    /// <summary>
    /// Janela que exibe o relatório de todos os usuários que fizeram check-in no programa.
    /// </summary>
    public partial class RelatoryWindow : Window
    {
        ObservableCollection<Person> Items;

        public RelatoryWindow()
        {
            InitializeComponent();

            Title = "Relatório - " + App.AppName + " Check-in";

            ObservableCollection<Event> events = new ObservableCollection<Event>();

            var _event = App.objConn.Prepare(@"SELECT * FROM events");

            while (_event.Step() == SQLiteResult.ROW)
            {
                events.Add(new Event() { Id = int.Parse(_event[1].ToString()), Name = _event[2].ToString() });
            }

            ListEvents.ItemsSource = events;

            if(events.Count > 0)
            {
                ListEvents.SelectedIndex = 0;
            }
        }

        private void ListEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PeopleGrid.Visibility = Visibility.Visible;
            SendFaults.Visibility = Visibility.Visible;

            Event _event = e.AddedItems[0] as Event;

            Items = new ObservableCollection<Person>();

            var person = App.objConn.Prepare(@"SELECT * FROM event_" + _event.Id);

            while (person.Step() == SQLiteResult.ROW)
            {
                Items.Add(new Person() { Id = int.Parse(person[1].ToString()), Name = person[2].ToString(), Date = person[3].ToString(), Checked = int.Parse(person[4].ToString()) });
            }

            ListCollectionView v = new ListCollectionView(Items);
            v.GroupDescriptions.Add(new PropertyGroupDescription("Veio"));

            PeopleGrid.ItemsSource = v;
        }

        private async void SendFaults_Click(object sender, RoutedEventArgs e)
        {
            Loading.Visibility = Visibility.Visible;
            SendFaults.IsEnabled = false;

            ObservableCollection<Person> faults = new ObservableCollection<Person>(Items.Where(P => P.Checked == 0));

            string json = JsonConvert.SerializeObject(faults, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            MessageBox.Show(json);

            var postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("app_key", App.GetAppKey()));
            postData.Add(new KeyValuePair<string, string>("api_key", Properties.Settings.Default.APIKey));
            postData.Add(new KeyValuePair<string, string>("json", json));

            try
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.PostAsync("http://" + App.BackendUrl + "post_faults.php?meetupid=" + App.MeetupId, new FormUrlEncodedContent(postData));

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    switch (result)
                    {
                        case "invalid_key":
                        case "invalid_user":
                            MessageBox.Show("Usuário ou chave do aplicativo inválida");
                            break;
                        case "error":
                            MessageBox.Show("Houve um erro ao receber os dados");
                            break;
                        case "success":
                            MessageBox.Show("Dados enviados com sucesso");
                            break;
                        default:
                            throw new Exception();
                    }

                    Loading.Visibility = Visibility.Collapsed;
                    SendFaults.IsEnabled = true;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                MessageBox.Show("Houve um erro ao receber os dados");

                Loading.Visibility = Visibility.Collapsed;
                SendFaults.IsEnabled = true;
            }
        }
    }
}