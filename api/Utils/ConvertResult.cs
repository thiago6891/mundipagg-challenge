using Newtonsoft.Json;

namespace api.Utils
{
    public class ConvertResult
    {
        private readonly City[] _cities;

        [JsonProperty("result")]
        public City[] Result
        {
            get
            {
                return _cities;
            }
        }

        public ConvertResult(City[] cities)
        {
            _cities = cities;
        }
    }
}