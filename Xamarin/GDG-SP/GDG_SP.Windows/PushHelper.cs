using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Windows.Data.Json;
using Windows.Networking.PushNotifications;
using Windows.UI.Popups;

namespace GDG_SP.Windows
{
    public static class PushHelper
    {
        public static PushNotificationChannel channel;

        /// <summary>
        /// Métodos que criam canal de notificações e enviam para um servidor PHP, código por https://arjunkr.quora.com/How-to-Windows-10-WNS-Windows-Notification-Service-via-PHP
        /// </summary>
        public static async void RequestChannel()
        {
            try
            {
                channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                string channelValue = Other.Other.GetSetting("ChannelUri");
                if (!channelValue.Equals(""))
                {
                    if (channel.Uri.ToString() != Other.Other.GetSetting("ChannelUri"))
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
            try
            {
                Dictionary<string, string> pairs = new Dictionary<string, string>();
                pairs.Add("ChannelUri", channel.Uri.ToString());
                pairs.Add("check_uri", "true");
                FormUrlEncodedContent formContent = new FormUrlEncodedContent(pairs);
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.PostAsync(new Uri("http://" + Other.Other.GetBackendUrl() + "notifications/wns/wns.php" + Other.Other.GetFinalUrl(), UriKind.Absolute), formContent);
                string result = await response.Content.ReadAsStringAsync();
                JsonObject json = JsonObject.Parse(result);
                bool uri_exists = json.GetNamedBoolean("uri_exists");
                if (!uri_exists)
                {
                    Other.Other.AddSetting("ChannelUri", channel.Uri.ToString());
                }
            }
            catch
            {
            }
        }
    }
}
