using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace SklepGenerator.OracleDataManager
{
    class InsertBuilder
    {
        public OracleConnection OracleConnection { get; set; }

        public InsertBuilder(OracleConnection oracleConnection)
        {
            OracleConnection = oracleConnection;
        }

        public OracleCommand Insert(string values, string tableName)
        {
            OracleCommand cmd = new OracleCommand
            {
                Connection = OracleConnection,
                CommandText = "INSERT INTO " + tableName + " VALUES (" + values + ")",
                CommandType = System.Data.CommandType.Text
            };
            return cmd;
        }

    }
}
