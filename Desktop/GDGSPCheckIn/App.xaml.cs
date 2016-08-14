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

using SQLitePCL;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace GDGSPCheckIn
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const string AppName = "GDG-SP";
        public const string MeetupId = "GDG-SP";
        public const string QRCode = "http://gdgsp.org/qrcode/";
        public const string BackendUrl = "apps.aloogle.net/meetup/backend/api/";
        public static SQLiteConnection objConn = objConn = new SQLiteConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppName + ".db"));

        public App()
        {
            objConn.Prepare("CREATE TABLE IF NOT EXISTS events (id integer primary key autoincrement, event_id int, event_name text)").Step();
        }

        /// <summary>
        /// Chave do aplicativo usada para garantir que é a versão oficial e não uma modificação que está entrando em contato com o servidor.
        /// </summary>
        public static string GetAppKey()
        {
            string appKey = @"";

            byte[] encodedAppKey = new UTF8Encoding().GetBytes(appKey);

            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedAppKey);

            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }
    }
}
