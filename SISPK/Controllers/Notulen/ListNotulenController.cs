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

namespace SISPK.Controllers.Notulen
{
    public class ListNotulenController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Create(int id = 0)
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
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
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            ViewData["AdopsiList"] = AdopsiList;
            ViewData["RevisiList"] = RevisiList;
            var IsKetua = db.Database.SqlQuery<string>("SELECT JABATAN FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + DataProposal.KOMTEK_ID + " AND USER_ID = " + USER_ID).SingleOrDefault();
            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_RELATED_ID = " + id).ToList();

            string SearchName = DataProposal.PROPOSAL_JUDUL_PNPS;
            string[] Name = SearchName.Split(' ');
            string QueryRefLain = "SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND (DOC_RELATED_ID <> " + id + " OR DOC_RELATED_ID IS NULL) AND ( ";
            string lastItem = Name.Last();

            foreach (string Res in Name)
            {
                if (!object.ReferenceEquals(Res, lastItem))
                {
                    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' OR ";
                }
                else
                {
                    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' )";
                }
            }
            var RefLain = db.Database.SqlQuery<VIEW_DOCUMENTS>(QueryRefLain).ToList();

            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 11 AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1").SingleOrDefault();
            if (DefaultDokumen == null)
            {
                var DefaultDokumenRSNI1 = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 7 AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1").SingleOrDefault();
                ViewData["DefaultDokumen"] = DefaultDokumenRSNI1;
            }
            else
            {
                ViewData["DefaultDokumen"] = DefaultDokumen;
            }
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 6").SingleOrDefault();
            ViewData["VERSION_RATEK"] = VERSION_RATEK;
            var DetailRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 6 ").ToList();
            ViewData["DetailRatek"] = (DetailRatek == null) ? null : DetailRatek;
            var NT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 10 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            ViewData["Notulen"] = NT;
            ViewData["IsKetua"] = ((IsKetua == "Ketua" || IsKetua == "Sekretariat") ? 1 : 0);
            ViewData["Dokumen"] = Dokumen;
            ViewData["RefLain"] = RefLain;
            return View();
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateRatek(HttpPostedFileBase NOTULEN, int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, int APPROVAL_TYPE = 0, string NO_RATEK = "", string TGL_RATEK = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;

            Stream STREAM_NOTULEN = NOTULEN.InputStream;
            byte[] APPDATA_NOTULEN = new byte[NOTULEN.ContentLength + 1];
            STREAM_NOTULEN.Read(APPDATA_NOTULEN, 0, NOTULEN.ContentLength);
            string Extension_NOTULEN = Path.GetExtension(NOTULEN.FileName);

            int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 6").SingleOrDefault();

            if (Extension_NOTULEN.ToLower() == ".docx" || Extension_NOTULEN.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_NOTULEN);
                string filePathdoc = path + "NOTULEN_TAS_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "NOTULEN_TAS_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "NOTULEN_TAS_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);


                int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                            "'14', " +
                            "'10', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen TAS Ver " + VERSION_RATEK + "', " +
                            "'Notulen Tenaga Ahli Standarisasi Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "NOTULEN_TAS_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_NOTULEN.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_NOTULEN + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_NOTULEN + ") VALUES (" + FVALUE_NOTULEN.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_NOTULEN.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_NOTULEN, objekTanggapan, 1);
            }
            else
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                NOTULEN.SaveAs(path + "BERITA_ACARA_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + Extension_NOTULEN.ToUpper());

                int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                            "'14', " +
                            "'10', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen TAS Ver " + VERSION_RATEK + "', " +
                            "'Notulen Tenaga Ahli Standarisasi Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "NOTULEN_TAS_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_NOTULEN.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_NOTULEN + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_NOTULEN + ") VALUES (" + FVALUE_NOTULEN.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_NOTULEN.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_NOTULEN, objekTanggapan, 1);
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        public ActionResult DataRSNI3(DataTables param)
        {
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("PROPOSAL_PNPS_CODE");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
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

            string where_clause = "PROPOSAL_STATUS = 6";

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
                search_clause += " OR LOWER(PROPOSAL_CREATE_DATE_NAME) LIKE LOWER('%" + search + "%'))";
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
                Convert.ToString(list.PROPOSAL_PNPS_CODE),
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString("<center>"+list.PROPOSAL_IS_URGENT_NAME+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                Convert.ToString("<center>"+((list.PROPOSAL_STATUS == 6 && list.PROPOSAL_STATUS_PROSES == 0)?"<a href='/Notulen/ListNotulen/Create/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Buat Notulen'><i class='action fa fa-pencil-square-o'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
