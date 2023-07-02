namespace Klyte.TransportLinesManager.Data.Base.Interfaces
{
    public interface ITLMAutoNameConfigurable
    {
        bool UseInAutoName { get; set; }
        string NamingPrefix { get; set; }
    }

}
