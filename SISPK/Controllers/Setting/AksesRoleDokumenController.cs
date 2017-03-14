using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;

namespace SISPK.Controllers.Setting
{
    public class AksesRoleDokumenController : Controller
    {
        //
        // GET: /AksesRoleDokumen/
        private SISPKEntities db = new SISPKEntities();

        [Auth(RoleTipe = 1)]
        public ActionResult Index()
        {
            return View();
        }

        [Auth(RoleTipe = 2)]
        public ActionResult Create()
        {
            ViewData["master_sni_style"] = db.Database.SqlQuery<MASTER_SNI_STYLE>("SELECT * FROM MASTER_SNI_STYLE where SNI_STYLE_STATUS = 1").ToList();
            ViewData["SYS_DOC_ACCESS_DETAIL"] = db.SYS_DOC_ACCESS_DETAIL.SqlQuery("SELECT * FROM SYS_DOC_ACCESS_DETAIL where DOC_ACCESS_DETAIL_ACCESS_ID = '" + 0 + "'").ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Create(int[] SNI_STYLE_ID, int[] status, string access_name = "")
        {
            int UserId = Convert.ToInt32(Session["USER_ID"]);

            int DocAksesId = MixHelper.GetSequence("SYS_DOC_ACCESS");
            string logcode = MixHelper.GetLogCode();
            var datenow = MixHelper.ConvertDateNow();

            if (SNI_STYLE_ID.Count() > 0)
            {
                var fname1 = "DOC_ACCESS_ID,DOC_ACCESS_NAME,DOC_ACCESS_CREATE_BY,DOC_ACCESS_CREATE_DATE,DOC_ACCESS_STATUS";
                var fvalue1 = "'" + DocAksesId + "', " +
                           "'" + access_name + "', " +
                           "'" + UserId + "', " +
                           datenow + "," +
                           "1";

                db.Database.ExecuteSqlCommand("INSERT INTO SYS_DOC_ACCESS (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")");

                int no = 0;
                foreach (var a in SNI_STYLE_ID)
                {
                    int DocAksesDetailId = MixHelper.GetSequence("SYS_DOC_ACCESS_DETAIL");
                    var fname = "DOC_ACCESS_DETAIL_ID,DOC_ACCESS_DETAIL_ACCESS_ID,DOC_ACCESS_DETAIL_STYLE_ID,DOC_ACCESS_DETAIL_CREATE_BY,DOC_ACCESS_DETAIL_CREATE_DATE,DOC_ACCESS_DETAIL_STYLE_STATUS";
                    var fvalue = "'" + DocAksesDetailId + "', " +
                               "'" + DocAksesId + "', " +
                               "" + SNI_STYLE_ID[no] + ", " +
                               "'" + UserId + "', " +
                               datenow + "," +
                               "" + status[no] + "";
                    no++;
                    //return Json(new
                    //{
                    //    sEcho = "INSERT INTO SYS_DOC_ACCESS_DETAIL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")"

                    //}, JsonRequestBehavior.AllowGet);
                    db.Database.ExecuteSqlCommand("INSERT INTO SYS_DOC_ACCESS_DETAIL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");
                }


                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            }
            else {
                TempData["Notifikasi"] = 2;
                TempData["NotifikasiText"] = "Data Gagal Disimpan";
            }
            
            return RedirectToAction("Index");
        }

        [Auth(RoleTipe = 1)]
        public ActionResult Detail(int id)
        {
            ViewData["master_sni_style"] = db.Database.SqlQuery<MASTER_SNI_STYLE>("SELECT * FROM MASTER_SNI_STYLE where SNI_STYLE_STATUS = 1").ToList();
            ViewData["SYS_DOC_ACCESS_DETAIL"] = db.SYS_DOC_ACCESS_DETAIL.SqlQuery("SELECT * FROM SYS_DOC_ACCESS_DETAIL where DOC_ACCESS_DETAIL_ACCESS_ID = '" + 0 + "'").ToList();
            return View();
        }

        [Auth(RoleTipe = 3)]
        public ActionResult Edit(int id)
        {
            ViewData["master_sni_style"] = db.Database.SqlQuery<MASTER_SNI_STYLE>("SELECT * FROM MASTER_SNI_STYLE where SNI_STYLE_STATUS = 1").ToList();
            ViewData["SYS_DOC_ACCESS_DETAIL"] = db.Database.SqlQuery<SYS_DOC_ACCESS_DETAIL>("SELECT * FROM SYS_DOC_ACCESS_DETAIL where DOC_ACCESS_DETAIL_ACCESS_ID = '" + id + "'").ToList();
            ViewData["doc_access"] = db.Database.SqlQuery<SYS_DOC_ACCESS>("SELECT * FROM SYS_DOC_ACCESS WHERE DOC_ACCESS_ID = "+id).SingleOrDefault();
            ViewData["id_doc_access"] = id;
            //return Json(new
            //{
            //    sEcho = ViewData["SYS_DOC_ACCESS_DETAIL"]
            //}, JsonRequestBehavior.AllowGet);
            return View();
        }

        [HttpPost]
        public ActionResult Edit(int id, int[] SNI_STYLE_ID, int[] status, string access_name = "")
        {
            int UserId = Convert.ToInt32(Session["USER_ID"]);

            int DocAksesId = MixHelper.GetSequence("SYS_DOC_ACCESS");
            string logcode = MixHelper.GetLogCode();
            var datenow = MixHelper.ConvertDateNow();
            if (SNI_STYLE_ID.Count() > 0)
            {
                var update_doc_access = "UPDATE SYS_DOC_ACCESS SET DOC_ACCESS_NAME = '" + access_name + "', DOC_ACCESS_UPDATE_BY = " + UserId + ", DOC_ACCESS_UPDATE_DATE = " + datenow + " WHERE DOC_ACCESS_ID = " + id;
                db.Database.ExecuteSqlCommand(update_doc_access);

                //return Json(new
                //{
                //    sEcho = update_doc_access
                //}, JsonRequestBehavior.AllowGet);

                int no = 0;
                foreach (var a in SNI_STYLE_ID)
                {
                    //var updateted = "UPDATE SYS_DOC_ACCESS_DETAIL SET DOC_ACCESS_DETAIL_STYLE_STATUS = " + status[no] + " WHERE DOC_ACCESS_DETAIL_STYLE_ID = " + SNI_STYLE_ID[no] + " AND DOC_ACCESS_DETAIL_ACCESS_ID = " + id;
                    //db.Database.ExecuteSqlCommand(updateted);

                    var dataDocRole = db.SYS_DOC_ACCESS_DETAIL.SqlQuery("SELECT * FROM SYS_DOC_ACCESS_DETAIL WHERE DOC_ACCESS_DETAIL_ACCESS_ID = " + id + " AND DOC_ACCESS_DETAIL_STYLE_ID = " + SNI_STYLE_ID[no]).FirstOrDefault();
                    if (dataDocRole != null)
                    {
                        var updateted = "UPDATE SYS_DOC_ACCESS_DETAIL SET DOC_ACCESS_DETAIL_STYLE_STATUS = " + status[no] + " WHERE DOC_ACCESS_DETAIL_STYLE_ID = " + SNI_STYLE_ID[no] + " AND DOC_ACCESS_DETAIL_ACCESS_ID = " + id;
                        db.Database.ExecuteSqlCommand(updateted);
                        //string query_update = "UPDATE SYS_ACCESS_DETAIL SET ACCESS_DETAIL_STATUS = 1, ACCESS_DETAIL_UPDATE_DATE = SYSDATE, ACCESS_DETAIL_UPDATE_BY = " + UserId + " where ACCESS_DETAIL_ACCESS_ID = '" + AksesId + "' AND ACCESS_DETAIL_MENU_ID = '" + y + "' AND ACCESS_DETAIL_TYPE = '" + z + "'";
                        //db.Database.ExecuteSqlCommand(query_update);
                    }
                    else
                    {
                        //int lastid = MixHelper.GetSequence("SYS_DOC_ACCESS_DETAIL");
                        //string query_insert = "INSERT INTO SYS_DOC_ACCESS_DETAIL (ACCESS_DETAIL_ID,ACCESS_DETAIL_ACCESS_ID,ACCESS_DETAIL_MENU_ID,ACCESS_DETAIL_TYPE,ACCESS_DETAIL_CREATE_DATE,ACCESS_CREATE_BY,ACCESS_DETAIL_STATUS) values (" + lastid + "," + AksesId + ", " + y + ", " + z + ", SYSDATE, " + UserId + ", 1)";

                        //db.Database.ExecuteSqlCommand(query_insert);
                        int DocAksesDetailId = MixHelper.GetSequence("SYS_DOC_ACCESS_DETAIL");
                        var fname = "DOC_ACCESS_DETAIL_ID,DOC_ACCESS_DETAIL_ACCESS_ID,DOC_ACCESS_DETAIL_STYLE_ID,DOC_ACCESS_DETAIL_CREATE_BY,DOC_ACCESS_DETAIL_CREATE_DATE,DOC_ACCESS_DETAIL_STYLE_STATUS";
                        var fvalue = "'" + DocAksesDetailId + "', " +
                                   "'" + id + "', " +
                                   "" + SNI_STYLE_ID[no] + ", " +
                                   "'" + UserId + "', " +
                                   datenow + "," +
                                   "" + status[no] + "";

                        db.Database.ExecuteSqlCommand("INSERT INTO SYS_DOC_ACCESS_DETAIL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");
                    }

                    no++;
                }

                

                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            }
            else {
                TempData["Notifikasi"] = 2;
                TempData["NotifikasiText"] = "Data Gagal Disimpan";
            }

            
            return RedirectToAction("Index");
        }

        public ActionResult ListData(DataTables param, int status)
        {
            var default_order = "DOC_ACCESS_NAME";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("DOC_ACCESS_NAME");
            order_field.Add("JML_ACCES");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "DOC_ACCESS_STATUS =  " + status;

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_DOC_ACCESS WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_DOC_ACCESS " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_DOC_ACCESS>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.DOC_ACCESS_NAME), 
                Convert.ToString("<center>" + list.JML_ACCES + "</center>"), 
                Convert.ToString(
                //"<a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Read/"+list.ACCESS_ID+"'><i class='action fa fa-file-text-o'></i></a>"+
                "<center><a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Setting/AksesRoleDokumen/Edit/"+list.DOC_ACCESS_ID+"'><i class='action fa fa-edit'></i></a>"+
                Convert.ToString((status == 1 && list.JML_ACCES == 0) ? "<a data-original-title='Non Aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Setting/AksesRoleDokumen/Delete/"+list.DOC_ACCESS_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>":"") +
                Convert.ToString((status == 0) ? "<a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='href='/Setting/AksesRoleDokumen/Activated/"+list.DOC_ACCESS_ID+"''><i class='action glyphicon glyphicon-check'></i></a></center>":"")
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

        public ActionResult Delete(int id = 0)
        {
            string query_update_group = "UPDATE SYS_DOC_ACCESS SET DOC_ACCESS_STATUS = 0 WHERE DOC_ACCESS_ID = '" + id + "'";
            db.Database.ExecuteSqlCommand(query_update_group);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di Nonaktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult Activated(int id = 0)
        {
            string query_update_group = "UPDATE SYS_DOC_ACCESS SET DOC_ACCESS_STATUS = 1 WHERE DOC_ACCESS_ID = '" + id + "'";
            db.Database.ExecuteSqlCommand(query_update_group);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di Aktifkan";
            return RedirectToAction("Index");
        }
    }
}
