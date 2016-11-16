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
    [Auth(RoleTipe = 1)]
    public class InstansiController : Controller
    {
        //
        // GET: /Instansi/
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            return View();
        }
        [Auth(RoleTipe = 2)]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(MASTER_INSTANSI master_instansi) 
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_INSTANSI");
            var datenow = MixHelper.ConvertDateNow();

            var fname = "INSTANSI_ID,INSTANSI_CODE,INSTANSI_NAME,INSTANSI_CREATE_BY,INSTANSI_CREATE_DATE,INSTANSI_LOG_CODE,INSTANSI_STATUS";
            var fvalue = "'" + lastid + "', " +
                        "'" + master_instansi.INSTANSI_CODE + "', " +
                        "'" + master_instansi.INSTANSI_NAME + "', " +
                        "'" + UserId + "', " +
                        datenow + "," +
                        "'" + logcode + "'," +
                        "1";
            //return Json(new { query = "INSERT INTO MASTER_INSTANSI (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("INSERT INTO MASTER_INSTANSI (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        [Auth(RoleTipe = 3)]
        public ActionResult Edit(int id = 0) 
        {
            ViewData["instansi_item"] = (from t in db.VIEW_INSTANSI where t.INSTANSI_ID == id select t).SingleOrDefault();
            //VIEW_INSTANSI instansi_item = db.VIEW_INSTANSI.SingleOrDefault(t => t.INSTANSI_ID == id);
            ////return Json(new { query = instansi_item }, JsonRequestBehavior.AllowGet);
            //if (instansi_item == null)
            //{
            //    return HttpNotFound();
            //}
            return View();
        }

        [HttpPost]
        public ActionResult Edit(MASTER_INSTANSI master_instansi) {

            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_INSTANSI");
            var datenow = MixHelper.ConvertDateNow();
            var status = "1";

            var update =
                         " INSTANSI_CODE = '" + master_instansi.INSTANSI_CODE + "'," +
                         " INSTANSI_NAME = '" + master_instansi.INSTANSI_NAME + "'," +
                         " INSTANSI_UPDATE_BY = '" + UserId + "'," +
                         " INSTANSI_UPDATE_DATE = " + datenow + "," +
                         " INSTANSI_LOG_CODE = '" + logcode + "', " +
                         " INSTANSI_STATUS = '" + status + "'";


            var clause = "where INSTANSI_ID = " + master_instansi.INSTANSI_ID;
            //return Json(new { query = "UPDATE MASTER_INSTANSI SET " + update.Replace("''", "NULL") + " " + clause }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE MASTER_INSTANSI SET " + update.Replace("''", "NULL") + " " + clause);

            //var logId = AuditTrails.GetLogId();
            String objek = update.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Read(int id = 0)
        {
            ViewData["instansi_item"] = (from t in db.VIEW_INSTANSI where t.INSTANSI_ID == id select t).SingleOrDefault();
            //VIEW_INSTANSI instansi_item = db.VIEW_INSTANSI.SingleOrDefault(t => t.INSTANSI_ID == id);
            ////return Json(new { query = instansi_item }, JsonRequestBehavior.AllowGet);
            //if (instansi_item == null)
            //{
            //    return HttpNotFound();
            //}
            return View();
        }

        public ActionResult Nonaktif(int id = 0)
        {

            db.Database.ExecuteSqlCommand("UPDATE MASTER_INSTANSI SET INSTANSI_STATUS = 0 WHERE INSTANSI_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");

        }

        public ActionResult Aktif(int id = 0)
        {
            db.Database.ExecuteSqlCommand("UPDATE MASTER_INSTANSI SET INSTANSI_STATUS = 1 WHERE INSTANSI_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult ListDataInstansi(DataTables param, int status = 0)
        {
            var default_order = "INSTANSI_CREATE_DATE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("INSTANSI_CODE");
            order_field.Add("INSTANSI_NAME");
            order_field.Add("INSTANSI_STATUS");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "INSTANSI_STATUS ="+status;

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_INSTANSI WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_INSTANSI " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_INSTANSI>(inject_clause_select);
            
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.INSTANSI_CODE), 
                Convert.ToString(list.INSTANSI_NAME),
                //Convert.ToString((list.INSTANSI_STATUS == 0)?"<span class='red'>Tidak Aktif</span>":"<span class='red'>Aktif</span>"),
                Convert.ToString((list.INSTANSI_STATUS == 1)?"<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Instansi/Read/"+list.INSTANSI_ID+"'><i class='action fa fa-file-text-o'></i></a>"+"<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Master/Instansi/Edit/"+list.INSTANSI_ID+"'><i class='action fa fa-edit'></i></a>"+"<a data-original-title='Non-Aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Master/Instansi/Nonaktif/"+list.INSTANSI_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>":"<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Instansi/Read/"+list.INSTANSI_ID+"'><i class='action fa fa-file-text-o'></i></a>"+"<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Master/Instansi/Edit/"+list.INSTANSI_ID+"'><i class='action fa fa-edit'></i></a>"+"<a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Master/Instansi/Aktif/"+list.INSTANSI_ID+"'><i class='action glyphicon glyphicon-ok'></i></a></center>"),
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

    }
}
