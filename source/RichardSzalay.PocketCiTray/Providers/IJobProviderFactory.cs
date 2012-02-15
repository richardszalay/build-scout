namespace RichardSzalay.PocketCiTray.Providers
{
    public interface IJobProviderFactory
    {
        IJobProvider Get(string serverType);
    }
}