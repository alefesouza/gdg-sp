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

namespace GDG_SP.Model
{
    /// <summary>
    /// Objeto que representa um evento retornado pelo JSON
    /// </summary>
    public class Event
    {
        public int Id { get; set; }
		public string Name { get; set; }
		public string Image { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string Who { get; set; }
        public string Place { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public double Lat { get; set; }
		public double Lon { get; set; }
		public string Start { get; set; }
		public string End { get; set; }
		public int Yes_rsvp_count { get; set; }
        public int Rsvp_limit { get; set; }
        public int Waitlist_count { get; set; }
        public string Response { get; set; }
        public List<Questions> Survey_questions { get; set; }
        public List<Questions> Answers { get; set; }
        public bool Rsvpable { get; set; }
        public double Image_width { get; set; }
        public double Image_height { get; set; }
        public double HeightRequest { get; set; }

        public struct Questions
        {
            public int Id { get; set; }
            public string Question { get; set; }
            public string Answer { get; set; }
        }
    }
}
