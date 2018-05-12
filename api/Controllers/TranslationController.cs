using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    public class ConvertController : Controller
    {
        // POST api/convert
        [HttpPost]
        public void Post()
        {
            var reader = new StreamReader(Request.Body);
            var input = reader.ReadToEnd();
            // TODO: convert input if there's an available format.
        }
    }
}
