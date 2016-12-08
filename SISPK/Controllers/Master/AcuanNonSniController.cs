using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;

namespace SISPK.Controllers.Master
{
    public class AcuanNonSniController : Controller
    {
        //
        // GET: /AcuanNonSni/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListDataAnn(DataTables param, int status)
        {
            var default_order = "ACUAN_NON_SNI_ID";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("ACUAN_NON_SNI_ID");
            order_field.Add("ACUAN_NON_SNI_TYPE");
            order_field.Add("ACUAN_NON_SNI_NO_STANDAR");
            order_field.Add("ACUAN_NON_SNI_JUDUL");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "asc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "ACUAN_NON_SNI_STATUS = " + status;

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
                        search_clause += "LOWER(" + fields + ")  LIKE LOWER('%" + search + "%')";
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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM MASTER_ACUAN_NON_SNI WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  MASTER_ACUAN_NON_SNI " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<MASTER_ACUAN_NON_SNI>(inject_clause_select);
            int no = 1;
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++), 
                Convert.ToString(list.ACUAN_NON_SNI_TYPE), 
                Convert.ToString(list.ACUAN_NON_SNI_NO_STANDAR), 
                Convert.ToString(list.ACUAN_NON_SNI_JUDUL),
                Convert.ToString((list.ACUAN_NON_SNI_STATUS == 1)?"<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/AcuanNonSni/Read/"+list.ACUAN_NON_SNI_ID+"'><i class='action fa fa-file-text-o'></i></a>"+"<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Master/AcuanNonSni/Edit/"+list.ACUAN_NON_SNI_ID+"'><i class='action fa fa-edit'></i></a>"+"<a data-original-title='Non-aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Master/AcuanNonSni/Nonaktif/"+list.ACUAN_NON_SNI_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>":"<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/AcuanNonSni/Read/"+list.ACUAN_NON_SNI_ID+"'><i class='action fa fa-file-text-o'></i></a>"+"<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Master/AcuanNonSni/Edit/"+list.ACUAN_NON_SNI_ID+"'><i class='action fa fa-edit'></i></a>"+"<a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Master/AcuanNonSni/Aktif/"+list.ACUAN_NON_SNI_ID+"'><i class='action glyphicon glyphicon-ok'></i></a></center>"),
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create() {
            return View();
        }

        [HttpPost]
        public ActionResult Create(MASTER_ACUAN_NON_SNI mann) {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_ACUAN_NON_SNI");
            var datenow = MixHelper.ConvertDateNow();

            var fname = "ACUAN_NON_SNI_ID,ACUAN_NON_SNI_TYPE,ACUAN_NON_SNI_NO_STANDAR,ACUAN_NON_SNI_JUDUL,ACUAN_NON_SNI_CREATE_BY,ACUAN_NON_SNI_CREATE_DATE,ACUAN_NON_SNI_STATUS,ACUAN_NON_SNI_LOGCODE";
            var fvalue = "'" + lastid + "', " +
                        "'" + mann.ACUAN_NON_SNI_TYPE + "', " +
                        "'" + mann.ACUAN_NON_SNI_NO_STANDAR + "', " +
                        "'" + mann.ACUAN_NON_SNI_JUDUL + "', " +
                        "'" + UserId + "'," +
                        datenow + "," +
                        "1," +
                        "'" + logcode + "'";
            //return Json(new { query = "INSERT INTO MASTER_BIDANG (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("INSERT INTO MASTER_ACUAN_NON_SNI (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Read(int id = 0)
        {
            ViewData["ans"] = (from a in db.MASTER_ACUAN_NON_SNI where a.ACUAN_NON_SNI_ID == id select a).SingleOrDefault();
            return View();
        }

        public ActionResult Edit(int id = 0)
        {
            ViewData["ans"] = (from a in db.MASTER_ACUAN_NON_SNI where a.ACUAN_NON_SNI_ID == id select a).SingleOrDefault();
            return View();
        }

        [HttpPost]
        public ActionResult Edit(MASTER_ACUAN_NON_SNI mann)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_ACUAN_NON_SNI");
            var datenow = MixHelper.ConvertDateNow();
            var status = "1";

            var update =
                        "ACUAN_NON_SNI_TYPE = '"+mann.ACUAN_NON_SNI_TYPE+"',"+
                        "ACUAN_NON_SNI_NO_STANDAR = '"+mann.ACUAN_NON_SNI_NO_STANDAR+"',"+
                        "ACUAN_NON_SNI_JUDUL = '"+mann.ACUAN_NON_SNI_JUDUL+"',"+
                        "ACUAN_NON_SNI_UPDATE_BY = '"+UserId+"',"+
                        "ACUAN_NON_SNI_UPDATE_DATE = "+datenow+","+
                        "ACUAN_NON_SNI_STATUS = '"+status+"',"+
                        "ACUAN_NON_SNI_LOGCODE = '" + logcode + "'";


            var clause = "where ACUAN_NON_SNI_ID = " + mann.ACUAN_NON_SNI_ID;
            //return Json(new { query = "UPDATE MASTER_ACUAN_NON_SNI SET " + update.Replace("''", "NULL") + " " + clause }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE MASTER_ACUAN_NON_SNI SET " + update.Replace("''", "NULL") + " " + clause);

            //var logId = AuditTrails.GetLogId();
            String objek = update.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Nonaktif(int id = 0)
        {
            db.Database.ExecuteSqlCommand("UPDATE MASTER_ACUAN_NON_SNI SET ACUAN_NON_SNI_STATUS = 0 WHERE ACUAN_NON_SNI_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");

        }

        public ActionResult Aktif(int id = 0)
        {
            db.Database.ExecuteSqlCommand("UPDATE MASTER_ACUAN_NON_SNI SET ACUAN_NON_SNI_STATUS = 1 WHERE ACUAN_NON_SNI_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");
        }

    }
}
