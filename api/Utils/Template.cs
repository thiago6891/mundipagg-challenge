using System;
using System.Text.RegularExpressions;
using api.Interfaces;
using api.Services;

namespace api.Utils
{
    public class Template
    {
        private const string CityNameRegexVar = "city";
        private const string CityPopulationRegexVar = "pop";
        private const string NeighborhoodNameRegexVar = "nb";
        private const string NeighborhoodPopulationRegexVar = "nbpop";

        private readonly bool _hasNeighborhoods;
        private readonly Regex[] _cityRegexes;
        private readonly Regex _neighborhoodRegex;

        private readonly string _templateString;
        private readonly ITemplateService _templateService;

        public Template(string template, ITemplateService templateService)
        {
            _templateService = templateService;
            _templateService.AssertValidity(template);

            _hasNeighborhoods = Regex.Matches(template, TemplateVariables.NeighborhoodLoop).Count == 1;

            template = _templateService.Clean(template);
            _templateString = template;

            // The for loops are the separators.
            var parts = SplitTemplateIntoParts(template);
            PreparePartsToBeTransformedInRegex(parts);
            
            _neighborhoodRegex = CreateNeighborHoodRegex(parts);
            _cityRegexes = CreateCityRegexes(parts);
        }

        public City[] ExtractCities(string input)
        {
            input = _templateService.Clean(input);

            if (_hasNeighborhoods)
            {
                var preNeighborhoodMatches = _cityRegexes[0] != null ? _cityRegexes[0].Matches(input) : null;
                var postNeighborhoodMatches = _cityRegexes[1] != null ? _cityRegexes[1].Matches(input) : null;
                
                if (preNeighborhoodMatches != null && postNeighborhoodMatches != null && 
                    preNeighborhoodMatches.Count != postNeighborhoodMatches.Count)
                        throw new InvalidOperationException("Wrong template.");
                
                var totalCityMatches = preNeighborhoodMatches != null ?
                    preNeighborhoodMatches.Count : postNeighborhoodMatches.Count;
                var cities = new City[totalCityMatches];
                
                for (int i = 0; i < cities.Length; i++)
                {
                    cities[i] = GetCity(
                        preNeighborhoodMatches != null ? preNeighborhoodMatches[i] : null,
                        postNeighborhoodMatches != null ? postNeighborhoodMatches[i] : null
                    );
                    
                    int cityStartIdx;
                    int? cityEndIdx;
                    if (preNeighborhoodMatches == null)
                    {
                        if (i > 0)
                            cityStartIdx = postNeighborhoodMatches[i - 1].Index;
                        else
                            cityStartIdx = 0;
                        
                        cityEndIdx = postNeighborhoodMatches[i].Index + postNeighborhoodMatches[i].Length;
                    }
                    else if (postNeighborhoodMatches == null)
                    {
                        cityStartIdx = preNeighborhoodMatches[i].Index;
                        
                        if (i + 1 < cities.Length)
                            cityEndIdx = preNeighborhoodMatches[i + 1].Index;
                        else
                            cityEndIdx = null;
                    }
                    else
                    {
                        cityStartIdx = preNeighborhoodMatches[i].Index;
                        cityEndIdx = postNeighborhoodMatches[i].Index + postNeighborhoodMatches[i].Length;
                    }
                    
                    var nbStr = cityEndIdx.HasValue ?
                        input.Substring(cityStartIdx, cityEndIdx.Value - cityStartIdx) :
                        input.Substring(cityStartIdx);
                    var nbMatches = _neighborhoodRegex.Matches(nbStr);

                    var neighborhoods = GetNeighborhoods(nbMatches);
                    foreach (var neighborhood in neighborhoods)
                        cities[i].AddNeighborhood(neighborhood);
                }

                return cities;
            }
            else
            {
                return ExtractCities(input, _cityRegexes[0]);
            }
        }

        public override string ToString()
        {
            return _templateString;
        }

        private Region[] GetNeighborhoods(MatchCollection matches)
        {
            var neighborhoods = new Region[matches.Count];

            for (int i = 0; i < matches.Count; i++)
            {
                var name = matches[i].Groups[NeighborhoodNameRegexVar].ToString();
                var populationMatch = matches[i].Groups[NeighborhoodPopulationRegexVar].ToString();
                UInt32 population;
                if (UInt32.TryParse(populationMatch.ToString(), out population))
                    neighborhoods[i] = new Region(name, population);
                else
                    neighborhoods[i] = new Region(name, null);
            }

            return neighborhoods;
        }

        private City GetCity(Match matchOne, Match matchTwo)
        {
            string cityName;
            string popStr;

            if (matchOne == null)
            {
                cityName = matchTwo.Groups[CityNameRegexVar].ToString();
                popStr = matchTwo.Groups[CityPopulationRegexVar].ToString();
            }
            else if (matchTwo == null)
            {
                cityName = matchOne.Groups[CityNameRegexVar].ToString();
                popStr = matchOne.Groups[CityPopulationRegexVar].ToString();
            }
            else
            {
                cityName = matchOne.Groups[CityNameRegexVar].Success ? 
                    matchOne.Groups[CityNameRegexVar].ToString() :
                    matchTwo.Groups[CityNameRegexVar].ToString();
                popStr = matchOne.Groups[CityPopulationRegexVar].Success ?
                    matchOne.Groups[CityPopulationRegexVar].ToString() :
                    matchTwo.Groups[CityPopulationRegexVar].ToString();
            }
            
            UInt32 cityPop;
            if (UInt32.TryParse(popStr, out cityPop))
                return new City(cityName, cityPop);
            else
                return new City(cityName, null);
        }
        
        private City[] ExtractCities(string input, Regex cityRegex)
        {
            var cityMatches = cityRegex.Matches(input);
                
            var cities = new City[cityMatches.Count];
            
            for (int i = 0; i < cities.Length; i++)
            {
                var cityName = cityMatches[i].Groups[CityNameRegexVar].ToString();
                var popMatch = cityMatches[i].Groups[CityPopulationRegexVar];
                UInt32 cityPop;
                if (UInt32.TryParse(popMatch.ToString(), out cityPop))
                    cities[i] = new City(cityName, cityPop);
                else
                    cities[i] = new City(cityName, null);
            }

            return cities;
        }

        private Regex[] CreateCityRegexes(string[] parts)
        {
            if (!_hasNeighborhoods)
                return new Regex[] { new Regex(parts[1]) };

            var cityRegexes = new Regex[2];

            // In this scenario, parts 1 and 3 contain the city
            // template. However, we're only interested in using as
            // regex the part(s) that contain the information we want.
            for (int i = 1; i <= 3; i += 2)
            {
                if (parts[i].Contains(string.Format("(?<{0}>.+)", CityNameRegexVar)) ||
                    parts[i].Contains(string.Format("(?<{0}>.*)", CityPopulationRegexVar)))
                        cityRegexes[i / 2] = new Regex(parts[i]);
            }

            return cityRegexes;
        }

        private Regex CreateNeighborHoodRegex(string[] parts)
        {
            if (_hasNeighborhoods) return new Regex(parts[2]);

            return null;
        }

        private void PreparePartsToBeTransformedInRegex(string[] parts)
        {
            // The first and last parts are ignored. In other words,
            // anything outside the "city loop" is ignored.
            for (int i = 1; i < parts.Length - 1; i++)
            {
                parts[i] = parts[i].Trim();

                parts[i] = Regex.Escape(parts[i]);
                parts[i] = ReplaceTagsWithRegexVariables(parts[i]);

                // Replace optional or unnecessary information with .+
                // to be ignored by the Regex engine.
                parts[i] = (new Regex("\\\\{\\\\{.*}}")).Replace(parts[i], ".*");
            }
        }

        private string[] SplitTemplateIntoParts(string template)
        {
            var separators = new string[]
            {
                TemplateVariables.CityLoop,
                TemplateVariables.NeighborhoodLoop,
                TemplateVariables.LoopEnd
            };
            return template.Split(separators, StringSplitOptions.None);
        }

        private string ReplaceTagsWithRegexVariables(string template)
        {
            return template
                .Replace(Regex.Escape(TemplateVariables.CityNameTag),
                    string.Format("(?<{0}>.+)", CityNameRegexVar))
                .Replace(Regex.Escape(TemplateVariables.CityPopulationTag),
                    string.Format("(?<{0}>.*)", CityPopulationRegexVar))
                .Replace(Regex.Escape(TemplateVariables.NeighborhoodNameTag),
                    string.Format("(?<{0}>.+)", NeighborhoodNameRegexVar))
                .Replace(Regex.Escape(TemplateVariables.NeighborhoodPopulationTag),
                    string.Format("(?<{0}>.*)", NeighborhoodPopulationRegexVar));
        }
    }
}