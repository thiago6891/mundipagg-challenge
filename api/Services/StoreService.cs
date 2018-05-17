using System;
using api.Interfaces;
using api.Utils;
using ServiceStack.Redis;

namespace api.Services
{
    public class StoreService : IStoreService
    {
        private const string TemplatesSetKey = "templates";

        private readonly RedisManagerPool _manager;

        public StoreService()
        {
            _manager = new RedisManagerPool("localhost:6379");
        }

        public string[] GetTemplates()
        {
            string[] result;
            
            using (var client = _manager.GetClient())
            {
                var templates = client.GetAllItemsFromSet(TemplatesSetKey);
                result = new string[templates.Count];
                templates.CopyTo(result);
            }
            
            return result;
        }

        public bool SaveTemplate(string template)
        {
            using (var client = _manager.GetClient())
            {
                try
                {
                    template = TemplateCleaner
                        .RemoveExcessWhiteSpaceAndNormalizeLineEndings(template);
                    client.AddItemToSet(TemplatesSetKey, template);
                }
                catch (RedisException)
                {
                    return false;
                }
            }
            return true;
        }

        public bool DeleteTemplate(string template)
        {
            using (var client = _manager.GetClient())
            {
                try
                {
                    template = TemplateCleaner
                        .RemoveExcessWhiteSpaceAndNormalizeLineEndings(template);
                    client.RemoveItemFromSet(TemplatesSetKey, template);
                }
                catch (RedisException)
                {
                    return false;
                }
            }
            return true;
        }
    }
}