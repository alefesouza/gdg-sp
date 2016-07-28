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
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.System;

namespace GDGSP
{
    /// <summary>
    /// Página principal do aplicativo, responsável pela barra lateral e alguns dos principais comandos.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ListBox settingsList, headerList;
        public Frame mainFrame;
        public TextBlock profileName, profileIntro;
        public ImageBrush profilePhoto;
        public ListBoxItem sendNotification;

        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static MainPage mainPage;

        /// <summary>
        /// Atributos de ações a serem tomadas caso o usuário foi para as configurações, para a WebViewPage ou fazer login.
        /// </summary>
        public bool toSettings = false, toWebView = false, toLogin = false;

        /// <summary>
        /// Informações do usuário atual.
        /// </summary>
        public Person member = null;

        /// <summary>
        /// Evento a ser aberto caso o usuário abra o aplicativo por uma notificação, depois de fazer RSVP ou login.
        /// </summary>
        public static int openEvent = 0;

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => { LeftViewModel = DataContext as ViewModels.LeftPanelViewModel; };

            mainPage = this;

            mainFrame = MainFrame;
            profileName = ProfileName;
            profilePhoto = ProfilePhoto;
            profileIntro = ProfileIntro;

            sendNotification = SendNotification;

            if (localSettings.Values.ContainsKey("member_profile"))
            {
                member = JsonConvert.DeserializeObject<Person>(localSettings.Values["member_profile"].ToString());

                ProfileName.Text = member.Name;
                ProfileIntro.Text = member.Intro;
                ProfilePhoto.ImageSource = new BitmapImage(new Uri(member.Photo, UriKind.Absolute));
            }

            settingsList = SettingsLB;
            headerList = HeaderLB;

            SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

            Other.Other.RequestChannel();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            MainFrame.Navigate(typeof(HomePage));
            Other.Other.CreateTile();

            string parameter = e.Parameter as string;
            if (!string.IsNullOrEmpty(parameter))
            {
                mainPage.LaunchParam = parameter + "|0";
            }
        }

        private void ListLeftPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListLeftPanel.SelectedIndex != -1)
            {
                Link info = e.AddedItems[0] as Link;

                ToWebView(info);
            }
        }

        /// <summary>
        /// Método que contém todas as medidas a serem tomadas ao abrir uma WebView.
        /// </summary>
        /// <param name="info">Objeto que contém título e link.</param>
        public void ToWebView(Link info)
        {
            info.Value = info.Value.StartsWith("http") ? info.Value : "http://" + info.Value;

            if (info.Value.Equals(Other.Other.GetLoginUrl()))
            {
                toLogin = true;
            }
            else
            {
                toLogin = false;
            }

            if (toSettings)
            {
                MainFrame.GoBack();
                toSettings = false;
            }

            if (toWebView)
            {
                WebViewPage.title.Text = info.Title;
                WebViewPage.WV.Navigate(new Uri(info.Value));
            }
            else
            {
                MainFrame.Navigate(typeof(WebViewPage), info);
            }

            toWebView = true;
            toSettings = false;

            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            if (MainSplitView.IsPaneOpen == true)
            {
                MainSplitView.IsPaneOpen = false;
            }

            ListLeftPanel.SelectedIndex = -1;
            SettingsLB.SelectedIndex = -1;
            HeaderLB.SelectedIndex = -1;
        }

        public ViewModels.LeftPanelViewModel LeftViewModel { get; set; }

        private void HamburguerButton_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = !MainSplitView.IsPaneOpen;
        }

        private void HeaderLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HeaderLB.SelectedIndex != -1)
            {
                if (toSettings || toWebView)
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = HomePage.homePage.eventopen.Visibility == Visibility.Visible ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                    MainFrame.GoBack();
                    toSettings = false;
                    toWebView = false;
                }

                if (MainSplitView.IsPaneOpen == true)
                {
                    MainSplitView.IsPaneOpen = false;
                }

                SettingsLB.SelectedIndex = -1;

                if (e.AddedItems[0] == SendNotification)
                {
                    HomePage.homePage.mainframe.Navigate(typeof(SendNotificationPage));
                }
                else
                {
                    HomePage.homePage.mainframe.Navigate(typeof(EventsPage));
                }
            }
        }

        private async void SettingsLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SettingsLB.SelectedIndex != -1)
            {
                if (SettingsLB.SelectedIndex == 0)
                {
                    await Launcher.LaunchUriAsync(new Uri("mailto:" + Other.Other.resourceLoader.GetString("ContactMail")));

                    SettingsLB.SelectedIndex = -1;
                }
                else if (SettingsLB.SelectedIndex == 1)
                {
                    if (member == null)
                    {
                        ToWebView(new Link() { Title = "Login", Value = Other.Other.GetLoginUrl() });
                    }
                    else
                    {
                        JsonObject profile = JsonObject.Parse(localSettings.Values["member_profile"].ToString());

                        ToWebView(new Link() { Title = profile.GetNamedString("name"), Value = "http://meetup.com/" + Other.Other.resourceLoader.GetString("MeetupId") + "/member/" + profile.GetNamedNumber("id") });
                    }
                    toWebView = true;

                    HeaderLB.SelectedIndex = -1;
                    SettingsLB.SelectedIndex = -1;
                }
                else if (SettingsLB.SelectedIndex == 2)
                {
                    if (toWebView)
                    {
                        MainFrame.GoBack();
                        toWebView = false;
                    }

                    MainFrame.Navigate(typeof(SettingsPage));

                    HeaderLB.SelectedIndex = -1;
                    toSettings = true;
                }
                else
                {
                    SettingsLB.SelectedIndex = -1;
                }

                if (MainSplitView.IsPaneOpen)
                {
                    MainSplitView.IsPaneOpen = false;
                }
            }
        }

        public void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            // Activity e Fragment fazem falta
            if (toSettings)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = HomePage.homePage.eventopen.Visibility == Visibility.Visible ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                toSettings = false;

                settingsList.SelectedIndex = -1;

                HeaderLB.SelectedIndex = 0;
                mainFrame.GoBack();
            }
            else if (toWebView)
            {
                if (WebViewPage.WV.CanGoBack)
                {
                    WebViewPage.WV.GoBack();
                }
                else
                {
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = HomePage.homePage.eventopen.Visibility == Visibility.Visible ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                    toWebView = false;

                    HeaderLB.SelectedIndex = 0;
                    mainFrame.GoBack();
                }
            }
            else
            {
                if (HomePage.homePage.mainframe.CanGoBack)
                {
                    HomePage.homePage.mainframe.GoBack();
                }
                else if (HomePage.homePage.eventopen.Visibility == Visibility.Visible)
                {
                    // My canGoBack always return false, that's my programmer way https://github.com/alefesouza/alefe-ultimate-programmer/
                    if (HomePage.homePage.eventopen.CanGoBack)
                    {
                        HomePage.homePage.eventopen.GoBack();
                    }
                    else
                    {
                        HomePage.homePage.eventopen.Visibility = Visibility.Collapsed;
                        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                    }
                }
                else
                {
                    Application.Current.Exit();
                }
            }

            e.Handled = true;
        }
    }
}
