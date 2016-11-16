using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.Security.Cryptography;
using System.IO;

namespace SISPK.Controllers.Portal
{
    public class NewsController : Controller
    {
        //
        // GET: /News/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create() {
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Create(VIEW_NEWS news)
        {

            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("PORTAL_NEWS");
            var datenow = MixHelper.ConvertDateNow();

            string path = Server.MapPath("~/Upload/Dokumen/LAINNYA/news/");
            HttpPostedFileBase file_att = Request.Files["image"];
            var file_name_att = "";
            var filePath = "";
            if (file_att != null)
            {
                string imagenews = file_att.FileName;
                if (imagenews.Trim() != "")
                {
                    imagenews = Path.GetFileNameWithoutExtension(file_att.FileName);
                    string fileExtension = Path.GetExtension(file_att.FileName);
                    file_name_att = "IMAGE_NEWS_" + lastid + fileExtension;
                    filePath = path + file_name_att;
                    file_att.SaveAs(filePath);
                }
            }
            var link = "http://localhost:4878/Upload/Dokumen/LAINNYA/news/";
            var PATS = "Upload/Dokumen/LAINNYA/news/";

            var fname = "NEWS_ID, NEWS_TITLE,NEWS_CONTENTS,NEWS_SUMBER, NEWS_TAG, NEWS_PATH_IMAGE, NEWS_LINK_IMAGE, NEWS_BRIEF_NEWS, NEWS_CREATE_BY, NEWS_LOG_CODE, NEWS_CREATE_DATE,NEWS_STATUS";
            var fvalue = "'" + lastid + "'," +
                         "'" + news.NEWS_TITLE + "'," +
                         "'" + news.NEWS_CONTENTS + "'," +
                         "'" + news.NEWS_SUMBER + "'," +
                         "'" + news.NEWS_TAG + "'," +
                         "'" + PATS+file_name_att + "'," +
                         "'" + link + file_name_att + "'," +
                         "'" + news.NEWS_BRIEF_NEWS + "'," +
                         "'" + UserId + "'," +
                         "'" + logcode + "'," +
                         datenow + "," +
                         "1";

            //return Json(new { query = "INSERT INTO MASTER_BIDANG (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("INSERT INTO PORTAL_NEWS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id = 0)
        {            
            var DataNews = (from news in db.VIEW_NEWS where news.NEWS_ID == id select news).SingleOrDefault();
            ViewData["datanews"] = DataNews;
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Edit(PORTAL_NEWS news) {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();            
            var datenow = MixHelper.ConvertDateNow();
            var status = "1";
            var link = "http://localhost:4878/Upload/Dokumen/LAINNYA/news/";

            string path = Server.MapPath("~/Upload/Dokumen/LAINNYA/news/");
            HttpPostedFileBase file_att = Request.Files["Image"];
            var file_name_att = "";
            var filePath = "";
            var location = "Upload/Dokumen/LAINNYA/news/";
            var update = "";
          
                string lampirananggotapath = file_att.FileName;
                if (lampirananggotapath.Trim() != "")
                {
                    lampirananggotapath = Path.GetFileNameWithoutExtension(file_att.FileName);
                    string fileExtension = Path.GetExtension(file_att.FileName);
                    file_name_att = "IMAGE_NEWS_" + news.NEWS_ID + fileExtension;
                    filePath = path + file_name_att;
                    file_att.SaveAs(filePath);

                    update =
                             " NEWS_TITLE = '" + news.NEWS_TITLE + "'," +
                             " NEWS_CONTENTS = '" + news.NEWS_CONTENTS + "'," +
                             " NEWS_SUMBER = '" + news.NEWS_SUMBER + "'," +
                             " NEWS_BRIEF_NEWS = '" + news.NEWS_BRIEF_NEWS + "'," +
                             " NEWS_TAG = '" + news.NEWS_TAG + "'," +
                             " NEWS_PATH_IMAGE = '" + location + file_name_att + "'," +
                             " NEWS_LINK_IMAGE = '" + link + file_name_att + "'," +
                             " NEWS_UPDATE_BY = '" + UserId + "'," +
                             " NEWS_UPDATE_DATE = " + datenow + "," +
                             " NEWS_LOG_CODE = '" + logcode + "', " +
                             " NEWS_STATUS = '" + status + "'";
                }

                else {
                    update =
                            " NEWS_TITLE = '" + news.NEWS_TITLE + "'," +
                            " NEWS_CONTENTS = '" + news.NEWS_CONTENTS + "'," +
                            " NEWS_SUMBER = '" + news.NEWS_SUMBER + "'," +
                            " NEWS_BRIEF_NEWS = '" + news.NEWS_BRIEF_NEWS + "'," +
                            " NEWS_TAG = '" + news.NEWS_TAG + "'," +
                            " NEWS_UPDATE_BY = '" + UserId + "'," +
                            " NEWS_UPDATE_DATE = " + datenow + "," +
                            " NEWS_LOG_CODE = '" + logcode + "', " +
                            " NEWS_STATUS = '" + status + "'";
                    } 
            var clause = "where NEWS_ID = " + news.NEWS_ID;
            //return Json(new { query = "UPDATE PORTAL_NEWS SET " + update.Replace("''", "NULL") + " " + clause }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE PORTAL_NEWS SET " + update.Replace("''", "NULL") + " " + clause);

            //var logId = AuditTrails.GetLogId();
            String objek = update.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Read(int id = 0)
        {
            var DataNews = (from news in db.VIEW_NEWS where news.NEWS_ID == id select news).SingleOrDefault();
            ViewData["datanews"] = DataNews;
            return View();
        }

        public ActionResult Aktif(int id = 0) {
            return View();
        }

        public ActionResult Nonaktif(int id = 0) {
            return View();
        }

        public ActionResult ListData(DataTables param, int status = 0)
        {
            var default_order = "NEWS_CREATE_DATE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("NEWS_CREATE_DATE");
            order_field.Add("NEWS_TITLE");
            order_field.Add("NEWS_PATH_IMAGE");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "asc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "NEWS_STATUS = " + status;

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_NEWS WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_NEWS " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_NEWS>(inject_clause_select);
            //int no = 1;
            var result = from list in SelectedData
                         select new string[] 
            {                 
                Convert.ToString(list.NEWS_CREATE_DATE),
                Convert.ToString(list.NEWS_TITLE), 
                Convert.ToString("<img src='../"+list.NEWS_PATH_IMAGE+"' width='100'>"),               
                (list.NEWS_STATUS == 0)?"<center><a data-original-title='Read' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Portal/News/Read/"+list.NEWS_ID+"'><i class='action fa-file-text-o'></i></a><a data-original-title='Edit' data-placement='top' data-container='body' class='btn yellow btn-sm action tooltips' href='/Portal/News/Edit/"+list.NEWS_ID+"'><i class='action fa fa-edit'></i></a><a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Portal/News/aktifkan/"+list.NEWS_ID+"'><i class='action glyphicon glyphicon-ok'></i></a></center>":"<center><a data-original-title='Read' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Portal/News/Read/"+list.NEWS_ID+"'><i class='action fa fa-file-text-o'></i></a><a data-original-title='Edit' data-placement='top' data-container='body' class='btn yellow btn-sm action tooltips' href='/Portal/News/Edit/"+list.NEWS_ID+"'><i class='action fa fa-edit'></i></a><a data-original-title='Nonaktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Portal/News/nonaktifkan/"+list.NEWS_ID+"'><i class='action fa fa-times'></i></a></center>"
            };
            
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult aktifkan(int id = 0)
        {
            db.Database.ExecuteSqlCommand("UPDATE PORTAL_NEWS SET NEWS_STATUS = 1 WHERE NEWS_ID = " + id);            
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Aktifkan";
            return RedirectToAction("Index");
        }
        public ActionResult nonaktifkan(int id = 0)
        {
            db.Database.ExecuteSqlCommand("UPDATE PORTAL_NEWS SET NEWS_STATUS = 0 WHERE NEWS_ID = " + id);            
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");
        }

    }
}
