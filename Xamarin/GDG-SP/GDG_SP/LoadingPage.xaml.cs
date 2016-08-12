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

using System.Collections.Generic;
using System.Net.Http;
using GDG_SP.Model;
using Xamarin.Forms;

namespace GDG_SP
{
	public partial class LoadingPage : ContentPage
	{
		public LoadingPage(string raffle_date, int seconds, Event _event)
		{
			InitializeComponent();

			Title = "Enviando...";

			SendRaffle(raffle_date, seconds, _event);
		}

		public async void SendRaffle(string raffle_date, int seconds, Event _event)
		{
			var postData = new List<KeyValuePair<string, string>>();
			postData.Add(new KeyValuePair<string, string>("app_key", Other.Other.AppKey));
			postData.Add(new KeyValuePair<string, string>("raffle_date", raffle_date));
			postData.Add(new KeyValuePair<string, string>("seconds", seconds.ToString()));
			postData.Add(new KeyValuePair<string, string>("refresh_token", Other.Other.GetSetting("refresh_token")));

			var client = new HttpClient();
			client.MaxResponseContentBufferSize = 256000;
			HttpResponseMessage response = await client.PostAsync(Other.Other.GetRaffleUrl(_event.Id), new FormUrlEncodedContent(postData));

			if (response.IsSuccessStatusCode)
			{
				string result = await response.Content.ReadAsStringAsync();
				string message;

				switch (result)
				{
					case "success":
						message = "Seu sorteio foi enviado, boa sorte!";
						break;
					case "invalid_user":
						message = "Usuário inválido";
						break;
					case "invalid_key":
						message = "Chave do aplicativo inválida";
						break;
					default:
						message = "Houve um erro";
						break;
				}

				await DisplayAlert("", message, "OK");

				await Navigation.PopAsync();
			}
			else
			{
				bool error = await DisplayAlert("Houve um erro ao enviar os dados", "Deseja tentar novamente?", "Sim", "Não");

				if (error)
				{
					SendRaffle(raffle_date, seconds, _event);
				}
				else
				{
					await Navigation.PopAsync();
				}
			}
		}
	}
}
