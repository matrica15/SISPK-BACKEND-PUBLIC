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

        public ActionResult Detail(int id) {
            ViewData["detail"] = db.Database.SqlQuery<VIEW_PEMELIHARAAN>("SELECT * FROM VIEW_PEMELIHARAAN WHERE MAINTENANCE_DETAIL_ID = " + id).SingleOrDefault();
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(TRX_MAINTENANCES mtn, FormCollection form)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("TRX_MAINTENANCES");
            var datenow = MixHelper.ConvertDateNow();

            //return Json(new
            //{
            //    sEcho = mtn,
            //    pas = mt_detail,
            //    aasss = form,

            //}, JsonRequestBehavior.AllowGet);

            

            if (Convert.ToInt32(form["count_rows"]) > 0)
            {
                DateTime MAINTENANCE_DATE_a = Convert.ToDateTime(mtn.MAINTENANCE_DATE);
                var MAINTENANCE_DATE = "TO_DATE('" + MAINTENANCE_DATE_a.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";

                //KOMTEK_BIDANG_ID,KOMTEK_INSTANSI_ID,
                var fname = "MAINTENANCE_ID,MAINTENANCE_DOC_NUMBER,MAINTENANCE_DATE,MAINTENANCE_CREATE_BY,MAINTENANCE_CREATE_DATE,MAINTENANCE_STATUS";
                var fvalue = "'" + lastid + "', " +
                            "'" + mtn.MAINTENANCE_DOC_NUMBER + "'," +
                            MAINTENANCE_DATE + ", " +
                            "'" + UserId + "'," +
                             datenow + "," +
                            "1";
                //return Json(new { query = "INSERT INTO MASTER_KOMITE_TEKNIS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_MAINTENANCES (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");


                String objek = fvalue.Replace("'", "-");
                MixHelper.InsertLog(logcode, objek, 1);

                for (var a = 0; a < Convert.ToInt32(form["count_rows"]); a++)
                {
                    int lastid_detail = MixHelper.GetSequence("TRX_MAINTENANCE_DETAILS");


                    DateTime MAINTENANCE_DETAIL_REV_DATE_ = Convert.ToDateTime(form["MAINTENANCE_DETAIL_REV_DATE_[" + a + "]"]);
                    var MAINTENANCE_DETAIL_REV_DATE = "TO_DATE('" + MAINTENANCE_DETAIL_REV_DATE_.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";

                    DateTime MAINTENANCE_DETAIL_REPORT_DATE_ = Convert.ToDateTime(form["MAINTENANCE_DETAIL_REPORT_DATE_[" + a + "]"]);
                    var MAINTENANCE_DETAIL_REPORT_DATE = "TO_DATE('" + MAINTENANCE_DETAIL_REPORT_DATE_.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";

                    DateTime MAINTENANCE_DETAIL_USUL_DATE_ = Convert.ToDateTime(form["MAINTENANCE_DETAIL_USUL_DATE_[" + a + "]"]);
                    var MAINTENANCE_DETAIL_USUL_DATE = "TO_DATE('" + MAINTENANCE_DETAIL_USUL_DATE_.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";

                    var fnameS = "MAINTENANCE_DETAIL_ID,MAINTENANCE_DETAIL_MTN_ID,MAINTENANCE_DETAIL_SNI_ID,MAINTENANCE_DETAIL_REV_DATE,MAINTENANCE_DETAIL_RESULT,MAINTENANCE_DETAIL_REPORT_DATE,MAINTENANCE_DETAIL_USUL_DATE";
                    var fvalueS = "'" + lastid_detail + "', " +
                                "'" + lastid + "', " +
                                "'" + form["SNI_ID_[" + a + "]"] + "', " +
                                MAINTENANCE_DETAIL_REV_DATE + "," +
                                "'" + form["MAINTENANCE_DETAIL_RESULT_[" + a + "]"] + "', " +
                                MAINTENANCE_DETAIL_REPORT_DATE + "," +
                                MAINTENANCE_DETAIL_USUL_DATE + "";
                    //return Json(new { query = "INSERT INTO MASTER_KOMITE_TEKNIS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_MAINTENANCE_DETAILS (" + fnameS + ") VALUES (" + fvalueS.Replace("''", "NULL") + ")");
                   

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

        public ActionResult listPemeliharaan(DataTables param)
        {
            var default_order = "MAINTENANCE_DATE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("MAINTENANCE_DOC_NUMBER");
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
                search_clause += " OR MAINTENANCE_DOC_NUMBER = '%" + search + "%')";
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

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString("<center>"+list.MAINTENANCE_DOC_NUMBER+"</center>"), 
                Convert.ToString(list.MAINTENANCE_DATE_TEXT),
                Convert.ToString(list.SNI_NOMOR),
                Convert.ToString(list.MAINTENANCE_REPORT_DATE_TEXT),
                Convert.ToString(list.DETAIL_RESULT_TEXT),
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
    }
}
