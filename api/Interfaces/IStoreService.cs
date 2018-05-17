namespace api.Interfaces
{
    public interface IStoreService
    {
        string[] GetTemplates();
        bool SaveTemplate(string template);
        bool DeleteTemplate(string template);
    }
}