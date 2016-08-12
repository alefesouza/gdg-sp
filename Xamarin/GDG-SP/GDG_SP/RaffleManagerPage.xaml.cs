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
using XLabs.Cryptography;
using System.Threading.Tasks;
using GDG_SP.Model;
using System.Collections.Generic;

namespace GDG_SP
{
	/// <summary>
	/// Página que exibe as pessoas que sortearam a si mesmo no evento, exibida apenas para organizadores e usuários permitidos.
	/// </summary>
	public partial class RaffleManagerPage : ContentPage
	{
		ObservableCollection<Raffle> listPeople = new ObservableCollection<Raffle>();
		bool itemsAdded;

		public RaffleManagerPage(int id)
		{
			InitializeComponent();

			Title = "Gerenciar sorteios";

			GetPeople(id);
		}

		/// <summary>
		/// Método que solicita a lista de pessoas que deram alguma resposta ao evento e preenche a lista.
		/// </summary>
		public async void GetPeople(int id, string empty = "false")
		{
			Loading.IsVisible = true;
			ListPeople.IsVisible = false;

			string jsonString = "";

			try
			{
				var postData = new List<KeyValuePair<string, string>>();
				postData.Add(new KeyValuePair<string, string>("app_key", Other.Other.AppKey));
				postData.Add(new KeyValuePair<string, string>("manager", "true"));
				postData.Add(new KeyValuePair<string, string>("empty", empty));
				postData.Add(new KeyValuePair<string, string>("refresh_token", Other.Other.GetSetting("refresh_token")));

				var client = new HttpClient();
				client.MaxResponseContentBufferSize = 256000;
				HttpResponseMessage response = await client.PostAsync(Other.Other.GetRaffleUrl(id), new FormUrlEncodedContent(postData));

				if (response.IsSuccessStatusCode)
				{
					string result = await response.Content.ReadAsStringAsync();

					switch (result)
					{
						case "invalid_user":
						case "invalid_key":
							await DisplayAlert("", "Usuário ou chave do aplicativo inválida", "OK");
							await Navigation.PopAsync();
							return;
						case "[]":
							bool refresh = await DisplayAlert("", "Não há pessoas sorteadas nesse evento", "Recarregar", "Voltar");

							if (refresh)
							{
								GetPeople(id);
							}
							else
							{
								await Navigation.PopAsync();
							}
							break;
						default:
							jsonString = result;
							break;
					}
				}
				else
				{
					throw new Exception();
				}
			}
			catch
			{
				await DisplayAlert("", "Verifique sua conexão de internet", "OK");
				await Navigation.PopAsync();
				return;
			}

			try
			{
				listPeople = JsonConvert.DeserializeObject<ObservableCollection<Raffle>>(jsonString);
			}
			catch
			{
				await DisplayAlert("", "Houve um erro ao receber a lista de pessoas", "OK");
				await Navigation.PopAsync();
				return;
			}

			if (!itemsAdded)
			{
				ToolbarItem refreshButton = new ToolbarItem("Atualizar", Other.Other.GetImage("Refresh"), () =>
				{
					GetPeople(id);
				});

				ToolbarItems.Add(refreshButton);

				ToolbarItem trashButton = new ToolbarItem("Esvaziar", Other.Other.GetImage("Delete"), () =>
				{
					GetPeople(id, "true");
				});

				ToolbarItems.Add(trashButton);
				itemsAdded = true;
			}

			ListPeople.ItemsSource = listPeople;

			Loading.IsVisible = false;
			ListPeople.IsVisible = true;
		}

		private void ListPerson_ItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
			if (e.SelectedItem == null) return;

			Device.StartTimer(TimeSpan.FromMilliseconds(50), () =>
			{
				((ListView)sender).SelectedItem = null;
				return false;
			});
		}
	}
}