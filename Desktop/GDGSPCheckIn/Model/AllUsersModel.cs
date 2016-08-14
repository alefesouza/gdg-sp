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

namespace GDGSPCheckIn.Model
{
    /// <summary>
    /// Objeto que representa uma pessoa retornada pelo JSON da AllUsersWindow.
    /// </summary>
    public class AllUsersModel
    {
        int _id;

        public int Id {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                Profile = "http://meetup.com/" + App.MeetupId + "/member/" + value;
            }
        }
        public string Name { get; set; }
        public int Faults { get; set; }
        public string Profile { get; set; }
    }
}
