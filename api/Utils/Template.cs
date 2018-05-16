using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace api.Utils
{
    public class Template
    {
        private const string CityLoop = "{{for city in cities}}";
        private const string NeighborhoodLoop = "{{for neighborhood in city.neighborhoods}}";
        private const string LoopEnd = "{{endfor}}";
        private const string CityNameTag = "{{city.name}}";
        private const string CityPopulationTag = "{{city.population}}";
        private const string NeighborhoodNameTag = "{{neighborhood.name}}";
        private const string NeighborhoodPopulationTag = "{{neighborhood.population}}";
        private const string CityNameRegexVar = "city";
        private const string CityPopulationRegexVar = "pop";
        private const string NeighborhoodNameRegexVar = "nb";
        private const string NeighborhoodPopulationRegexVar = "nbpop";

        private readonly bool _hasNeighborhoods;
        private readonly Regex[] _cityRegexes;
        private readonly Regex _neighborhoodRegex;

        public Template(string template)
        {
            AssertCorrectNumberOfLoops(template);
            AssertCorrectNumberOfTags(template);
            AssertCorrectLoopsPositioning(template);

            _hasNeighborhoods = Regex.Matches(template, NeighborhoodLoop).Count == 1;

            template = RemoveExcessWhiteSpace(template);

            // The for loops are the separators.
            var parts = SplitTemplateIntoParts(template);

            if ((_hasNeighborhoods && parts.Length != 5) ||
                (!_hasNeighborhoods && parts.Length != 3))
                throw new InvalidOperationException("Template couldn't be divided in proper parts.");
            
            PreparePartsToBeTransformedInRegex(parts);
            
            _neighborhoodRegex = CreateNeighborHoodRegex(parts);
            _cityRegexes = CreateCityRegexes(parts);
        }

        public City[] ExtractCities(string input)
        {
            input = RemoveExcessWhiteSpace(input);

            if (_hasNeighborhoods)
            {
                var preNbCityMatches = _cityRegexes[0].Matches(input);
                var postNbCityMatches = _cityRegexes.Length > 1 ?
                    _cityRegexes[1].Matches(input) : null;
                
                if (postNbCityMatches != null && (preNbCityMatches.Count != postNbCityMatches.Count))
                    throw new InvalidOperationException("Wrong template.");
                
                var cities = new City[preNbCityMatches.Count];
                
                for (int i = 0; i < cities.Length; i++)
                {
                    string cityName;
                    string popStr;
                    if (postNbCityMatches != null)
                    {
                        cityName = preNbCityMatches[i].Groups[CityNameRegexVar].Success ? 
                            preNbCityMatches[i].Groups[CityNameRegexVar].ToString() :
                            postNbCityMatches[i].Groups[CityNameRegexVar].ToString();
                        popStr = preNbCityMatches[i].Groups[CityPopulationRegexVar].Success ?
                            preNbCityMatches[i].Groups[CityPopulationRegexVar].ToString() :
                            postNbCityMatches[i].Groups[CityPopulationRegexVar].ToString();
                    }
                    else
                    {
                        cityName = preNbCityMatches[i].Groups[CityNameRegexVar].ToString();
                        popStr = preNbCityMatches[i].Groups[CityPopulationRegexVar].ToString();
                    }
                    
                    UInt32 cityPop;
                    if (UInt32.TryParse(popStr, out cityPop))
                        cities[i] = new City(cityName, cityPop);
                    else
                        cities[i] = new City(cityName, null);
                    
                    var cityStartIdx = preNbCityMatches[i].Index;
                    
                    int? cityEndIdx;
                    if (i + 1 < cities.Length)
                        cityEndIdx = preNbCityMatches[i + 1].Index;
                    else
                        cityEndIdx = null;
                    
                    var nbStr = cityEndIdx.HasValue ?
                        input.Substring(cityStartIdx, cityEndIdx.Value - cityStartIdx) :
                        input.Substring(cityStartIdx);
                    var nbMatches = _neighborhoodRegex.Matches(nbStr);

                    for (int j = 0; j < nbMatches.Count; j++)
                    {
                        var nbName = nbMatches[j].Groups[NeighborhoodNameRegexVar].ToString();
                        var nbPopMatch = nbMatches[j].Groups[NeighborhoodPopulationRegexVar];
                        UInt32 nbPop;
                        if (UInt32.TryParse(nbPopMatch.ToString(), out nbPop))
                            cities[i].AddNeighborhood(new Region(nbName, nbPop));
                        else
                            cities[i].AddNeighborhood(new Region(nbName, null));
                    }
                }

                return cities;
            }
            else
            {
                return ExtractCities(input, _cityRegexes[0]);
            }
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

            var cityRegexes = new List<Regex>();

            // In this scenario, parts 1 and 3 contain the city
            // template. However, we're only interested in using as
            // regex the part(s) that contain the information we want.
            for (int i = 1; i <= 3; i += 2)
            {
                if (parts[i].Contains(string.Format("(?<{0}>.+)", CityNameRegexVar)) ||
                    parts[i].Contains(string.Format("(?<{0}>.+)", CityPopulationRegexVar)))
                        cityRegexes.Add(new Regex(parts[i]));
            }

            return cityRegexes.ToArray();
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
                CityLoop,
                NeighborhoodLoop,
                LoopEnd
            };
            return template.Split(separators, StringSplitOptions.None);
        }

        private string ReplaceTagsWithRegexVariables(string template)
        {
            return template
                .Replace(Regex.Escape(CityNameTag), string.Format("(?<{0}>.+)", CityNameRegexVar))
                .Replace(Regex.Escape(CityPopulationTag), string.Format("(?<{0}>.+)", CityPopulationRegexVar))
                .Replace(Regex.Escape(NeighborhoodNameTag), string.Format("(?<{0}>.+)", NeighborhoodNameRegexVar))
                .Replace(Regex.Escape(NeighborhoodPopulationTag), string.Format("(?<{0}>.+)", NeighborhoodPopulationRegexVar));
        }

        private string RemoveExcessWhiteSpace(string template)
        {
            return (new Regex(@" +")).Replace(template, " ");
        }

        private static void AssertCorrectNumberOfLoops(string template)
        {
            var cityLoopCount = Regex.Matches(template, CityLoop).Count;
            var nbLoopCount = Regex.Matches(template, NeighborhoodLoop).Count;
            var loopEndCount = Regex.Matches(template, LoopEnd).Count;

            if (cityLoopCount != 1)
                throw new FormatException(string.Format(
                    "Expected 1 \"{0}\". Found {1}.", CityLoop, cityLoopCount));

            if (nbLoopCount > 1)
                throw new FormatException(string.Format(
                    "Expected no more than 1 \"{0}\". Found {1}.", NeighborhoodLoop, nbLoopCount));

            if (loopEndCount != cityLoopCount + nbLoopCount)
                throw new FormatException(string.Format(
                    "Number of \"{0}\" doesn't match open loops.", LoopEnd));
        }

        private static void AssertCorrectNumberOfTags(string template)
        {
            var cityNameTagCount = Regex.Matches(template, CityNameTag).Count;
            var cityPopTagCount = Regex.Matches(template, CityPopulationTag).Count;
            var nbNameTagCount = Regex.Matches(template, NeighborhoodNameTag).Count;
            var nbPopTagCount = Regex.Matches(template, NeighborhoodPopulationTag).Count;

            if (cityNameTagCount != 1)
                throw new FormatException(string.Format(
                    "Expected 1 \"{0}\". Found {1}.", CityNameTag, cityNameTagCount));

            if (cityPopTagCount != 1)
                throw new FormatException(string.Format(
                    "Expected 1 \"{0}\". Found {1}.", CityPopulationTag, cityPopTagCount));

            if (nbNameTagCount > 1)
                throw new FormatException(string.Format(
                    "Expected no more than 1 \"{0}\". Found {1}.", NeighborhoodNameTag, nbNameTagCount));

            if (nbNameTagCount > 1)
                throw new FormatException(string.Format(
                    "Expected no more than 1 \"{0}\". Found {1}.", NeighborhoodPopulationTag, nbPopTagCount));
        }

        private void AssertCorrectLoopsPositioning(string template)
        {
            AssertNeighborhoodLoopInsideCityLoop(template);
            AssertCityTagsInsideCityLoop(template);
            AssertNeighborhoodTagsInsideNeighborhoodLoop(template);
        }

        private void AssertNeighborhoodLoopInsideCityLoop(string template)
        {
            var nbLoops = Regex.Matches(template, NeighborhoodLoop);
            var cityLoops = Regex.Matches(template, CityLoop);
            var loopEnds = Regex.Matches(template, LoopEnd);

            if (nbLoops.Count == 0)
            {
                if (cityLoops[0].Index < loopEnds[0].Index) return;

                throw new FormatException(string.Format("\"{0}\" not closed correctly.", CityLoop));
            }
            else
            {
                if ((cityLoops[0].Index < nbLoops[0].Index) &&
                    (nbLoops[0].Index < loopEnds[0].Index) &&
                    (loopEnds[0].Index < loopEnds[1].Index)) return;
                
                throw new FormatException("Loops are not in the correct order.");
            }
        }

        private void AssertNeighborhoodTagsInsideNeighborhoodLoop(string template)
        {
            var nbLoops = Regex.Matches(template, NeighborhoodLoop);
            var nbNameTags = Regex.Matches(template, NeighborhoodNameTag);
            var nbPopTags = Regex.Matches(template, NeighborhoodPopulationTag);
            
            if (nbLoops.Count == 0) {
                if (nbNameTags.Count == 0 && nbPopTags.Count == 0) return;

                throw new FormatException(string.Format(
                    "Found \"{0}\" or \"{1}\" but no \"{2}\".", 
                    NeighborhoodNameTag, NeighborhoodPopulationTag, NeighborhoodLoop));
            } else {
                var nbLoopEnd = Regex.Matches(template, LoopEnd)[0];

                if ((nbLoops[0].Index < nbNameTags[0].Index && nbNameTags[0].Index < nbLoopEnd.Index) &&
                    (nbLoops[0].Index < nbPopTags[0].Index && nbPopTags[0].Index < nbLoopEnd.Index))
                        return;
                
                throw new FormatException(string.Format(
                    "Found \"{0}\" or \"{1}\" outside a \"{2}\".", 
                    NeighborhoodNameTag, NeighborhoodPopulationTag, NeighborhoodLoop));
            }
        }

        private void AssertCityTagsInsideCityLoop(string template)
        {
            var cityLoopStart = Regex.Matches(template, CityLoop)[0];
            var cityNameTag = Regex.Matches(template, CityNameTag)[0];
            var cityPopTag = Regex.Matches(template, CityPopulationTag)[0];
            
            var loopEnds = Regex.Matches(template, LoopEnd);
            var cityLoopEnd = loopEnds[loopEnds.Count - 1];

            if ((cityLoopStart.Index < cityNameTag.Index && cityNameTag.Index < cityLoopEnd.Index) &&
                (cityLoopStart.Index < cityPopTag.Index && cityPopTag.Index < cityLoopEnd.Index))
                    return;
            
            throw new FormatException(string.Format(
                "Found \"{0}\" or \"{1}\" outside a \"{2}\".", 
                CityNameTag, CityPopulationTag, CityLoop));
        }
    }
}