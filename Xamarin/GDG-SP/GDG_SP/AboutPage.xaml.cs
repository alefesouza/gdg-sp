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
using Xamarin.Forms;

namespace GDG_SP
{
    /// <summary>
    /// Página Sobre do aplicativo.
    /// </summary>
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();

            AppVersion.Text = DependencyService.Get<IDependencies>().GetAppVersion();

            // Como o Xamarin não possui uma RichTextBlock como o UWP nativo ou Html.fromHtml como no Android, decidi adicionar um TapGestureRecognizer em cada link nessa página
            var recognizer = new TapGestureRecognizer();
            recognizer.Tapped += Login_Tapped;
            GdV.GestureRecognizers.Add(recognizer);
            AlS.GestureRecognizers.Add(recognizer);
            GtH.GestureRecognizers.Add(recognizer);
            OnS.GestureRecognizers.Add(recognizer);
            Ic8.GestureRecognizers.Add(recognizer);
            NsJ.GestureRecognizers.Add(recognizer);
            ImC.GestureRecognizers.Add(recognizer);
            PlS.GestureRecognizers.Add(recognizer);
            PlSh.GestureRecognizers.Add(recognizer);
        }

        private void Login_Tapped(object sender, EventArgs e)
        {
            string url = "";

            if(sender == GdV)
            {
                url = "https://developers.google.com/groups/directory/Brazil";
            }
            else if(sender == AlS)
            {
                url = "http://alefesouza.com";
            }
            else if(sender == GtH)
            {
                url = "http://github.com/alefesouza/gdg-sp";
            }
            else if (sender == OnS)
            {
                url = "http://onesignal.com";
            }
            else if(sender == Ic8)
            {
                url = "http://icons8.com";
            }
            else if(sender == NsJ)
            {
                url = "http://www.newtonsoft.com/json";
            }
            else if(sender == ImC)
            {
                url = "https://github.com/jamesmontemagno/Xamarin.Plugins/tree/master/ImageCircle";
            }
            else if(sender == PlS)
            {
                url = "https://github.com/jamesmontemagno/Xamarin.Plugins/tree/master/Settings";
            }
            else if(sender == PlSh)
            {
                url = "https://github.com/jguertl/SharePlugin";
            }

            Other.Other.OpenSite(url, this);
        }
    }
}
