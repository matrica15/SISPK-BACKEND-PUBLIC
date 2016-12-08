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
    public class BidangController : Controller
    {
        //
        // GET: /Bidang/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read(int id = 0)
        {
            ViewData["bidang_item"] = (from t in db.VIEW_BIDANG where t.BIDANG_ID == id select t).SingleOrDefault();
            /*VIEW_BIDANG bidang_item = db.VIEW_BIDANG.SingleOrDefault(t => t.BIDANG_ID == id);
            //return Json(new { data = bidang_item },JsonRequestBehavior.AllowGet);
            if (bidang_item == null)
            {
                return HttpNotFound();
            }*/
            return View();
        }
        [Auth(RoleTipe = 2)]
        public ActionResult Create() {
            return View();
        }

        [HttpPost]
        public ActionResult Create(MASTER_BIDANG master_bidang)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_BIDANG");
            var datenow = MixHelper.ConvertDateNow();

            var fname = "BIDANG_ID,BIDANG_CODE,BIDANG_NAME,BIDANG_SHORT_NAME,BIDANG_CREATE_BY,BIDANG_CREATE_DATE,BIDANG_LOG_CODE,BIDANG_STATUS";
            var fvalue = "'" + lastid + "', " +
                        "'" + master_bidang.BIDANG_CODE + "', " +
                        "'" + master_bidang.BIDANG_NAME + "', " +
                        "'" + master_bidang.BIDANG_SHORT_NAME + "', " +
                        "'" + UserId + "'," +
                        datenow + "," +
                        "'" + logcode + "'," +
                        "1";
            //return Json(new { query = "INSERT INTO MASTER_BIDANG (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
           db.Database.ExecuteSqlCommand("INSERT INTO MASTER_BIDANG (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        [Auth(RoleTipe = 3)]
        public ActionResult Edit(int id = 0)
        {
            ViewData["bidang_item"] = (from t in db.VIEW_BIDANG where t.BIDANG_ID == id select t).SingleOrDefault();
            /*VIEW_BIDANG bidang_item = db.VIEW_BIDANG.SingleOrDefault(t => t.BIDANG_ID == id);
            if (bidang_item == null)
            {
                return HttpNotFound();
            }*/
            return View();
        }

        public ActionResult Nonaktif(int id = 0) {

            db.Database.ExecuteSqlCommand("UPDATE MASTER_BIDANG SET BIDANG_STATUS = 0 WHERE BIDANG_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");

        }

        public ActionResult Aktif(int id = 0)
        {
            db.Database.ExecuteSqlCommand("UPDATE MASTER_BIDANG SET BIDANG_STATUS = 1 WHERE BIDANG_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Edit(MASTER_BIDANG master_bidang) {

            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_BIDANG");
            var datenow = MixHelper.ConvertDateNow();
            var status = "1";

               var update =
                            " BIDANG_CODE = '" + master_bidang.BIDANG_CODE + "'," +
                            " BIDANG_NAME = '" + master_bidang.BIDANG_NAME + "'," +
                            " BIDANG_SHORT_NAME = '" + master_bidang.BIDANG_SHORT_NAME + "'," +
                            " BIDANG_UPDATE_BY = '" + UserId + "'," +
                            " BIDANG_UPDATE_DATE = " + datenow + "," +
                            " BIDANG_LOG_CODE = '" + logcode + "', " +
                            " BIDANG_STATUS = '" + status + "'";


                var clause = "where BIDANG_ID = " + master_bidang.BIDANG_ID;
                //return Json(new { query = "UPDATE MASTER_BIDANG SET " + update.Replace("''", "NULL") + " " + clause }, JsonRequestBehavior.AllowGet);
                db.Database.ExecuteSqlCommand("UPDATE MASTER_BIDANG SET " + update.Replace("''", "NULL") + " " + clause);

                //var logId = AuditTrails.GetLogId();
                String objek = update.Replace("'", "-");
                MixHelper.InsertLog(logcode, objek, 1);
                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data Berhasil Disimpan";
                return RedirectToAction("Index");

        }

        public ActionResult ListDataBidang(DataTables param, int status)
        {
            var default_order = "BIDANG_CODE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("BIDANG_CODE");
            order_field.Add("BIDANG_NAME");
            order_field.Add("BIDANG_SHORT_NAME");
            order_field.Add("BIDANG_STATUS");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "BIDANG_STATUS = "+status;

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_BIDANG WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_BIDANG " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_BIDANG>(inject_clause_select);           
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.BIDANG_CODE), 
                Convert.ToString(list.BIDANG_NAME), 
                Convert.ToString(list.BIDANG_SHORT_NAME), 
                //Convert.ToString((list.BIDANG_STATUS == 0)?"<span class='red'>Tidak Aktif</span>":"<span class='red'>Aktif</span>"),
                Convert.ToString((list.BIDANG_STATUS == 1)?"<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Bidang/Read/"+list.BIDANG_ID+"'><i class='action fa fa-file-text-o'></i></a>"+"<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Master/Bidang/Edit/"+list.BIDANG_ID+"'><i class='action fa fa-edit'></i></a>"+"<a data-original-title='Non-aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Master/Bidang/Nonaktif/"+list.BIDANG_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>":"<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Bidang/Read/"+list.BIDANG_ID+"'><i class='action fa fa-file-text-o'></i></a>"+"<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Master/Bidang/Edit/"+list.BIDANG_ID+"'><i class='action fa fa-edit'></i></a>"+"<a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Master/Bidang/Aktif/"+list.BIDANG_ID+"'><i class='action glyphicon glyphicon-ok'></i></a></center>"),
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
