using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.IO;

namespace SISPK.Controllers.Pengajuan
{
    //[Auth(RoleTipe = 1)]
    public class PenetapanKomtekController : Controller
    {
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }
        //[Auth(RoleTipe = 5)]
        public ActionResult AssignKomtek(int id = 0)
        {
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 && komtek.KOMTEK_ID == DataProposal.KOMTEK_ID select komtek).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var ListKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
            ViewData["DataProposal"] = DataProposal;
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            ViewData["Komtek"] = DataKomtek;
            ViewData["ListKomtek"] = ListKomtek;
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            ViewData["AdopsiList"] = AdopsiList;
            ViewData["RevisiList"] = RevisiList;
            return View();
        }
        public ActionResult PenugasanKomtek(int id = 0)
        {
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 && komtek.KOMTEK_ID == DataProposal.KOMTEK_ID select komtek).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            ViewData["DataProposal"] = DataProposal;
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            ViewData["Komtek"] = DataKomtek;
            return View();
        }
        [HttpPost]
        public ActionResult PenugasanKomtek(int PROPOSAL_ID = 0, string PROPOSAL_ST_KOM_NO = "", string PROPOSAL_ST_KOM_LAMPIRAN = "", string PROPOSAL_ST_KOM_DATE = "", string PROPOSAL_ST_KOM_TIME = "")
        {
            var LOGCODE = MixHelper.GetLogCode();
            int LASTID = MixHelper.GetSequence("TRX_SURAT_TUGAS");
            var DATENOW = MixHelper.ConvertDateNow();
            var join_tgl_surat = (PROPOSAL_ST_KOM_DATE + " " + PROPOSAL_ST_KOM_TIME);
            String ST_DATE = "TO_DATE('" + join_tgl_surat + "', 'yyyy-mm-dd hh24:mi:ss')";
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var datenow = MixHelper.ConvertDateNow();
            var fname = "ST_ID,ST_PROPOSAL_ID,ST_NO_SURAT,ST_LAMPIRAN,ST_DATE,ST_PROPOSAL_STATUS,ST_VERSI,ST_CREATE_BY,ST_CREATE_DATE,ST_STATUS";
            var fvalue = "'" + LASTID + "', " +
                        "'" + PROPOSAL_ID + "', " +
                        "'" + PROPOSAL_ST_KOM_NO + "', " +
                        "'" + PROPOSAL_ST_KOM_LAMPIRAN + "', " +
                        ST_DATE + ", " +
                        "'1', " +
                        "'1', " +
                        "'" + USER_ID + "', " +
                        DATENOW + "," +
                        "'1'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_SURAT_TUGAS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");
            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
            //return View();
        }
        [HttpPost]
        public ActionResult Approval(int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, int[] PROPOSAL_ICS_REF_ICS_ID = null)
        {
            var USER_ID = Session["USER_ID"];
            var logcode = db.Database.SqlQuery<String>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();

            var TANGGAL_SKR = MixHelper.ConvertDateNow();

            var year_now = DateTime.Now.Year;
            var LOGCODE_POLLING = MixHelper.GetLogCode();
            int LASTID_POLLING = MixHelper.GetSequence("TRX_POLLING");
            var LAST_POLLING_VERSION = db.Database.SqlQuery<int>("SELECT NVL(CAST(MAX(POLLING_VERSION) AS INT),1) FROM TRX_POLLING WHERE POLLING_TYPE = 2 AND POLLING_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();

            var POLLING_IS_EXIST = db.Database.SqlQuery<TRX_POLLING>("SELECT * FROM TRX_POLLING WHERE POLLING_VERSION = " + LAST_POLLING_VERSION + " AND POLLING_TYPE = 2 AND POLLING_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            var JML_HARI = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 8").SingleOrDefault();
            
            if (POLLING_IS_EXIST == null)
            {
                var fname = "POLLING_ID,POLLING_PROPOSAL_ID,POLLING_TYPE,POLLING_START_DATE,POLLING_END_DATE,POLLING_VERSION,POLLING_CREATE_BY,POLLING_CREATE_DATE,POLLING_STATUS,POLLING_LOGCODE";
                var fvalue = "'" + LASTID_POLLING + "', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + 2 + "', " +
                                "TO_DATE(SYSDATE), " +
                                "TO_DATE(SYSDATE)+" + ((JML_HARI != null) ? JML_HARI.CONFIG_VALUE : "14") + ", " +
                                "'" + LAST_POLLING_VERSION + "', " +
                                "'" + USER_ID + "'," +
                                TANGGAL_SKR + "," +
                                "'1', " +
                                "'" + LOGCODE_POLLING + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_POLLING (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

                String objek2 = fvalue.Replace("'", "-");
                MixHelper.InsertLog(logcode, objek2, 1);
            }
            var NEW_LASTID_POLLING = db.Database.SqlQuery<int>("SELECT POLLING_ID FROM TRX_POLLING WHERE POLLING_PROPOSAL_ID =" + PROPOSAL_ID + " AND POLLING_VERSION = " + LAST_POLLING_VERSION).SingleOrDefault();

            var PROPOSAL_PNPS_CODE = db.Database.SqlQuery<string>("SELECT CAST(TO_CHAR (SYSDATE, 'YYYY') || '.' || KOMTEK_CODE || '.' || ( SELECT CAST ( ( CASE WHEN LENGTH (COUNT(PROPOSAL_ID) + 1) = 1 THEN '0' || CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) ELSE CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) END ) AS VARCHAR2 (255) ) PNPSCODE FROM TRX_PROPOSAL WHERE PROPOSAL_KOMTEK_ID = KOMTEK_ID ) AS VARCHAR2(255)) AS PNPSCODE FROM MASTER_KOMITE_TEKNIS WHERE KOMTEK_ID = " + PROPOSAL_KOMTEK_ID).SingleOrDefault();
            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_PNPS_CODE = (CASE WHEN PROPOSAL_TYPE = 2 THEN '" + PROPOSAL_PNPS_CODE + "' ELSE PROPOSAL_PNPS_CODE END), PROPOSAL_KOMTEK_ID = " + PROPOSAL_KOMTEK_ID + ", PROPOSAL_STATUS = 2, PROPOSAL_IS_POLLING = 1,PROPOSAL_POLLING_ID = " + NEW_LASTID_POLLING + ", PROPOSAL_UPDATE_DATE = " + TANGGAL_SKR + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);

            String objek = "PROPOSAL_KOMTEK_ID = " + PROPOSAL_KOMTEK_ID + ", PROPOSAL_STATUS = 2, PROPOSAL_IS_POLLING = 1, PROPOSAL_POLLING_ID = " + NEW_LASTID_POLLING + ", PROPOSAL_UPDATE_DATE = " + TANGGAL_SKR + ", PROPOSAL_UPDATE_BY = " + USER_ID;
            MixHelper.InsertLog(logcode, objek.Replace("'", "-"), 2);

            if (PROPOSAL_ICS_REF_ICS_ID != null)
            {
                foreach (var i in PROPOSAL_ICS_REF_ICS_ID)
                {
                    int PROPOSAL_ICS_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_ICS_REF");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_ICS_REF (PROPOSAL_ICS_REF_ID,PROPOSAL_ICS_REF_PROPOSAL_ID,PROPOSAL_ICS_REF_ICS_ID)VALUES(" + PROPOSAL_ICS_REF_ID + "," + PROPOSAL_ID + "," + i + ")");
                }
            }

            int lastid = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + lastid + "," + PROPOSAL_ID + ",1," + TANGGAL_SKR + "," + USER_ID + ",1,1,1)");

            db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_MTPS = " + TANGGAL_SKR + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
            //return RedirectToAction("Index");
        }
        public ActionResult DataPenetapanKomtek(DataTables param, int id = 0, int id2 = 0)
        {
            var Status = id;
            var ApprovalStatus = id2;
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("PROPOSAL_TYPE_NAME");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_TAHAPAN");
            order_field.Add("PROPOSAL_IS_URGENT_NAME");
            order_field.Add("PROPOSAL_STATUS_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "PROPOSAL_STATUS = 1 AND PROPOSAL_STATUS_PROSES = 1";

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
                search_clause += " OR PROPOSAL_CREATE_DATE_NAME LIKE '%" + search + "%' OR KOMTEK_NAME LIKE '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString("<center>"+list.PROPOSAL_CREATE_DATE_NAME+"</center>"),
                Convert.ToString(list.PROPOSAL_TYPE_NAME),
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString((list.KOMTEK_CODE==null)?"-":list.KOMTEK_CODE+"."+list.INSTANSI_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString("<center>"+list.PROPOSAL_IS_URGENT_NAME+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                Convert.ToString("<center><a href='/Pengajuan/Usulan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 1 && list.PROPOSAL_APPROVAL_STATUS == 1)?"<a href='/Pengajuan/PenetapanKomtek/AssignKomtek/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips btn_approval' data-container='body' data-placement='top' data-original-title='Penetapan Komtek'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips btn_print' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult FindICS(int KOMTEK_ID = 0)
        {
            var ics = db.Database.SqlQuery<VIEW_KOMTEK_ICS>("SELECT * FROM VIEW_KOMTEK_ICS WHERE KOMTEK_ICS_KOMTEK_ID = " + KOMTEK_ID).ToList();
            return Json(new { ics }, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
        public ActionResult FindICSrasni(int KOMTEK_ID = 0, int id = 0)
        {
            var ICS_Pil = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var ics = db.Database.SqlQuery<VIEW_KOMTEK_ICS>("SELECT * FROM VIEW_KOMTEK_ICS WHERE KOMTEK_ICS_KOMTEK_ID = " + KOMTEK_ID).ToList();
            return Json(new { ics, ICS_Pil }, JsonRequestBehavior.AllowGet);

        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult FindKomtek(int KOMTEK_ID = 0)
        {
            var DATAKOMTEK = db.Database.SqlQuery<VIEW_KOMTEK_FULL>("SELECT * FROM VIEW_KOMTEK_FULL WHERE KOMTEK_ID = " + KOMTEK_ID).SingleOrDefault();
            var DATAICS = db.Database.SqlQuery<VIEW_KOMTEK_ICS>("SELECT * FROM VIEW_KOMTEK_ICS WHERE KOMTEK_ICS_KOMTEK_ID = " + KOMTEK_ID + " ORDER BY ICS_CODE ASC").ToList();
            var DATAANGGOTA = db.Database.SqlQuery<VIEW_ANGGOTA>("SELECT * FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + KOMTEK_ID + " AND KOMTEK_ANGGOTA_JABATAN <> 32 ORDER BY KOMTEK_ANGGOTA_JABATAN ASC").ToList();
            return Json(new { DATAKOMTEK, DATAICS, DATAANGGOTA }, JsonRequestBehavior.AllowGet);
        }
    }
}
