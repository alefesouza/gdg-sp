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
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Xamarin.Forms;

namespace GDG_SP
{
    /// <summary>
    /// Página que exibe as pessoas que deram alguma resposta ao evento.
    /// </summary>
    public partial class PeoplePage : ContentPage
	{
		ObservableCollection<Person> listPeople = new ObservableCollection<Person>();
        int id;

        public PeoplePage(int id)
		{
			InitializeComponent ();
            this.id = id;
            GetPeople();
		}

        public PeoplePage(ObservableCollection<Person> content)
        {
            InitializeComponent();
            Loading.IsVisible = false;
            ListPeople.IsVisible = true;
            ListPeople.ItemsSource = content;
        }

        /// <summary>
        /// Método que solicita a lista de pessoas que deram alguma resposta ao evento e preenche a lista.
        /// </summary>
        public async void GetPeople()
		{
			string jsonString = "";

			try
			{
                if (!Other.Other.GetSetting("refresh_token").Equals(""))
                {
                    var client = new HttpClient();
                    client.MaxResponseContentBufferSize = 256000;
                    HttpResponseMessage response = await client.PostAsync(Other.Other.GetRSVPSUrl(id), Other.Other.GetRefreshToken());

                    if (response.IsSuccessStatusCode)
                    {
                        jsonString = await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
                else
                {
                    jsonString = await new HttpClient().GetStringAsync(new Uri(Other.Other.GetRSVPSUrl(id)));
                }
            }
			catch
			{
                ShowMessage("Verifique sua conexão de internet");
				return;
			}

            try
            {
                listPeople = JsonConvert.DeserializeObject<ObservableCollection<Person>>(jsonString);
            }
            catch
            {
                ShowMessage("Houve um erro ao receber a lista de pessoas");
                return;
            }

            ListPeople.ItemsSource = listPeople.Where(P => P.Response.Equals("yes"));

			Loading.IsVisible = false;
            
            ToolbarItem randomButton = new ToolbarItem("Sortear", Device.OnPlatform(null, null, "Assets/Images/Random.png"), () =>
            {
                // Para sortear, primeiro o aplicativo pega a lista de todas as pessoas que foram
                ObservableCollection<Person> go = new ObservableCollection<Person>(listPeople.Where(P => P.Response.Equals("yes")));

                // Se o total de itens na lista for maior que 0
                if (go.Count > 0)
                {
                    Random random = new Random();
                    // Ele criará um random de 0 até o número de pessoas na lista menos 1
                    int number = random.Next(go.Count);
                    // E dará scroll até esta posição na lista, exibindo o nome da pessoa.
                    ListPeople.ScrollTo(go[number], ScrollToPosition.Start, true);
                    Other.Other.ShowMessage(go[number].Name, this);
                }
            });

            ToolbarItems.Add(randomButton);

            Title = "Vão";

            ObservableCollection<Person> wait = new ObservableCollection<Person>(listPeople.Where(P => P.Response.Equals("waitlist") || P.Response.Equals("watching")));
            ObservableCollection<Person> no = new ObservableCollection<Person>(listPeople.Where(P => P.Response.Equals("no")));

                TabbedPeoplePage.page.Children.Add(new PeoplePage(wait) { Title = "Esperando" });

            if (no.Count > 0)
            {
                TabbedPeoplePage.page.Children.Add(new PeoplePage(no) { Title = "Não vão" });
            }

            ListPeople.IsVisible = true;
        }

        private async void ShowMessage(string title)
        {
            var alert = await DisplayAlert(title, "Tentar novamente?", "Sim", "Não");
            if (alert)
            {
                GetPeople();
            }
            else
            {
                await Navigation.PopAsync();
            }
        }

        private void ListPerson_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            Person person = e.SelectedItem as Person;

            Other.Other.OpenSite("meetup.com/" + AppResources.MeetupId + "/member/" + person.Id, this, true, person.Name);

            Device.StartTimer(TimeSpan.FromMilliseconds(50), () => {
                ((ListView)sender).SelectedItem = null;
                return false;
            });
        }
    }
}

