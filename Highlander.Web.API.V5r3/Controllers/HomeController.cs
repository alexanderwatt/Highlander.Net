using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace Highlander.Web.API.V5r3.Controllers
{
    public class HomeController : ApiController
    {
        public IHttpActionResult Index()
        {
            return Ok();
        }
    }
}
