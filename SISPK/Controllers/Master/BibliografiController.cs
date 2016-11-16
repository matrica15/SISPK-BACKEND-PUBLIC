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
    public class BibliografiController : Controller
    {
        //
        // GET: /Bibliografi/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListBibliografi(DataTables param, int status = 0)
        {
            var default_order = "BIBLIOGRAFI_ID";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("BIBLIOGRAFI_ID");
            order_field.Add("BIBLIOGRAFI_JUDUL");            

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "asc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "BIBLIOGRAFI_STATUS = " + status;

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM MASTER_BIBLIOGRAFI WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  MASTER_BIBLIOGRAFI " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<MASTER_BIBLIOGRAFI>(inject_clause_select);
            int no = 1;
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(no++), 
                Convert.ToString(list.BIBLIOGRAFI_JUDUL),              
                Convert.ToString((list.BIBLIOGRAFI_STATUS == 1)?"<center><a data-original-title='Detail' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Bibliografi/Read/"+list.BIBLIOGRAFI_ID+"'><i class='action fa fa-file-text-o'></i></a><a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Master/Bibliografi/Edit/"+list.BIBLIOGRAFI_ID+"'><i class='action fa fa-edit'></i></a>"+"<a data-original-title='Non-Aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Master/Bibliografi/Nonaktif/"+list.BIBLIOGRAFI_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>":"<center><a data-original-title='Add anggota' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Bibliografi/CreateAnggota/"+list.BIBLIOGRAFI_ID+"'><i class='action fa fa-users'></i></a><a data-original-title='Detail' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Bibliografi/Read/"+list.BIBLIOGRAFI_ID+"'><i class='action fa fa-file-text-o'></i></a>"+"<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Master/Bibliografi/Edit/"+list.BIBLIOGRAFI_ID+"'><i class='action fa fa-edit'></i></a>"+"<a data-original-title='Hapus' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Master/Bibliografi/Aktif/"+list.BIBLIOGRAFI_ID+"'><i class='action glyphicon glyphicon-ok'></i></a></center>"),
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
        public ActionResult Create(MASTER_BIBLIOGRAFI mb)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_BIBLIOGRAFI");
            var datenow = MixHelper.ConvertDateNow();

            var fname = "BIBLIOGRAFI_ID,BIBLIOGRAFI_JUDUL,BIBLIOGRAFI_CREATE_BY,BIBLIOGRAFI_CREATE_DATE,BIBLIOGRAFI_STATUS,BIBLIOGRAFI_LOGCODE";
            var fvalue = "'" + lastid + "', " +
                        "'" + mb.BIBLIOGRAFI_JUDUL + "', " +
                        "'" + UserId + "'," +
                        datenow + "," +
                        "1," +
                        "'" + logcode + "'";
            //return Json(new { query = "INSERT INTO MASTER_BIDANG (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("INSERT INTO MASTER_BIBLIOGRAFI (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Read(int id = 0)
        {
            ViewData["bib"] = (from a in db.MASTER_BIBLIOGRAFI where a.BIBLIOGRAFI_ID == id select a).SingleOrDefault();
            return View();
        }

        public ActionResult Edit(int id = 0) {
            ViewData["bib"] = (from a in db.MASTER_BIBLIOGRAFI where a.BIBLIOGRAFI_ID == id select a).SingleOrDefault();
            return View();
        }       

        [HttpPost]
        public ActionResult Edit(MASTER_BIBLIOGRAFI mb)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_BIBLIOGRAFI");
            var datenow = MixHelper.ConvertDateNow();
            var status = "1";

            var update =
                         " BIBLIOGRAFI_JUDUL = '" + mb.BIBLIOGRAFI_JUDUL + "'," +
                         " BIBLIOGRAFI_UPDATE_BY = " + UserId + "," +
                         " BIBLIOGRAFI_UPDATE_DATE = " + datenow + "," +
                         " BIBLIOGRAFI_LOGCODE = '" + logcode + "', " +
                         " BIBLIOGRAFI_STATUS = " + status + "";


            var clause = "where BIBLIOGRAFI_ID = " + mb.BIBLIOGRAFI_ID;
            //return Json(new { query = "UPDATE MASTER_BIDANG SET " + update.Replace("''", "NULL") + " " + clause }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE MASTER_BIBLIOGRAFI SET " + update.Replace("''", "NULL") + " " + clause);

            //var logId = AuditTrails.GetLogId();
            String objek = update.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Nonaktif(int id = 0)
        {

            db.Database.ExecuteSqlCommand("UPDATE MASTER_BIBLIOGRAFI SET BIBLIOGRAFI_STATUS = 0 WHERE BIBLIOGRAFI_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");

        }

        public ActionResult Aktif(int id = 0)
        {
            db.Database.ExecuteSqlCommand("UPDATE MASTER_BIBLIOGRAFI SET BIBLIOGRAFI_STATUS = 1 WHERE BIBLIOGRAFI_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");
        }

    }
}
