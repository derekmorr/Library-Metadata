using System;
using System.Collections.Generic;
using System.Xml;
using Landis.Core;

namespace Landis.Library.Metadata
{
    public class OutputMetadata: IMetadata
    {
        private string metadataFilePath;

        public OutputType Type{get; set;}
        public string Name { get; set; } // e.g."Century Succession monthly by Ecoregion" 
        public string FilePath { get; set; } // e.g. "Century-sucession-monthly-log.csv" 
        public string MetadataFilePath { get { return metadataFilePath; } set { if (Type == OutputType.Map) throw new ApplicationException("Error in setting MetadataFilePath for OutputMeradata: OutputMetatdata of type Mape does not require MetadataFilePath."); metadataFilePath = value; } }
        public List<FieldMetadata> Fields { get; set; }
        public MapDataType? Map_DataType { get; set; } //set null for table type
        public string Map_Unit { get; set; } //set null for table type
        public bool Visualize { get; set; }  // should be visualized?


        public OutputMetadata()
        {
            Fields = new List<FieldMetadata>();
        }



        public void RetriveFields(Type dataObjectType)
        {
            var tpDataObject = dataObjectType;
            Fields.Clear();
            foreach (var property in tpDataObject.GetProperties())
            {
                var attributes = property.GetCustomAttributes(typeof(DataFieldAttribute), false);
                if (attributes.Length > 0)
                {
                    if (null != attributes)
                    {
                        if (property.CanRead)
                        {
                            bool sppString = ((DataFieldAttribute)attributes[0]).SppList;
                            bool columnList = ((DataFieldAttribute)attributes[0]).ColumnList;

                            if (sppString)
                            {
                                foreach (ISpecies species in ExtensionMetadata.ModelCore.Species)
                                {
                                    Fields.Add(new FieldMetadata { Name = (property.Name + species.Name), Unit = ((DataFieldAttribute)attributes[0]).Unit, Desc = ((DataFieldAttribute)attributes[0]).Desc, Format = ((DataFieldAttribute)attributes[0]).Format });
                                }
                            }
                            else if (columnList)
                            {
                                foreach (String columnName in ExtensionMetadata.ColumnNames)
                                {
                                    Fields.Add(new FieldMetadata { Name = (property.Name + columnName), Unit = ((DataFieldAttribute)attributes[0]).Unit, Desc = ((DataFieldAttribute)attributes[0]).Desc, Format = ((DataFieldAttribute)attributes[0]).Format });
                                }
                            }
                            else
                            {
                                Fields.Add(new FieldMetadata { Name = property.Name, Unit = ((DataFieldAttribute)attributes[0]).Unit, Desc = ((DataFieldAttribute)attributes[0]).Desc, Format = ((DataFieldAttribute)attributes[0]).Format });
                            }
                        }
                    }
                }
                else 
                {
                    throw new ApplicationException("Error in OutputMetadata Retriving Fields: No DataFieldAttribute coud be found in th provide object type.");
                }
            }
        }


        public XmlNode Get_XmlNode(XmlDocument doc)
        {
            XmlNode node = doc.CreateElement("output");

            XmlAttribute typeAtt = doc.CreateAttribute("type");
            typeAtt.Value = this.Type.ToString();
            node.Attributes.Append(typeAtt);

            XmlAttribute nameAtt = doc.CreateAttribute("name");
            nameAtt.Value = this.Name;
            node.Attributes.Append(nameAtt);

            XmlAttribute fileAtt = doc.CreateAttribute("filePath"); //the output file path
            fileAtt.Value = this.FilePath;
            node.Attributes.Append(fileAtt);

            XmlAttribute vizAtt = doc.CreateAttribute("visualize"); //the output file path
            vizAtt.Value = this.Visualize.ToString();
            node.Attributes.Append(vizAtt);

            if (this.Type == OutputType.Table)
            {
                XmlAttribute MetadataFileAtt = doc.CreateAttribute("MetadataFilePath");
                MetadataFileAtt.Value = this.metadataFilePath;
                node.Attributes.Append(MetadataFileAtt);
            }
            else if (this.Type == OutputType.Map)
            {
                XmlAttribute map_unitAtt = doc.CreateAttribute("unit");
                map_unitAtt.Value = this.Map_Unit;
                node.Attributes.Append(map_unitAtt);

                XmlAttribute map_DataTypeAtt = doc.CreateAttribute("dataType");
                map_DataTypeAtt.Value = this.Map_DataType.ToString();
                node.Attributes.Append(map_DataTypeAtt);
            }

            return node;
        }

        public XmlNode Get_Fields_XmlNode(XmlDocument doc)
        {
            XmlNode nodeColl = doc.CreateElement("fields");

            foreach (FieldMetadata fld in Fields)
            {
                nodeColl.AppendChild(fld.Get_XmlNode(doc));
            }

            return nodeColl;
        }
    }
}