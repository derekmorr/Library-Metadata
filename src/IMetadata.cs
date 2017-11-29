using System.Xml;

namespace Landis.Library.Metadata
{
    public interface IMetadata
    {
        XmlNode Get_XmlNode(XmlDocument doc);

    }
}
