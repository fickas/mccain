using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Transparity.C2C.Client.Example;
using Transparity.Services.C2C.Interfaces.TMDDInterface.Client;

namespace IntersectionApi.Controllers
{
    public class IntersectionController : ApiController
    {
        private McCainData intersectionData = new McCainData();

        public List<IntersectionInventoryItem> GetAll()
        {
            return intersectionData.GetSignalInventory();
        }

        // GET api/<controller>/5
        public IHttpActionResult GetStatusId(string id)
        {
            return Ok(intersectionData.GetIntersectionStatus(id));
        }
    }
}
