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
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace GDGSP
{
    /// <summary>
    /// Página onde é exibido o evento selecionado.
    /// </summary>
    public sealed partial class EventPage : Page
    {
        Event _event;

        public EventPage()
        {
            Other.Other.Transition(this);
            this.InitializeComponent();

            if (Other.Other.IsMobile())
            {
                Grid.SetRow(CB, 2);
                CB.Padding = new Thickness(0, 0, 0, 0);
                CBMobile.Visibility = Visibility.Visible;
                CBTitle.Visibility = Visibility.Collapsed;
                CB.Background = new SolidColorBrush(Color.FromArgb(255, 27, 27, 27));
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _event = e.Parameter as Event;

            CBConfirmed.Label = _event.Who;
            EventWebView.NavigateToString(_event.Description);
            EventWebView.NavigationStarting += EventWebView_NavigationStarting;
        }

        public void EventWebView_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            string url = args.Uri.ToString();

            if (url.StartsWith("http://do_login"))
            {
                MainPage.mainPage.toLogin = true;
                WebViewPage.openEvent = _event.Id;
                MainPage.mainPage.ToWebView(new Link() { Title = "Login", Value = Other.Other.GetLoginUrl() });
            }
            else
            {
                MainPage.mainPage.ToWebView(new Link() { Title = "GDG-SP", Value = url });
            }

            MainPage.mainPage.toWebView = true;

            args.Cancel = true;
        }

        private void CBShare_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager datatransfermanager = DataTransferManager.GetForCurrentView();
            datatransfermanager.DataRequested += new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(ShareTextHandler);
            DataTransferManager.ShowShareUI();
        }

        private void CBCopyLink_Click(object sender, RoutedEventArgs e)
        {
            DataPackage dataPackage = new DataPackage();
            dataPackage.RequestedOperation = DataPackageOperation.Copy;
            dataPackage.SetText(_event.Link);
            Clipboard.SetContent(dataPackage);
        }

        private void CBOpenSite_Click(object sender, RoutedEventArgs e)
        {
            MainPage.mainPage.ToWebView(new Link() { Title = _event.Name, Value = _event.Link });
        }

        private async void CBRSVP_Click(object sender, RoutedEventArgs e)
        {
            if (Other.Other.localSettings.Values.ContainsKey("refresh_token"))
            {
                if (_event.Rsvpable)
                {
                    Frame.Navigate(typeof(RSVPPage), _event);
                }
                else
                {
                    Other.Other.ShowMessage("Não é possível fazer RSVP nesse evento");
                }
            }
            else
            {
                MessageDialog md = new MessageDialog("Você precisa fazer login para fazer RSVP, deseja fazer login agora?");
                md.Title = "RSVP";

                md.Commands.Add(new UICommand("Sim", new UICommandInvokedHandler((c) => {
                    MainPage.mainPage.toLogin = true;
                    MainPage.mainPage.ToWebView(new Link() { Title = "Login", Value = Other.Other.GetLoginUrl() });
                    WebViewPage.openEvent = _event.Id;
                }))
                { Id = 0 });
                md.Commands.Add(new UICommand("Não", null) { Id = 1 });

                await md.ShowAsync();
            }
        }

        private async void ShareTextHandler(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;

            request.Data.Properties.Title = "Compartilhar link";
            request.Data.Properties.Description = Convert.ToString(_event.Link);

            try
            {
                string text = _event.Name + " " + Convert.ToString(_event.Link);
                request.Data.SetText(text);
            }
            catch
            {
                MessageDialog dialog = new MessageDialog("Houve um erro.");
                await dialog.ShowAsync();
            }
        }

        private void CBConfirmed_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(PeoplePage), _event);
        }
    }
}