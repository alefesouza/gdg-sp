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

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace GDGSP
{
    /// <summary>
    /// Página que divide o aplicativo em duas colunas sem dar conflito com a barra lateral.
    /// </summary>
    public sealed partial class HomePage : Page
    {
        public Frame eventopen;
        public static HomePage homePage;

        public HomePage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            homePage = this;

            eventopen = EventFrame;

            EventsFrame.Navigate(typeof(EventsPage));
            MainPage.mainPage.headerList.SelectedIndex = 0;
        }
    }
}
