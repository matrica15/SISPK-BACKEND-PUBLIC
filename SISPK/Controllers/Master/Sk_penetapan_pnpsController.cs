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
    public class Sk_penetapan_pnpsController : Controller
    {
        //
        // GET: /Sk_penetapan_pnps/
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult create(TRX_SK_PENETAPAN INPUT, string TANGGAL_SK = "")
        {

            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("SK_P_PNPS");
            var datenow = MixHelper.ConvertDateNow();

            string pathnya = Server.MapPath("~/Upload/Dokumen/SK_PENETAPAN_PNPS/");
            HttpPostedFileBase file_paten = Request.Files["FILES"];
            var file_name_paten = "";
            var filePath_paten = "";
            var fileExtension_paten = "";
            if (file_paten != null)
            {
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(pathnya))
                {
                    //If Directory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(pathnya);
                }
                string lampiranregulasipath = file_paten.FileName;
                if (lampiranregulasipath.Trim() != "")
                {
                    lampiranregulasipath = Path.GetFileNameWithoutExtension(file_paten.FileName);
                    fileExtension_paten = Path.GetExtension(file_paten.FileName);
                    file_name_paten = "SK_PENETAPAN_PNPS_" + lastid + fileExtension_paten;
                    filePath_paten = pathnya + file_name_paten.Replace(" ", "_");
                    file_paten.SaveAs(filePath_paten);
                }
            }

            var fname = "PENETAPAN_ID,PENETAPAN_NO_SK,TANGGAL_SK,JUDUL_SK,SK_LOCATION,FILES,CREATE_DATE,CREATE_BY"; 
            var fvalueS = ""+ lastid + ","+
                          "'" + INPUT.PENETAPAN_NO_SK + "'," +
                          "TO_DATE ('" + TANGGAL_SK + "','yyyy-mm-dd hh24:mi:ss')," +
                          "'" + INPUT.JUDUL_SK + "'," +
                          "'/Upload/Dokumen/SK_PENETAPAN_PNPS/'," +
                          "'" + file_name_paten.Replace(" ", "_") + "'," +
                          "" + datenow + "," +
                          "'" + UserId + "'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_SK_PENETAPAN (" + fname + ") VALUES (" + fvalueS.Replace("''", "NULL") + ")");

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";


            return RedirectToAction("Index");

            //var TT = ("INSERT INTO TRX_SK_PENETAPAN (" + fname + ") VALUES (" + fvalueS.Replace("''", "NULL") + ")");
            //return Content(TT);
        }

        public ActionResult listSkPNPS(DataTables param)
        {
            var default_order = "CREATE_DATE";
            var limit = 10;
            var user_akses = Session["USER_ACCESS_ID"];
            var KomtekID = Session["KOMTEK_ID"];

            List<string> order_field = new List<string>();
            order_field.Add("PENETAPAN_NO_SK");
            order_field.Add("TANGGAL_SK");
            order_field.Add("JUDUL_SK");

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
                search_clause += " OR CREATE_DATE = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM TRX_SK_PENETAPAN WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  TRX_SK_PENETAPAN " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<TRX_SK_PENETAPAN>(inject_clause_select);


            var result = from list in SelectedData
                         select new string[]
            
         {
            
            Convert.ToString("<center>"+list.PENETAPAN_NO_SK+"</center>"),
            Convert.ToString(list.JUDUL_SK),
            //Convert.ToString("<center>"+list.TANGGAL_SK+"</center>"),
            "<center>"+(Convert.ToString(list.TANGGAL_SK)).Substring(0,10)+"</center>",
            Convert.ToString("<center><a href='" + list.SK_LOCATION + "" + list.FILES + "' target='_blank'>Show SK</a></center"),
            Convert.ToString("<center><a href='Sk_penetapan_pnps/Edit/" + list.PENETAPAN_ID + "' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Ubah'><i class='action fa fa-file-text-o'></i></a><a href='Sk_penetapan_pnps/Hapus/" + list.PENETAPAN_ID + "' class='btn red btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Hapus'><i class='action fa fa-trash-o'></i></a>"),
             //Convert.ToString("<center><a href='PenetapanSNI/Detail/"+list.SNI_SK_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a></center>"),
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
        public ActionResult Edit(int id)
        {
            int idsk = id;
            var sk_pnps = db.Database.SqlQuery<TRX_SK_PENETAPAN>("SELECT * FROM TRX_SK_PENETAPAN WHERE PENETAPAN_ID = " + idsk).SingleOrDefault();
            string tgl = Convert.ToString(sk_pnps.TANGGAL_SK);
            ViewData["sk"] = sk_pnps;
            return View();
        }
        [HttpPost]
        public ActionResult Edit(TRX_SK_PENETAPAN edit)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            var id = edit.PENETAPAN_ID;
            var datenow = MixHelper.ConvertDateNow();
            DateTime TANGGAL_SK_NEW = Convert.ToDateTime(edit.TANGGAL_SK);

            string pathnya = Server.MapPath("~/Upload/Dokumen/SK_PENETAPAN_PNPS/");
            HttpPostedFileBase file_paten = Request.Files["FILES"];
            var file_name_paten = "";
            var filePath_paten = "";
            var fileExtension_paten = "";
            if (file_paten != null)
            {
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(pathnya))
                {
                    //If Directory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(pathnya);
                }
                string lampiranregulasipath = file_paten.FileName;
                if (lampiranregulasipath.Trim() != "")
                {
                    lampiranregulasipath = Path.GetFileNameWithoutExtension(file_paten.FileName);
                    fileExtension_paten = Path.GetExtension(file_paten.FileName);
                    file_name_paten = "SK_PENETAPAN_PNPS_" + id + fileExtension_paten;
                    filePath_paten = pathnya + file_name_paten.Replace(" ", "_");
                    file_paten.SaveAs(filePath_paten);
                }
            }

            var update = "PENETAPAN_NO_SK = '" + edit.PENETAPAN_NO_SK + "'," +
                        "TANGGAL_SK = TO_DATE ('" + edit.TANGGAL_SK + "','dd-mm-yyyy hh24:mi:ss')," +
                        "JUDUL_SK = '" + edit.JUDUL_SK + "'," +
                        "CREATE_DATE = " + datenow + "";

            var clause = "where PENETAPAN_ID = " + edit.PENETAPAN_ID;
            db.Database.ExecuteSqlCommand("UPDATE TRX_SK_PENETAPAN SET " + update.Replace("''", "NULL") + " " + clause);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Diubah";

            return RedirectToAction("Index");

            //var tt = ("UPDATE TRX_SK_PENETAPAN SET " + update.Replace("''", "NULL") + " " + clause);
            //return Content(tt);
        }
        public ActionResult Hapus(int id)
        {
            var idsk = id;
            db.Database.ExecuteSqlCommand("DELETE FROM TRX_SK_PENETAPAN WHERE PENETAPAN_ID = "+idsk);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Dihapus";

            return RedirectToAction("Index");
        }

    }
}
