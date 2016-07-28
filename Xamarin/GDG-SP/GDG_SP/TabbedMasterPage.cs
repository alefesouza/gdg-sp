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
using Xamarin.Forms;

namespace GDG_SP
{
    /// <summary>
    /// Página inicial do aplicativo, com abas de Eventos, Links e Sobre.
    /// </summary>
    public class TabbedMasterPage : TabbedPage
    {
        public TabbedMasterPage()
        {
			Title = AppResources.AppName;

            Children.Add(new MainPage() { Title = "Eventos", Icon = Device.OnPlatform("Home.png", null, null) });
            Children.Add(new LinksPage() { Title = "Links", Icon = Device.OnPlatform("Links.png", null, null) });
            Children.Add(new AboutPage() { Title = "Sobre", Icon = Device.OnPlatform("About.png", null, null) });
        }
    }
}
