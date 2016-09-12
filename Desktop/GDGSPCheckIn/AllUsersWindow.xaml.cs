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
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;

namespace GDGSPCheckIn
{
    /// <summary>
    /// Janela que exibe o total de faltas dos usuários.
    /// </summary>
    public partial class AllUsersWindow : Window
    {
        public AllUsersWindow()
        {
            InitializeComponent();

            Title = "Todos os usuário com falta - " + App.AppName + " Check-in";

            GetPeople();
        }

        public async void GetPeople()
        {
            var postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("app_key", App.GetAppKey()));
            postData.Add(new KeyValuePair<string, string>("api_key", Properties.Settings.Default.APIKey));

            try
            {
                var client = new HttpClient();
                HttpResponseMessage response = await client.PostAsync("http://" + App.BackendUrl + "all_users.php?meetupid=" + App.MeetupId, new FormUrlEncodedContent(postData));

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();

                    switch (jsonString)
                    {
                        case "invalid_key":
                        case "invalid_user":
                            MessageBox.Show("Usuário ou chave do aplicativo inválida");
                            Close();
                            return;
                        case "error":
                            MessageBox.Show("Houve um erro ao receber os dados");
                            Close();
                            return;
                        default:
                            List<AllUsersModel> listAUM = JsonConvert.DeserializeObject<List<AllUsersModel>>(jsonString);

                            if (listAUM.Count > 0)
                            {
                                PeopleGrid.ItemsSource = listAUM;
                                GetData.Visibility = Visibility.Collapsed;
                                PeopleGrid.Visibility = Visibility.Visible;
                            }
                            else
                            {
                                MessageBox.Show("Não há usuários com faltas");
                                Close();
                            }
                            return;
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                MessageBox.Show("Houve um erro ao receber os dados");
                Close();
            }
        }
    }
}
