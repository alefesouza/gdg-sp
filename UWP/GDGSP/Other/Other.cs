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

using System;
using System.Collections.Generic;
using System.Net.Http;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
using Windows.Foundation.Metadata;
using Windows.Networking.Connectivity;
using Windows.Networking.PushNotifications;
using Windows.Storage;
using Windows.System;
using Windows.System.Profile;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace GDGSP.Other
{
    /// <summary>
    /// Aqui ficam métodos e atributos utilizados em várias classes do aplicativo.
    /// </summary>
    public static class Other
    {
        public static PushNotificationChannel channel;
        public static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static ResourceLoader resourceLoader = new ResourceLoader();

        /// <summary>
        /// String que tem que aparecer no final de todas as URLs no aplicativo.
        /// </summary>
        public static string finalUrl = "?meetupid=" + resourceLoader.GetString("MeetupId") + "&platform=windows&build=" + GetOSVersion() + "&appversion=" + GetAppVersion();

        /// <summary>
        /// String que representa a URL do back-end nos resources do aplicativo.
        /// </summary>
        public static string backendUrl = resourceLoader.GetString("BackendUrl");

        /// <summary>
        /// Checa se o sistema suporta jump list
        /// </summary>
        public static bool jumpListPresent = ApiInformation.IsTypePresent("Windows.UI.StartScreen.JumpList");

        /// <summary>
        /// Métodos que retorna a URL de receber os eventos.
        /// </summary>
        /// <returns>A URL final para receber os eventos.</returns>
        public static string GetEventsUrl()
        {
            return "http://" + backendUrl + "api/events.php" + finalUrl;
        }

        /// <summary>
        /// Método que retorna a URL para fazer RSVP.
        /// </summary>
        /// <param name="id">O id do evento que o usuário está fazendo RSVP.</param>
        /// <returns>A URL final para fazer RSVP no evento que o usuário quer ir.</returns>
        public static string GetRSVPUrl(int id)
        {
            return "http://" + backendUrl + "api/rsvp.php" + finalUrl + "&eventid=" + id;
        }

        /// <summary>
        /// Método que retorna a URL das pessoas que vão para o evento.
        /// </summary>
        /// <param name="id">O id do evento.</param>
        /// <returns>A URL final para receber as respostas das pessoas sobre o evento.</returns>
        public static string GetRSVPSUrl(int id)
        {
            return "http://" + backendUrl + "api/people.php" + finalUrl + "&eventid=" + id;
        }

        /// <summary>
        /// Método que retorna a URL para fazer login.
        /// </summary>
        public static string GetLoginUrl()
        {
            return "http://" + backendUrl + "api/login.php" + finalUrl;
        }

        /// <summary>
        /// Método que retorna a URL para o administrador enviar notificação.
        /// </summary>
        public static string GetNotificationUrl()
        {
            return "http://" + backendUrl + "notifications/send.php" + finalUrl;
        }

        /// <summary>
        /// Método que retorna o refresh token armazenado nas configurações para fazer um post junto com uma URL.
        /// </summary>
        /// <returns>FormUrlEncodedContent configurado para post do refresh token no servidor.</returns>
        public static FormUrlEncodedContent GetRefreshToken(bool withChannel = false)
        {
            string token = localSettings.Values.ContainsKey("refresh_token") ? localSettings.Values["refresh_token"].ToString() : "";
            var postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("refresh_token", token));

            if (withChannel)
            {
                string channel = localSettings.Values.ContainsKey("ChannelUri") ? localSettings.Values["ChannelUri"].ToString() : "";
                postData.Add(new KeyValuePair<string, string>("ChannelUri", channel));
            }

            return new FormUrlEncodedContent(postData);
        }

        /// <summary>
        /// Exibir uma caixa de mensagem é algo comum no aplicativo.
        /// </summary>
        /// <param name="message">Mensagem a ser exibida.</param>
        public static async void ShowMessage(string message)
        {
            MessageDialog mb = new MessageDialog(message);
            await mb.ShowAsync();
        }

        /// <summary>
        /// Verifica conexão de internet.
        /// </summary>
        /// <returns>Se há conexão de internet.</returns>
        public static bool IsConnected()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool connected = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return connected;
        }

        /// <summary>
        /// Método que configura a URL das live tiles do aplicativo.
        /// </summary>
        public static void CreateTile()
        {
            var uris = new List<Uri>();

            for (int i = 0; i < 5; i++)
            {
                uris.Add(new Uri("http://" + backendUrl + "notifications/tiles/tile.php" + finalUrl + "&tile=" + i));
            }

            TileUpdater LiveTileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
            LiveTileUpdater.Clear();
            LiveTileUpdater.EnableNotificationQueue(true);
            LiveTileUpdater.StartPeriodicUpdateBatch(uris, PeriodicUpdateRecurrence.Daily);

            BadgeUpdater BadgeUpdater = BadgeUpdateManager.CreateBadgeUpdaterForApplication();
            BadgeUpdater.Clear();
            BadgeUpdater.StartPeriodicUpdate(new Uri("http://" + backendUrl + "notifications/tiles/badge.php" + finalUrl), PeriodicUpdateRecurrence.Daily);
        }

        /// <summary>
        /// Exibir uma página com uma transição de entrada.
        /// </summary>
        /// <param name="p">Página a ser adicionada a transição.</param>
        public static void Transition(Page p)
        {
            TransitionCollection collection = new TransitionCollection();
            NavigationThemeTransition theme = new NavigationThemeTransition();

            var info = new DrillInNavigationTransitionInfo();

            theme.DefaultNavigationTransitionInfo = info;
            collection.Add(theme);
            p.Transitions = collection;
        }

        /// <summary>
        /// Links exibidos ao clicar com o botão direito na live tile ou ícone do aplicativo.
        /// </summary>
        public async static void SetJumpList()
        {
            var jumpList = await JumpList.LoadCurrentAsync();

            jumpList.Items.Clear();

            var opensite = JumpListItem.CreateWithArguments("opensite", "Abrir meetup");
            opensite.Description = "Abrir meetup no navegador";
            opensite.Logo = new Uri("ms-appx:///Assets/Images/opensite.png");

            jumpList.Items.Add(opensite);

            await jumpList.SaveAsync();
        }

        /// <summary>
        /// Abrir uma URL no navegador.
        /// </summary>
        /// <param name="url">URL que vai ser aberta.</param>
        public async static void OpenSite(string url)
        {
            await Launcher.LaunchUriAsync(new Uri(url));
        }

        /// <summary>
        /// Método que retorna a versão do aplicativo.
        /// </summary>
        /// <returns>A versão do aplicativo.</returns>
        public static string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }

        /// <summary>
        /// Método que retorna a versão do sistema operacional.
        /// </summary>
        /// <returns>A versão do sistema operacional.</returns>
        public static string GetOSVersion()
        {
            string sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong v = ulong.Parse(sv);
            ulong v1 = (v & 0xFFFF000000000000L) >> 48;
            ulong v2 = (v & 0x0000FFFF00000000L) >> 32;
            ulong v3 = (v & 0x00000000FFFF0000L) >> 16;
            ulong v4 = (v & 0x000000000000FFFFL);
            string version = $"{v1}.{v2}.{v3}.{v4}";

            return version;
        }

        /// <summary>
        /// Método que retorna se o aplicativo está rodando em um celular.
        /// </summary>
        /// <returns>Se está rodando em um celular.</returns>
        public static bool IsMobile()
        {
            return ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");
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
        /// Métodos que criam canal de notificações e enviam para um servidor PHP, código por https://arjunkr.quora.com/How-to-Windows-10-WNS-Windows-Notification-Service-via-PHP
        /// </summary>
        public static async void RequestChannel()
        {
            try
            {
                channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                object value = localSettings.Values["ChannelUri"];
                if (value != null)
                {
                    if (channel.Uri.ToString() != localSettings.Values["ChannelUri"].ToString())
                    {
                        try
                        {
                            SendUri();
                        }
                        catch
                        {
                        }
                    }
                }
                else
                {
                    try
                    {
                        SendUri();
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
                try
                {
                    RequestChannel();
                }
                catch
                {
                }
            }
        }

        public static async void SendUri()
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            try
            {
                Dictionary<string, string> pairs = new Dictionary<string, string>();
                pairs.Add("ChannelUri", channel.Uri.ToString());
                pairs.Add("check_uri", "true");
                Windows.Web.Http.HttpFormUrlEncodedContent formContent = new Windows.Web.Http.HttpFormUrlEncodedContent(pairs);
                Windows.Web.Http.HttpClient client = new Windows.Web.Http.HttpClient();
                Windows.Web.Http.HttpResponseMessage response = await client.PostAsync(new Uri("http://" + backendUrl + "notifications/wns/wns.php" + finalUrl, UriKind.Absolute), formContent);
                string result = await response.Content.ReadAsStringAsync();
                JsonObject json = JsonObject.Parse(result);
                bool uri_exists = json.GetNamedBoolean("uri_exists");
                if (!uri_exists)
                {
                    localSettings.Values["ChannelUri"] = channel.Uri.ToString();
                }
            }
            catch
            {
            }
        }
    }
}
