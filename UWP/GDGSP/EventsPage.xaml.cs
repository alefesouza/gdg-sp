﻿/*
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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using Windows.Networking.PushNotifications;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace GDGSP
{
    /// <summary>
    /// Página onde é exibida a lista de eventos.
    /// </summary>
    public sealed partial class EventsPage : Page
    {
        public ProgressRing toKnow;
        TextBlock title;
        public static EventsPage events;
        public ObservableCollection<Event> listEvents = null;
        public static Event actualEvent = null;

        public EventsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            Other.Other.Transition(this);

            events = this;

            // ToKnow é um elemento/variável que eu uso para saber se a tela está com duas colunas
            toKnow = ToKnow;

            if (HomePage.homePage.eventopen.Visibility == Visibility.Collapsed)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
            }

            // Gosto de deixar o mais próximo possível dos aplicativos da Microsoft, por isso deixo essa barra embaixo no mobile
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

            title.Text = Other.Other.resourceLoader.GetString("AppName");

            TryAgain.Click += (s, e) =>
            {
                ErrorScreen.Visibility = Visibility.Collapsed;
                GetEvents();
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GetEvents();
        }

        /// <summary>
        /// Método que solicita a lista de eventos e preenche a lista.
        /// </summary>
        /// <param name="refresh">Se o método é chamado pelo botão de atualizar ou pelo botão "Tentar novamente".</param>
        public async void GetEvents(bool refresh = false)
        {
            ErrorScreen.Visibility = Visibility.Collapsed;

            ListEvents.Visibility = Visibility.Collapsed;
            PRing.Visibility = Visibility.Visible;

            string jsonString = "";

            try
            {
                var client = new HttpClient();
                client.MaxResponseContentBufferSize = 256000;
                HttpResponseMessage response = await client.PostAsync(Other.Other.GetEventsUrl(), Other.Other.GetRefreshToken(true));

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
                if (refresh)
                {
                    MessageDialog md = new MessageDialog("Deseja tentar novamente?");
                    md.Title = "Verifique sua conexão de internet";

                    md.Commands.Add(new UICommand("Sim", new UICommandInvokedHandler((c) => {
                        GetEvents(true);
                        ErrorScreen.Visibility = Visibility.Collapsed;
                    }))
                    { Id = 0 });
                    md.Commands.Add(new UICommand("Não", new UICommandInvokedHandler((c) => {
                        ListEvents.Visibility = Visibility.Visible;
                        PRing.Visibility = Visibility.Collapsed;
                    }))
                    { Id = 1 });

                    await md.ShowAsync();
                }
                else
                {
                    ErrorMessage.Text = "Verifique sua conexão de internet";
                    ErrorScreen.Visibility = Visibility.Visible;
                    PRing.Visibility = Visibility.Collapsed;
                }
                return;
            }

            JObject root = null;

            try
            {
                root = JObject.Parse(jsonString);
                HeaderImage.Source = new BitmapImage(new Uri(root["header"].ToString(), UriKind.Absolute));
                listEvents = JsonConvert.DeserializeObject<ObservableCollection<Event>>(root["events"].ToString());
            }
            catch
            {
                ErrorMessage.Text = "Houve um erro ao receber a lista de eventos";
                ErrorScreen.Visibility = Visibility.Visible;
                PRing.Visibility = Visibility.Collapsed;
                return;
            }

            // Se o ID for diferente de 0 significa que o usuário está fez login, então preenche as informações dele
            if ((int)root["member"]["id"] != 0)
            {
                MainPage.mainPage.member = JsonConvert.DeserializeObject<Person>(root["member"].ToString());
                Other.Other.localSettings.Values["member_profile"] = root["member"].ToString();

                MainPage.mainPage.profilePhoto.ImageSource = new BitmapImage(new Uri(MainPage.mainPage.member.Photo));
                MainPage.mainPage.profileName.Text = MainPage.mainPage.member.Name;
                MainPage.mainPage.profileIntro.Text = MainPage.mainPage.member.Intro;

                if ((bool)root["member"]["is_admin"])
                {
                    MainPage.mainPage.sendNotification.Visibility = Visibility.Visible;
                    MainPage.mainPage.raffleManager.Visibility = Visibility.Visible;
                }
            }
            else
            {
                // Se retornar 0 e o aplicativo conter a configuração refresh_token, significa que tem algo de errado com o login deve, como revogação de acesso por exemplo, nesse caso apaga todas as informações de login
                if (Other.Other.localSettings.Values.ContainsKey("refresh_token"))
                {
                    Other.Other.localSettings.Values.Remove("refresh_token");
                    Other.Other.localSettings.Values.Remove("member_profile");
                    MainPage.mainPage.profilePhoto.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Images/Logo.png", UriKind.Absolute));
                    MainPage.mainPage.profileName.Text = "Fazer login";
                    MainPage.mainPage.profileIntro.Text = "Fazer login";
                }
            }

            listEvents = JsonConvert.DeserializeObject<ObservableCollection<Event>>(root["events"].ToString());

            if (listEvents.Count > 0)
            {
                HeaderImage.Source = new BitmapImage(new Uri(root["header"].ToString(), UriKind.Absolute));

                ListEvents.ItemsSource = listEvents;

                if (MainPage.openEvent != 0)
                {
                    for (int i = 0; i < listEvents.Count; i++)
                    {
                        if (listEvents[i].Id == MainPage.openEvent)
                        {
                            HomePage.homePage.eventopen.SetNavigationState("1,0");
                            ListEvents.SelectedIndex = i;
                            MainPage.openEvent = 0;
                        }
                    }
                }
                else if (HomePage.homePage.eventopen.Visibility == Visibility.Collapsed && toKnow.IsActive)
                {
                    actualEvent = listEvents[0];

                    HomePage.homePage.eventopen.Navigate(typeof(EventPage), listEvents[0]);

                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                    HomePage.homePage.eventopen.Visibility = Visibility.Visible;
                }

                ListEvents.Visibility = Visibility.Visible;
            }
            else
            {
                ErrorMessage.Text = "Não há eventos futuros";
                ErrorScreen.Visibility = Visibility.Visible;
            }

            PRing.Visibility = Visibility.Collapsed;
            ErrorScreen.Visibility = Visibility.Collapsed;

            SuggestLogin();
        }

        private async void SuggestLogin()
        {
            if (!Other.Other.localSettings.Values.ContainsKey("suggest_login"))
            {
                MessageDialog md = new MessageDialog("Ao fazer login você:\n\n• Participa de sorteios exclusivos para quem usa o app.\n• Faz RSVP e check-in em eventos usando o aplicativo.\n• Vê a localização de eventos com localização oculta para não membros.\n• Recebe notificações sobre eventos que você marcou presença.\n\nDeseja fazer login agora?");
                md.Title = "Bem-vindo ao app do " + Other.Other.resourceLoader.GetString("AppName") + "!";

                md.Commands.Add(new UICommand("Sim", new UICommandInvokedHandler((c) =>
                {
                    MainPage.mainPage.settingsList.SelectedIndex = 1;
                }))
                { Id = 0 });
                md.Commands.Add(new UICommand("Não", null)
                { Id = 1 });

                await md.ShowAsync();

                Other.Other.localSettings.Values["suggest_login"] = true;
            }
        }

        private void ListEvents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListEvents.SelectedIndex != -1)
            {
                if (HomePage.homePage.eventopen.Visibility == Visibility.Collapsed)
                {
                    HomePage.homePage.eventopen.SetNavigationState("1,0");
                }

                Event a = e.AddedItems[0] as Event;
                HomePage.homePage.eventopen.Navigate(typeof(EventPage), a);

                actualEvent = a;

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                HomePage.homePage.eventopen.Visibility = Visibility.Visible;

                ListEvents.SelectedIndex = -1;
            }
        }

        public void Refresh_Click(object sender, RoutedEventArgs e)
        {
            GetEvents(true);
        }

        private void OpenMeetup_Click(object sender, RoutedEventArgs e)
        {
            Other.Other.OpenSite("http://meetup.com/" + Other.Other.resourceLoader.GetString("MeetupId"));
        }

        private async void DoCheckin_Click(object sender, RoutedEventArgs e)
        {
            if (Other.Other.localSettings.Values.ContainsKey("qr_code"))
            {
                HomePage.homePage.mainframe.Navigate(typeof(CheckinPage));
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
			// Caso o usuário esteja com uma versão antiga do app
            else if (Other.Other.localSettings.Values.ContainsKey("refresh_token"))
            {
                MainPage.mainPage.ToWebView(new Link() { Title = "Recebendo QR Code...", Value = Other.Other.GetLoginUrl() });
            }
            else
            {
                MessageDialog md = new MessageDialog("Deseja fazer login agora?");
                md.Title = "Você precisa fazer login para fazer check-in";

                md.Commands.Add(new UICommand("Sim", new UICommandInvokedHandler((c) =>
                {
                    MainPage.mainPage.settingsList.SelectedIndex = 1;
                }))
                { Id = 0 });
                md.Commands.Add(new UICommand("Não", null)
                { Id = 1 });

                await md.ShowAsync();
            }
        }
    }
}