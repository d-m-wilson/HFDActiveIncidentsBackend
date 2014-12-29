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
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using HFDActiveIncidents.Domain.Models;
using HFDActiveIncidents.API.Models;

namespace HFDActiveIncidents.API.Controllers
{
    public class ActiveIncidentController : ApiController
    {
        private HFDActiveIncidentsContext db;

        public ActiveIncidentController()
        {
            db = new HFDActiveIncidentsContext();
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
        }

        // GET api/ActiveIncident
        public IQueryable<Incident> GetActiveIncidents()
        {
            return db.ActiveIncidents
                .Include(ai => ai.IncidentType.Agency)
                .ToArray()
                .AsQueryable()
                .Select(ai => new Incident(ai));
        }

        // GET api/ActiveIncident/5
        [ResponseType(typeof(Incident))]
        public IHttpActionResult GetActiveIncident(long id)
        {
            ActiveIncident activeincident = db.ActiveIncidents
                .Include(ai => ai.IncidentType.Agency)
                .Where(ai => ai.Id == id)
                .FirstOrDefault();

            if (activeincident == null)
            {
                return NotFound();
            }

            Incident incident = new Incident(activeincident);

            return Ok(incident);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ActiveIncidentExists(long id)
        {
            return db.ActiveIncidents.Any(e => e.Id == id);
        }
    }
}