using System;
using System.Configuration;
using System.Net;
using System.Xml;

namespace TTRider.uEpisodes
{
    public static class CredentialsUtilities
    {
        static DpapiProtectedConfigurationProvider provider = new DpapiProtectedConfigurationProvider();

        public static bool InitializeFromDpApi(this NetworkCredential credential, string key)
        {
            if (credential == null) throw new ArgumentNullException("credential");

            try
            {
                var doc = new XmlDocument();
                doc.LoadXml(key);

                var node = provider.Decrypt(doc.DocumentElement) as XmlElement;

                if (node.HasAttribute("UserName"))
                    credential.UserName = node.GetAttribute("UserName");
                if (node.HasAttribute("Password"))
                    credential.Password = node.GetAttribute("Password");
                if (node.HasAttribute("Domain"))
                    credential.Domain = node.GetAttribute("Domain");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string StoreToDpApi(this NetworkCredential credential)
        {
            if (credential == null) throw new ArgumentNullException("credential");

            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("NetworkCredential"));
            doc.DocumentElement.SetAttribute("UserName", credential.UserName);
            doc.DocumentElement.SetAttribute("Password", credential.Password);
            doc.DocumentElement.SetAttribute("Domain", credential.Domain);

            return provider.Encrypt(doc.DocumentElement).OuterXml;
        }
    }
}
