using System;
using System.Collections.Generic;
using System.IO;
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

        [HttpGet]
        public IEnumerable<string> Get()
        {
            return _storeService.GetTemplates();
        }

        [HttpPost]
        public void Post()
        {
            var input = (new StreamReader(Request.Body)).ReadToEnd();
            Response.StatusCode = _storeService.SaveTemplate(input) ? 201 : 500;
        }

        [HttpDelete]
        public void Delete()
        {
            var input = (new StreamReader(Request.Body)).ReadToEnd();
            Response.StatusCode = _storeService.DeleteTemplate(input) ? 200 : 500;
        }
    }
}
