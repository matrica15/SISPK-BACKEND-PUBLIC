using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.IO;
using Aspose.Words;

namespace SISPK.Controllers.Pemeliharaan
{
    [Auth(RoleTipe = 1)]
    public class PemeliharaanController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        [Auth(RoleTipe = 1)]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Detail(int id)
        {
            ViewData["detail"] = db.Database.SqlQuery<VIEW_PEMELIHARAAN>("SELECT * FROM VIEW_PEMELIHARAAN WHERE MAINTENANCE_DETAIL_ID = " + id).SingleOrDefault();
            return View();
        }

        public ActionResult Create()
        {
            var ListKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
            var V_sni = (from sni in db.TRX_SNI orderby sni.SNI_ID ascending select sni).ToList();
            ViewData["V_sni"] = V_sni;
            ViewData["ListKomtek"] = ListKomtek;
            return View();
        }

        [HttpPost]
        public ActionResult Create(TRX_MAINTENANCES mtn, TRX_MAINTENANCE_DETAILS mtd, string MAINTENANCE_KOMTEK2 = "")
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("TRX_MAINTENANCES");
            var datenow = MixHelper.ConvertDateNow();
            var id_sni = Convert.ToString(mtd.MAINTENANCE_DETAIL_SNI_ID);

            var komtek_id = "";
            if (MAINTENANCE_KOMTEK2 != "-")
            {
                komtek_id = MAINTENANCE_KOMTEK2;
            }
            else if (MAINTENANCE_KOMTEK2 == "-")
            {
                komtek_id = mtn.MAINTENANCE_KOMTEK;
            }



            //return Content(komtek_id);

            string pathnya = Server.MapPath("~/Upload/Dokumen/FORM_KAJI_ULANG/");
            HttpPostedFileBase file_paten = Request.Files["MAINTENANCE_DETAIL_KJ_ULG_NAME"];
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
                    file_name_paten = "FORM_KAJI_ULANG_PEMELIHARAAN_IDM_" + lastid + fileExtension_paten;
                    filePath_paten = pathnya + file_name_paten.Replace(" ", "_");
                    file_paten.SaveAs(filePath_paten);
                }
            }



            //if (Convert.ToInt32(form["count_rows"]) > 0)
            //{
            DateTime MAINTENANCE_DATE_a = Convert.ToDateTime(mtn.MAINTENANCE_DATE);
            var MAINTENANCE_DATE = "TO_DATE('" + MAINTENANCE_DATE_a.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";

            //KOMTEK_BIDANG_ID,KOMTEK_INSTANSI_ID,
            var fname = "MAINTENANCE_ID,MAINTENANCE_DOC_NUMBER,MAINTENANCE_KOMTEK,MAINTENANCE_CREATE_BY,MAINTENANCE_CREATE_DATE,MAINTENANCE_STATUS";
            var fvalue = "'" + lastid + "', " +
                        "'" + mtn.MAINTENANCE_DOC_NUMBER + "'," +
                        "'" + komtek_id + "', " +
                        "'" + UserId + "'," +
                            datenow + "," +
                        "1";
            //return Json(new { query = "INSERT INTO MASTER_KOMITE_TEKNIS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_MAINTENANCES (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");


            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);

            //for (var a = 0; a < Convert.ToInt32(form["count_rows"]); a++)
            //{
            int lastid_detail = MixHelper.GetSequence("TRX_MAINTENANCE_DETAILS");


            DateTime MAINTENANCE_DETAIL_REV_DATE_ = Convert.ToDateTime(mtd.MAINTENANCE_DETAIL_REV_DATE);
            var MAINTENANCE_DETAIL_REV_DATE = "TO_DATE('" + MAINTENANCE_DETAIL_REV_DATE_.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";

            DateTime MAINTENANCE_DETAIL_REPORT_DATE_ = Convert.ToDateTime(mtd.MAINTENANCE_DETAIL_REPORT_DATE);
            var MAINTENANCE_DETAIL_REPORT_DATE = "TO_DATE('" + MAINTENANCE_DETAIL_REPORT_DATE_.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";
            var fnameS = "";
            var fvalueS = "";
            if (mtd.MAINTENANCE_DETAIL_KJ_ULG_NAME != null)
            {
                fnameS = "MAINTENANCE_DETAIL_ID,MAINTENANCE_DETAIL_MTN_ID,MAINTENANCE_DETAIL_SNI_ID,MAINTENANCE_DETAIL_REV_DATE,MAINTENANCE_DETAIL_RESULT,MAINTENANCE_DETAIL_REPORT_DATE,MAINTENANCE_DETAIL_KJ_ULG_LOC,MAINTENANCE_DETAIL_KJ_ULG_NAME,MAINTENANCE_DETAIL_NO_SURAT";
                fvalueS = "'" + lastid_detail + "', " +
                        "'" + lastid + "', " +
                        "'" + mtd.MAINTENANCE_DETAIL_SNI_ID + "', " +
                        MAINTENANCE_DETAIL_REV_DATE + "," +
                        "'" + mtd.MAINTENANCE_DETAIL_RESULT + "', " +
                        MAINTENANCE_DETAIL_REPORT_DATE + "," +
                        "'/Upload/Dokumen/FORM_KAJI_ULANG/'," +
                        "'" + file_name_paten.Replace(" ", "_") + "'," +
                        "'" + mtd.MAINTENANCE_DETAIL_NO_SURAT + "'";
            }
            else
            {
                fnameS = "MAINTENANCE_DETAIL_ID,MAINTENANCE_DETAIL_MTN_ID,MAINTENANCE_DETAIL_SNI_ID,MAINTENANCE_DETAIL_REV_DATE,MAINTENANCE_DETAIL_RESULT,MAINTENANCE_DETAIL_REPORT_DATE,MAINTENANCE_DETAIL_NO_SURAT";
                fvalueS = "'" + lastid_detail + "', " +
                        "'" + lastid + "', " +
                        "'" + mtd.MAINTENANCE_DETAIL_SNI_ID + "', " +
                        MAINTENANCE_DETAIL_REV_DATE + "," +
                        "'" + mtd.MAINTENANCE_DETAIL_RESULT + "', " +
                        MAINTENANCE_DETAIL_REPORT_DATE + "," +
                        "'" + mtd.MAINTENANCE_DETAIL_NO_SURAT + "'";
            }

            //return Json(new { query = "INSERT INTO MASTER_KOMITE_TEKNIS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_MAINTENANCE_DETAILS (" + fnameS + ") VALUES (" + fvalueS.Replace("''", "NULL") + ")");

            //update trx_sni
            db.Database.ExecuteSqlCommand("UPDATE TRX_SNI SET SNI_MAINTENANCE_STS = '1' WHERE SNI_ID = " + id_sni);

            //}

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            //}
            //else
            //{
            //    TempData["Notifikasi"] = 2;
            //    TempData["NotifikasiText"] = "Data Gagal Disimpan";
            //}


            return RedirectToAction("Index");

        }

        //public ActionResult listPemeliharaan(DataTables param)
        //{
        //    var default_order = "MAINTENANCE_DATE";
        //    var limit = 10;

        //    List<string> order_field = new List<string>();
        //    order_field.Add("SNI_JUDUL");
        //    order_field.Add("MAINTENANCE_DATE_TEXT");
        //    order_field.Add("SNI_NOMOR");
        //    order_field.Add("MAINTENANCE_REPORT_DATE_TEXT");
        //    order_field.Add("DETAIL_RESULT_TEXT");

        //    string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
        //    string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
        //    string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
        //    string search = (param.sSearch == "") ? "" : param.sSearch;

        //    limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
        //    var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


        //    string where_clause = "";

        //    string search_clause = "";
        //    if (search != "")
        //    {
        //        if (where_clause != "")
        //        {
        //            search_clause += " AND ";
        //        }
        //        search_clause += "(";
        //        var i = 1;
        //        foreach (var fields in order_field)
        //        {
        //            if (fields != "")
        //            {
        //                search_clause += fields + "  LIKE '%" + search + "%'";
        //                if (i < order_field.Count())
        //                {
        //                    search_clause += " OR ";
        //                }
        //            }
        //            i++;
        //        }
        //        search_clause += " OR SNI_JUDUL = '%" + search + "%')";
        //    }

        //    string inject_clause_count = "";
        //    string inject_clause_select = "";
        //    if (where_clause != "" || search_clause != "")
        //    {
        //        inject_clause_count = "WHERE " + where_clause + " " + search_clause;
        //        inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER,(TO_CHAR(SYSDATE,'YYYY ')-REGEXP_SUBSTR(SNI_NOMOR, '[^:'']+', 1,2)) AS UMUR_SNI FROM (SELECT * FROM VIEW_PEMELIHARAAN WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE UMUR_SNI <= 5 AND ROWNUMBER > " + Convert.ToString(start);
        //    }
        //    var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PEMELIHARAAN " + inject_clause_count);
        //    var SelectedData = db.Database.SqlQuery<VIEW_PEMELIHARAAN>(inject_clause_select);

        //    //return Content(inject_clause_select);

        //    var result = from list in SelectedData
        //                 select new string[]
        //    {
        //        Convert.ToString("<center>"+list.SNI_JUDUL+"</center>"), 
        //        //Convert.ToString(list.MAINTENANCE_DATE_TEXT),
        //        Convert.ToString("<center>"+list.SNI_NOMOR+"</center>"),
        //        Convert.ToString("<center>"+list.MAINTENANCE_REPORT_DATE_TEXT+"</center>"),
        //        Convert.ToString("<center>"+list.DETAIL_RESULT_TEXT+"</center>"),
        //        Convert.ToString("<center><a href='Pemeliharaan/Detail/"+list.MAINTENANCE_DETAIL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a></center>"),
        //        //Convert.ToString("<center><a href='PenetapanSNI/Detail/"+list.SNI_SK_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a></center>"),
        //    };
        //    return Json(new
        //    {
        //        SelectedData,
        //        sEcho = param.sEcho,
        //        iTotalRecords = CountData,
        //        iTotalDisplayRecords = CountData,
        //        aaData = result.ToArray()
        //    }, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult Detail_sni(int id)
        {
            ViewData["detail"] = db.Database.SqlQuery<VIEW_SNI>("SELECT * FROM VIEW_SNI WHERE SNI_ID = " + id).SingleOrDefault();
            //return Content("lllll : "+id); 
            return View();
        }

        public ActionResult listPemeliharaan1(DataTables param)
        {
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_JUDUL");
            order_field.Add("SNI_NOMOR");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("KOMTEK_NAME");
            order_field.Add("SNI_SK_DATE_NAME");

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
                search_clause += " OR LOWER(SNI_JUDUL) LIKE LOWER('%" + search + "%'))";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE UMUR_SNI >5 AND " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI WHERE UMUR_SNI >5 AND " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE SNI_MAINTENANCE_STS IS NULL  AND ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SNI>(inject_clause_select);

            //return Content(inject_clause_select);

            var result = from list in SelectedData
                         select new string[]
            {

                Convert.ToString("<center>"+list.SNI_NOMOR+"</center>"),
                Convert.ToString(list.SNI_JUDUL),
                Convert.ToString(list.KOMTEK_CODE+"."+list.KOMTEK_NAME),
                Convert.ToString("<center>"+list.SNI_SK_DATE_NAME+"</center>"),
                //Convert.ToString("<center>"+list.DETAIL_RESULT_TEXT+"</center>"),
                //Convert.ToString("<center><a href='Pemeliharaan/Detail/"+list.MAINTENANCE_DETAIL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a></center>"),
                //Convert.ToString("<center><a href='Pemeliharaan/Detail_sni/"+list.SNI_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a></center>"),
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

        public ActionResult listPemeliharaan(DataTables param)
        {
            var default_order = "MAINTENANCE_DATE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_NOMOR");
            order_field.Add("SNI_JUDUL");
            //order_field.Add("MAINTENANCE_DATE_TEXT");
            order_field.Add("MAINTENANCE_DETAIL_REV_DATE");
            order_field.Add("DETAIL_RESULT_TEXT");
            order_field.Add("MAINTENANCE_DETAIL_REPORT_DATE");

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
                search_clause += " OR SNI_JUDUL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PEMELIHARAAN WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PEMELIHARAAN " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PEMELIHARAAN>(inject_clause_select);

            //return Content(inject_clause_select);

            var result = from list in SelectedData
                         select new string[]
            {
                Convert.ToString("<center>"+list.SNI_NOMOR+"</center>"),
                Convert.ToString("<center>"+list.SNI_JUDUL+"</center>"),
                Convert.ToString("<center>"+list.MAINTENANCE_DETAIL_NO_SURAT+"</center>"), 
                //Convert.ToString(list.MAINTENANCE_DATE_TEXT),
                "<center>"+(Convert.ToString(list.MAINTENANCE_DETAIL_REV_DATE)).Substring(0,10)+"</center>",
                Convert.ToString("<center>"+list.DETAIL_RESULT_TEXT+"</center>"),
                "<center>"+(Convert.ToString(list.MAINTENANCE_DETAIL_REPORT_DATE)).Substring(0,10)+"</center>",
                Convert.ToString("<center><a href='Pemeliharaan/Detail/"+list.MAINTENANCE_DETAIL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a></center>"),
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
        public ActionResult listPemeliharaan5th(DataTables param)
        {
            var default_order = "MAINTENANCE_DATE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_JUDUL");
            order_field.Add("MAINTENANCE_DATE_TEXT");
            order_field.Add("SNI_NOMOR");
            order_field.Add("MAINTENANCE_REPORT_DATE_TEXT");
            order_field.Add("DETAIL_RESULT_TEXT");

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
                search_clause += " OR SNI_JUDUL = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER,(TO_CHAR(SYSDATE,'YYYY ')-REGEXP_SUBSTR(SNI_NOMOR, '[^:'']+', 1,2)) AS UMUR_SNI FROM (SELECT * FROM VIEW_PEMELIHARAAN WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE UMUR_SNI > 5 AND ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PEMELIHARAAN " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PEMELIHARAAN>(inject_clause_select);

            //return Content(inject_clause_select);

            var result = from list in SelectedData
                         select new string[]
            {
                Convert.ToString("<center>"+list.SNI_JUDUL+"</center>"), 
                //Convert.ToString(list.MAINTENANCE_DATE_TEXT),
                Convert.ToString("<center>"+list.SNI_NOMOR+"</center>"),
                Convert.ToString("<center>"+list.MAINTENANCE_REPORT_DATE_TEXT+"</center>"),
                Convert.ToString("<center>"+list.DETAIL_RESULT_TEXT+"</center>"),
                Convert.ToString("<center><a href='Pemeliharaan/Detail/"+list.MAINTENANCE_DETAIL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a></center>"),
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
        [HttpGet]
        public ActionResult Find_SNI(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);
            var CountDataS = "";
            string inject_clause_selects = "";
            CountDataS = "SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM VIEW_SNI_FIVE_YEARS WHERE SNI_STATUS = 1 AND (LOWER(VIEW_SNI_FIVE_YEARS.SNI_NOMOR) LIKE '%" + q.ToLower() + "%' OR LOWER(VIEW_SNI_FIVE_YEARS.SNI_JUDUL) LIKE '%" + q.ToLower() + "%')";
            inject_clause_selects = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI_FIVE_YEARS WHERE SNI_STATUS = 1 AND (LOWER(VIEW_SNI_FIVE_YEARS.SNI_NOMOR) LIKE '%" + q.ToLower() + "%' OR LOWER(VIEW_SNI_FIVE_YEARS.SNI_JUDUL) LIKE '%" + q.ToLower() + "%')) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var CountData = db.Database.SqlQuery<decimal>(CountDataS).SingleOrDefault();
            string inject_clause_select = inject_clause_selects;

            var datarasni = db.Database.SqlQuery<VIEW_SNI_FIVE_YEARS>(inject_clause_select);
            var rasni = from cust in datarasni select new { id = cust.SNI_ID, text = cust.SNI_NOMOR };

            return Json(new { rasni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Cek_sni(VIEW_SNI SNI)
        {
            var id = Convert.ToInt32(SNI.SNI_ID);
            var kmt = db.Database.SqlQuery<VIEW_SNI>("SELECT * FROM VIEW_SNI WHERE SNI_ID = " + id).FirstOrDefault();
            var kd = kmt.KOMTEK_CODE;
            var nm = kmt.KOMTEK_NAME;
            return Json(new { idL = id, komtek_kd = kd, komtek_nm = nm }, JsonRequestBehavior.AllowGet);
        }
    }
}
