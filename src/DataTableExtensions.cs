using System;
using System.Collections;
using System.Data;
using System.Text;
using Landis.Core;

namespace Landis.Library.Metadata
{
    public static class DataTableExtensions
    {
        private static System.IO.StreamWriter file;

        
        /// <summary>
        /// Set DataFieldAttributes of the given Type to the columns of the DataTable.
        /// This should be called befor adding data to the data table.
        /// </summary>
        /// <typeparam name="T"></typeparam>ok
        /// 
        /// <param name="tbl"></param>
        public static void SetColumns<T>(this DataTable tbl) where T : new()
        { 
            var dataObject = Activator.CreateInstance<T>();
            var tpDataObject = dataObject.GetType();
            tbl.Rows.Clear();
            tbl.Columns.Clear();

            foreach (var property in tpDataObject.GetProperties())
            {
                var attributes = property.GetCustomAttributes(typeof(DataFieldAttribute), true);
                if (null != attributes && attributes.Length > 0)
                {
                    if (property.CanRead)
                    {
                        bool sppString = ((DataFieldAttribute)attributes[0]).SppList;
                        bool columnList = ((DataFieldAttribute)attributes[0]).ColumnList;
                        if (sppString)
                        {
                            foreach (ISpecies species in ExtensionMetadata.ModelCore.Species)
                            {
                                tbl.Columns.Add(String.Format(property.Name + species.Name), typeof(double));
                            }
                        }
                        else if (columnList)
                        {
                            foreach (String columnName in ExtensionMetadata.ColumnNames)
                            {
                                tbl.Columns.Add(String.Format(property.Name + columnName), typeof(double));
                            }
                        }
                        else
                        {
                            tbl.Columns.Add(property.Name, property.PropertyType);
                        }
                    }
                }
            }
        }

        //------
        public static T GetDataObjectAt<T>(this DataTable tbl, int index) where T : new()
        {
            return GetDataObject<T>(tbl.Rows[index]);
        }

        //------
        public static T GetDataObject<T>(this DataRow dataRow) where T: new()
        {
            var dataObject = Activator.CreateInstance<T>();
            var tpDataObject = dataObject.GetType();

            foreach (var property in tpDataObject.GetProperties())
            {
                var attributes = property.GetCustomAttributes(typeof(DataFieldAttribute), true);
                if (null != attributes && attributes.Length > 0)
                {
                    if (property.CanWrite)
                    {
                        DataColumn clm = dataRow.Table.Columns[property.Name];
                        if (null != clm)
                        {
                            object value = dataRow[clm];
                            property.SetValue(dataObject, (value == DBNull.Value)?null:value, null);
                        }
                    }
                }
            }
            return dataObject;
        }

        //------
        public static DataRow GetDataRow(object dataObject, DataTable tbl)
        {
            var tpDataObject = dataObject.GetType();

            DataRow dataRow = tbl.NewRow();
            foreach (var property in tpDataObject.GetProperties())
            {
                var attributes = property.GetCustomAttributes(typeof(DataFieldAttribute), true);
                if (null != attributes && attributes.Length > 0)
                {
                    if (property.CanRead)
                    {
                        object value = property.GetValue(dataObject, null);
                        DataColumn clm = tbl.Columns.Add(property.Name, property.PropertyType);
                        dataRow[clm] = value;
                    }
                }
            }
            return dataRow;
        }

        //------
        public static void AppendDataObjects(this DataTable tbl, IEnumerable dataObjects)
        {
            foreach (object obj in dataObjects)
            {
                AddDataObject(tbl, obj);
            }
            tbl.AcceptChanges();
        }


        //------
        // Function adds a row of data to a data table.
        public static void AddDataObject(this DataTable tbl, object dataObject)
        {
            if (tbl.Columns.Count == 0)
                throw new ApplicationException("Error in adding DataObject/s into the table: No culomn has been defined in the table. Call SetColumns() function befor adding DataObject to the table.");

            var tpDataObject = dataObject.GetType();

            DataRow dataRow = tbl.NewRow();
            foreach (var property in tpDataObject.GetProperties())
            {
                var attributes = property.GetCustomAttributes(typeof(DataFieldAttribute), true);
                if (null != attributes && attributes.Length > 0)
                {
                    if (property.CanRead)
                    {
                        bool sppString = ((DataFieldAttribute)attributes[0]).SppList;
                        bool columnList = ((DataFieldAttribute)attributes[0]).ColumnList;
                        if (sppString)
                        {

                            double[] sppValue = (double[])property.GetValue(dataObject, null);
                            foreach (ISpecies species in ExtensionMetadata.ModelCore.Species)
                            {
                                DataColumn clm = tbl.Columns[(property.Name + species.Name)];
                                string format = ((DataFieldAttribute)attributes[0]).Format;
                                dataRow[clm] = format == null ? sppValue[species.Index].ToString() : string.Format("{0:" + format + "}", sppValue[species.Index].ToString());
                            }
                        }
                        else if (columnList)
                        {
                            double[] columnValue = (double[])property.GetValue(dataObject, null);
                            int i = 0;
                            foreach (String columnName in ExtensionMetadata.ColumnNames)
                            {
                                DataColumn clm = tbl.Columns[(property.Name + columnName)];
                                string format = ((DataFieldAttribute)attributes[0]).Format;
                                dataRow[clm] = format == null ? columnValue[i].ToString() : string.Format("{0:" + format + "}", columnValue[i].ToString());
                                i++;
                            }
                        }
                        else
                        {
                            object value = property.GetValue(dataObject, null);
                            DataColumn clm = tbl.Columns[property.Name];
                            string format = ((DataFieldAttribute)attributes[0]).Format;
                            dataRow[clm] = format == null ? value : string.Format("{0:" + format + "}", value);
                        }
                    }
                }
            }
            tbl.Rows.Add(dataRow);
            tbl.AcceptChanges();

        }

        //------
        public static void WriteToFile(this DataTable tbl, string filePath, bool append)
        {
            StringBuilder strb = new StringBuilder();
            if (!append)
            {
                try
                {
                    file = new System.IO.StreamWriter(filePath, append);
                }
                catch (Exception err)
                {
                    string mesg = string.Format("{0}", err.Message);
                    throw new System.ApplicationException(mesg);
                }
                file.AutoFlush = true;

                
                foreach (DataColumn col in tbl.Columns)
                {
                    strb.AppendFormat("{0},", col.ColumnName);
                }
                file.WriteLine(strb);
            }
            else
            {
                file = new System.IO.StreamWriter(filePath, append);
                foreach (DataRow dr in tbl.Rows)
                {
                    strb = new StringBuilder();
                    foreach (DataColumn col in tbl.Columns)
                    {
                        strb.AppendFormat("{0}, ", dr[col]);
                    }
                    file.WriteLine(strb);
                }
            }
            file.Close();
            file.Dispose();
        }
       
    }
}
