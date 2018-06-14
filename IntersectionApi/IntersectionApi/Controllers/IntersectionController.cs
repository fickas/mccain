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

        [HttpGet]
        [Route("api/intersection")]
        public List<IntersectionInventoryItem> GetAll()
        {
            return intersectionData.GetSignalInventory();
        }

        [HttpGet]
        [Route("api/intersection/{id}")]
        public IHttpActionResult GetStatusId(string id)
        {
            return Ok(intersectionData.GetIntersectionStatus(id));
        }

        [HttpGet]
        [Route("api/intersection/register/{id}")]
        public IHttpActionResult RegisterIntersectionForUpdates(string id)
        {
            intersectionData.RegisterIntersectionForUpdates(id);
            return Ok("success");
        }

        [HttpGet]
        [Route("api/intersection/unregister/{id}")]
        public IHttpActionResult UnregisterIntersectionForUpdates(string id)
        {
            intersectionData.UnregisterIntersectionForUpdates(id);
            return Ok("success");
        }
    }
}
