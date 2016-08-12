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

using GDG_SP.Resx;
using Plugin.Settings;
using Plugin.Share;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Xamarin.Forms;

namespace GDG_SP.Other
{
    /// <summary>
    /// Aqui ficam métodos e atributos utilizados em várias classes do aplicativo.
    /// </summary>
    public class Other
    {
        /// <summary>
        /// String que tem que aparecer no final de todas as URLs no aplicativo.
        /// </summary>
		public static string GetFinalUrl()
        {
            string platform = Device.OnPlatform("ios", "android", "wp");

            if (Device.Idiom == TargetIdiom.Desktop)
            {
                platform = "windows";
            }
            else if (Device.Idiom == TargetIdiom.Tablet && Device.OS == TargetPlatform.Windows)
            {
                platform = "windows81";
            }

            return "?meetupid=" + AppResources.MeetupId + "&appversion=" + DependencyService.Get<IDependencies>().GetAppVersion() + "&systemversion=" + DependencyService.Get<IDependencies>().GetOSVersion() + "&platform=" + platform + "&via=xamarin";
        }

        /// <summary>
        /// Métodos que retorna a URL de receber os eventos.
        /// </summary>
        /// <returns>A URL final para receber os eventos.</returns>
        public static string GetEventsUrl()
        {
            return "http://" + AppResources.BackendUrl + "api/events.php" + GetFinalUrl();
        }

        /// <summary>
        /// Método que retorna a URL para fazer RSVP.
        /// </summary>
        /// <param name="id">O id do evento que o usuário está fazendo RSVP.</param>
        /// <returns>A URL final para fazer RSVP no evento que o usuário quer ir.</returns>
        public static string GetRSVPUrl(int id)
        {
            return "http://" + AppResources.BackendUrl + "api/rsvp.php" + GetFinalUrl() + "&eventid=" + id;
        }

        /// <summary>
        /// Método que retorna a URL das pessoas que vão para o evento.
        /// </summary>
        /// <param name="id">O id do evento.</param>
        /// <returns>A URL final para receber as respostas das pessoas sobre o evento.</returns>
        public static string GetRSVPSUrl(int id)
        {
			return "http://" + AppResources.BackendUrl + "api/people.php" + GetFinalUrl() + "&eventid=" + id;
        }

        /// <summary>
        /// Método que retorna a URL para fazer login.
        /// </summary>
        public static string GetLoginUrl()
		{
			return "http://" + AppResources.BackendUrl + "api/login.php" + GetFinalUrl();
        }

        /// <summary>
        /// Método que retorna a URL para o administrador enviar notificação.
        /// </summary>
        public static string GetNotificationUrl()
        {
            return "http://" + AppResources.BackendUrl + "notifications/send.php" + GetFinalUrl();
        }

        /// <summary>
        /// Método que retorna o refresh token armazenado nas configurações para fazer um post junto com uma URL.
        /// </summary>
        /// <returns>FormUrlEncodedContent configurado para post do refresh token no servidor.</returns>
        public static FormUrlEncodedContent GetRefreshToken(bool withChannel = false)
        {
            var postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("refresh_token", GetSetting("refresh_token")));

            if ((Device.Idiom == TargetIdiom.Desktop || (Device.Idiom == TargetIdiom.Tablet && Device.OS == TargetPlatform.Windows)) && withChannel)
            {
                postData.Add(new KeyValuePair<string, string>("ChannelUri", GetSetting("ChannelUri")));
            }

            return new FormUrlEncodedContent(postData);
        }

        /// <summary>
        /// Exibir uma caixa de mensagem é algo comum no aplicativo.
        /// </summary>
        /// <param name="message">Mensagem a ser exibida.</param>
        public static async void ShowMessage(string message, Page page)
        {
            await page.DisplayAlert("", message, "OK");
        }

        /// <summary>
        /// Método que abre uma URL.
        /// </summary>
        /// <param name="url">URL que deve ser aberta.</param>
        /// <param name="page">Page que "herdará" a navegação.</param>
        /// <param name="inApp">Se a URL deve ser aberta no aplicativo ou no navegador externo.</param>
        /// <param name="title">Título da página a ser aberta.</param>
        public static void OpenSite(string url, Page page, bool inApp = false, string title = "")
        {
            url = url.StartsWith("http") ? url : "http://" + url;
            if (inApp)
            {
                page.Navigation.PushAsync(new WebViewPage(url) { Title = title });
            }
            else
            {
				CrossShare.Current.OpenBrowser(url, null);
            }
        }

        /// <summary>
        /// Método que compartilha um link com o plugin CrossShare
        /// </summary>
        /// <param name="title">Título a ser exibido na tela de compartilhamento.</param>
        /// <param name="link">Link a ser compartilhado.</param>
		public static async void ShareLink(string title, string link)
		{
			await CrossShare.Current.ShareLink(link, "", title);
		}
        
        /// <summary>
        /// Método que retorna a URL do back-end, usado no GDG_SP.WinPhone para criar as live tiles, não conseguir acessar o AppResources diretamente no outro projeto.
        /// </summary>
        /// <returns>A URL do back-end.</returns>
        public static string GetBackendUrl()
        {
            return AppResources.BackendUrl;
        }

        /// <summary>
        /// Mesma coisa do método acima, porém retorna o app id do One Signal.
        /// </summary>
        /// <returns>A app id do One Signal.</returns>
        public static string GetOneSignalAppId()
        {
            return AppResources.OneSignalAppId;
        }

        /// <summary>
        /// Mesma coisa do método acima, porém a cor principal.
        /// </summary>
        /// <returns>A cor principal.</returns>
        public static string GetColorPrimary()
        {
            return AppResources.ColorPrimary;
        }

        /// <summary>
        /// Adiciona configuração.
        /// </summary>
        /// <param name="key">Chave da configuração.</param>
        /// <param name="value">Valor da configuração.</param>
        public static void AddSetting(string key, string value)
        {
            CrossSettings.Current.AddOrUpdateValue<string>(key, value);
        }

        /// <summary>
        /// Retorna uma configuração armazenada.
        /// </summary>
        /// <param name="key">Chave da configuração.</param>
        /// <returns>Valor da configuração.</returns>
        public static string GetSetting(string key)
        {
            return CrossSettings.Current.GetValueOrDefault<string>(key, "");
        }
        
        /// <summary>
        /// Método que retorna a um parâmetro de uma URL.
        /// </summary>
        /// <param name="url">URL para extrair o parâmetro.</param>
        /// <param name="what">Parâmetro a ser extraído.</param>
        public static string GetQuery(string url, string what)
        {
            string[] parts = url.Split(new char[] { '?', '&' });
            foreach (string p in parts)
            {
                if (p.Contains(what))
                {
                    return p.Split('=')[1];
                }
            }

            return null;
        }

        /// <summary>
        /// Xamarin não muda a altura com HorizontalOptions="FillAndExpand" no iOS igual no Windows Phone, tive a ideia de fazer isso
        /// </summary>
        /// <param name="actualWidth">Largura atual da imagem oculpando a tela toda.</param>
        /// <param name="width">Largura da imagem retornada pelo JSON.</param>
        /// <param name="height">Altura da imagem retornada pelo JSON.</param>
        /// <returns>A altura que a imagem deve ficar para manter a proporção tendo a largura como tela/coluna inteira.</returns>
        public static double GetHeightImage(double actualWidth, double width, double height)
        {
            double ratio = width / height;
            double targetWidth = actualWidth;

            double targetHeight = targetWidth / ratio;

            return targetHeight;
        }

        public static string GetImage(string name)
        {
            switch(Device.OS)
            {
                case TargetPlatform.Android:
                    name = "ic_" + name.ToLower();
                    break;
                case TargetPlatform.Windows:
                    name = "Assets/Images/" + name;
                    break;
            }
            string directory = name + ".png";
            return directory;
        }
    }
}
