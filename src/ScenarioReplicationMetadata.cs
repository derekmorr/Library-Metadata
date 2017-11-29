using System.Xml;

namespace Landis.Library.Metadata
{
    public class ScenarioReplicationMetadata: IMetadata
    { 
        public string FolderName {get; set;}
        public int TimeMin {get; set;}
        public int TimeMax {get; set;}
        public float RasterOutCellArea {get; set;}
        public string ProjectionFilePath {get; set;}  // Need to fix LSML first.

        public XmlNode Get_XmlNode(XmlDocument doc)
        {
            XmlNode node = doc.CreateElement("scenario-replication");

            XmlAttribute timeMinAtt = doc.CreateAttribute("timeMin");
            timeMinAtt.Value = this.TimeMin.ToString();
            node.Attributes.Append(timeMinAtt);

            XmlAttribute timeMaxAtt = doc.CreateAttribute("timeMax");
            timeMaxAtt.Value = this.TimeMax.ToString();
            node.Attributes.Append(timeMaxAtt);

            XmlAttribute rasterOutCellSizeAtt = doc.CreateAttribute("rasterOutCellArea");
            rasterOutCellSizeAtt.Value = this.RasterOutCellArea.ToString();
            node.Attributes.Append(rasterOutCellSizeAtt);
            return node;
        }
    }
}
