using System;

namespace api.Utils
{
    public class Region
    {
        private readonly string _name;
        private readonly UInt32? _population;

        public string Name
        {
            get
            {
                return _name;
            }
        }

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