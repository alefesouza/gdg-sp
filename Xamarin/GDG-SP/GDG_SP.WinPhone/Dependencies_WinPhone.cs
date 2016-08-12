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
using GDG_SP.WinPhone;
using OneSignalSDK_WP_WNS;
using Windows.ApplicationModel;

[assembly: Xamarin.Forms.Dependency(typeof(Dependencies_WinPhone))]
namespace GDG_SP.WinPhone
{
    public class Dependencies_WinPhone : IDependencies
    {
        public Dependencies_WinPhone()
        {
        }

        public string GetAppVersion()
        {
            PackageVersion pv = Package.Current.Id.Version;

            // Windows Store só deixa subir pacotes 8.1 se a versão for menor que o pacote para Windows 10, por isso faço o 0 ser o Minor no appxmanifest
            Version version = new Version(Package.Current.Id.Version.Major,
                Package.Current.Id.Version.Build,
                Package.Current.Id.Version.Revision);

            return version.ToString();
        }

        public string GetOSVersion()
        {
            Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation deviceInfo = new Windows.Security.ExchangeActiveSyncProvisioning.EasClientDeviceInformation();
            string firmwareVersion = deviceInfo.SystemFirmwareVersion;
            return "8.1&firmwareversion=" + firmwareVersion;
        }

        public void SendOneSignalTag(string key, string value)
        {
            OneSignal.SendTag(key, value);
        }
    }
}

