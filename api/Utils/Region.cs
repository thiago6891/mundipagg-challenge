using System;
using Newtonsoft.Json;

namespace api.Utils
{
    public class Region
    {
        private readonly string _name;
        private readonly UInt32? _population;

        [JsonProperty("nome", Order = 1)]
        public virtual string Name
        {
            get
            {
                return _name;
            }
        }

        [JsonProperty("habitantes", Order = 2)]
        public UInt32? Population
        {
            get
            {
                return _population;
            }
        }

        public Region(string name, UInt32? population)
        {
            _name = name;
            _population = population;
        }
    }
}