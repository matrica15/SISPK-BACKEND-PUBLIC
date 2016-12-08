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


namespace SISPK.Controllers.Setting
{
    [Auth(RoleTipe = 1)]
    public class AksesRoleController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        //
        // GET: /Pengguna/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ListData(DataTables param, int status)
        {
            var default_order = "ACCESS_NAME";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("ACCESS_NAME");
            order_field.Add("JML_PENGGUNA");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "ACCESS_STATUS =  "+status;

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_ACCESS WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_ACCESS " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_ACCESS>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.ACCESS_NAME), 
                Convert.ToString("<center>" + list.JML_PENGGUNA + "</center>"), 
                Convert.ToString(
                //"<a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Read/"+list.ACCESS_ID+"'><i class='action fa fa-file-text-o'></i></a>"+
                "<center><a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Setting/AksesRole/Edit/"+list.ACCESS_ID+"'><i class='action fa fa-edit'></i></a>"+
                Convert.ToString((status == 1 && list.JML_PENGGUNA == 0) ? "<a data-original-title='Non Aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Setting/AksesRole/Delete/"+list.ACCESS_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>":"") +
                Convert.ToString((status == 0) ? "<a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Setting/AksesRole/Activated/"+list.ACCESS_ID+"'><i class='action glyphicon glyphicon-check'></i></a></center>":"")
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
            VIEW_ACCESS access = db.VIEW_ACCESS.Find(id);
            if (access == null)
            {
                return HttpNotFound();
            }
            ViewData["datamenu"] = db.Database.SqlQuery<VIEW_MENU_HIRARKI>("SELECT * FROM VIEW_MENUS").ToList();
            ViewData["datamenuemployee"] = db.SYS_ACCESS_DETAIL.SqlQuery("SELECT * FROM SYS_ACCESS_DETAIL where ACCESS_DETAIL_ACCESS_ID = '" + id + "'").ToList();
            return View(access);
        }

        [HttpPost]
        public ActionResult Edit(FormCollection formCollection)
        {
            
            if (ModelState.IsValid)
            {
                string MenuId = formCollection["menu_selector"];
                int AksesId = Convert.ToInt32(formCollection["access_id"]);
                string NamaAkses = formCollection["access_name"];
                int IndexPage = Convert.ToInt32(formCollection["indexpage"]);
                int UserId = Convert.ToInt32(Session["USER_ID"]);

                string query_reset = "UPDATE SYS_ACCESS_DETAIL SET ACCESS_DETAIL_STATUS = 0 WHERE ACCESS_DETAIL_ACCESS_ID = '" + AksesId + "'";
                db.Database.ExecuteSqlCommand(query_reset);

                string query_update_group = "UPDATE SYS_ACCESS SET ACCESS_NAME = '" + NamaAkses + "', ACCESS_UPDATE_DATE = SYSDATE, ACCESS_UPDATE_BY = " + UserId + " where ACCESS_ID = '" + AksesId + "'";
                db.Database.ExecuteSqlCommand(query_update_group);

                if (MenuId != null)
                {
                    string[] vals = MenuId.Split(',');
                    foreach (String val in vals)
                    {
                        string[] valval = val.Split('|');

                        int y = Convert.ToInt32(valval[0]);
                        int z = Convert.ToInt32(valval[1]);

                        var dataMenuEmployee = db.SYS_ACCESS_DETAIL.SqlQuery("SELECT * FROM SYS_ACCESS_DETAIL WHERE ACCESS_DETAIL_ACCESS_ID = '" + AksesId + "' AND ACCESS_DETAIL_MENU_ID = '" + y + "' AND ACCESS_DETAIL_TYPE = '" + z + "'").FirstOrDefault();
                        if (dataMenuEmployee != null)
                        {
                            string query_update = "UPDATE SYS_ACCESS_DETAIL SET ACCESS_DETAIL_STATUS = 1, ACCESS_DETAIL_UPDATE_DATE = SYSDATE, ACCESS_DETAIL_UPDATE_BY = " + UserId + " where ACCESS_DETAIL_ACCESS_ID = '" + AksesId + "' AND ACCESS_DETAIL_MENU_ID = '" + y + "' AND ACCESS_DETAIL_TYPE = '" + z + "'";
                            db.Database.ExecuteSqlCommand(query_update);
                        }
                        else
                        {
                            int lastid = MixHelper.GetSequence("SYS_ACCESS_DETAIL");
                            string query_insert = "INSERT INTO SYS_ACCESS_DETAIL (ACCESS_DETAIL_ID,ACCESS_DETAIL_ACCESS_ID,ACCESS_DETAIL_MENU_ID,ACCESS_DETAIL_TYPE,ACCESS_DETAIL_CREATE_DATE,ACCESS_CREATE_BY,ACCESS_DETAIL_STATUS) values (" + lastid + "," + AksesId + ", " + y + ", " + z + ", SYSDATE, " + UserId + ", 1)";
                            
                            db.Database.ExecuteSqlCommand(query_insert);

                        }
                    }
                }
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");

        }

        public ActionResult Create()
        {
            ViewData["datamenu"] = db.Database.SqlQuery<VIEW_MENU_HIRARKI>("SELECT * FROM VIEW_MENUS").ToList();
            ViewData["datamenuemployee"] = db.SYS_ACCESS_DETAIL.SqlQuery("SELECT * FROM SYS_ACCESS_DETAIL where ACCESS_DETAIL_ACCESS_ID = '" + 0 + "'").ToList();
            return View();
        }        

        [HttpPost]
        public ActionResult Create(FormCollection formCollection)
        {
            string MenuId = formCollection["menu_selector"];
            //int AksesId = Convert.ToInt32(formCollection["access_id"]);
            string NamaAkses = formCollection["access_name"];
            int IndexPage = Convert.ToInt32(formCollection["indexpage"]);
            int UserId = Convert.ToInt32(Session["USER_ID"]);

            int AksesId = MixHelper.GetSequence("SYS_ACCESS");
            string logcode = MixHelper.GetLogCode();
            string query_insert_access = "INSERT INTO SYS_ACCESS (ACCESS_ID,ACCESS_NAME,ACCESS_CREATE_DATE,ACCESS_CREATE_BY,ACCESS_STATUS,ACCESS_DEFAULT_MENU,ACCESS_LOG_CODE) values (" + AksesId + ",'" + NamaAkses + "',SYSDATE,'" + UserId + "', 1, 1,NULL)";
            db.Database.ExecuteSqlCommand(query_insert_access);
            //SYS_ACCESS model2 = new SYS_ACCESS { ACCESS_ID = AksesId, ACCESS_CREATE_DATE = DateTime.Now, ACCESS_CREATE_BY = UserId, ACCESS_STATUS = 1, ACCESS_DEFAULT_MENU = 1, ACCESS_LOG_CODE = logcode };
           
            if (MenuId != null)
            {
                string[] vals = MenuId.Split(',');
                foreach (String val in vals)
                {
                    string[] valval = val.Split('|');

                    int y = Convert.ToInt32(valval[0]);
                    int z = Convert.ToInt32(valval[1]);
                    int lastid = MixHelper.GetSequence("SYS_ACCESS_DETAIL");
                    string query_insert = "INSERT INTO SYS_ACCESS_DETAIL (ACCESS_DETAIL_ID,ACCESS_DETAIL_ACCESS_ID,ACCESS_DETAIL_MENU_ID,ACCESS_DETAIL_TYPE,ACCESS_DETAIL_CREATE_DATE,ACCESS_CREATE_BY,ACCESS_DETAIL_STATUS) values (" + lastid + "," + AksesId + ", " + y + ", " + z + ", SYSDATE, " + UserId + ", 1)";
                    db.Database.ExecuteSqlCommand(query_insert);

                }
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id = 0)
        {
            string query_update_group = "UPDATE SYS_ACCESS SET ACCESS_STATUS = 0 WHERE ACCESS_ID = '" + id + "'";
            db.Database.ExecuteSqlCommand(query_update_group);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di Nonaktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult Activated(int id = 0)
        {
            string query_update_group = "UPDATE SYS_ACCESS SET ACCESS_STATUS = 1 WHERE ACCESS_ID = '" + id + "'";
            db.Database.ExecuteSqlCommand(query_update_group);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di Aktifkan";
            return RedirectToAction("Index");
        }
       

    }
}
