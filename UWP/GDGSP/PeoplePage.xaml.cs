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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
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
        bool peopleWithApp, timerBlock;
        TextBlock title;
        int count;

        public PeoplePage()
        {
            this.InitializeComponent();

            if (Other.Other.IsMobile())
            {
                Grid.SetRow(CB, 2);

                DispatcherTimer tmr = new DispatcherTimer();
                tmr.Interval = TimeSpan.FromMilliseconds(0);
                tmr.Tick += (s, e) => { CB.Padding = new Thickness(0, 0, 0, 0); tmr.Stop(); };
                tmr.Start();

                CBMobile.Visibility = Visibility.Visible;
                CBTitle.Visibility = Visibility.Collapsed;
                CB.Background = new SolidColorBrush(Color.FromArgb(255, 27, 27, 27));
                title = CBMobileTitle;
            }
            else
            {
                title = CBTitle;
            }

            ListsPivot.SelectionChanged += (s, e) =>
            {
                CBRandom.Visibility = ListsPivot.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
                CBPeopleWithApp.Visibility = ListsPivot.SelectedIndex == 0 ? Visibility.Visible : Visibility.Collapsed;
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _event = e.Parameter as Event;
            GetPeople();

            title.Text = _event.Who;
        }

        /// <summary>
        /// Método que solicita a lista de pessoas que deram alguma resposta ao evento e preenche a lista.
        /// </summary>
        public async void GetPeople()
        {
            string jsonString = "";

            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.PostAsync(Other.Other.GetRSVPSUrl(_event.Id), Other.Other.GetRefreshToken());

            try
            {
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

            if (ListPeopleWait.Items.Count == 0)
            {
                ListsPivot.Items.Remove(PeopleWait);
            }

            if (ListPeopleNo.Items.Count == 0)
            {
                ListsPivot.Items.Remove(PeopleNo);
            }

            CBRandom.Visibility = Visibility.Visible;
            CBPeopleWithApp.Visibility = Visibility.Visible;

            PRing.Visibility = Visibility.Collapsed;
            ListsPivot.Visibility = Visibility.Visible;
        }

        private async void ShowMessage(string title)
        {
            MessageDialog md = new MessageDialog("Deseja tentar novamente?");
            md.Title = title;

            md.Commands.Add(new UICommand("Sim", new UICommandInvokedHandler((c) =>
            {
                GetPeople();
            }))
            { Id = 0 });
            md.Commands.Add(new UICommand("Não", new UICommandInvokedHandler((c) =>
            {
                HomePage.Instance.eventopen.GoBack();
            }))
            { Id = 1 });

            await md.ShowAsync();
        }

        public void ListPeople_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ListView).SelectedIndex != -1)
            {
                Person selected = e.AddedItems[0] as Person;

                MainPage.Instance.ToWebView(new Link() { Title = selected.Name, Value = "http://meetup.com/" + Other.Other.resourceLoader.GetString("MeetupId") + "/member/" + selected.Id });

                (sender as ListView).SelectedIndex = -1;
            }
        }

        private async void CBRandom_Click(object sender, RoutedEventArgs e)
        {
            // Para sortear, primeiro o aplicativo pega a lista de todas as pessoas que foram
            ObservableCollection<Person> go;
            if (peopleWithApp)
            {
                go = new ObservableCollection<Person>(listPeople.Where(P => P.Response.Equals("yes") && P.Has_app));
            }
            else
            {
                go = new ObservableCollection<Person>(listPeople.Where(P => P.Response.Equals("yes")));
            }

            // Se o total de itens na lista for maior que 0
            if (go.Count > 0)
            {
                Random random = new Random();
                // Ele criará um random de 0 até o número de pessoas na lista menos 1
                int number = random.Next(go.Count);
                // E dará scroll até esta posição na lista, exibindo o nome da pessoa.
                ListPeople.ScrollIntoView(go[number], ScrollIntoViewAlignment.Leading);

                string localDate = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
                string dbDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (MainPage.Instance.member != null && go[number].Id == MainPage.Instance.member.Id)
                {
                    count = 0;
                    timerBlock = false;
                    StartTimer();

                    MessageDialog md = new MessageDialog("Você!\n\nSorteado às " + localDate);

                    md.Commands.Add(new UICommand("Enviar", new UICommandInvokedHandler((c) => {
                        timerBlock = true;
                        SendRaffle(dbDate, count, _event);
                    }))
                    { Id = 0 });
                    md.Commands.Add(new UICommand("Cancelar", null)
                    { Id = 1 });

                    await md.ShowAsync();
                }
                else
                {
                    Other.Other.ShowMessage(go[number].Name);
                }
            }
        }

        /// <summary>
        /// Gambiarra para contar os segundos, reaproveitando código do Xamarin por preguiça de pesquisar algo pro UWP...
        /// </summary>
		private async void StartTimer()
        {
            if (!timerBlock)
            {
                await Task.Delay(1000);
                count++;
                StartTimer();
            }
        }

        public async void SendRaffle(string raffle_date, int seconds, Event _event)
        {
            var postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("app_key", Other.Other.GetAppKey()));
            postData.Add(new KeyValuePair<string, string>("raffle_date", raffle_date));
            postData.Add(new KeyValuePair<string, string>("seconds", seconds.ToString()));
            postData.Add(new KeyValuePair<string, string>("refresh_token", Other.Other.localSettings.Values["refresh_token"].ToString()));

            var client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
            HttpResponseMessage response = await client.PostAsync(Other.Other.GetRaffleUrl(_event.Id), new FormUrlEncodedContent(postData));

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                string message;

                switch (result)
                {
                    case "success":
                        message = "Seu sorteio foi enviado, boa sorte!";
                        break;
                    case "invalid_user":
                        message = "Usuário inválido";
                        break;
                    case "invalid_key":
                        message = "Chave do aplicativo inválida";
                        break;
                    default:
                        message = "Houve um erro";
                        break;
                }

                Other.Other.ShowMessage(message);
            }
            else
            {
                MessageDialog md = new MessageDialog("Deseja tentar novamente?");
                md.Title = "Houve um erro ao enviar os dados";

                md.Commands.Add(new UICommand("Sim", new UICommandInvokedHandler((c) => {
                    SendRaffle(raffle_date, seconds, _event);
                }))
                { Id = 0 });
                md.Commands.Add(new UICommand("Não", null)
                { Id = 1 });

                await md.ShowAsync();
            }
        }

        private void PeopleWithApp_Click(object sender, RoutedEventArgs e)
        {
            peopleWithApp = !peopleWithApp;

            if (peopleWithApp)
            {
                ListPeople.ItemsSource = new ObservableCollection<Person>(listPeople.Where(P => P.Response.Equals("yes") && P.Has_app));
                (sender as AppBarButton).Label = "Todos";
            }
            else
            {
                ListPeople.ItemsSource = new ObservableCollection<Person>(listPeople.Where(P => P.Response.Equals("yes")));
                (sender as AppBarButton).Label = "Com o app";
            }
        }
    }
}
