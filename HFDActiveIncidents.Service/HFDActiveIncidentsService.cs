// HFD Active Incidents
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
using System;
using System.Data.Entity;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using Newtonsoft.Json;
using HFDActiveIncidents.Domain;
using HFDActiveIncidents.Domain.Models;

namespace HFDActiveIncidents.Service
{
    public partial class HFDActiveIncidentsService : ServiceBase
    {
        private System.Threading.Timer _timer;

        public HFDActiveIncidentsService()
        {
            InitializeComponent();
            _timer = new System.Threading.Timer(
                new TimerCallback(timerCallback),
                null,
                Timeout.Infinite,
                Timeout.Infinite);
        }

        protected override void OnStart(string[] args)
        {
#if (DEBUG)
            RequestAdditionalTime(10000);
            Thread.Sleep(9500);
#endif
            _timer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromMinutes(5));
        }

        protected override void OnStop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();
        }

        

        private void timerCallback(object state)
        {
            using (HFDActiveIncidentsContext context = new HFDActiveIncidentsContext())
            {
                ActiveIncidentResult wsResult;
                DateTime dtRetrieved;
                Int64 lCurrentSessionId;
                Int64 lPreviousSessionId;

                context.CreateServiceSession(out lCurrentSessionId, out lPreviousSessionId);

                var agencies = context.Agencies.ToList();

                var incidentTypes = context.IncidentTypes.Include(it => it.Agency).ToList();

                var existingIncidents = context.ActiveIncidents
                    .Include(i => i.IncidentType.Agency)
                    .Where(i => i.ServiceSessionId >= lPreviousSessionId)
                    .ToList();

                // FOR DEBUGGING ONLY!
                var sentinel = existingIncidents.FirstOrDefault(i => i.ServiceSessionId > lPreviousSessionId);
                if (sentinel != null)
                {
                    System.Diagnostics.Debugger.Break();
                }

                using (var client = new HoustonFireDept.WebService1SoapClient())
                {
                    try
                    {
                        var json = client.GetIncidents();
                        dtRetrieved = DateTime.Now;
                        wsResult = JsonConvert.DeserializeObject<ActiveIncidentResult>(json);
                    }
                    catch (Exception ex)
                    {
                        context.LogException(10, this.ServiceName, ex.Message, ex);
                        return;
                    }
                }

                foreach (var i in wsResult.ActiveIncidentDataTable)
                {
                    IncidentType incidentType = null;
                    ActiveIncident ai = null;

                    Agency agency = agencies.FirstOrDefault(a => a.Code == i.Agency);

                    if (agency == null)
                    {
                        agency = agencies.First(a => a.Code == null);
                    }

                    incidentType = incidentTypes
                        .Where(it => it.Agency == agency && String.Compare(it.Name, i.IncidentType, true) == 0)
                        .FirstOrDefault();

                    if (incidentType == null)
                    {
                        // auto-create incident type
                        incidentType = new IncidentType
                        {
                            Agency = agency,
                            Name = i.IncidentType.Trim(),
                        };

                        incidentTypes.Add(incidentType);
                    }
                    else
                    {
                        // Check for existing, duplicate record here!
                        ai = existingIncidents
                            //.Include(a => a.IncidentType)
                            //.Include(a => a.Agency)
                            .FirstOrDefault(a =>
                                a.IncidentTypeId == incidentType.Id &&
                                a.Address == i.Address.Trim() &&
                                a.KeyMap == i.KeyMap.Trim()
                            );
                    }

                    if (ai != null)
                    {
                        var blCombinedResponse = String.Compare(i.CombinedResponse, "Y", true) == 0;

                        if (!ai.CombinedResponse && blCombinedResponse)
                        {
                            ai.CombinedResponse = blCombinedResponse;
                        }

                        if (ai.AlarmLevel < i.AlarmLevelInt)
                        {
                            ai.AlarmLevel = i.AlarmLevelInt;
                        }

                        if (ai.NumberOfUnits < i.NumberOfUnitsInt)
                        {
                            ai.NumberOfUnits = i.NumberOfUnitsInt;
                            ai.Units = i.Units.Trim();
                        }
                    }
                    else
                    {
                        ai = new ActiveIncident
                        {
                            RetrievedDT = dtRetrieved,
                            Address = i.Address.Trim(),
                            CrossStreet = i.CrossStreet.Trim(),
                            KeyMap = i.KeyMap.Trim(),
                            Latitude = i.Latitude,
                            Longitude = i.Longitude,
                            CombinedResponse = String.Compare(i.CombinedResponse.Trim(), "Y", true) == 0,
                            CallTimeOpened = i.CallTimeOpenedDT,
                            IncidentType = incidentType,
                            AlarmLevel = i.AlarmLevelInt,
                            NumberOfUnits = i.NumberOfUnitsInt,
                            Units = i.Units.Trim()
                        };

                        context.ActiveIncidents.Add(ai);
                    }

                    ai.LastSeenDT = dtRetrieved;
                    ai.ServiceSessionId = lCurrentSessionId;
                }

                // Get all ActiveIncidents that were not added/updated this session
                var staleIncidents = context.ActiveIncidents
                    .Include(i => i.IncidentType.Agency)
                    .Where(i => i.ServiceSessionId < lPreviousSessionId)
                    .ToList();

                foreach (var incident in staleIncidents)
                {
                    var archivedIncident = ArchivedIncident.CreateFromActiveIncident(incident);
                    archivedIncident.ArchivedDT = dtRetrieved;
                    context.ArchivedIncidents.Add(archivedIncident);
                    context.ActiveIncidents.Remove(incident);
                }

                context.SaveChanges();
            }
        }

    }
}
