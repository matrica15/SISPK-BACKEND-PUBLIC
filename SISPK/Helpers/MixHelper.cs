using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SISPK.Models;
using System.Text;
using Oracle.DataAccess.Client;

namespace SISPK.Helpers
{
    public class MixHelper
    {
        private SISPKEntities db = new SISPKEntities();

        public static String GetLogCode()
        {
            using (var db = new SISPKEntities())
            {
                string LogId = "";
                string InitialCode = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0') + DateTime.Now.Day.ToString().PadLeft(2, '0');
                int NextNumber = 1;

                var GetLast = db.Database.SqlQuery<String>("SELECT MAX(LOG_CODE) AS CODE FROM SYS_LOGS WHERE LOG_CODE LIKE '" + InitialCode + "%'").SingleOrDefault();
                if (GetLast != null)
                {
                    LogId = GetLast.Replace(InitialCode, "");
                    int.TryParse(LogId, out NextNumber);
                    NextNumber = NextNumber + 1;
                    LogId = "";
                }
                LogId = InitialCode + NextNumber.ToString().PadLeft(8, '0');
                return LogId;

            }
        }
        public static String InsertLog(String code, String objek, int action)
        {

            using (var db = new SISPKEntities())
            {
                var GetIP = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 12").FirstOrDefault();
                var GetUser = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 13").FirstOrDefault();
                var GetPassword = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 14").FirstOrDefault();
                using (OracleConnection con = new OracleConnection("Data Source=" + GetIP.CONFIG_VALUE + ";User ID=" + GetUser.CONFIG_VALUE + ";PASSWORD=" + GetPassword.CONFIG_VALUE + ";"))
                {
                    con.Open();

                    using (OracleCommand cmd = new OracleCommand())
                    {
                        int id = db.Database.SqlQuery<int>("SELECT SEQ_SYS_LOGS.NEXTVAL FROM DUAL").SingleOrDefault();
                        string UserId = HttpContext.Current.Session["USER_ID"].ToString();
                        string AccesId = HttpContext.Current.Session["USER_ACCESS_ID"].ToString();
                        var tipe = Convert.ToString(HttpContext.Current.Request.RequestContext.RouteData.Values["tipe"]);
                        var controller = Convert.ToString(HttpContext.Current.Request.RequestContext.RouteData.Values["controller"]);
                        var menu_url = ("/" + tipe + "/" + controller).ToLower();
                        var menu_id = db.Database.SqlQuery<Nullable<Int32>>("SELECT CAST(MENU_ID AS INT) FROM SYS_MENU WHERE LOWER(MENU_URL) = '" + menu_url + "'").FirstOrDefault();
                        var fname = "LOG_ID, LOG_CODE, LOG_MENU_ID, LOG_USER, LOG_USER_TYPE, LOG_ACTION, LOG_OBJECT, LOG_DATE";
                        var fvalue = "'" + id + "', " +
                                     "'" + code + "', " +
                                      "'" + menu_id + "', " +
                                     "'" + UserId + "', " +
                                     "'" + AccesId + "', " +
                                     "'" + action + "', " +
                                     ":parameter, " +
                                     "SYSDATE";

                        cmd.Connection = con;
                        cmd.CommandType = System.Data.CommandType.Text;

                        cmd.CommandText = " INSERT INTO SYS_LOGS (" + fname + ") VALUES (" + fvalue + ") ";

                        OracleParameter oracleParameterClob = new OracleParameter();
                        oracleParameterClob.OracleDbType = OracleDbType.Clob;
                        //1 million long string
                        oracleParameterClob.Value = objek;


                        cmd.Parameters.Add(oracleParameterClob);

                        cmd.ExecuteNonQuery();
                    }

                    con.Close();
                }
                
                return "1";

            }
        }


        public static int GetSequence(String _table)
        {
            using (var db = new SISPKEntities())
            {
                int id = db.Database.SqlQuery<int>("SELECT SEQ_" + _table + ".NEXTVAL FROM DUAL").SingleOrDefault();
                return id;
            }
        }
        public static bool GetAksesRole(int USER_ID = 0, int MENU_ID = 0, int ACCESS_ID = 0, string TYPE = "view")
        {
            //var ACCESS_DETAIL_TYPE = 0;
            //if (TYPE == "view")
            //{
            //    ACCESS_DETAIL_TYPE = 1;
            //}
            //else if (TYPE == "create")
            //{
            //    ACCESS_DETAIL_TYPE = 2;
            //}
            //else if (TYPE == "update")
            //{
            //    ACCESS_DETAIL_TYPE = 3;
            //}
            //else if (TYPE == "delete")
            //{
            //    ACCESS_DETAIL_TYPE = 4;
            //}
            //else if (TYPE == "approve")
            //{
            //    ACCESS_DETAIL_TYPE = 5;
            //}
            //else if (TYPE == "print")
            //{
            //    ACCESS_DETAIL_TYPE = 6;
            //}
            var CekAkses = false;
            using (var db = new SISPKEntities())
            {
                int id = db.Database.SqlQuery<int>("SELECT  FROM DUAL").SingleOrDefault();
                return CekAkses;
            }
        }
        public static String ConvertDateNow()
        {
            String datenow = "TO_DATE('" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";
            return datenow;
        }

    }
}