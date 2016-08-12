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

using Plugin.Share;
using System;
using System.Collections.Generic;
using GDG_SP.Resx;
using Xamarin.Forms;
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace GDG_SP
{
    public partial class WebViewPage : ContentPage
    {
        bool isLogin;
        int eventId;

        public WebViewPage(string value, bool isLogin = false, int eventId = 0)
        {
            InitializeComponent();

            this.isLogin = isLogin;
            this.eventId = eventId;

            if (!isLogin)
            {
                ToolbarItems.Add(new ToolbarItem("Compartilhar", Other.Other.GetImage("Share"), () =>
                {
                    CrossShare.Current.ShareLink((WView.Source as UrlWebViewSource).Url, "", AppResources.AppName);
                }));
            }

            if (Device.OS == TargetPlatform.iOS)
            {
                ToolbarItems.Add(new ToolbarItem("Mais", "More.png", async () =>
                {
                    string action = "";
                    
                    if(this.isLogin)
                    {
                        action = await DisplayActionSheet("Menu", "Cancelar", null, "Voltar", "Avançar", "Recarregar");
                    }
                    else
                    {
                        action = await DisplayActionSheet("Menu", "Cancelar", null, "Voltar", "Avançar", "Recarregar", "Abrir no navegador");
                    }

                    if (action != null)
                    {
                        if (action.Equals("Voltar"))
                        {
                            if (WView.CanGoBack)
                            {
                                WView.GoBack();
                            }
                        }
                        else if (action.Equals("Avançar"))
                        {
                            if (WView.CanGoForward)
                            {
                                WView.GoForward();
                            }
                        }
                        else if (action.Equals("Recarregar"))
                        {
                            WView.Source = (WView.Source as UrlWebViewSource).Url;
                        }
                        else if (action.Equals("Abrir no navegador"))
                        {
                            Other.Other.OpenSite((WView.Source as UrlWebViewSource).Url, this);
                        }
                    }
                }));
            }
            else
            {
                ToolbarItems.Add(new ToolbarItem("Voltar", null, () =>
                {
                    if (WView.CanGoBack)
                    {
                        WView.GoBack();
                    }
                }, ToolbarItemOrder.Secondary));

                ToolbarItems.Add(new ToolbarItem("Avançar", null, () =>
                {
                    if (WView.CanGoForward)
                    {
                        WView.GoForward();
                    }
                }, ToolbarItemOrder.Secondary));

                ToolbarItems.Add(new ToolbarItem("Recarregar", null, () =>
                {
                    WView.Source = (WView.Source as UrlWebViewSource).Url;
                }, ToolbarItemOrder.Secondary));

                if(!isLogin)
                {
                    ToolbarItems.Add(new ToolbarItem("Abrir no navegator", null, () =>
                    {
                        Other.Other.OpenSite((WView.Source as UrlWebViewSource).Url, this);
                    }, ToolbarItemOrder.Secondary));
                }
            }

            WView.Source = value;
            WView.Navigating += WView_Navigating;
        }

        private void WView_Navigating(object sender, WebNavigatingEventArgs e)
        {
			if (isLogin && e.Url.Contains(AppResources.BackendUrl))
			{
				if (e.Url.Contains("code="))
				{
					GetCode(Other.Other.GetQuery(e.Url, "code"));
				}
			}
			// Gambiarrinha que evita um erro que ocorre no site do Meetup ao fazer login com redes sociais.
			else if (e.Url.Contains("submit=") && e.Url.Contains("response=") && e.Url.Contains("meetup.com"))
			{
				WView.Source = Other.Other.GetLoginUrl();
			}
        }

		private async void GetCode(string code)
		{
			var postData = new List<KeyValuePair<string, string>>();
			postData.Add(new KeyValuePair<string, string>("code", code));

			var client = new HttpClient();
			client.MaxResponseContentBufferSize = 256000;
			HttpResponseMessage response = await client.PostAsync(Other.Other.GetLoginUrl(), new FormUrlEncodedContent(postData));

			if (response.IsSuccessStatusCode)
			{
				string jsonString = await response.Content.ReadAsStringAsync();
				JObject json = JObject.Parse(jsonString);

				if (!(bool)json["is_error"])
				{
					string refresh_token = json["refresh_token"].ToString();
					// Tentei usar o ZXing.Net.Mobile direto porém não consegui o BarcodeWriter em uma Image do Xamarin, por isso fiz em Base64 no momento do login
					string qr_code = json["qr_code"].ToString();

					if (!refresh_token.Equals(""))
					{
						Other.Other.AddSetting("refresh_token", refresh_token);
						Other.Other.AddSetting("qr_code", qr_code);
						await DisplayAlert("", "Login realizado com sucesso", "OK");
						MainPage.openEvent = eventId;
						MainPage.main.GetEvents(false);
					}
				}
				else
				{
					string description = json["description"].ToString();

					if (description.Equals("error"))
					{
                        loginError();
					}
					else if (description.Equals("none"))
					{
						bool alert = await DisplayAlert(null, "Parece que você ainda não faz parte do " + AppResources.AppName + ", deseja participar agora?", "Sim", "Não");

						if (alert)
						{
							ToolbarItems.Add(new ToolbarItem("Compartilhar", Device.OnPlatform("Share.png", "", "Assets/Share.png"), () =>
							{
								CrossShare.Current.ShareLink((WView.Source as UrlWebViewSource).Url, "", AppResources.AppName);
							}));

							if (Device.OS == TargetPlatform.Windows)
							{
								ToolbarItems.Add(new ToolbarItem("Abrir no navegator", null, () =>
								{
									Other.Other.OpenSite((WView.Source as UrlWebViewSource).Url, this);
								}, ToolbarItemOrder.Secondary));
							}

							isLogin = false;

							WView.Source = new Uri("http://meetup.com/" + AppResources.MeetupId);
						}
						else
						{
							await Navigation.PopAsync();
						}
					}
					else if (description.Equals("pending"))
					{
						await DisplayAlert(null, "Por favor, aguarde sua aprovação no " + AppResources.AppName + " e tente novamente", "OK");

						await Navigation.PopAsync();
					}
				}
			}
			else
			{
                loginError();
			}
		}

        private async void loginError()
        {
            bool alert = await DisplayAlert("Erro ao fazer login", "Deseja tentar novamente?", "Sim", "Não");

            if (alert)
            {
                WView.Source = Other.Other.GetLoginUrl();
            }
            else
            {
                await Navigation.PopAsync();
            }
        }
    }
}
