using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [Route("api/[controller]")]
    public class TemplatesController : Controller
    {
        private readonly IStoreService _storeService;

        public TemplatesController(IStoreService storeService)
        {
            _storeService = storeService;
        }

        // GET api/templates
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "template1", "template2" };
        }

        // GET api/templates/5
        // [HttpGet("{id}")]
        // public string Get(int id)
        // {
        //     return "value";
        // }

        // POST api/templates
        [HttpPost]
        public void Post(string value)
        {
        }

        // PUT api/templates/5
        // [HttpPut("{id}")]
        // public void Put(int id, [FromBody]string value)
        // {
        // }

        // DELETE api/templates/5
        // [HttpDelete("{id}")]
        // public void Delete(int id)
        // {
        // }
    }
}
