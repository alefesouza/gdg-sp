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
using GDGSPCheckIn.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Windows;

namespace GDGSPCheckIn
{
    /// <summary>
    /// Janela de configurações do programa.
    /// </summary>
    public partial class SettingsWindow : Window
    {
        bool useLocal;

        public SettingsWindow()
        {
            InitializeComponent();

            Title = "Configurações - " + App.AppName + " Check-in";

            APIKey.Text = Settings.Default.APIKey;
        }

        private async void OK_Click(object sender, RoutedEventArgs e)
        {
            if (APIKey.Text.Equals(""))
            {
                MessageBox.Show("Digite a API Key");
            }
            else
            {
                useLocal = false;

                Settings.Default.APIKey = APIKey.Text;
                Settings.Default.Save();

                OK.IsEnabled = false;

                StackList.Visibility = Visibility.Collapsed;
                Save.Visibility = Visibility.Collapsed;

                GetData.Visibility = Visibility.Visible;

                Info.Text = "Recebendo eventos";

                string jsonString = "";

                try
                {
                    var client = new HttpClient();
                    HttpResponseMessage response = await client.GetAsync("http://api.meetup.com/" + App.MeetupId + "/events?key=" + APIKey.Text);

                    if (response.IsSuccessStatusCode)
                    {
                        jsonString = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    OK.IsEnabled = true;
                    GetData.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Houve um erro ao receber a lista de eventos");
                    return;
                }

                List<Event> listEvents = JsonConvert.DeserializeObject<List<Event>>(jsonString);

                if (listEvents.Count > 0)
                {
                    ListEvents.ItemsSource = listEvents;
                    ListEvents.SelectedIndex = 0;
                    StackList.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("Não há eventos");
                }

                GetData.Visibility = Visibility.Collapsed;
                Save.Visibility = Visibility.Visible;
                OK.IsEnabled = true;
            }
        }

        /// <summary>
        /// Ao clicar em Salvar, o programa vai receber todos os confirmados no evento e preencher o banco de dados local.
        /// </summary>
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            Event _event = ListEvents.SelectedItem as Event;

            int eventId = _event.Id;

            if (!useLocal)
            {
                Save.IsEnabled = false;
                GetData.Visibility = Visibility.Visible;

                Info.Text = "Recebendo participantes";

                string jsonString = "";

                try
                {
                    var client = new HttpClient();
                    HttpResponseMessage response = await client.GetAsync("http://api.meetup.com/" + App.MeetupId + "/events/" + eventId + "/rsvps?key=" + APIKey.Text);

                    if (response.IsSuccessStatusCode)
                    {
                        jsonString = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                catch
                {
                    GetData.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Houve um erro ao receber a lista de participantes");
                    Save.IsEnabled = true;
                    return;
                }

                Info.Text = "Preenchendo banco de dados";

                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;

                    App.objConn.Prepare($"INSERT OR REPLACE INTO events (id, event_id, event_name) VALUES ((SELECT id FROM events WHERE event_id={eventId}), {eventId}, \"{_event.Name}\")").Step();

                    App.objConn.Prepare("CREATE TABLE IF NOT EXISTS event_" + eventId + " (id integer primary key autoincrement, member_id int, member_name varchar(255), date varchar(30), checked int)").Step();

                    var jsonArray = JArray.Parse(jsonString);

                    foreach (JObject member in jsonArray)
                    {
                        if (member["response"].ToString().Equals("yes"))
                        {
                            Person p = JsonConvert.DeserializeObject<Person>(member["member"].ToString());

                            string query = $"INSERT OR REPLACE INTO event_{eventId} (id, member_id, member_name, date, checked) VALUES ((SELECT id FROM event_{eventId} WHERE member_id={p.Id}), {p.Id}, \"{p.Name.Replace("\"", "\"\"")}\", \"\", 0)";
                            App.objConn.Prepare(query).Step();
                        }
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        new MainWindow(eventId, _event.Name).Show();
                        Close();
                    });
                }).Start();
            }
            else
            {
                new MainWindow(eventId, _event.Name).Show();
                Close();
            }
        }

        private void ShowRelatories_Click(object sender, RoutedEventArgs e)
        {
            if(App.objConn.Prepare(@"SELECT * FROM events").Step() != SQLitePCL.SQLiteResult.ROW)
            {
                MessageBox.Show("Não há eventos com relatório");
            }
            else
            {
                new RelatoryWindow().Show();
            }
        }

        private void ShowAllUsers_Click(object sender, RoutedEventArgs e)
        {
            if (APIKey.Text.Equals(""))
            {
                MessageBox.Show("Digite a API Key");
            }
            else
            {
                Settings.Default.APIKey = APIKey.Text;
                Settings.Default.Save();
                new AllUsersWindow().Show();
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void UseLocal_Click(object sender, RoutedEventArgs e)
        {
            if (App.objConn.Prepare(@"SELECT * FROM events").Step() != SQLitePCL.SQLiteResult.ROW)
            {
                MessageBox.Show("Não há eventos na base de dados local");
            }
            else
            {
                useLocal = true;

                ObservableCollection<Event> events = new ObservableCollection<Event>();

                var _event = App.objConn.Prepare(@"SELECT * FROM events");

                while (_event.Step() == SQLiteResult.ROW)
                {
                    events.Add(new Event() { Id = int.Parse(_event[1].ToString()), Name = _event[2].ToString() });
                }

                ListEvents.ItemsSource = events;

                StackList.Visibility = Visibility.Visible;

                if (events.Count > 0)
                {
                    ListEvents.SelectedIndex = 0;
                }

                Save.Visibility = Visibility.Visible;
            }
        }

        private void PrinterConfig_Click(object sender, RoutedEventArgs e)
        {
            new PrinterConfigWindow().Show();
        }
    }
}
