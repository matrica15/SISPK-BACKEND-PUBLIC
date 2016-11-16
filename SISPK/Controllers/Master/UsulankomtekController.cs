using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.Security.Cryptography;

namespace SISPK.Controllers.Master
{
    public class UsulankomtekController : Controller
    {
        //
        // GET: /Usulankomtek/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UsulanpublicList(DataTables param, int status = 0)
        {
            var default_order = "T_KOMTEK_NAME";
            var limit = 10;

            List<string> order_field = new List<string>();
            //order_field.Add("T_KOMTEK_CODE");
            order_field.Add("T_KOMTEK_NAME");
            order_field.Add("BIDANG_CODE");
            order_field.Add("INSTANSI_NAME");
            //order_field.Add("ICS_NAME");

            //order_field.Add("T_KOMTEK_SK_PENETAPAN");
            //order_field.Add("KOMTEK_SEKRETARIAT");
            //order_field.Add("KOMTEK_EMAIL");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "asc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "T_KOMTEK_STATUS = " + status;

            string search_clause = "";
            if (search != "")
            {
                if (where_clause != "")
                {
                    search_clause += " AND ";
                }
                search_clause += "(";
                var i = 1;
                foreach (var fields in order_field)
                {
                    if (fields != "")
                    {
                        search_clause += fields + "  LIKE '%" + search + "%'";
                        if (i < order_field.Count())
                        {
                            search_clause += " OR ";
                        }
                    }
                    i++;
                }
                search_clause += ")";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_T_KOMTEK WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_T_KOMTEK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_T_KOMTEK>(inject_clause_select);
            //int no = 1;
            var result = from list in SelectedData
                         select new string[] 
            { 
                //Convert.ToString(list.T_KOMTEK_CODE),
                Convert.ToString(list.T_KOMTEK_NAME),
                Convert.ToString(list.BIDANG_CODE), 
                Convert.ToString(list.INSTANSI_NAME),                
                //Convert.ToString(list.KOMTEK_SEKRETARIAT),
                //Convert.ToString(list.KOMTEK_EMAIL),
                //Convert.ToString(list.T_KOMTEK_SK_PENETAPAN),
                //Convert.ToString((list.KOMTEK_STATUS == 0)?"<span class='red'>Tidak Aktif</span>":"<span class='red'>Aktif</span>"),
                //<a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Komtek/Read/"+list.KOMTEK_ID+"'><i class='action fa fa-file-text-o'></i></a>
                //<a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Komtek/Read/"+list.KOMTEK_ID+"'><i class='action fa fa-file-text-o'></i></a>
                (list.T_KOMTEK_STATUS == 0)?"<center><a data-original-title='Approve' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Master/Usulankomtek/Detailusulanpublic/"+list.T_KOMTEK_ID+"'><i class='action fa fa-check'></i></a><a data-original-title='Tolak' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Master/Usulankomtek/RejectUsulan/"+list.T_KOMTEK_ID+"'><i class='action fa fa-times'></i></a></center>":"",
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detailusulanpublic(int id)
        {
            ViewData["uspub"] = (from t in db.VIEW_T_KOMTEK where t.T_KOMTEK_ID == id select t).SingleOrDefault();
            ViewData["komtek_ics"] = (from t in db.VIEW_KOMTEK_ICS where t.KOMTEK_ICS_KOMTEK_ID == id && t.KOMTEK_ICS_STATUS == 1 select t).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Detailusulanpublic(T_MASTER_KOMITE_TEKNIS tmkt)
        {
            VIEW_T_KOMTEK vtk = db.VIEW_T_KOMTEK.SingleOrDefault(t => t.T_KOMTEK_ID == tmkt.T_KOMTEK_ID);
            if (vtk == null)
            {
                return HttpNotFound();
            }

            string query_update_group = "UPDATE T_MASTER_KOMITE_TEKNIS SET T_KOMTEK_STATUS = 1 WHERE T_KOMTEK_ID = '" + tmkt.T_KOMTEK_ID + "'";
            db.Database.ExecuteSqlCommand(query_update_group);

            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_KOMITE_TEKNIS");
            var datenow = MixHelper.ConvertDateNow();
            //int t_komtek_code = (db.Database.SqlQuery<int>("SELECT SUBSTR (MAX(MK.KOMTEK_CODE), 1, 2)+1  AS CODE_AWAL FROM MASTER_KOMITE_TEKNIS MK WHERE MK.KOMTEK_PARENT_CODE = '0'").SingleOrDefault()) + 1;

            DateTime dates = Convert.ToDateTime(tmkt.T_KOMTEK_TANGGAL_PEMBENTUKAN);
            var date = "TO_DATE('" + dates.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";
            var fname = "KOMTEK_ID,KOMTEK_PARENT_CODE,KOMTEK_CODE,KOMTEK_BIDANG_ID,KOMTEK_INSTANSI_ID,KOMTEK_NAME,KOMTEK_SEKRETARIAT,KOMTEK_ADDRESS,KOMTEK_PHONE,KOMTEK_FAX,KOMTEK_EMAIL,KOMTEK_SK_PENETAPAN,KOMTEK_TANGGAL_PEMBENTUKAN,KOMTEK_DESCRIPTION,KOMTEK_CREATE_BY,KOMTEK_CREATE_DATE,KOMTEK_LOG_CODE,KOMTEK_TEMP_ID,KOMTEK_STATUS";
            var fvalue = "'" + lastid + "', " +
                        "'" + vtk.T_KOMTEK_PARENT_CODE + "', " +
                        "'" + tmkt.T_KOMTEK_CODE + "', " +
                        "'" + vtk.T_KOMTEK_BIDANG_ID + "', " +
                        "'" + vtk.T_KOMTEK_INSTANSI_ID + "'," +
                        "'" + vtk.T_KOMTEK_NAME + "'," +
                        "'" + vtk.T_KOMTEK_SEKRETARIAT + "'," +
                        "'" + vtk.T_KOMTEK_ADDRESS + "'," +
                        "'" + vtk.T_KOMTEK_PHONE + "'," +
                        "'" + vtk.T_KOMTEK_FAX + "'," +
                        "'" + vtk.T_KOMTEK_EMAIL + "'," +
                        "'" + tmkt.T_KOMTEK_SK_PENETAPAN + "'," +
                        date + "," +
                        "'" + vtk.T_KOMTEK_DESCRIPTION + "'," +
                        "'" + UserId + "'," +
                         datenow + "," +
                        "'" + logcode + "'," +
                        "'" + vtk.T_KOMTEK_ID + "'," +
                        "1";
            //return Json(new { query = "INSERT INTO MASTER_KOMITE_TEKNIS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("INSERT INTO MASTER_KOMITE_TEKNIS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            //var logId = AuditTrails.GetLogId();
            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);

            ViewData["komtek_ics"] = (from t in db.VIEW_T_KOMTEK_ICS where t.T_KOMTEK_ICS_KOMTEK_ID == vtk.T_KOMTEK_ID && t.T_KOMTEK_ICS_STATUS == 1 select t).ToList();

            foreach (var a in ViewBag.komtek_ics)
            {
                int lastid_mki = MixHelper.GetSequence("MASTER_KOMTEK_ICS");
                string query_update = "INSERT INTO MASTER_KOMTEK_ICS (KOMTEK_ICS_ID, KOMTEK_ICS_KOMTEK_ID, KOMTEK_ICS_ICS_ID, KOMTEK_ICS_CREATE_BY, KOMTEK_ICS_CREATE_DATE, KOMTEK_ICS_STATUS, KOMTEK_ICS_LOG_CODE) VALUES (" + lastid_mki + "," + lastid + "," + a.T_KOMTEK_ICS_ICS_ID + "," + UserId + "," + datenow + ",1,'" + logcode + "')";
                db.Database.ExecuteSqlCommand(query_update);
            }
            var linkend = "";
            if (vtk.T_KOMTEK_PARENT_CODE == "0")
            {
                linkend = "../Komtek/CreateAnggota";
            }
            else {
                linkend = "../Subkomtek/CreateAnggota";
            }

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di setujui";
            return RedirectToAction(linkend + "/" + lastid);
        }

        public ActionResult RejectUsulan(int id)
        {
            string query_update = "UPDATE T_MASTER_KOMITE_TEKNIS SET T_KOMTEK_STATUS = 2 WHERE T_KOMTEK_ID = '" + id + "'";
            db.Database.ExecuteSqlCommand(query_update);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Reject";

            return RedirectToAction("Index");
        }

    }
}
