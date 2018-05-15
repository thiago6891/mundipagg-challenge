namespace api.Utils
{
    public class Neighborhood
    {
        private readonly string _name;
        private readonly int _population;

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

        public Neighborhood(string name, int population)
        {
            _name = name;
            _population = population;
        }
    }
}