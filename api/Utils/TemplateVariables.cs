namespace api.Utils
{
    public class TemplateVariables
    {
        public const string CityLoop = "{{for city in cities}}";
        public const string NeighborhoodLoop = "{{for neighborhood in city.neighborhoods}}";
        public const string LoopEnd = "{{endfor}}";
        public const string CityNameTag = "{{city.name}}";
        public const string CityPopulationTag = "{{city.population}}";
        public const string NeighborhoodNameTag = "{{neighborhood.name}}";
        public const string NeighborhoodPopulationTag = "{{neighborhood.population}}";
    }
}