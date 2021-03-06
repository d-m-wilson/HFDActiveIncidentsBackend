﻿// HFD Active Incidents
// Copyright © 2014 David M. Wilson
// https://twitter.com/dmwilson_dev
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
namespace HFDActiveIncidents.API.Models
{
    public class Agency
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }

        public Agency(long id, string name, string code, string shortName)
        {
            Id = id;
            Name = name;
            Code = code;
            ShortName = shortName;
        }

        public Agency(HFDActiveIncidents.Domain.Models.Agency a)
        {
            Id = a.Id;
            Code = a.Code;
            Name = a.Name;
            ShortName = a.ShortName;
        }
    }
}