using System;
using System.IO;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface IHelpService
    {
        Uri GetHelpUri(string topic);
    }

    public class HelpService : IHelpService
    {
        private const string StorageBasePath = "Help";

        private const string SharedContentBasePath = StorageBasePath + @"\shared";

        private const string ThemeFilePath = SharedContentBasePath + @"\theme.css";

        private readonly IIsolatedStorageFacade isolatedStorageFacade;
        private readonly IApplicationResourceFacade applicationResources;
        private readonly IThemeCssGenerator themeCssGenerator;

        private Uri[] sharedContentUris = new Uri[]
        {
            new Uri("Content/Help/shared/help.css", UriKind.Relative),
        };

        private const string TopicUriTemplate = "Content/Help/{0}.html";

        public HelpService(IIsolatedStorageFacade isolatedStorageFacade, IApplicationResourceFacade applicationResources,
            IThemeCssGenerator themeCssGenerator)
        {
            this.isolatedStorageFacade = isolatedStorageFacade;
            this.applicationResources = applicationResources;
            this.themeCssGenerator = themeCssGenerator;
        }

        public Uri GetHelpUri(string topic)
        {
            if (!isolatedStorageFacade.DirectoryExists(StorageBasePath))
            {
                isolatedStorageFacade.CreateDirectory(StorageBasePath);
            }

            WriteSharedContent();

            return WriteTopic(topic);
        }

        private Uri WriteTopic(string topic)
        {
            string uriString = String.Format(TopicUriTemplate, topic);
            var uri = new Uri(uriString, UriKind.Relative);

            return CopyResourceToFile(uri, StorageBasePath);
        }

        private void WriteSharedContent()
        {
            if (!isolatedStorageFacade.DirectoryExists(SharedContentBasePath))
            {
                isolatedStorageFacade.CreateDirectory(SharedContentBasePath);
            }

            CreateThemeFile();

            foreach(Uri sharedContentUri in sharedContentUris)
            {
                CopyResourceToFile(sharedContentUri, SharedContentBasePath);
            }
        }

        private Uri CopyResourceToFile(Uri resourceUri, string storageDirectory)
        {
            string resourceFilename = Path.GetFileName(resourceUri.OriginalString);

            string storagePath = Path.Combine(storageDirectory, resourceFilename);

            using (Stream inputStream = applicationResources.GetResourceStream(resourceUri))
            using (Stream outputStream = isolatedStorageFacade.CreateFile(storagePath))
            {
                inputStream.CopyTo(outputStream);
            }

            return new Uri(storagePath, UriKind.Relative);
        }

        private void CreateThemeFile()
        {
            string cssContent = themeCssGenerator.Generate();

            using (Stream outputStream = isolatedStorageFacade.CreateFile(ThemeFilePath))
            using (StreamWriter writer = new StreamWriter(outputStream))
            {
                writer.Write(cssContent);
            }
        }
    }
}
