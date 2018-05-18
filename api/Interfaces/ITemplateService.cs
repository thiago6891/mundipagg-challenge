namespace api.Interfaces
{
    public interface ITemplateService
    {
        bool IsValid(string input);
        string Clean(string input);
        void AssertValidity(string template);
    }
}