using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace SklepGenerator.OracleDataManager
{
    class QuerryBuilder
    {
        public OracleConnection OracleConnection { get; set; }

        public QuerryBuilder(OracleConnection oracleConnection)
        {
            OracleConnection = oracleConnection;
        }

        public OracleCommand Select(string items, string tableName)
        {
            OracleCommand cmd = new OracleCommand
            {
                Connection = OracleConnection,
                CommandText = "SELECT "+items +" FROM " + tableName,
                CommandType = System.Data.CommandType.Text
            };

            return cmd;
        }

        public OracleCommand SelectWhere(string items, string tableName, string where)
        {
            OracleCommand cmd = new OracleCommand
            {
                Connection = OracleConnection,
                CommandText = "SELECT " + items + " FROM " + tableName+" WHERE "+where,
                CommandType = System.Data.CommandType.Text
            };
            return cmd;
        }
    }
}
