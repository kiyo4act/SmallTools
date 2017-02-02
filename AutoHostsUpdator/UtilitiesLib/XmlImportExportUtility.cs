using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace UtilitiesLib
{
    public static class XmlImportExportUtility
    {
        public static bool XmlSerialize(string filePath, XmlResources obj)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlResources));
                StreamWriter streamWriter = new StreamWriter(filePath, false, new UTF8Encoding(false));
                xmlSerializer.Serialize(streamWriter, obj);
                streamWriter.Dispose();
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }

        public static XmlResources XmlDeserialize(string filePath)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(XmlResources));
                StreamReader streamReader = new StreamReader(filePath, new UTF8Encoding(false));
                XmlResources obj = (XmlResources)xmlSerializer.Deserialize(streamReader);
                streamReader.Dispose();
                return obj;
            }
            catch(Exception)
            {
                return null;
            }
        }
    }
}
