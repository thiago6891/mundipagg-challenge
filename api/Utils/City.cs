using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace api.Utils
{
    public class City : Region
    {
        private List<Region> _neighborhoods;

        [JsonProperty("bairros", Order = 3)]
        public Region[] Neighborhoods
        {
            get
            {
                return _neighborhoods.ToArray();
            }
        }

        [JsonProperty("cidade", Order = 1)]
        public override string Name
        {
            get
            {
                return base.Name;
            }
        }

        public City(string name, UInt32? population) : base(name, population)
        {
            _neighborhoods = new List<Region>();
        }

        public void AddNeighborhood(Region neighborhood)
        {
            _neighborhoods.Add(neighborhood);
        }
    }
}