using Plugin.Share;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GDG_SP.Resx;

using Xamarin.Forms;

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
                ToolbarItems.Add(new ToolbarItem("Compartilhar", Device.OnPlatform("Share.png", "", "Assets/Images/Share.png"), () =>
                {
                    CrossShare.Current.ShareLink((WView.Source as UrlWebViewSource).Url, "", AppResources.AppName);
                }));
            }

            if (Device.OS == TargetPlatform.iOS)
            {
                ToolbarItems.Add(new ToolbarItem("Mais", "More.png", async () =>
                {
                    string action = "";

                    if (this.isLogin)
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

                if (!isLogin)
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

        private async void WView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (isLogin && e.Url.Contains(AppResources.BackendUrl))
            {
                if (e.Url.Contains("refresh_token"))
                {
                    Other.Other.AddSetting("refresh_token", Other.Other.GetQuery(e.Url, "refresh_token"));
                    await DisplayAlert("", "Login realizado com sucesso", "OK");
                    MainPage.openEvent = eventId;
                    MainPage.main.GetEvents(false);
                }
                else if (e.Url.Contains("error"))
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
                else if (e.Url.Contains("nonmember=none"))
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
                else if (e.Url.Contains("nonmember=pending"))
                {
                    await DisplayAlert(null, "Por favor, aguarde sua aprovação no " + AppResources.AppName + " e tente novamente", "OK");

                    await Navigation.PopAsync();
                }
            }
            // Gambiarrinha que evita um erro que ocorre no site do Meetup ao fazer login com redes sociais.
            else if (e.Url.Contains("submit=") && e.Url.Contains("response=") && e.Url.Contains("meetup.com"))
            {
                WView.Source = Other.Other.GetLoginUrl();
            }
        }
    }
}
