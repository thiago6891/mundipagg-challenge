using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using api.Interfaces;
using api.Utils;

namespace api.Controllers
{
    [Route("api/[controller]")]
    public class ConvertController : Controller
    {
        private readonly IStoreService _storeService;

        public ConvertController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        // POST api/convert
        [HttpPost]
        public void Post()
        {
            var reader = new StreamReader(Request.Body);
            var input = reader.ReadToEnd();
            string[] templates = _storeService.GetTemplates();
            foreach (var t in templates)
            {
                var template = new Template(t);
                var cities = template.ExtractCities(input);
                if (cities.Length > 0)
                {
                    // Return cities in specified output format
                }
            }

            // Return response stating it's not possible to convert input
        }
    }
}
