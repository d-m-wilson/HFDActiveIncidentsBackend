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
    public class ArchivedIncidentController : ApiController
    {
        private const int DEFAULT_MAX_RESULTS = 100;
        private HFDActiveIncidentsContext db;

        public ArchivedIncidentController()
        {
            db = new HFDActiveIncidentsContext();
            db.Configuration.ProxyCreationEnabled = false;
            db.Configuration.LazyLoadingEnabled = false;
        }

        // GET api/ArchivedIncident
        public IQueryable<Incident> GetArchivedIncidents()
        {
            return db.ArchivedIncidents
                .Include(ai => ai.IncidentType.Agency)
                .Take(DEFAULT_MAX_RESULTS)
                .ToArray()
                .AsQueryable()
                .Select(ai => new Incident(ai));
        }

        // GET api/ActiveIncident/5
        [ResponseType(typeof(Incident))]
        public IHttpActionResult GetArchivedIncident(long id)
        {
            ArchivedIncident archivedincident = db.ArchivedIncidents
                .Include(ai => ai.IncidentType.Agency)
                .Where(ai => ai.Id == id)
                .FirstOrDefault();

            if (archivedincident == null)
            {
                return NotFound();
            }

            Incident incident = new Incident(archivedincident);

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

        private bool ArchivedIncidentExists(long id)
        {
            return db.ArchivedIncidents.Any(e => e.Id == id);
        }
    }
}