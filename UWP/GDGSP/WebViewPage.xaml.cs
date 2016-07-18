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
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace GDGSP
{
    /// <summary>
    /// Página para exibir páginas web dentro do aplicativo
    /// </summary>
    public sealed partial class WebViewPage : Page
    {
        public static WebView WV;
        public static TextBlock title;
        public static int openEvent = 0;

        public WebViewPage()
        {
            this.InitializeComponent();
            Other.Other.Transition(this);
            WV = webView1;

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
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Link info = e.Parameter as Link;

            title.Text = info.Title;

            if (info.Title.Equals("Login"))
            {
                CBOpen.Visibility = Visibility.Collapsed;
                CBShare.Visibility = Visibility.Collapsed;
            }

            webView1.Navigate(new Uri(info.Value));
            webView1.ContentLoading += webView1_ContentLoading;
            webView1.NavigationStarting += webView1_NavigationStarting;
            webView1.LoadCompleted += webView1_LoadCompleted;
        }

        private void webView1_ContentLoading(WebView sender, WebViewContentLoadingEventArgs args)
        {
            CBBack.IsEnabled = webView1.CanGoForward;
            CBForward.IsEnabled = webView1.CanGoBack;

            progress1.IsActive = true;
            progress.IsIndeterminate = true;
            progress.Visibility = Visibility.Visible;
        }

        private async void webView1_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            string url = args.Uri.ToString();
            progress.IsIndeterminate = true;
            progress.Visibility = Visibility.Visible;

            if(!MainPage.mainPage.toLogin)
            {
                CBOpen.Visibility = Visibility.Visible;
                CBShare.Visibility = Visibility.Visible;
            }
            else
            {
                CBOpen.Visibility = Visibility.Collapsed;
                CBShare.Visibility = Visibility.Collapsed;
            }

            if (url.Contains(Other.Other.backendUrl) && MainPage.mainPage.toLogin)
            {
                if (url.Contains("refresh_token"))
                {
                    ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                    localSettings.Values["refresh_token"] = Other.Other.GetQuery(url, "refresh_token");
                    MainPage.openEvent = openEvent;
                    openEvent = 0;
                    HomePage.homePage.eventopen.Visibility = Visibility.Collapsed;
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                    Other.Other.CreateTile();
                    EventsPage.events.GetEvents(true);
                    MainPage.mainPage.toWebView = false;
                    FrameGoBack();
                }
                else if(url.Contains("error"))
                {
                    MessageDialog md = new MessageDialog("Deseja tentar novamente?");
                    md.Title = "Erro ao fazer login";

                    md.Commands.Add(new UICommand("Sim", new UICommandInvokedHandler((c) => {
                        (sender as WebView).Source = new Uri(Other.Other.GetLoginUrl());
                    }))
                    { Id = 0 });
                    md.Commands.Add(new UICommand("Não", new UICommandInvokedHandler((c) => {
                        FrameGoBack();
                    }))
                    { Id = 1 });

                    await md.ShowAsync();
                }
                else if (url.Contains("nonmember=none"))
                {
                    MessageDialog md = new MessageDialog("Parece que você ainda não faz parte do " + Other.Other.resourceLoader.GetString("AppName") + ", deseja participar agora?");

                    md.Commands.Add(new UICommand("Sim", new UICommandInvokedHandler((c) => {
                        MainPage.mainPage.toLogin = false;

                        (sender as WebView).Source = new Uri("http://meetup.com/" + Other.Other.resourceLoader.GetString("MeetupId"));
                    }))
                    { Id = 0 });
                    md.Commands.Add(new UICommand("Não", new UICommandInvokedHandler((c) => {
                        FrameGoBack();
                    }))
                    { Id = 1 });

                    await md.ShowAsync();
                }
                else if (url.Contains("nonmember=pending"))
                {
                    MessageDialog md = new MessageDialog("Por favor, aguarde sua aprovação no " + Other.Other.resourceLoader.GetString("AppName") + " e tente novamente");

                    md.Commands.Add(new UICommand("OK", new UICommandInvokedHandler((c) => {
                        FrameGoBack();
                    }))
                    { Id = 0 });

                    await md.ShowAsync();
                }
            }

            progress1.IsActive = true;

            CBForward.IsEnabled = false;
        }

        private void FrameGoBack()
        {
            MainPage.mainPage.headerList.SelectedIndex = 0;
            MainPage.mainPage.mainFrame.GoBack();
        }

        private void webView1_LoadCompleted(object sender, NavigationEventArgs e)
        {
            progress1.IsActive = false;
            progress.IsIndeterminate = false;
            progress.Visibility = Visibility.Collapsed;

            if (webView1.DocumentTitle.ToString().Equals(""))
            {
                title.Text = Other.Other.resourceLoader.GetString("AppName");
            }
            else
            {
                title.Text = webView1.DocumentTitle.ToString();
            }
        }

        private async void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;

            request.Data.Properties.Title = "Compartilhar link";
            request.Data.Properties.Description = Convert.ToString(webView1.Source);

            try
            {
                string text = webView1.DocumentTitle + " " + Convert.ToString(webView1.Source);
                request.Data.SetText(text);
            }
            catch
            {
                MessageDialog dialog = new MessageDialog("Houve um erro.");
                await dialog.ShowAsync();
            }
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager datatransfermanager = DataTransferManager.GetForCurrentView();
            datatransfermanager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(ShareTextHandler);
            DataTransferManager.ShowShareUI();
        }

        private void CBBack_Click(object sender, RoutedEventArgs e)
        {
            if (webView1.CanGoBack)
            {
                webView1.GoBack();
            }
        }

        private void CBForward_Click(object sender, RoutedEventArgs e)
        {
            if (webView1.CanGoForward)
            {
                webView1.GoForward();
            }
        }

        private void CBRefresh_Click(object sender, RoutedEventArgs e)
        {
            webView1.Refresh();
        }

        private void CBOpen_Click(object sender, RoutedEventArgs e)
        {
            OpenBrowser();
        }

        private async void OpenBrowser()
        {
            await Launcher.LaunchUriAsync(new Uri(webView1.Source.ToString()));
        }
    }
}
