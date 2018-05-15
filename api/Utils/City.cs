using System.Collections.Generic;

namespace api.Utils
{
    public class City
    {
        private readonly string _name;
        private readonly int _population;
        private List<Neighborhood> _neighborhoods;

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public int Population
        {
            get
            {
                return _population;
            }
        }

        public Neighborhood[] Neighborhoods
        {
            get
            {
                return _neighborhoods.ToArray();
            }
        }

        public City(string name, int population)
        {
            _name = name;
            _population = population;
            _neighborhoods = new List<Neighborhood>();
        }

        public void AddNeighborhood(Neighborhood nb)
        {
            _neighborhoods.Add(nb);
        }
    }
}