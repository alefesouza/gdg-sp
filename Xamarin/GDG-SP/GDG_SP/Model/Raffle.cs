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

namespace GDG_SP
{
	/// <summary>
	/// Objeto que representa uma pessoa sorteada retornada pelo JSON
	/// </summary>
	public class Raffle
	{
		string _raffle_date, _post_date, _seconds;

		public int Id;
		public string Name { get; set; }
		public string Photo { get; set; }
		public string Raffle_date { get { return "Sorteado em: " + _raffle_date; } set { _raffle_date = value; } }
		public string Post_date { get { return "Recebido em: " + _post_date; } set { _post_date = value; } }
		public string Seconds { get { return "Segundos com o alerta aberto: " + _seconds; } set { _seconds = value; } }
	}
}

