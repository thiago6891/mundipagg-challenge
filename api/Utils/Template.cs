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

            template = RemoveExcessWhiteSpaceAndNormalizeLineEndings(template);

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
            input = RemoveExcessWhiteSpaceAndNormalizeLineEndings(input);

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
                .Replace(Regex.Escape(CityPopulationTag), string.Format("(?<{0}>.*)", CityPopulationRegexVar))
                .Replace(Regex.Escape(NeighborhoodNameTag), string.Format("(?<{0}>.+)", NeighborhoodNameRegexVar))
                .Replace(Regex.Escape(NeighborhoodPopulationTag), string.Format("(?<{0}>.*)", NeighborhoodPopulationRegexVar));
        }

        private string RemoveExcessWhiteSpaceAndNormalizeLineEndings(string template)
        {
            template = (new Regex(@" +")).Replace(template, " ");
            template = template.Replace("\r\n", "\n");
            template = template.Replace("\r", "\n");
            return template;
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
            var nbLoopCount = Regex.Matches(template, NeighborhoodLoop).Count;

            if (cityNameTagCount != 1)
                throw new FormatException(string.Format(
                    "Expected 1 \"{0}\". Found {1}.", CityNameTag, cityNameTagCount));

            if (cityPopTagCount != 1)
                throw new FormatException(string.Format(
                    "Expected 1 \"{0}\". Found {1}.", CityPopulationTag, cityPopTagCount));

            if (nbNameTagCount != nbLoopCount)
                throw new FormatException(string.Format(
                    "Expected {0} \"{1}\". Found {2}.", nbLoopCount, NeighborhoodNameTag, nbNameTagCount));

            if (nbNameTagCount != nbLoopCount)
                throw new FormatException(string.Format(
                    "Expected {0} \"{1}\". Found {2}.", nbLoopCount, NeighborhoodPopulationTag, nbPopTagCount));
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