using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace SklepGenerator.OracleDataManager
{
    public class DataInserter
    {
        public OracleConnection OracleConnection { get; set; }
        private InsertBuilder InsertBuilder { get; set; }
        public DataInserter(OracleConnection oracleConnection)
        {
            OracleConnection = oracleConnection;
            InsertBuilder = new InsertBuilder(oracleConnection);
        }

        public string InsertData(string data, string tableName)
        {
            Table temp = new Table();
            OracleCommand cmd = InsertBuilder.Insert(data, tableName);
            Console.WriteLine(cmd.CommandText);
            cmd.ExecuteNonQuery();
            return cmd.CommandText;
        }


    }
}
