using System.IO;
using Microsoft.AspNetCore.Mvc;
using api.Interfaces;
using api.Utils;

namespace api.Controllers
{
    [Route("api/[controller]")]
    public class ConvertController : Controller
    {
        private readonly IStoreService _storeService;
        private readonly ITemplateService _templateService;

        public ConvertController(IStoreService storeService, ITemplateService templateService)
        {
            _storeService = storeService;
            _templateService = templateService;
        }

        [HttpPost]
        public JsonResult Post()
        {
            var input = (new StreamReader(Request.Body)).ReadToEnd();
            string[] templates = _storeService.GetTemplates();
            foreach (var t in templates)
            {
                var template = new Template(t, _templateService);
                var cities = template.ExtractCities(input);
                if (cities.Length > 0)
                {
                    Response.StatusCode = 200;
                    return Json(new ConvertResult(cities));
                }
            }

            Response.StatusCode = 400;
            return Json("Não foi possível converter o formato fornecido.");
        }
    }
}
