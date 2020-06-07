using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace SklepGenerator.OracleDataManager
{
    public class DataLoader
    {
        OracleConnection OracleConnection { get; set; }
        public Dictionary<string, Table> DataScheme { get; set; } = new Dictionary<string, Table>();
        private QuerryBuilder QuerryBuilder { get; set; }

        public DataLoader(OracleConnection oracleConnection)
        {
            OracleConnection = oracleConnection;
            QuerryBuilder = new QuerryBuilder(oracleConnection);
        }

        void LoadScheme()
        {
            Console.WriteLine("Loading tables");
            OracleCommand cmd = QuerryBuilder.Select("TABLE_NAME, COLUMN_NAME, DATA_TYPE", "USER_TAB_COLUMNS");
            OracleDataReader reader = cmd.ExecuteReader();
            reader.Read();
            string tableName = reader.GetString(0);
            Table temp = new Table();
            do
            {
                if (tableName != reader.GetString(0))
                {
                    if (temp.Columns.Count > 0)
                    {
                        temp.Columns.Reverse();
                        temp.Name = tableName;
                        DataScheme.Add(tableName, temp);
                    }
                    temp = new Table();
                    tableName = reader.GetString(0);
                    temp.Columns.Add(new DataTypes.Varchar
                    {
                        Name = reader.GetString(1),
                        Value = reader.GetString(2)
                    });
                    temp.Name = tableName;
                }
                else
                {
                    temp.Columns.Add(new DataTypes.Varchar
                    {
                        Name = reader.GetString(1),
                        Value = reader.GetString(2)
                    });
                }
            } while (reader.Read());
            temp.Columns.Reverse();
            DataScheme.Add(tableName, temp);
            reader.Close();
        }

        void LoadPKNames()
        {
            foreach(Table t in DataScheme.Values)
            {
                t.PrimaryKeyName = GetPKColumnName(t.Name);
                if(t.Columns[0].Name != t.PrimaryKeyName)
                {
                    t.Columns.Reverse();
                }
            }
        }

        void LoadRelations()
        {
            Console.WriteLine("Loading relations");
                OracleCommand cmd = QuerryBuilder.Select(" distinct table_name, column_name, r_table_name",
                    "(SELECT uc.table_name, uc.constraint_name, cols.column_name, " +
                    "(select table_name from user_constraints " +
                    "where constraint_name = uc.r_constraint_name) r_table_name, " +
                    "(select column_name from user_cons_columns " +
                    "where constraint_name = uc.r_constraint_name " +
                    "and position = cols.position) " +
                    "r_column_name, " +
                    "cols.position, " +
                    "uc.constraint_type FROM user_constraints uc " +
                    "inner join user_cons_columns cols on uc.constraint_name = cols.constraint_name " +
                    "where constraint_type = 'R')"); 
                OracleDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string tableName = reader.GetString(0);
                DataScheme[tableName].Relations.Add(reader.GetString(1), reader.GetString(2));
            }
        }

        string GetPKColumnName(string tableName)    /// Returns the name of primary key column for a given table
        {
            string name = null;

            OracleCommand cmd = QuerryBuilder.SelectWhere("column_name",
                "all_cons_columns",
                "constraint_name = (select constraint_name from user_constraints where " +
                "UPPER(table_name) = UPPER('" + tableName + "') AND CONSTRAINT_TYPE = 'P')");
            OracleDataReader reader = cmd.ExecuteReader();
            reader.Read();
            name = reader.GetString(0); 
            return name;
        }

        public int GetLastID(string tableName)
        {
            string pkColumName = GetPKColumnName(tableName);
            
            OracleCommand cmd = QuerryBuilder.Select("max(" + pkColumName + ")", tableName);
            OracleDataReader reader = cmd.ExecuteReader();
            reader.Read();
            if (reader.HasRows && !reader.IsDBNull(0))
            {
                return (int)reader.GetDouble(0);
            }else{
                return 0;
            }
        }

        public DateTime GetLastDate(string tableName, string columnName)
        {
            DateTime date = new DateTime(1970,1,1);
            OracleCommand cmd = QuerryBuilder.Select("max(" + columnName + ")", tableName);
            OracleDataReader reader = cmd.ExecuteReader();
            reader.Read();
            if (reader.HasRows)
            {
                return reader.GetDateTime(0);
            }
            else
            {
                return date;
            }
        }

        List<string> GetRelationsOfTable(string tableName)
        {
            List<string> tableNames = new List<string>();
            foreach(Table t in DataScheme.Values)
            {
                if (t.Relations.ContainsValue(tableName)) tableNames.Add(t.Name);
            }
            return tableNames;
        }

        public bool CheckID(int id, string tableName, string columName)
        {
            OracleCommand cmd = QuerryBuilder.SelectWhere(columName,tableName,columName+"="+id);
            OracleDataReader reader = cmd.ExecuteReader();
            reader.Read();
            if (reader.HasRows)
            {
                return true;
            }
            return false;
        }


        public List<int> GetUnusedRowsIds(string tableName)
        {
            List<int> idList = new List<int>();
            List<string> tableNames = GetRelationsOfTable(tableName);
            if (tableNames.Count != 0)
            {
                string pkName = GetPKColumnName(tableName);
                string where = "";
                int i = 0;
                foreach (string s in tableNames)
                {
                    where += pkName + " NOT IN (SELECT " + pkName + " FROM " + s + ")";
                    i++;
                    if (i != tableNames.Count) where += " AND ";
                }
                OracleCommand cmd = QuerryBuilder.SelectWhere(pkName, tableName, where);
                OracleDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    idList.Add((int)reader.GetDouble(0));
                }
            }
            return idList;
        }

        public List<int> GetUnusedIDsOfRelations(string tableName)
        {
            List<int> ids = new List<int>();
            foreach (string s in DataScheme[tableName].Relations.Values)
            {
                ids.AddRange(GetUnusedRowsIds(s));
            }
            return ids;
        }

        void LoadFreeIDs()
        {
            Console.WriteLine("Loading free keys");
            foreach(Table t in DataScheme.Values)
            {
                t.FreeIDs = GetUnusedRowsIds(t.Name);
            }
        }

        public void LoadDB()
        {
            LoadScheme();
            LoadRelations();
            //LoadFreeIDs();
            Console.WriteLine("Loading finished");
        }

        public List<string> LoadDataByTable(string tableName)
        {
            Console.WriteLine("Loading data for " + tableName);
            List<string> tab = new List<string>();
            OracleCommand cmd = QuerryBuilder.Select("*",tableName);
            OracleDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                for (int i = 0; i < DataScheme[tableName].Columns.Count; i++)
                {
                    tab.Add(reader[i].ToString());
                }
            }
            return tab;
        }
    }
}
