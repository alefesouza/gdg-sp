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
    /// Página de abas da PeoplePage, primeiro carrega um página vazia e sem título, e após receber os dados adiciona até duas abas com título.
    /// </summary>
    public class TabbedPeoplePage : TabbedPage
    {
        public static TabbedPeoplePage page;

        public TabbedPeoplePage(int id)
        {
            page = this;

            Children.Add(new PeoplePage(id));
        }
    }
}
