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


namespace SISPK.Controllers.CONFIG
{
    [Auth(RoleTipe = 1)]
    public class AplikasiController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        //
        // GET: /Aplikasi/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListData(DataTables param)
        {
            var default_order = "CONFIG_NAME";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("CONFIG_NAME");
            order_field.Add("CONFIG_VALUE");

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM SYS_CONFIG WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM SYS_CONFIG " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<SYS_CONFIG>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.CONFIG_NAME), 
                Convert.ToString(list.CONFIG_VALUE), 
                Convert.ToString(
                //"<a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Read/"+list.ACCESS_ID+"'><i class='action fa fa-file-text-o'></i></a>"+
                "<center><a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Setting/Aplikasi/Edit/"+list.CONFIG_ID+"'><i class='action fa fa-edit'></i></a>"
                
                ),
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id = 0)
        {
            SYS_CONFIG config = db.SYS_CONFIG.Find(id);
            if (config == null)
            {
                return HttpNotFound();
            }
            return View(config);
        }

        [HttpPost]
        public ActionResult Edit(SYS_CONFIG config)
        {
            var UserId = Session["USER_ID"];
            var datenow = MixHelper.ConvertDateNow();
            var logcode = config.CONFIG_LOG_CODE;
            var fupdate = "CONFIG_NAME = '" + config.CONFIG_NAME + "'," +
                        "CONFIG_VALUE = '" + config.CONFIG_VALUE + "'," +
                        "CONFIG_UPDATE_BY = '" + UserId + "'," +
                        "CONFIG_UPDATE_DATE = " + datenow;
            db.Database.ExecuteSqlCommand("UPDATE SYS_CONFIG SET " + fupdate + " WHERE CONFIG_ID = " + config.CONFIG_ID);

            String objek = fupdate.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");

        }

    }
}
