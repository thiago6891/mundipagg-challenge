using System;
using System.Collections.Generic;

namespace api.Utils
{
    public class City : Region
    {
        private List<Region> _neighborhoods;

        public Region[] Neighborhoods
        {
            get
            {
                return _neighborhoods.ToArray();
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