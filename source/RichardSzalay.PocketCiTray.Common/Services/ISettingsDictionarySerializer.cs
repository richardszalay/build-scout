using System.Collections.Generic;
using System.IO;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface ISettingsDictionarySerializer
    {
        void Serialize(IDictionary<string, object> settings, Stream stream);

        IDictionary<string, object> Deserialize(Stream stream);
    }
}