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

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using GDGSP.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Windows.UI.Popups;

namespace GDGSP
{
    /// <summary>
	/// Página que exibe as pessoas que sortearam a si mesmo no evento, exibida apenas para organizadores e usuários permitidos.
    /// </summary>
    public sealed partial class RaffleManagerPage : Page
    {
        ObservableCollection<Raffle> listPeople = new ObservableCollection<Raffle>();

        public RaffleManagerPage()
        {
            this.InitializeComponent();

            GetPeople(EventsPage.actualEvent.Id);
        }

        /// <summary>
        /// Método que solicita a lista de pessoas que deram alguma resposta ao evento e preenche a lista.
        /// </summary>
        public async void GetPeople(int id, string empty = "false")
        {
            Loading.Visibility = Visibility.Visible;
            ListPeople.Visibility = Visibility.Collapsed;

            string jsonString = "";

            try
            {
                var postData = new List<KeyValuePair<string, string>>();
                postData.Add(new KeyValuePair<string, string>("app_key", Other.Other.GetAppKey()));
                postData.Add(new KeyValuePair<string, string>("manager", "true"));
                postData.Add(new KeyValuePair<string, string>("empty", empty));
                postData.Add(new KeyValuePair<string, string>("refresh_token", Other.Other.localSettings.Values["refresh_token"].ToString()));

                var client = new HttpClient();
                client.MaxResponseContentBufferSize = 256000;
                HttpResponseMessage response = await client.PostAsync(Other.Other.GetRaffleUrl(id), new FormUrlEncodedContent(postData));

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    switch (result)
                    {
                        case "invalid_user":
                        case "invalid_key":
                            MainPage.mainPage.headerList.SelectedIndex = 0;
                            Other.Other.ShowMessage("Usuário ou chave do aplicativo inválida" + Other.Other.GetAppKey());
                            return;
                        case "[]":
                            MessageDialog md = new MessageDialog("Não há pessoas sorteadas nesse evento");
                            md.Title = "Tentar novamente?";

                            md.Commands.Add(new UICommand("Sim", new UICommandInvokedHandler((c) => {
                                GetPeople(id);
                            }))
                            { Id = 0 });
                            md.Commands.Add(new UICommand("Não", new UICommandInvokedHandler((c) => {
                                MainPage.mainPage.headerList.SelectedIndex = 0;
                            }))
                            { Id = 1 });
                            return;
                        default:
                            jsonString = result;
                            break;
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                Other.Other.ShowMessage("Verifique sua conexão de internet");
                MainPage.mainPage.headerList.SelectedIndex = 0;
                return;
            }

            try
            {
                listPeople = JsonConvert.DeserializeObject<ObservableCollection<Raffle>>(jsonString);
            }
            catch
            {
                Other.Other.ShowMessage("Houve um erro ao receber a lista de pessoas");
                MainPage.mainPage.headerList.SelectedIndex = 0;
                return;
            }

            ListPeople.ItemsSource = listPeople;

            Loading.Visibility = Visibility.Collapsed;
            ListPeople.Visibility = Visibility.Visible;
        }

        private void ListPeople_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListPeople.SelectedIndex != -1)
            {
                ListPeople.SelectedIndex = -1;
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetPeople(EventsPage.actualEvent.Id, "false");
        }

        private void Empty_Click(object sender, RoutedEventArgs e)
        {
            GetPeople(EventsPage.actualEvent.Id, "true");
        }
    }
}
