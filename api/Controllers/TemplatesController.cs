using System;
using System.Collections.Generic;
using System.IO;
using api.Interfaces;
using api.Utils;
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
            
            if (!IsTemplateValid(input))
            {
                Response.StatusCode = 400;
            }
            else if (_storeService.SaveTemplate((new Template(input)).ToString()))
            {
                Response.StatusCode = 201;
            }
            else
            {
                Response.StatusCode = 500;
            }
        }

        [HttpDelete]
        public void Delete()
        {
            var input = (new StreamReader(Request.Body)).ReadToEnd();

            if (!IsTemplateValid(input))
            {
                Response.StatusCode = 400;
            }
            else if (_storeService.DeleteTemplate((new Template(input)).ToString()))
            {
                Response.StatusCode = 200;
            }
            else
            {
                Response.StatusCode = 500;
            }
        }

        private bool IsTemplateValid(string input)
        {
            try
            {
                var template = new Template(input);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}
