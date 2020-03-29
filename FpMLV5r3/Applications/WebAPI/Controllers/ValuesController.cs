using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Tracing;

namespace Highlander.WebAPI.V5r3.Controllers
{
    /// <summary>
    /// The main controller
    /// </summary>
    public class ValuesController : ApiController
    {
        // GET api/values
        /// <summary>
        /// Gets some very important data from the server.
        /// </summary>
        public IEnumerable<string> Get()
        {
            //Configuration.Services.GetTraceWriter().Info(
            //    Request, "ValuesController", "Get the list of values.");
            return new[] { "value1", "value2" };
        }

        // GET api/values/5
        /// <summary>
        /// Gets some very important data from the server.
        /// </summary>
        /// <param name="id">The ID of the data.</param>
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        /// <summary>
        /// Posts some very important data to the server.
        /// </summary>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        /// <summary>
        /// Posts some very important data to the server.
        /// </summary>
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        /// <summary>
        /// Deletes some very important data from the server.
        /// </summary>
        public void Delete(int id)
        {
        }
    }
}
