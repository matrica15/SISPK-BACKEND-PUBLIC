using SISPK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SISPK.Helpers;
using SISPK.Filters;

namespace SISPK.Controllers.Master
{
    [Auth(RoleTipe = 1)]
    public class IcsController : Controller
    {
        //
        // GET: /Ics/
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListDataIcs(DataTables param)
        {
            var default_order = "ICS_CODE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("ICS_CODE");
            order_field.Add("ICS_NAME");
            order_field.Add("ICS_YEAR");
            //order_field.Add("ICS_STATUS");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "ASC" : param.sSortDir_0;
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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_ICS WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_ICS " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_ICS>(inject_clause_select);
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.ICS_CODE), 
                Convert.ToString(list.ICS_NAME_IND), 
                //Convert.ToString("<center>" + list.ICS_YEAR + "</center>"), 
                //Convert.ToString((list.ICS_STATUS == 0)?"<center class='red'>Tidak Aktif</center>":"<center class='red'>Aktif</center>"),
                Convert.ToString("<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='Ics/Read/"+list.ICS_ID+"'><i class='action fa fa-file-text-o'></i></a>"+
                Convert.ToString((list.ICS_STATUS == 1) ? "<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='Ics/Edit/"+list.ICS_ID+"'><i class='action fa fa-edit'></i></a>":"")+
                //Convert.ToString((list.ICS_STATUS == 1) ? "<a data-original-title='Non-aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='Ics/Nonaktif/"+list.ICS_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>" : "<a data-original-title='Hapus' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='Ics/Aktif/"+list.ICS_ID+"'><i class='action glyphicon glyphicon-check'></i></a>") +
                "</center>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        [Auth(RoleTipe = 2)]
        public ActionResult Create()
        {
            ViewData["listICS"] = (from t in db.MASTER_ICS where t.ICS_PARENT_CODE == null orderby t.ICS_CODE ascending select t).ToList();
            return View();
        }
        [HttpPost]
        public ActionResult Create(MASTER_ICS ics)
        {
            //return Content(ics.ICS_PARENT_CODE);
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_ICS");
            var datenow = MixHelper.ConvertDateNow();
            var fname = "ICS_ID," +
                        "ICS_PARENT_CODE," +
                        "ICS_CODE," +
                        "ICS_NAME," +
                        "ICS_NAME_IND," +
                        //"ICS_YEAR," +
                        "ICS_CREATE_BY," +
                        "ICS_CREATE_DATE," +
                        "ICS_STATUS," +
                        "ICS_LOG_CODE";
            var fvalue = "'" + lastid + "', " +
                        "'" + ics.ICS_PARENT_CODE + "', " +
                        "'" + ics.ICS_PARENT_CODE + "." + ics.ICS_CODE + "'," +
                        "'" + ics.ICS_NAME + "'," +
                        "'" + ics.ICS_NAME_IND + "'," +
                        //"'" + ics.ICS_YEAR + "'," +
                        "'" + UserId + "', " +
                        datenow + "," +
                        "1," +
                        "'" + logcode + "'";

            db.Database.ExecuteSqlCommand("INSERT INTO MASTER_ICS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");

        }


        public ActionResult Read(int id = 0) {
            ViewData["ics_item"] = (from t in db.VIEW_ICS where t.ICS_ID == id select t).SingleOrDefault();
            //VIEW_ICS ics_item = db.VIEW_ICS.SingleOrDefault(t => t.ICS_ID == id);
            ////return Json(new { data = bidang_item },JsonRequestBehavior.AllowGet);
            //if (ics_item == null)
            //{
            //    return HttpNotFound();
            //}
            return View();
        }
        [Auth(RoleTipe = 3)]
        public ActionResult Edit(int id = 0)
        {
            ViewData["ics_item"] = (from t in db.VIEW_ICS where t.ICS_ID == id select t).SingleOrDefault();
            //VIEW_ICS ics_item = db.VIEW_ICS.SingleOrDefault(t => t.ICS_ID == id);
            //if (ics_item == null)
            //{
            //    return HttpNotFound();
            //}
            return View();
        }

        [HttpPost]
        public ActionResult Edit(MASTER_ICS ics)
        {
            var UserId = Session["USER_ID"];
            var logcode = ics.ICS_LOG_CODE;
            var datenow = MixHelper.ConvertDateNow();
            var fupdate = "ICS_PARENT_CODE = '" + ics.ICS_PARENT_CODE + "'," +
                        "ICS_CODE = '" + ics.ICS_CODE + "'," +
                        "ICS_NAME = '" + ics.ICS_NAME + "'," +
                        "ICS_NAME_IND = '" + ics.ICS_NAME_IND + "'," +
                        //"ICS_YEAR = '" + ics.ICS_YEAR + "'," +
                        "ICS_UPDATE_BY = '" + UserId + "'," +
                        "ICS_UPDATE_DATE = " + datenow;

            db.Database.ExecuteSqlCommand("UPDATE MASTER_ICS SET " + fupdate + " WHERE ICS_ID = '"+ics.ICS_ID+"'");

            String objek = fupdate.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");

        }

        public ActionResult GetChild(string code = "")
        {
            int status = 1;
            var list = (from t in db.MASTER_ICS where t.ICS_PARENT_CODE == code && t.ICS_STATUS == 1 orderby t.ICS_CODE ascending select t).ToList();
            var result = from lists in list
                         select new string[] 
            { 
                Convert.ToString("<option value='"+lists.ICS_CODE+"'>"+lists.ICS_CODE + " " +lists.ICS_NAME+"</option>")
            };
            return Json(new
            {
                message = status,
                value = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckCode(string code = "")
        {
            int status = 1;
            var name = "";
            var list = (from t in db.MASTER_ICS where t.ICS_CODE == code select t).SingleOrDefault();
            if (list != null) {
                status = 0;
                name = list.ICS_NAME;
            }
            return Json(new
            {
                message = status,
                value = name
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
