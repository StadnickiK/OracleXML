using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using SklepGenerator.OracleDataManager;

namespace SklepGenerator.Connection
{
    public class OracleDB
    {
        public OracleConnection OConnection { get; set; }
        public string ConString { get; set; }

        DataGenerator DataGenerator = new DataGenerator();
        DataLoader DataLoader { get; set; }
        FileManager _fileManager = new FileManager();
        public DataInserter DataInserter { get; set; }

        public OracleDB(string conString) {
            ConString = conString;
        }

        public void Connect()
        {
            OConnection = new OracleConnection
            {
                ConnectionString = ConString
            };
            OConnection.Open();
            this.DataLoader = new DataLoader(OConnection);
            DataInserter = new DataInserter(OConnection);
            Console.WriteLine("Connected to Oracle " + OConnection.ServerVersion);
        }
        public Dictionary<string, Table> GetDataScheme() { return DataLoader.DataScheme; }

        public void Connect(string conString)
        {
            OConnection = new OracleConnection
            {
                ConnectionString = conString
            };
            OConnection.Open();
            Console.WriteLine("Connected to Oracle " + OConnection.ServerVersion);
        }

        public void Close()
        {
            OConnection.Close();
            OConnection.Dispose();
            Console.WriteLine("Connection closed");
        }

        public void SaveAsXML(string tableName)
        {
            
            _fileManager.AddLine(DataToXML(DataLoader.LoadDataByTable(tableName), tableName));
            _fileManager.SaveAsXMLFile(tableName);
        }

        string DataToXML(List<string> data, string tableName)
        {
            string xmlString = "<?xml version="+'"'+"1.0"+'"'+"?> \n" +
                "<"+tableName+">\n";
            Table t = DataLoader.DataScheme[tableName];
            int count = t.Columns.Count;
            for(int i = 0; i < data.Count; i += count)
            {
                xmlString += "\t<" + t.Name + " id=" +'"'+i/count+'"'  + ">\n";
                for (int j = 1; j < count; j++)
                {
                    string name = t.Columns[j].Name;
                    xmlString += "\t\t<" + name +">" + data[i+j]+"</"+name+">\n";
                }
                xmlString += "\t</" + tableName + ">\n";
            }
            xmlString += "</" + tableName + ">";
            return xmlString;
        }

        List<string> ReadImportData(XmlDocument data)
        {
            List<string> importData = new List<string>();
            try
            {
                importData.Add(data.DocumentElement.Name);
                foreach (XmlNode node in data.DocumentElement.ChildNodes)
                {
                    importData.Add(node.Attributes["id"]?.InnerText);
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        importData.Add(childNode.InnerText);
                    }
                }
            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

            return importData;
        }

        public void ImportData(XmlDocument data)
        {
            Console.WriteLine("Importing file");
            if (data != null)
            {
                List<string> stringData = new List<string>();
                stringData = ReadImportData(data);
                string tableName;
                if(stringData != null)
                {
                    if (DataLoader.DataScheme.ContainsKey(stringData[0]))
                    {
                        tableName = stringData[0];
                        int id;
                        if (Int32.TryParse(stringData[1], out id))
                        {
                            if (id > DataLoader.GetLastID(tableName))
                            {
                                InsertData(stringData, tableName);
                            }
                            else
                            {
                                Console.WriteLine("Invalid ID "+id+" for "+tableName);
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine("Invalid ID "+stringData[1] +" for "+tableName);
                            return;
                        }
                    }
                    else
                    {
                        Console.WriteLine("Invalid table name");
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
            else
            {
                Console.WriteLine("Invalid data selected");
                return;
            }
        }



        public void InsertData(List<string> data, string tableName)
        {
            Table t = DataLoader.DataScheme[tableName];
            int count = t.Columns.Count;
            if ((data.Count-1) % count == 0)
            {
                string row = "";
                for (int i = 1; i < data.Count; i += count)
                {
                    for (int j = 0; j < count; j++)
                    {
                        if (!t.Relations.ContainsKey(t.Columns[j].Name))
                        {
                            if (t.Columns[j].Value == "VARCHAR2")
                            {
                                row += "'" + data[i + j] + "'";
                            }
                            else
                            {
                                row += data[i + j];
                            }
                            if (j != count - 1)
                            {
                                row += ",";
                            }
                        }
                        else
                        {
                            int id;
                            if (Int32.TryParse(data[i + j], out id)) {
                                if (DataLoader.CheckID(id, tableName, t.Columns[j].Name))
                                {
                                    row += data[i + j];
                                    if (j != count - 1)
                                    {
                                        row += ",";
                                    }
                                }
                                else
                                {
                                    Console.WriteLine(data[i+j]+" ID does't exist at "+(i+j)+" row");
                                }
                            }
                            else
                            {
                                Console.WriteLine("NaN ID at " + (i + j) + " row");
                            }
                        }
                    }
                    DataInserter.InsertData(row, tableName);
                    row = "";
                }
            }
            else
            {
                Console.WriteLine("Improper amount of columns for the given table.");
                return;
            }
            Console.WriteLine("Operation successful");
        }

        public void LoadData()
        {
            DataLoader.LoadDB();
        }

        public int GetLastID(string tableName)
        {
            return DataLoader.GetLastID(tableName);
        }

        public List<int> GetUnusedIDs(string tableName)
        {
            return DataLoader.GetUnusedRowsIds(tableName);
        }

        public List<int> GetUnusedIDsOfRelations(string tableName)
        {
            return DataLoader.GetUnusedIDsOfRelations(tableName);
        }


        public int InsertRandValues(string tableName)
        {
            Table t = DataLoader.DataScheme[tableName];
            string data = "";
            int i = 0;
            int id = 0;
            foreach(DataTypes.DataType d in t.Columns)
            {
                DateTime date = new DateTime(1970,1,1);
                if (d.Value == "DATE")
                {
                    date = DataLoader.GetLastDate(tableName, d.Name);
                }
                if (t.Relations.ContainsKey(d.Name))
                {
                    if(DataLoader.DataScheme[t.Relations[d.Name]].FreeIDs.Count != 0)
                    {
                        int freeID = DataLoader.DataScheme[t.Relations[d.Name]].FreeIDs[DataGenerator.GetRandNumber(0,
                            DataLoader.DataScheme[t.Relations[d.Name]].FreeIDs.Count)];
                        DataLoader.DataScheme[t.Relations[d.Name]].FreeIDs.Remove(freeID);
                        data += freeID;
                    }
                    else
                    {
                        data += InsertRandValues(t.Relations[d.Name]).ToString();
                    }
                }
                else
                {

                    if (i == t.Columns.Count - 1)
                    {
                        data += DataGenerator.GenerateRandData(d.Value, date);
                    }
                    else if (i == 0)
                    {
                        id = GetLastID(tableName);
                        id++;
                        data += id.ToString();
                    }
                    else
                    {
                        data += DataGenerator.GenerateRandData(d.Value, date);
                    }
                }
                if (i != t.Columns.Count - 1)
                    data += ",";
                i++;
            }
            string s = DataInserter.InsertData(data, tableName);
            Console.WriteLine(s);
            _fileManager.AddLine(s);
            return id;
        }

        public int InsertRandValues(string tableName, int count)
        {
            Console.WriteLine("Inserting " + count + " rows into " + tableName);
            for (int i = 0; i < count;i++)
            {
                InsertRandValues(tableName);
            }
            Console.WriteLine("Inserting finished");
            _fileManager.SaveToFile(tableName);
            return 0;
        }

    }
}
