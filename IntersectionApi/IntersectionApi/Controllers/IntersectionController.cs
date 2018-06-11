using System.Collections.Generic;
using System.Web.Http;
using Transparity.C2C.Client.Example;

namespace IntersectionApi.Controllers
{
    /// <summary>
    /// Intersection controller. Add new routes to this file.
    /// </summary>
    public class IntersectionController : ApiController
    {
        private McCainData intersectionData = new McCainData();

        // GET api/intersection
        public List<IntersectionInventoryItem> GetAll()
        {
            return intersectionData.GetSignalInventory();
        }

        // GET api/intersection/<intersection id>
        public IHttpActionResult GetStatusId(string id)
        {
            return Ok(intersectionData.GetIntersectionStatus(id));
        }
    }
}
