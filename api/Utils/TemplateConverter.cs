using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace api.Utils
{
    public class TemplateConverter
    {
        public City[] GetCitiesFromTemplate(string template, string input)
        {
            template = (new Regex(@" +")).Replace(template, " ");
            input = (new Regex(@" +")).Replace(input, " ");

            template = template.Replace("{{city}}", "(?<city>.+)");
            template = template.Replace("{{population}}", "(?<pop>.+)");
            template = template.Replace("{{neighborhood}}", "(?<nb>.+)");
            template = template.Replace("{{neighborhood.population}}", "(?<nbpop>.+)");

            var separators = new string[] {
                "{{for city in cities}}",
                "{{for neighborhood in city.neighborhoods}}",
                "{{endfor}}"
            };

            var parts = template.Split(separators, StringSplitOptions.None);
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim();
            }

            var cities = new List<City>();

            // Get rid of extra unnecessary information
            var cityRegexStrOne = (new Regex("{{.+}}")).Replace(parts[1], ".+");
            var cityRegexStrTwo = (new Regex("{{.+}}")).Replace(parts[3], ".+");

            var cityRegexOne = new Regex(cityRegexStrOne);
            var cityRegexTwo = new Regex(cityRegexStrTwo);

            var matchesOne = cityRegexOne.Matches(input);
            var matchesTwo = cityRegexTwo.Matches(input);

            if (matchesOne.Count == matchesTwo.Count)
            {
                for (int i = 0; i < matchesOne.Count; i++)
                {
                    var cityName = (matchesOne[i].Groups["city"] ?? matchesTwo[i].Groups["city"]).ToString();
                    var population = Int32.Parse((matchesOne[i].Groups["pop"] ?? matchesTwo[i].Groups["pop"]).ToString());
                    var city = new City(cityName, population);

                    var nbRegexStr = (new Regex("{{.+}}")).Replace(parts[2], ".+");
                    //var nbRegex = new Regex(nbRegexStr);
                    var inputSubstr = input.Substring(matchesOne[i].Index, matchesTwo[i].Index - matchesOne[i].Index + matchesTwo[i].Length);
                    var nbMatches = Regex.Matches(inputSubstr, nbRegexStr, RegexOptions.Multiline);

                    for (int j = 0; j < nbMatches.Count; j++)
                    {
                        var nbName = nbMatches[j].Groups["nb"].ToString();
                        var nbPop = Int32.Parse(nbMatches[j].Groups["nbpop"].ToString());
                        city.AddNeighborhood(new Neighborhood(nbName, nbPop));
                    }

                    cities.Add(city);
                }
            }
            else
            {
                // TODO: Shouldn't reach this point
            }

            return cities.ToArray();
        }
    }
}