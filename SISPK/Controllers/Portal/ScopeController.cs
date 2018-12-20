using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;

namespace SISPK.Controllers.Portal
{
    [Auth(RoleTipe = 1)]
    public class ScopeController : Controller
    {
        //
        // GET: /Scope/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Scope/Read/5

        public ActionResult Read(int id = 0)
        {
            ViewData["dt_scope"] = (from t in db.MASTER_SCOPE where t.SCOPE_ID == id select t).SingleOrDefault();
            
            return View();
        }

        //
        // GET: /Scope/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Scope/Create

        [HttpPost]
        public ActionResult Create(MASTER_SCOPE dt_form)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_SCOPE");
            var datenow = MixHelper.ConvertDateNow();

            var fname = "SCOPE_ID,SCOPE_CODE,SCOPE_NAME,SCOPE_CREATE_BY,SCOPE_CREATE_DATE,SCOPE_STATUS";
            var fvalue = "'" + lastid + "', " +
                        "'" + dt_form.SCOPE_CODE + "', " +
                        "'" + dt_form.SCOPE_NAME + "', " +
                        "'" + UserId + "'," +
                        datenow + "," +
                        "1";

            db.Database.ExecuteSqlCommand("INSERT INTO MASTER_SCOPE (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";

            return RedirectToAction("Index");
        }

        //
        // GET: /Scope/Edit/5
        [Auth(RoleTipe = 3)]
        public ActionResult Edit(int id)
        {
            ViewData["dt_scope"] = (from t in db.MASTER_SCOPE where t.SCOPE_ID == id select t).SingleOrDefault();

            return View();
        }

        //
        // POST: /Scope/Edit/5

        [HttpPost]
        public ActionResult Edit(MASTER_SCOPE dt_form)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            var datenow = MixHelper.ConvertDateNow();
            var status = "1";

            var update =
                    " SCOPE_CODE = '" + dt_form.SCOPE_CODE + "'," +
                    " SCOPE_NAME = '" + dt_form.SCOPE_NAME + "'," +
                    " SCOPE_UPDATE_BY = '" + UserId + "'," +
                    " SCOPE_UPDATE_DATE = " + datenow + "," +
                    " SCOPE_STATUS = '" + status + "'";

            var clause = "where SCOPE_ID = " + dt_form.SCOPE_ID;
            //return Json(new { query = "UPDATE MASTER_BIDANG SET " + update.Replace("''", "NULL") + " " + clause }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE MASTER_SCOPE SET " + update.Replace("''", "NULL") + " " + clause);

            String objek = update.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Nonaktif(int id = 0)
        {

            db.Database.ExecuteSqlCommand("UPDATE MASTER_SCOPE SET SCOPE_STATUS = 0 WHERE SCOPE_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");

        }

        public ActionResult Aktif(int id = 0)
        {
            db.Database.ExecuteSqlCommand("UPDATE MASTER_SCOPE SET SCOPE_STATUS = 1 WHERE SCOPE_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Aktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult ListDataScope(DataTables param, int status = 0)
        {
            var default_order = "SCOPE_CODE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SCOPE_CODE");
            order_field.Add("SCOPE_NAME");
            order_field.Add("SCOPE_STATUS");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "ASC" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "SCOPE_STATUS = " + status;

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM MASTER_SCOPE WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  MASTER_SCOPE " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<MASTER_SCOPE>(inject_clause_select);
            var result = from list in SelectedData
                         select new string[]
            {
                Convert.ToString(list.SCOPE_CODE),
                Convert.ToString(list.SCOPE_NAME), 
                //Convert.ToString((list.BIDANG_STATUS == 0)?"<span class='red'>Tidak Aktif</span>":"<span class='red'>Aktif</span>"),
                Convert.ToString((list.SCOPE_STATUS == 1)?"<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Portal/Scope/Read/"+list.SCOPE_ID+"'><i class='action fa fa-file-text-o'></i></a>"+"<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Portal/Scope/Edit/"+list.SCOPE_ID+"'><i class='action fa fa-edit'></i></a>"+"<a data-original-title='Non-aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Portal/Scope/Nonaktif/"+list.SCOPE_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>":"<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Portal/Scope/Read/"+list.SCOPE_ID+"'><i class='action fa fa-file-text-o'></i></a>"+"<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Portal/Scope/Edit/"+list.SCOPE_ID+"'><i class='action fa fa-edit'></i></a>"+"<a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Portal/Scope/Aktif/"+list.SCOPE_ID+"'><i class='action glyphicon glyphicon-ok'></i></a></center>"),
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
