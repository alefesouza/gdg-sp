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

using GDG_SP.Model;
using GDG_SP.Resx;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Share;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;

using Xamarin.Forms;

namespace GDG_SP
{
    /// <summary>
    /// Página onde é exibido os eventos.
    /// </summary>
    public partial class MainPage : ContentPage
    {
        public static MainPage main;
        public ObservableCollection<Event> listEvents;
        Event _event;
        public static int openEvent = 0;
        int time = 0;
        double imageWidth = 0;
        ToolbarItem more;
        bool tabletMenu;
        bool notificationAdded;

        public MainPage()
        {
            InitializeComponent();

            main = this;

            if (Device.Idiom == TargetIdiom.Desktop)
            {
                Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
                {
                    Column1.Width = 350;
                    Column2.Width = new GridLength(1, GridUnitType.Star);
                    return false;
                });
            }

            if (Device.Idiom == TargetIdiom.Tablet || Device.Idiom == TargetIdiom.Desktop)
            {
                imageWidth = 350;
            }

            if (Device.Idiom == TargetIdiom.Phone)
            {
                MainGrid.Children.Remove(EventWebView);
                MainGrid.ColumnDefinitions.Remove(Column2);
            }

            if (Device.OS == TargetPlatform.iOS || Device.Idiom == TargetIdiom.Tablet || Device.Idiom == TargetIdiom.Desktop)
            {
                more = new ToolbarItem("Mais", Other.Other.GetImage("More"), async () =>
                {
                    if (!tabletMenu)
                    {
                        string action = await DisplayActionSheet("Menu", "Cancelar", null, "Atualizar", "Fazer check-in", "Abrir meetup");

                        if (action != null)
                        {
                            if (action.Equals("Atualizar"))
                            {
                                GetEvents(true);
                            }
                            else if (action.Equals("Fazer check-in"))
                            {
                                OpenCheckin();
                            }
                            else if (action.Equals("Abrir meetup"))
                            {
                                Other.Other.OpenSite("meetup.com/" + AppResources.MeetupId, this);
                            }
                        }
                    }
                });

                ToolbarItems.Add(more);
            }
            else if(Device.OS == TargetPlatform.WinPhone)
            {
                ToolbarItems.Add(new ToolbarItem("Atualizar", Other.Other.GetImage("Refresh"), () =>
                {
                    GetEvents(true);
                }));

                ToolbarItems.Add(new ToolbarItem("Abrir meetup", Other.Other.GetImage("OpenMeetup"), () =>
                {
                    Other.Other.OpenSite("meetup.com/" + AppResources.MeetupId, this);
                }));

				ToolbarItems.Add(new ToolbarItem("Fazer check-in", null, () =>
				{
					OpenCheckin();
				}, ToolbarItemOrder.Secondary));
            }
            else
            {
                ToolbarItems.Add(new ToolbarItem("Atualizar", null, () =>
                {
                    GetEvents(true);
                }, ToolbarItemOrder.Secondary));

                ToolbarItems.Add(new ToolbarItem("Fazer check-in", null, () =>
                {
                    OpenCheckin();
                }, ToolbarItemOrder.Secondary));

                ToolbarItems.Add(new ToolbarItem("Abrir meetup", null, () =>
                {
                    Other.Other.OpenSite("meetup.com/" + AppResources.MeetupId, this);
                }, ToolbarItemOrder.Secondary));
            }

            TryAgain.Clicked += (s, e) =>
            {
                GetEvents();
            };

            GetEvents();
        }

        public async void GetEvents(bool refresh = false)
        {
            ErrorScreen.IsVisible = false;

            // PopToRootAsync não funciona como esperado no Windows
            if (Device.OS == TargetPlatform.Windows)
            {
                while (Navigation.NavigationStack.Count > 1)
                {
                    await Navigation.PopAsync();
                }
            }
            else
            {
                if (Navigation.NavigationStack.Count > 1)
                {
                    await Navigation.PopToRootAsync();
                }
            }

            ListEvents.IsVisible = false;
            Loading.IsVisible = true;

            listEvents = new ObservableCollection<Event>();

            string jsonString = "";

            try
            {
                // Pelo que parece o Windows Phone armazena uma solicitação http em cache e não atualiza tão facilmente, por isso precisa mudar alguma query
                if (!Other.Other.GetSetting("refresh_token").Equals(""))
                {
                    var client = new HttpClient();
                    client.MaxResponseContentBufferSize = 256000;
                    HttpResponseMessage response = await client.PostAsync(Other.Other.GetEventsUrl() + "&time=" + time++, Other.Other.GetRefreshToken(true));

                    if (response.IsSuccessStatusCode)
                    {
                        jsonString = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    jsonString = await new HttpClient().GetStringAsync(new Uri(Other.Other.GetEventsUrl() + "&time=" + time++));
                }
            }
            catch
            {
                if (refresh)
                {
                    var alert = await DisplayAlert("Verifique sua conexão de internet", "Deseja tentar novamente?", "Sim", "Não");
                    if (alert)
                    {
                        GetEvents(true);
                        ErrorScreen.IsVisible = false;
                    }
                    else
                    {
                        ListEvents.IsVisible = true;
                        Loading.IsVisible = false;
                    }
                }
                else
                {
                    ErrorMessage.Text = "Verifique sua conexão de internet";
                    ErrorScreen.IsVisible = true;
                    Loading.IsVisible = false;
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
                ErrorMessage.Text = "Houve um erro ao receber os eventos";
                ErrorScreen.IsVisible = true;
                Loading.IsVisible = false;
                return;
            }

            HeaderImage.Source = ImageSource.FromUri(new Uri(root["header"].ToString()));

            DependencyService.Get<IDependencies>().SendOneSignalTag("app_version", DependencyService.Get<IDependencies>().GetAppVersion());

            if ((int)root["member"]["id"] != 0)
            {
                LinksPage.member = JsonConvert.DeserializeObject<Person>(root["member"].ToString());
                Other.Other.AddSetting("member_profile", root["member"].ToString());

                LinksPage.linksPage.profileImage.Source = ImageSource.FromUri(new Uri(LinksPage.member.Photo));
                LinksPage.linksPage.profileName.Text = LinksPage.member.Name;
                LinksPage.linksPage.profileIntro.Text = LinksPage.member.Intro;

                if((bool)root["member"]["is_admin"])
                {
                    if (!notificationAdded)
                    {
                        LinksPage.linksPage.listLinks.Insert(0, new Link() { Title = "Enviar notificação", Icon = Device.OnPlatform("SendNotification.png", "ic_send.png", "Assets/Images/SendNotification.png"), Value = "send_notification" });
                        notificationAdded = true;
                    }
                }

				DependencyService.Get<IDependencies>().SendOneSignalTag("member_id", LinksPage.member.Id.ToString());
            }
            else
            {
                if (!Other.Other.GetSetting("refresh_token").Equals(""))
                {
                    Other.Other.AddSetting("refresh_token", "");
                    Other.Other.AddSetting("member_profile", "");
                    LinksPage.member = null;

                    string image = "";

                    image = Device.OnPlatform("Icon-72.png", "ic_launcher.png", "Assets/Square71x71Logo.scale-140.png");

                    if (Device.Idiom == TargetIdiom.Desktop)
                    {
                        image = "Assets/Square71x71Logo.scale-150.png";
                    }
                    else if ((Device.Idiom == TargetIdiom.Tablet && Device.OS == TargetPlatform.Windows))
                    {
                        image = "Assets/Square70x70Logo.scale-100.png";
                    }

                    LinksPage.linksPage.profileImage.Source = ImageSource.FromFile(image);
                    LinksPage.linksPage.profileName.Text = "Fazer login";
                    LinksPage.linksPage.profileIntro.Text = "Fazer login";
                }
            }

            // Notei que a primeira vez que abre no iOS ocorre um bug no tamanho da lista, e corrige se atualizar, como não tenho muito tempo para mexer em um Mac/simulador de iOS então vou deixar assim mesmo
            if (Device.OS == TargetPlatform.iOS)
            {
                if (Other.Other.GetSetting("isfirst").Equals(""))
                {
                    GetEvents();
                    Other.Other.AddSetting("isfirst", "n");
                }
            }

            if (listEvents.Count > 0)
            {
                    if (imageWidth > 0)
                    {
                        for (int i = 0; i < listEvents.Count; i++)
                        {
                            listEvents[i].HeightRequest = Other.Other.GetHeightImage(imageWidth - 20, listEvents[i].Image_width, listEvents[i].Image_height);
                        }
                    }

                    HeaderImage.SizeChanged += (s, e) =>
                    {
                        ForceLayout();

                        if (imageWidth == 0)
                        {
                            imageWidth = HeaderImage.Width;
                        }

                        HeaderImage.HeightRequest = Other.Other.GetHeightImage(imageWidth, (double)root["header_width"], (double)root["header_height"]);

                        if (imageWidth > 0)
                        {
                            for (int i = 0; i < listEvents.Count; i++)
                            {
                                listEvents[i].HeightRequest = Other.Other.GetHeightImage(imageWidth - 20, listEvents[i].Image_width, listEvents[i].Image_height);
                            }
                        }

                        ListEvents.ItemsSource = listEvents;
                        ForceLayout();
                    };

                ListEvents.ItemsSource = listEvents;

                if ((Device.Idiom == TargetIdiom.Desktop || Device.Idiom == TargetIdiom.Tablet || Device.Idiom == TargetIdiom.Desktop) && !refresh)
                {
                    _event = listEvents[0];

                    if (!tabletMenu)
                    {
                        ToolbarItems.Remove(more);

                        ToolbarItems.Add(new ToolbarItem("RSVP", Other.Other.GetImage("Rsvp"), async () =>
                        {
                            if (Other.Other.GetSetting("refresh_token").Length > 0)
                            {
                                if (_event.Rsvpable)
                                {
                                    await Navigation.PushAsync(new RSVPPage(_event));
                                }
                                else
                                {
                                    await DisplayAlert("", "Não é possível fazer RSVP nesse evento", "OK");
                                }
                            }
                            else
                            {
                                bool alert = await DisplayAlert("RSVP", "Você precisa fazer login para fazer RSVP, deseja fazer login agora?", "Sim", "Não");
                                if (alert)
                                {
                                    await Navigation.PushAsync(new WebViewPage(Other.Other.GetLoginUrl(), true, _event.Id) { Title = "Login" });
                                }
                            }
                        }));

                        ToolbarItems.Add(new ToolbarItem(_event.Who, Other.Other.GetImage("People"), () =>
                        {
                            Navigation.PushAsync(new TabbedPeoplePage(_event.Id) { Title = _event.Who });
                        }));

                        ToolbarItems.Add(new ToolbarItem("Compartilhar", Other.Other.GetImage("Share"), () =>
                        {
                            Other.Other.ShareLink(_event.Name, _event.Link);
                        }));

                        more.Command = new Command(async () =>
                        {
                            string action = await DisplayActionSheet("Menu", "Cancelar", null, "Atualizar", "Fazer check-in", "Abrir meetup", "Abrir no navegador");

                            if (action != null)
                            {
                                if (action.Equals("Atualizar"))
                                {
                                    GetEvents(true);
                                }
                                else if (action.Equals("Fazer check-in"))
                                {
                                    OpenCheckin();
                                }
                                else if (action.Equals("Abrir meetup"))
                                {
                                    Other.Other.OpenSite("meetup.com/" + AppResources.MeetupId, this);
                                }
                                else if (action.Equals("Abrir no navegador"))
                                {
                                    Other.Other.OpenSite(_event.Link, this);
                                }
                            }
                        });

                        ToolbarItems.Add(more);

                        tabletMenu = true;
                    }
                }

                if (openEvent != 0)
                {
                    foreach (Event e in listEvents)
                    {
                        if (e.Id == openEvent)
                        {
                            ListEvents.SelectedItem = e;
                        }
                    }
                    openEvent = 0;
                }
                else if (Device.Idiom == TargetIdiom.Desktop || Device.Idiom == TargetIdiom.Tablet)
                {
                    _event = listEvents[0];
                    TabletPost();
                }

                ListEvents.IsVisible = true;
            }
            else
            {
                ErrorMessage.Text = "Não há eventos futuros";
                ErrorScreen.IsVisible = true;
            }

            Loading.IsVisible = false;

            SuggestLogin();
        }

		private async void OpenCheckin()
		{
			if (!Other.Other.GetSetting("qr_code").Equals(""))
			{
				await Navigation.PushAsync(new CheckinPage());
			}
			else if(!Other.Other.GetSetting("refresh_token").Equals(""))
            {
                await Navigation.PushAsync(new WebViewPage(Other.Other.GetLoginUrl(), true) { Title = "Recebendo QR Code..." });
            }
            else
            {
				var alert = await DisplayAlert("Você precisa fazer login para fazer check-in", "Deseja fazer login agora?", "Sim", "Não");
				if (alert)
				{
					await Navigation.PushAsync(new WebViewPage(Other.Other.GetLoginUrl(), true) { Title = "Login" });
				}
			}
		}

        private void TabletPost()
        {
            EventWebView.Navigating -= WView_Navigating;
            string content = _event.Description;
            var htmlSource = new HtmlWebViewSource();
            htmlSource.Html = content;

            EventWebView.Source = htmlSource;
            EventWebView.Navigating += WView_Navigating;
        }

        private async void SuggestLogin()
        {
            if (Other.Other.GetSetting("suggest_login").Equals(""))
            {
                var alert = await DisplayAlert("Bem-vindo ao app do " + AppResources.AppName + "!", "Ao fazer login você:\n\n• Participa de sorteios exclusivos para quem usa o app.\n• Faz RSVP e check-in em eventos usando o app.\n• Vê a localização de eventos com localização oculta para não membros.\n• Recebe notificações sobre eventos que você marcou presença.\n\nDeseja fazer login agora?", "Sim", "Não");
                if (alert)
                {
                    await Navigation.PushAsync(new WebViewPage(Other.Other.GetLoginUrl(), true) { Title = "Login" });
                }
                Other.Other.AddSetting("suggest_login", "true");
            }
        }

        private void WView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url.StartsWith("http://do_login"))
            {
                Navigation.PushAsync(new WebViewPage(Other.Other.GetLoginUrl(), true) { Title = "Login" });
                e.Cancel = true;
            }
            else if (e.Url.StartsWith("http"))
            {
                Other.Other.OpenSite(e.Url, this);
                e.Cancel = true;
            }
        }

        // Menu ao pressionar um evento na lista no Windows Phone ou arrastar no iOS
        private async void HandleEventItem(object sender, EventArgs e)
        {
            Event _event = (sender as MenuItem).BindingContext as Event;

            if (_event == null)
                return;

            string action;

            if (Device.OS == TargetPlatform.Windows)
            {
                action = await DisplayActionSheet(_event.Name, "Cancelar", null, "Copiar link", "Compartilhar", "Abrir no navegador");
            }
            else
            {
                // O compartilhar do iOS já tem um copiar link
                action = await DisplayActionSheet(_event.Name, "Cancelar", null, "Compartilhar", "Abrir no navegador");
            }

            if (action != null)
            {
                if (action.Equals("Copiar link"))
                {
                    await CrossShare.Current.SetClipboardText(_event.Link);
                    Other.Other.ShowMessage("Link copiado com sucesso", this);
                }
                else if (action.Equals("Compartilhar"))
                {
                    await CrossShare.Current.ShareLink(_event.Link, _event.Name, _event.Name);
                }
                else if (action.Equals("Abrir no navegador"))
                {
                    Other.Other.OpenSite(_event.Link, this);
                }
            }
        }

        private async void ListEvents_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            Event _event = e.SelectedItem as Event;

            if (Device.Idiom == TargetIdiom.Desktop || Device.Idiom == TargetIdiom.Tablet)
            {
                this._event = _event;
                TabletPost();
            }
            else
            {
                await Navigation.PushAsync(new EventPage(_event));
            }

            Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
            {
                ((ListView)sender).SelectedItem = null;
                return false;
            });
        }
    }
}
