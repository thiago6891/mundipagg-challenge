using System;
using System.Text.RegularExpressions;
using api.Interfaces;
using api.Utils;

namespace api.Services
{
    public class TemplateService : ITemplateService
    {
        public string Clean(string input)
        {
            input = input.Replace("\t", " ");
            input = (new Regex(@" +")).Replace(input, " ");
            input = input.Replace("\r\n", "\n");
            input = input.Replace("\r", "\n");
            return input;
        }

        public bool IsValid(string input)
        {
            try
            {
                AssertValidity(input);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void AssertValidity(string input)
        {
            AssertCorrectNumberOfLoops(input);
            AssertCorrectNumberOfTags(input);
            AssertCorrectLoopsPositioning(input);
        }

        private static void AssertCorrectNumberOfLoops(string template)
        {
            var cityLoopCount = Regex.Matches(template, TemplateVariables.CityLoop).Count;
            var nbLoopCount = Regex.Matches(template, TemplateVariables.NeighborhoodLoop).Count;
            var loopEndCount = Regex.Matches(template, TemplateVariables.LoopEnd).Count;

            if (cityLoopCount != 1)
                throw new FormatException(string.Format(
                    "Expected 1 \"{0}\". Found {1}.",
                    TemplateVariables.CityLoop,
                    cityLoopCount));

            if (nbLoopCount > 1)
                throw new FormatException(string.Format(
                    "Expected no more than 1 \"{0}\". Found {1}.",
                    TemplateVariables.NeighborhoodLoop,
                    nbLoopCount));

            if (loopEndCount != cityLoopCount + nbLoopCount)
                throw new FormatException(string.Format(
                    "Number of \"{0}\" doesn't match open loops.",
                    TemplateVariables.LoopEnd));
        }

        private static void AssertCorrectNumberOfTags(string template)
        {
            var cityNameTagCount = Regex.Matches(template, TemplateVariables.CityNameTag).Count;
            var cityPopTagCount = Regex.Matches(template, TemplateVariables.CityPopulationTag).Count;
            var nbNameTagCount = Regex.Matches(template, TemplateVariables.NeighborhoodNameTag).Count;
            var nbPopTagCount = Regex.Matches(template, TemplateVariables.NeighborhoodPopulationTag).Count;
            var nbLoopCount = Regex.Matches(template, TemplateVariables.NeighborhoodLoop).Count;

            if (cityNameTagCount != 1)
                throw new FormatException(string.Format(
                    "Expected 1 \"{0}\". Found {1}.",
                    TemplateVariables.CityNameTag,
                    cityNameTagCount));

            if (cityPopTagCount != 1)
                throw new FormatException(string.Format(
                    "Expected 1 \"{0}\". Found {1}.",
                    TemplateVariables.CityPopulationTag,
                    cityPopTagCount));

            if (nbNameTagCount != nbLoopCount)
                throw new FormatException(string.Format(
                    "Expected {0} \"{1}\". Found {2}.",
                    nbLoopCount,
                    TemplateVariables.NeighborhoodNameTag,
                    nbNameTagCount));

            if (nbNameTagCount != nbLoopCount)
                throw new FormatException(string.Format(
                    "Expected {0} \"{1}\". Found {2}.",
                    nbLoopCount,
                    TemplateVariables.NeighborhoodPopulationTag,
                    nbPopTagCount));
        }

        private void AssertCorrectLoopsPositioning(string template)
        {
            AssertNeighborhoodLoopInsideCityLoop(template);
            AssertCityTagsInsideCityLoop(template);
            AssertNeighborhoodTagsInsideNeighborhoodLoop(template);
        }

        private void AssertNeighborhoodLoopInsideCityLoop(string template)
        {
            var nbLoops = Regex.Matches(template, TemplateVariables.NeighborhoodLoop);
            var cityLoops = Regex.Matches(template, TemplateVariables.CityLoop);
            var loopEnds = Regex.Matches(template, TemplateVariables.LoopEnd);

            if (nbLoops.Count == 0)
            {
                if (cityLoops[0].Index < loopEnds[0].Index) return;

                throw new FormatException(string.Format("\"{0}\" not closed correctly.", TemplateVariables.CityLoop));
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
            var nbLoops = Regex.Matches(template, TemplateVariables.NeighborhoodLoop);
            var nbNameTags = Regex.Matches(template, TemplateVariables.NeighborhoodNameTag);
            var nbPopTags = Regex.Matches(template, TemplateVariables.NeighborhoodPopulationTag);
            
            if (nbLoops.Count == 0) {
                if (nbNameTags.Count == 0 && nbPopTags.Count == 0) return;

                throw new FormatException(string.Format(
                    "Found \"{0}\" or \"{1}\" but no \"{2}\".", 
                    TemplateVariables.NeighborhoodNameTag,
                    TemplateVariables.NeighborhoodPopulationTag,
                    TemplateVariables.NeighborhoodLoop));
            } else {
                var nbLoopEnd = Regex.Matches(template, TemplateVariables.LoopEnd)[0];

                if ((nbLoops[0].Index < nbNameTags[0].Index && nbNameTags[0].Index < nbLoopEnd.Index) &&
                    (nbLoops[0].Index < nbPopTags[0].Index && nbPopTags[0].Index < nbLoopEnd.Index))
                        return;
                
                throw new FormatException(string.Format(
                    "Found \"{0}\" or \"{1}\" outside a \"{2}\".", 
                    TemplateVariables.NeighborhoodNameTag,
                    TemplateVariables.NeighborhoodPopulationTag,
                    TemplateVariables.NeighborhoodLoop));
            }
        }

        private void AssertCityTagsInsideCityLoop(string template)
        {
            var cityLoopStart = Regex.Matches(template, TemplateVariables.CityLoop)[0];
            var cityNameTag = Regex.Matches(template, TemplateVariables.CityNameTag)[0];
            var cityPopTag = Regex.Matches(template, TemplateVariables.CityPopulationTag)[0];
            
            var loopEnds = Regex.Matches(template, TemplateVariables.LoopEnd);
            var cityLoopEnd = loopEnds[loopEnds.Count - 1];

            if ((cityLoopStart.Index < cityNameTag.Index && cityNameTag.Index < cityLoopEnd.Index) &&
                (cityLoopStart.Index < cityPopTag.Index && cityPopTag.Index < cityLoopEnd.Index))
                    return;
            
            throw new FormatException(string.Format(
                "Found \"{0}\" or \"{1}\" outside a \"{2}\".", 
                TemplateVariables.CityNameTag,
                TemplateVariables.CityPopulationTag,
                TemplateVariables.CityLoop));
        }
    }
}