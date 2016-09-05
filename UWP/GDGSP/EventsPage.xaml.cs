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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
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
        public ObservableCollection<Event> listEvents = null, pastEvents;
        ObservableCollection<Tweet> listTweets;
        public static Event actualEvent = null;
        string max_id;
        bool isLoadingTweets, isLoadingPast;
        int page = 2;

        private static EventsPage instance;

        public static EventsPage Instance
        {
            get
            {
                return instance;
            }
        }

        public EventsPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            Other.Other.Transition(this);

            instance = this;

            // ToKnow é um elemento/variável que eu uso para saber se a tela está com duas colunas
            toKnow = ToKnow;

            if (HomePage.Instance.eventopen.Visibility == Visibility.Collapsed)
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

            PastTryAgainFooter.Click += (s, e) =>
            {
                GetEvents(page);
            };

            TweetsTryAgainFooter.Click += (s, e) =>
            {
                GetTweets(max_id);
            };

            ListsPivot.SelectionChanged += (s, e) =>
            {
                TweetsRefresh.Visibility = ListsPivot.SelectedIndex == 2 ? Visibility.Visible : Visibility.Collapsed;
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            GetEvents();
            GetTweets();
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

            PastErrorScreen.Visibility = Visibility.Collapsed;
            PastScroll.Visibility = Visibility.Collapsed;
            PastPRing.Visibility = Visibility.Visible;

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

                        PastScroll.Visibility = Visibility.Visible;
                        PastPRing.Visibility = Visibility.Collapsed;
                    }))
                    { Id = 1 });

                    await md.ShowAsync();
                }
                else
                {
                    ErrorMessage.Text = "Verifique sua conexão de internet";
                    ErrorScreen.Visibility = Visibility.Visible;
                    PRing.Visibility = Visibility.Collapsed;

                    PastErrorMessage.Text = "Verifique sua conexão de internet";
                    PastErrorScreen.Visibility = Visibility.Visible;
                    PastPRing.Visibility = Visibility.Collapsed;
                }
                return;
            }

            JObject root = null;

            try
            {
                root = JObject.Parse(jsonString);
                listEvents = JsonConvert.DeserializeObject<ObservableCollection<Event>>(root["events"].ToString());
            }
            catch
            {
                ErrorMessage.Text = "Houve um erro ao receber a lista de eventos";
                ErrorScreen.Visibility = Visibility.Visible;
                PRing.Visibility = Visibility.Collapsed;

                PastErrorMessage.Text = "Houve um erro ao receber a lista de eventos";
                PastErrorScreen.Visibility = Visibility.Visible;
                PastPRing.Visibility = Visibility.Collapsed;
                return;
            }

            // Se o ID for diferente de 0 significa que o usuário está fez login, então preenche as informações dele
            if ((int)root["member"]["id"] != 0)
            {
                MainPage mainPage = MainPage.Instance;

                mainPage.member = JsonConvert.DeserializeObject<Person>(root["member"].ToString());
                Other.Other.localSettings.Values["member_profile"] = root["member"].ToString();

                mainPage.profilePhoto.ImageSource = new BitmapImage(new Uri(mainPage.member.Photo));
                mainPage.profileName.Text = mainPage.member.Name;
                mainPage.profileIntro.Text = mainPage.member.Intro;

                if ((bool)root["member"]["is_admin"])
                {
                    mainPage.sendNotification.Visibility = Visibility.Visible;
                    mainPage.raffleManager.Visibility = Visibility.Visible;
                }
            }
            else
            {
                // Se retornar 0 e o aplicativo conter a configuração refresh_token, significa que tem algo de errado com o login deve, como revogação de acesso por exemplo, nesse caso apaga todas as informações de login
                if (Other.Other.localSettings.Values.ContainsKey("refresh_token"))
                {
                    MainPage mainPage = MainPage.Instance;

                    Other.Other.localSettings.Values.Remove("refresh_token");
                    Other.Other.localSettings.Values.Remove("member_profile");
                    mainPage.profilePhoto.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/Images/Logo.png", UriKind.Absolute));
                    mainPage.profileName.Text = "Fazer login";
                    mainPage.profileIntro.Text = "Fazer login";
                }
            }

            HeaderImage.Source = new BitmapImage(new Uri(root["header"].ToString(), UriKind.Absolute));
            PastHeaderImage.Source = new BitmapImage(new Uri(root["header"].ToString(), UriKind.Absolute));

            listEvents = JsonConvert.DeserializeObject<ObservableCollection<Event>>(root["events"].ToString());
            pastEvents = JsonConvert.DeserializeObject<ObservableCollection<Event>>(root["past_events"].ToString());

            ListPastEvents.ItemsSource = pastEvents;
            PastScroll.Visibility = Visibility.Visible;

            if (listEvents.Count > 0)
            {
                HomePage homePage = HomePage.Instance;

                ListEvents.ItemsSource = listEvents;

                if (MainPage.openEvent != 0)
                {
                    for (int i = 0; i < listEvents.Count; i++)
                    {
                        if (listEvents[i].Id == MainPage.openEvent)
                        {
                            homePage.eventopen.SetNavigationState("1,0");
                            ListEvents.SelectedIndex = i;
                            MainPage.openEvent = 0;
                        }
                    }
                }
                else if (homePage.eventopen.Visibility == Visibility.Collapsed && toKnow.IsActive)
                {
                    actualEvent = listEvents[0];

                    homePage.eventopen.Navigate(typeof(EventPage), listEvents[0]);

                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                    homePage.eventopen.Visibility = Visibility.Visible;
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

            PastPRing.Visibility = Visibility.Collapsed;
            PastErrorScreen.Visibility = Visibility.Collapsed;

            SuggestLogin();
        }

        public async void GetEvents(int page)
        {
            PastTryAgainFooter.Visibility = Visibility.Collapsed;
            PastErrorScreen.Visibility = Visibility.Collapsed;
            PastLoading.Visibility = Visibility.Visible;

            string jsonString = "";

            try
            {
                var client = new HttpClient();
                client.MaxResponseContentBufferSize = 256000;
                HttpResponseMessage response = await client.GetAsync(Other.Other.GetEventsUrl(page));

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    jsonString = result;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                PastLoading.Visibility = Visibility.Collapsed;
                PastTryAgainFooter.Visibility = Visibility.Visible;
                Other.Other.ShowMessage("Eventos anteriores\n\nVerifique sua conexão de internet");
                return;
            }

            JObject root = null;

            try
            {
                root = JObject.Parse(jsonString);
                this.page++;
                string events = root["past_events"].ToString();
                ObservableCollection<Event> newEvents = JsonConvert.DeserializeObject<ObservableCollection<Event>>(events);
                foreach (Event _event in newEvents)
                {
                    pastEvents.Add(_event);
                }
            }
            catch
            {
                Other.Other.ShowMessage("Houve um erro ao receber a lista de tweets");
                return;
            }

            if ((bool)root["more_past_events"])
            {
                isLoadingPast = false;
                PastLoading.Visibility = Visibility.Visible;
                PastMessage.Visibility = Visibility.Collapsed;
            }
            else
            {
                isLoadingPast = true;
                PastLoading.Visibility = Visibility.Collapsed;
                PastMessage.Visibility = Visibility.Visible;
            }

            PastLoading.Visibility = Visibility.Collapsed;
            ListEvents.Visibility = Visibility.Visible;
        }

        public async void GetTweets(string max_id = "")
        {
            if (max_id.Equals(""))
            {
                TweetsPRing.Visibility = Visibility.Visible;
                TweetsScroll.Visibility = Visibility.Collapsed;
            }

            TweetsTryAgainFooter.Visibility = Visibility.Collapsed;
            TweetsErrorScreen.Visibility = Visibility.Collapsed;
            TweetsLoading.Visibility = Visibility.Visible;

            string jsonString = "";

            try
            {
                var client = new HttpClient();
                client.MaxResponseContentBufferSize = 256000;
                HttpResponseMessage response = await client.GetAsync(Other.Other.GetTweetsUrl(max_id));

                if (response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();

                    jsonString = result;
                }
                else
                {
                    throw new Exception();
                }
            }
            catch
            {
                TweetsLoading.Visibility = Visibility.Collapsed;
                TweetsTryAgainFooter.Visibility = Visibility.Visible;
                Other.Other.ShowMessage("Tweets\n\nVerifique sua conexão de internet");
                return;
            }

            JObject root = null;

            try
            {
                if (max_id.Equals(""))
                {
                    root = JObject.Parse(jsonString);
                    this.max_id = root["max_id"].ToString();
                    string tweets = root["tweets"].ToString();
                    listTweets = JsonConvert.DeserializeObject<ObservableCollection<Tweet>>(tweets);
                    ListTweets.ItemsSource = listTweets;
                }
                else
                {
                    root = JObject.Parse(jsonString);
                    this.max_id = root["max_id"].ToString();
                    string tweets = root["tweets"].ToString();
                    ObservableCollection<Tweet> newTweets = JsonConvert.DeserializeObject<ObservableCollection<Tweet>>(tweets);
                    foreach (Tweet tweet in newTweets)
                    {
                        listTweets.Add(tweet);
                    }
                }
            }
            catch
            {
                Other.Other.ShowMessage("Houve um erro ao receber a lista de tweets");
                return;
            }

            if ((bool)root["more_tweets"])
            {
                isLoadingTweets = false;
                TweetsLoading.Visibility = Visibility.Visible;
                TweetsMessage.Visibility = Visibility.Collapsed;
            }
            else
            {
                isLoadingTweets = true;
                TweetsLoading.Visibility = Visibility.Collapsed;
                TweetsMessage.Visibility = Visibility.Visible;
            }

            TweetsLoading.Visibility = Visibility.Collapsed;
            TweetsScroll.Visibility = Visibility.Visible;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var verticalOffset = (sender as ScrollViewer).VerticalOffset;
            var maxVerticalOffset = (sender as ScrollViewer).ScrollableHeight;

            if (maxVerticalOffset < 0 ||
                verticalOffset == maxVerticalOffset)
            {
                if(sender == PastScroll)
                {
                    if (!isLoadingPast)
                    {
                        GetEvents(page);
                        isLoadingPast = true;
                    }
                }
                else if(sender == TweetsScroll)
                {
                    if (!isLoadingTweets)
                    {
                        GetTweets(max_id);
                        isLoadingTweets = true;
                    }
                }
            }
        }

        private async void SuggestLogin()
        {
            if (!Other.Other.localSettings.Values.ContainsKey("suggest_login"))
            {
                MessageDialog md = new MessageDialog("Ao fazer login você:\n\n• Participa de sorteios exclusivos para quem usa o app.\n• Faz RSVP e check-in em eventos usando o aplicativo.\n• Vê a localização de eventos com localização oculta para não membros.\n• Recebe notificações sobre eventos que você marcou presença.\n\nDeseja fazer login agora?");
                md.Title = "Bem-vindo ao app do " + Other.Other.resourceLoader.GetString("AppName") + "!";

                md.Commands.Add(new UICommand("Sim", new UICommandInvokedHandler((c) =>
                {
                    MainPage.Instance.settingsList.SelectedIndex = 1;
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
            if (ListEvents.SelectedIndex != -1 || ListPastEvents.SelectedIndex != -1)
            {
                HomePage homePage = HomePage.Instance;

                if (homePage.eventopen.Visibility == Visibility.Collapsed)
                {
                    homePage.eventopen.SetNavigationState("1,0");
                }

                Event a = e.AddedItems[0] as Event;
                homePage.eventopen.Navigate(typeof(EventPage), a);

                actualEvent = a;

                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                homePage.eventopen.Visibility = Visibility.Visible;

                ListEvents.SelectedIndex = -1;
                ListPastEvents.SelectedIndex = -1;
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

        private void TweetsRefresh_Click(object sender, RoutedEventArgs e)
        {
            GetTweets();
        }

        private void TryAgain_Click(object sender, RoutedEventArgs e)
        {
            ErrorScreen.Visibility = Visibility.Collapsed;
            PastErrorScreen.Visibility = Visibility.Collapsed;
            GetEvents();
        }

        private async void DoCheckin_Click(object sender, RoutedEventArgs e)
        {
            if (Other.Other.localSettings.Values.ContainsKey("qr_code"))
            {
                HomePage.Instance.mainframe.Navigate(typeof(CheckinPage));
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            }
			// Caso o usuário esteja com uma versão antiga do app
            else if (Other.Other.localSettings.Values.ContainsKey("refresh_token"))
            {
                MainPage.Instance.ToWebView(new Link() { Title = "Recebendo QR Code...", Value = Other.Other.GetLoginUrl() });
            }
            else
            {
                MessageDialog md = new MessageDialog("Deseja fazer login agora?");
                md.Title = "Você precisa fazer login para fazer check-in";

                md.Commands.Add(new UICommand("Sim", new UICommandInvokedHandler((c) =>
                {
                    MainPage.Instance.settingsList.SelectedIndex = 1;
                }))
                { Id = 0 });
                md.Commands.Add(new UICommand("Não", null)
                { Id = 1 });

                await md.ShowAsync();
            }
        }
    }
}