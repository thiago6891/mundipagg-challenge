using System.Text.RegularExpressions;

namespace api.Utils
{
    public class TemplateCleaner
    {
        public static string RemoveExcessWhiteSpaceAndNormalizeLineEndings(string template)
        {
            template = template.Replace("\t", " ");
            template = (new Regex(@" +")).Replace(template, " ");
            template = template.Replace("\r\n", "\n");
            template = template.Replace("\r", "\n");
            return template;
        }
    }
}