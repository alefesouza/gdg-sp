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
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GDGSP
{
    /// <summary>
    /// Página que exibe as pessoas que deram alguma resposta ao evento.
    /// </summary>
    public sealed partial class PeoplePage : Page
    {
        Event _event;
        ObservableCollection<Person> listPeople = new ObservableCollection<Person>();

        public PeoplePage()
        {
            this.InitializeComponent();

            ListsPivot.SelectionChanged += (s, e) =>
            {
                CBRandom.Visibility = ListsPivot.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _event = e.Parameter as Event;
            GetPeople();

            CBTitle.Text = _event.Who;
        }

        /// <summary>
        /// Método que solicita a lista de pessoas que deram alguma resposta ao evento e preenche a lista.
        /// </summary>
        public async void GetPeople()
        {
            string jsonString = "";

            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.PostAsync(Other.Other.GetRSVPSUrl(_event.Id), Other.Other.GetRefreshToken());

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
                ShowMessage("Verifique sua conexão de internet");
                return;
            }
            
            try
            {
                listPeople = JsonConvert.DeserializeObject<ObservableCollection<Person>>(jsonString);
            }
            catch
            {
                ShowMessage("Houve um erro ao receber a lista de pessoas");
                return;
            }

            ListPeople.ItemsSource = listPeople.Where(P => P.Response.Equals("yes"));
            ListPeopleWait.ItemsSource = listPeople.Where(P => P.Response.Equals("waitlist") || P.Response.Equals("watching"));
            ListPeopleNo.ItemsSource = listPeople.Where(P => P.Response.Equals("no"));

            if(ListPeopleWait.Items.Count == 0)
            {
                ListsPivot.Items.Remove(PeopleWait);
            }

            if (ListPeopleNo.Items.Count == 0)
            {
                ListsPivot.Items.Remove(PeopleNo);
            }

            CBRandom.Visibility = Visibility.Visible;
            PRing.Visibility = Visibility.Collapsed;
            ListsPivot.Visibility = Visibility.Visible;
        }

        private async void ShowMessage(string title)
        {
            MessageDialog md = new MessageDialog("Deseja tentar novamente?");
            md.Title = title;

            md.Commands.Add(new UICommand("Sim", new UICommandInvokedHandler((c) => {
                GetPeople();
            }))
            { Id = 0 });
            md.Commands.Add(new UICommand("Não", new UICommandInvokedHandler((c) => {
                HomePage.homePage.eventopen.GoBack();
            }))
            { Id = 1 });

            await md.ShowAsync();
        }

        public void ListPeople_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView).SelectedIndex != -1)
            {
                Person selected = e.AddedItems[0] as Person;
                
                MainPage.mainPage.ToWebView(new Link() { Title = selected.Name, Value = "http://meetup.com/" + Other.Other.resourceLoader.GetString("MeetupId") + "/member/" + selected.Id });

                (sender as ListView).SelectedIndex = -1;
            }
        }

        private void CBRandom_Click(object sender, RoutedEventArgs e)
        {
            // Para sortear, primeiro o aplicativo pega a lista de todas as pessoas que foram
            ObservableCollection<Person> go = new ObservableCollection<Person>(listPeople.Where(P => P.Response.Equals("yes")));

            // Se o total de itens na lista for maior que 0
            if (go.Count > 0) {
                Random random = new Random();
                // Ele criará um random de 0 até o número de pessoas na lista menos 1
                int number = random.Next(go.Count);
                // E dará scroll até esta posição na lista, exibindo o nome da pessoa.
                ListPeople.ScrollIntoView(go[number]);
                Other.Other.ShowMessage(go[number].Name);
            }
        }
    }
}
