using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using Aspose.Pdf;

namespace SISPK.Controllers.Home
{
    [Auth(RoleTipe = 1)]
    public class DashboardController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
       
        public ActionResult Index()
        {
            var year_now = (DateTime.Now.Year);
            var IS_KOMTEK = Convert.ToInt32(Session["IS_KOMTEK"]);
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var AktifData = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM ( SELECT AA.*, ROWNUM NOMOR FROM ( SELECT * FROM VIEW_PROPOSAL_DASHBOARD WHERE PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID + " AND PROPOSAL_STATUS > 1 ORDER BY PROPOSAL_ID DESC) AA WHERE ROWNUM <= 1 ) WHERE NOMOR > 0").FirstOrDefault();
            var JML_MTPS = db.Database.SqlQuery<int>("SELECT CAST(COUNT(*) AS INT) FROM TRX_PROPOSAL WHERE PROPOSAL_STATUS = 2 " + ((IS_KOMTEK == 1) ? " AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID : "")).FirstOrDefault();
            var JML_PNPS = db.Database.SqlQuery<int>("SELECT CAST(COUNT(*) AS INT) FROM TRX_PROPOSAL WHERE PROPOSAL_STATUS = 3 " + ((IS_KOMTEK == 1) ? " AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID : "")).FirstOrDefault();
            var JML_RASNI = db.Database.SqlQuery<int>("SELECT CAST(COUNT(*) AS INT) FROM TRX_PROPOSAL WHERE PROPOSAL_STATUS = 10 " + ((IS_KOMTEK == 1) ? " AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID : "")).FirstOrDefault();
            var JML_SNI = db.Database.SqlQuery<int>("SELECT CAST(COUNT(*) AS INT) FROM TRX_SNI").FirstOrDefault();
            ViewData["JML_MTPS"] = JML_MTPS;
            ViewData["JML_PNPS"] = JML_PNPS;
            ViewData["JML_RASNI"] = JML_RASNI;
            ViewData["JML_SNI"] = JML_SNI;

            ViewData["IS_KOMTEK"] = IS_KOMTEK;
            ViewData["USER_KOMTEK_ID"] = USER_KOMTEK_ID;
            ViewData["AktifData"] = AktifData;

            return View();

            //var tt = ("SELECT * FROM ( SELECT AA.*, ROWNUM NOMOR FROM ( SELECT * FROM VIEW_PROPOSAL_DASHBOARD WHERE PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID + " AND PROPOSAL_STATUS > 1 ORDER BY PROPOSAL_ID DESC) AA WHERE ROWNUM <= 1 ) WHERE NOMOR > 0");
            //return Content(tt);
        }
        public ActionResult Home()
        {
            return View();
        }
        public ActionResult DeniTest()
        {
            return View();
        }
        public ActionResult DataDashboard(DataTables param)
        {

            var default_order = "PROPOSAL_ID";
            var limit = 10;
          var BIDANG_ID = Convert.ToInt32(Session["BIDANG_ID"]);
            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_ID");
            order_field.Add("PROPOSAL_PNPS_CODE");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_RUANG_LINGKUP");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("KOMTEK_NAME");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PROGRESS");
            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;
            string where_clause = "";
            if (BIDANG_ID == 0)
            {
                where_clause = "PROPOSAL_STATUS < 13 AND PROPOSAL_STATUS_PROSES > 0 AND PROPOSAL_STATUS != 9 AND PROPOSAL_APPROVAL_STATUS != 0 AND (PROPOSAL_IS_BATAL <> 1 OR PROPOSAL_IS_BATAL IS NULL) ";
            } else
            {
                where_clause = "PROPOSAL_STATUS < 13 AND PROPOSAL_STATUS_PROSES > 0 AND PROPOSAL_STATUS != 9 AND PROPOSAL_APPROVAL_STATUS != 0 AND (PROPOSAL_IS_BATAL <> 1 OR PROPOSAL_IS_BATAL IS NULL) AND KOMTEK_BIDANG_ID = " + BIDANG_ID;
            }
            

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
                search_clause += " OR PROPOSAL_CREATE_DATE_NAME = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL_DASHBOARD WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL_DASHBOARD " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString("<center>"+list.PROPOSAL_CREATE_DATE_NAME+"</center>"),
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME), 
                Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center><ul class='progress'><li class='usulan_0'><a href='/Pengajuan/Usulan'>Usulan</a></li><li class='usulan_2'><a href='/Pengajuan/Usulan'>MTPS</a></li><li class='usulan_3'><a href='/Pengajuan/Usulan'>PNPS</a></li><li class='usulan_4'><a href='/Perumusan/RSNI1'>RSNI1</a></li><li class='usulan_5'><a href='/Perumusan/RSNI2'>RSNI2</a></li><li class='usulan_6'><a href='/Perumusan/RSNI3'>RSNI3</a></li><li class='usulan_7'><a href='/Perumusan/RSNI4'>RSNI4</a></li><li class='usulan_8'><a href='/Perumusan/RSNI5'>RSNI5</a></li><li class='usulan_9'><a href='/Perumusan/RSNI6'>RSNI6</a></li><li class='usulan_10'><a href='/Perumusan/RASNI'>RASNI</a></li><li class='usulan_11'><a href='/SNI/SNIList'>SNI</a></li></ul></center>"),
                Convert.ToString("<center><a href='/Pengajuan/Usulan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS==0 && list.PROPOSAL_APPROVAL_STATUS == 1)?"<a href='/Pengajuan/Usulan/Update/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Ubah'><i class='action fa fa-edit'></i></a><a href='javascript:void(0)' onclick='hapus_usulan("+list.PROPOSAL_ID+")' class='btn red btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Hapus'><i class='action glyphicon glyphicon-remove'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")'  class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
            };
            var sni = from cust in SelectedData
                      select new
                      {
                          PROPOSAL_ID = cust.PROPOSAL_ID,
                          PROPOSAL_PNPS_CODE = cust.PROPOSAL_PNPS_CODE,
                          PROPOSAL_JENIS_PERUMUSAN_NAME = cust.PROPOSAL_JENIS_PERUMUSAN_NAME,
                          PROPOSAL_JUDUL_PNPS = cust.PROPOSAL_JUDUL_PNPS,
                          PROPOSAL_RUANG_LINGKUP = cust.PROPOSAL_RUANG_LINGKUP,
                          KOMTEK_CODE = cust.KOMTEK_CODE,
                          KOMTEK_NAME = cust.KOMTEK_NAME,
                          KOMTEK_FULLNAME = cust.KOMTEK_CODE + " " + cust.KOMTEK_NAME,
                          PROGRESS = cust.PROGRESS
                      };
            return Json(new
            {
                //wew = inject_clause_select,
                draw = param.sEcho,
                recordsTotal = CountData,
                recordsFiltered = CountData,
                data = sni
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Test()
        {
            var Query = db.Database.SqlQuery<SYS_MENU>("SELECT T1.MENU_ID, T1.MENU_PARENT_ID, T1.MENU_URL, T1.MENU_NAME, T1.MENU_SORT, T1.MENU_ICON, T1.MENU_POSITION, T1.MENU_CREATE_BY, T1.MENU_CREATE_DATE, T1.MENU_UPDATE_BY, T1.MENU_UPDATE_DATE, T1.MENU_STATUS FROM SYS_MENU T1 LEFT JOIN SYS_MENU T2 ON T2.MENU_ID = T1.MENU_PARENT_ID WHERE T1.MENU_STATUS = 1 START WITH T1.MENU_PARENT_ID = 0 CONNECT BY PRIOR T1.MENU_ID = T1.MENU_PARENT_ID ORDER SIBLINGS BY T1.MENU_SORT,T2.MENU_SORT").ToList();
            return Json(new { Hasil = Query }, JsonRequestBehavior.AllowGet);
        }
        public partial class ChartData
        {
            public string name { get; set; }
            public double total { get; set; }
        }
        public partial class ChartDataProposal
        {
            public Int32 PROPOSAL_YEAR { get; set; }
            public string PROPOSAL_TAHAPAN { get; set; }
            public Int32 JUMLAH { get; set; }
        }
        [HttpPost]
        public ActionResult chart_bar_1()
        {
            //var categoriesxx = (from p in db.VIEW_PROPOSAL group p by p.PROPOSAL_YEAR into g orderby g.Key ascending select g.Key).ToArray();
            var categoriestemp = db.Database.SqlQuery<GROUP_DASHBOARD>("SELECT PROPOSAL_YEAR FROM VIEW_PROPOSAL WHERE PROPOSAL_YEAR IS NOT NULL AND PROPOSAL_STATUS IS NOT NULL AND PROPOSAL_STATUS != 1 GROUP BY PROPOSAL_YEAR ORDER BY PROPOSAL_YEAR ASC").ToArray();
            var categories = (from p in categoriestemp select p.PROPOSAL_YEAR).ToArray();

            //var categories = (from p in db.VIEW_PROPOSAL group p by p.PROPOSAL_YEAR into g orderby g.Key ascending select g.Key).ToArray();
            int[] NOT_PROPOSAL_ID = { 2, 3, 10, 11 };
            var tempseries = db.Database.SqlQuery<GROUP_DASHBOARD>("SELECT PROPOSAL_TAHAPAN,PROPOSAL_STATUS FROM VIEW_PROPOSAL WHERE PROPOSAL_STATUS IS NOT NULL AND PROPOSAL_STATUS != 1 GROUP BY PROPOSAL_TAHAPAN,PROPOSAL_STATUS").ToList();
            //var tempseriesx = (from p in db.VIEW_PROPOSAL
            //                   group p by new
            //                   {
            //                       p.PROPOSAL_TAHAPAN,
            //                       p.PROPOSAL_STATUS
            //                   } into g
            //                   orderby g.Key.PROPOSAL_STATUS ascending
            //                   select new
            //                   {
            //                       PROPOSAL_TAHAPAN = g.Key.PROPOSAL_TAHAPAN,
            //                       PROPOSAL_STATUS = g.Key.PROPOSAL_STATUS,
            //                   }).ToList();
            //var tempseries = tempseriesx.Where(p => p.PROPOSAL_STATUS != null && p.PROPOSAL_STATUS != 1).ToList();
            int[][] data = new int[tempseries.Count()][];
            var index = 0;
            var index2 = 0;
            foreach (var x in tempseries)
            {

                data[index] = new int[categories.Length];
                index2 = 0;
                foreach (var i in categories)
                {
                    var Jml = db.Database.SqlQuery<int>("SELECT CAST(COUNT(*) AS INT) AS Jml FROM VIEW_PROPOSAL WHERE PROPOSAL_YEAR = " + i + " AND PROPOSAL_STATUS = " + x.PROPOSAL_STATUS).SingleOrDefault();
                    data[index][index2] = Jml;
                    index2++;
                }
                index++;
            }
            var series = tempseries.Select((c, i) => new { name = c.PROPOSAL_TAHAPAN, data = data[i] }).ToArray();

            return Json(new { categories, series }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult chart_pie(int year = 0)
        {
            var YearNow = DateTime.Now.Year;
            if (year == 0) {
                year = YearNow;
            }
            var newtempseries = (from p in db.VIEW_PROPOSAL
                                 where p.PROPOSAL_YEAR == year
                               group p by new
                               {
                                   p.PROPOSAL_YEAR,
                                   p.PROPOSAL_TAHAPAN,
                                   p.PROPOSAL_STATUS
                               } into g
                               orderby g.Key.PROPOSAL_STATUS ascending
                               select new
                               {
                                   name = g.Key.PROPOSAL_TAHAPAN,
                                   y = g.Count(),
                               }).ToList();
            var Jml = newtempseries.Select(c => c.y).Sum();
            var series = newtempseries.Select((c, i) => new { name = c.name, y = Math.Round((Convert.ToDecimal(c.y) / Convert.ToDecimal(Jml)) * 100, 2) }).ToArray();
            return Json(new { series }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult chart_line(int year = 0)
        {
            var YearNow = DateTime.Now.Year;
            if (year == 0)
            {
                year = YearNow;
            }
            var categories = db.Database.SqlQuery<string>("WITH JUMLAH_BULAN AS ( SELECT LEVEL - 1 AS ID FROM DUAL CONNECT BY LEVEL <= 12 ), BULAN AS ( SELECT TO_CHAR ( ADD_MONTHS ( TO_DATE ('01/01/1000', 'DD/MM/RRRR'), ID ), 'mm' ) AS BULAN_NOMOR, TO_CHAR ( ADD_MONTHS ( TO_DATE ('01/01/1000', 'DD/MM/RRRR'), ID ), 'Mon' ) AS BULAN FROM JUMLAH_BULAN ) SELECT BULAN FROM BULAN").ToArray();
            var seriesdata = db.Database.SqlQuery<int>("WITH JUMLAH_BULAN AS ( SELECT LEVEL - 1 AS ID FROM DUAL CONNECT BY LEVEL <= 12 ), BULAN AS ( SELECT TO_CHAR ( ADD_MONTHS ( TO_DATE ('01/01/1000', 'DD/MM/RRRR'), ID ), 'mm' ) AS BULAN_NOMOR, TO_CHAR ( ADD_MONTHS ( TO_DATE ('01/01/1000', 'DD/MM/RRRR'), ID ), 'Mon' ) AS BULAN FROM JUMLAH_BULAN ), DATAJUMLAH AS ( SELECT TO_CHAR ( POLLING_DETAIL_CREATE_DATE, 'Mon' ) BULAN, COUNT (POLLING_DETAIL_ID) JUMLAH FROM TRX_POLLING_DETAILS WHERE TO_CHAR ( POLLING_DETAIL_CREATE_DATE, 'YYYY' ) = '" + year + "' GROUP BY TO_CHAR ( POLLING_DETAIL_CREATE_DATE, 'Mon' ) ) SELECT CAST(NVL (T2.JUMLAH, 0) AS INT) AS JUMLAH FROM BULAN T1 LEFT JOIN DATAJUMLAH T2 ON T1.BULAN = T2.BULAN ORDER BY T1.BULAN_NOMOR").ToArray();

            return Json(new { categories, seriesdata }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult getyear()
        {
            var YearNow = DateTime.Now.Year;
            var tahun = (from p in db.VIEW_PROPOSAL
                         group p by p.PROPOSAL_YEAR into g
                         orderby g.Key ascending
                         select g.Key).ToArray();
            return Json(new { tahun, YearNow}, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetJumlahSNI(int years = 0)
        {
            var Tahun = (from student in db.VIEW_PROPOSAL
                         group student by student.PROPOSAL_YEAR into newGroup
                         orderby newGroup.Key
                         select newGroup.Key).ToArray();
            var Data = db.Database.SqlQuery<ChartDataProposal>("SELECT CAST(PROPOSAL_YEAR AS INT) PROPOSAL_YEAR, PROPOSAL_TAHAPAN, CAST (COUNT(*) AS INT) AS JUMLAH FROM VIEW_PROPOSAL GROUP BY PROPOSAL_YEAR,PROPOSAL_TAHAPAN").ToArray();
            return Json(new { Tahun, Data }, JsonRequestBehavior.AllowGet);
        }
    }
}
