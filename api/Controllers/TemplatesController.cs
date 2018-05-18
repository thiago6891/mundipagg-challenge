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
        private readonly ITemplateService _templateService;

        public TemplatesController(IStoreService storeService, ITemplateService templateService)
        {
            _storeService = storeService;
            _templateService = templateService;
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
            
            if (!_templateService.IsValid(input))
            {
                Response.StatusCode = 400;
            }
            else if (_storeService.SaveTemplate(_templateService.Clean(input)))
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

            if (!_templateService.IsValid(input))
            {
                Response.StatusCode = 400;
            }
            else if (_storeService.DeleteTemplate(_templateService.Clean(input)))
            {
                Response.StatusCode = 200;
            }
            else
            {
                Response.StatusCode = 500;
            }
        }
    }
}
