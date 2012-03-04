using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface ICredentialEncryptor
    {
        NetworkCredential Decrypt(byte[] encryptedData);
        byte[] Encrypt(NetworkCredential credential);
    }

    public class CredentialEncryptor : ICredentialEncryptor
    {
        public NetworkCredential Decrypt(byte[] encryptedData)
        {
            byte[] rawData = ProtectedData.Unprotect(encryptedData, null);

            using (var ms = new MemoryStream(rawData))
            using (var reader = new BinaryReader(ms))
            {
                if (reader.ReadBoolean())
                {
                    string username = reader.ReadString();

                    string password = reader.ReadBoolean()
                                          ? reader.ReadString()
                                          : "";

                    return new NetworkCredential(username, password);
                }

                return null;
            }
        }

        public byte[] Encrypt(NetworkCredential credential)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                bool hasUsername = !(credential == null || String.IsNullOrEmpty(credential.UserName));
                bool hasPassword = hasUsername && !String.IsNullOrEmpty(credential.Password);

                writer.Write(hasUsername);

                if (hasUsername)
                {
                    writer.Write(credential.UserName);
                    
                    writer.Write(hasPassword);
                    if (hasPassword)
                    {
                        writer.Write(credential.Password);
                    }
                }

                var rawData = ms.ToArray();

                return ProtectedData.Protect(rawData, null);
            }
        }
    }
}
