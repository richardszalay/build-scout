namespace RichardSzalay.PocketCiTray
{
    public class LocalizedResources
    {
        private readonly Strings strings = new Strings();
        private readonly AboutStrings aboutStrings = new AboutStrings();
        private readonly SettingsStrings settingsStrings = new SettingsStrings();

        public Strings Strings
        {
            get { return strings; }
        }

        public AboutStrings AboutStrings
        {
            get { return aboutStrings; }
        }

        public SettingsStrings SettingsStrings
        {
            get { return settingsStrings; }
        }
    }
}
