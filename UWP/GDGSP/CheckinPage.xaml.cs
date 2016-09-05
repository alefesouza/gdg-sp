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
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace GDGSP
{
    /// <summary>
    /// Página que exibe o QR Code de check-in do usuário
    /// </summary>
    public sealed partial class CheckinPage : Page
    {
        public CheckinPage()
        {
            this.InitializeComponent();

            string qr_code_base64 = Other.Other.localSettings.Values["qr_code"].ToString();
            byte[] qr_code = Convert.FromBase64String(qr_code_base64);

            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(qr_code);
                    writer.StoreAsync().GetResults();
                }

                var image = new BitmapImage();
                image.SetSource(ms);

                CheckInQrCode.Source = image;
            }

            if (MainPage.Instance.member != null)
            {
                MemberName.Text = MainPage.Instance.member.Name;
            }
        }
    }
}
