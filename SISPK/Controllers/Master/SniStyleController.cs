using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Aspose.Words;

namespace SISPK.Controllers.Master
{
    
    public class SniStyleController : Controller
    {
        //
        // GET: /SniStyle/
        private SISPKEntities db = new SISPKEntities();

        [Auth(RoleTipe = 1)]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult listSNI(DataTables param)
        {
            var default_order = "SNI_STYLE_SORT";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_STYLE_NAME");
            order_field.Add("SNI_STYLE_VALUE");
            order_field.Add("SNI_STYLE_SORT");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "";

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM MASTER_SNI_STYLE WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  MASTER_SNI_STYLE " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<MASTER_SNI_STYLE>(inject_clause_select);

            //return Json(new
            //{
            //    SACE = inject_clause_count,
            //    sEcho = inject_clause_select
            //}, JsonRequestBehavior.AllowGet);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.SNI_STYLE_NAME), 
                Convert.ToString(list.SNI_STYLE_VALUE),
                Convert.ToString(list.SNI_STYLE_SORT),
                Convert.ToString("<center><a href='SniStyle/Detail/"+list.SNI_STYLE_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a><a href='SniStyle/Edit/"+list.SNI_STYLE_ID+"' class='btn yellow btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Edit'><i class='action fa fa-edit'></i></a></center>"),
            };
            return Json(new
            {
                SelectedData,
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        [Auth(RoleTipe = 2)]
        public ActionResult Create() {
            return View();
        }
        [HttpPost]
        public ActionResult Create(MASTER_SNI_STYLE mss) {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var LOGCODE = MixHelper.GetLogCode();
            int LASTID = MixHelper.GetSequence("MASTER_SNI_STYLE");
            var DATENOW = MixHelper.ConvertDateNow();

            var sort_max = db.Database.SqlQuery<Int32>("SELECT NVL(MAX(SNI_STYLE_SORT), 0) AS SNI_STYLE_SORT FROM MASTER_SNI_STYLE").SingleOrDefault();
            var sort = Convert.ToInt32(sort_max) + 1;

            var fname = "SNI_STYLE_ID,SNI_STYLE_NAME,SNI_STYLE_VALUE,SNI_STYLE_SORT,SNI_STYLE_CREATE_BY,SNI_STYLE_CREATE_DATE,SNI_STYLE_STATUS";
            var fvalue = "" + LASTID + ", " +
                        "'" + mss.SNI_STYLE_NAME + "', " +
                        "'" + mss.SNI_STYLE_VALUE + "', " +
                        "'" + sort + "', " +
                        "'" + USER_ID + "', " +
                        DATENOW + "," +
                        "1";

            db.Database.ExecuteSqlCommand("INSERT INTO MASTER_SNI_STYLE (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data SNI Berhasil di Publikasikan";
            return RedirectToAction("Index");
        }

        [Auth(RoleTipe = 3)]
        public ActionResult Edit(int id) {
            ViewData["data"] = (from s in db.MASTER_SNI_STYLE where s.SNI_STYLE_ID == id select s).SingleOrDefault();
            return View();
        }

        [HttpPost]
        public ActionResult Edit(MASTER_SNI_STYLE mss) {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var LOGCODE = MixHelper.GetLogCode();
            int LASTID = MixHelper.GetSequence("MASTER_SNI_STYLE");
            var DATENOW = MixHelper.ConvertDateNow();

            if (mss.SNI_STYLE_ID > 0)
            {

                var uPDATE = "UPDATE MASTER_SNI_STYLE SET SNI_STYLE_NAME = '" + mss.SNI_STYLE_NAME + "', SNI_STYLE_VALUE = '" + mss.SNI_STYLE_VALUE + "', SNI_STYLE_UPDATE_BY = " + LASTID + ", SNI_STYLE_UPDATE_DATE = " + DATENOW + " WHERE SNI_STYLE_ID = " + mss.SNI_STYLE_ID;
                db.Database.ExecuteSqlCommand(uPDATE);

                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data SNI Style Berhasil di Publikasikan";
            }
            else {
                TempData["Notifikasi"] = 2;
                TempData["NotifikasiText"] = "Data SNI Style gagal di Publikasikan";
            }

            return RedirectToAction("Index");
        }

        [Auth(RoleTipe = 1)]
        public ActionResult Detail(int id) {
            ViewData["data"] = (from s in db.MASTER_SNI_STYLE where s.SNI_STYLE_ID == id select s).SingleOrDefault();
            return View();        
        }

        public ActionResult Delete(int id = 0)
        {
            string query_update_group = "UPDATE MASTER_SNI_STYLE SET SNI_STYLE_STATUS = 0 WHERE SNI_STYLE_ID = '" + id + "'";
            db.Database.ExecuteSqlCommand(query_update_group);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di Nonaktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult Activated(int id = 0)
        {
            string query_update_group = "UPDATE MASTER_SNI_STYLE SET SNI_STYLE_STATUS = 1 WHERE SNI_STYLE_ID = '" + id + "'";
            db.Database.ExecuteSqlCommand(query_update_group);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di Aktifkan";
            return RedirectToAction("Index");
        }

    }
}
