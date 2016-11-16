using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oracle.DataAccess;

namespace SISPK.Models
{
    public class clsOracle
    {
        private Oracle.DataAccess.Client.OracleConnection connOracle;
        private Oracle.DataAccess.Client.OracleDataReader rstOracle;
        private Oracle.DataAccess.Client.OracleCommand sqlCommandOracle;
        private Oracle.DataAccess.Client.OracleTransaction txn;
        private Oracle.DataAccess.Types.OracleClob clob;

        public clsOracle()
        {
            string p_conn_db = "Data Source=localhost:1521/XE;User ID=C##BSN_SISPK;PASSWORD=BsnPassword1;";
            connOracle = new Oracle.DataAccess.Client.OracleConnection(p_conn_db);
            connOracle.Open();
        }

        public void InsertRecord(string SQLStatement)
        {
            if (SQLStatement.Length > 0)
            {
                if (connOracle.State.ToString().Equals("Open"))
                {
                    sqlCommandOracle =
                      new Oracle.DataAccess.Client.OracleCommand(SQLStatement, connOracle);
                    sqlCommandOracle.ExecuteScalar();
                }
            }
        }

        public void InsertCLOB(string SQLStatement, string str)
        {
            if (SQLStatement.Length > 0)
            {
                if (connOracle.State.ToString().Equals("Open"))
                {
                    byte[] newvalue = System.Text.Encoding.Unicode.GetBytes(str);
                    sqlCommandOracle =
                      new Oracle.DataAccess.Client.OracleCommand(SQLStatement, connOracle);
                    rstOracle = sqlCommandOracle.ExecuteReader();
                    rstOracle.Read();
                    txn = connOracle.BeginTransaction();
                    clob = rstOracle.GetOracleClob(0);
                    clob.Write(newvalue, 0, newvalue.Length);
                    txn.Commit();
                }
            }
        }
        public void CloseDatabase()
        {
            connOracle.Close();
            connOracle.Dispose();
        }   
    }
}