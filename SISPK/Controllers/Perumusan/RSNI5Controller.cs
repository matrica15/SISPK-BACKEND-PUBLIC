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

namespace SISPK.Controllers.Perumusan
{
    [Auth(RoleTipe = 1)]
    public class RSNI5Controller : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            var IS_KOMTEK = Convert.ToInt32(Session["IS_KOMTEK"]);
            string ViewName = ((IS_KOMTEK == 1) ? "DaftarPenyusunanRSNI5" : "DaftarPengesahanRSNI5");
            return View(ViewName);
        }
        public ActionResult Detail(int id = 0)
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            var Lampiran = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
            var Bukti = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
            var Surat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
            var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 && komtek.KOMTEK_ID == DataProposal.KOMTEK_ID select komtek).SingleOrDefault();
            var IsKetua = db.Database.SqlQuery<string>("SELECT JABATAN FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + DataProposal.KOMTEK_ID + " AND USER_ID = " + USER_ID).SingleOrDefault();

            var SURAT_UNDANGAN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 43 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var SURAT_JP = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 45 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var BERITA_ACARA_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 42 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DAFTAR_HADIR_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 16 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var NOTULEN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 44 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DataPolling = db.Database.SqlQuery<VIEW_POLLING_SINGLE>("SELECT ZZ.* FROM VIEW_POLLING_SINGLE ZZ WHERE ZZ.POLLING_PROPOSAL_ID = " + id + " AND ZZ.POLLING_TYPE = 12 AND ROWNUM = 1 ORDER BY ZZ.POLLING_ID DESC").SingleOrDefault();

            ViewData["DataPolling"] = DataPolling;
            ViewData["SURAT_UNDANGAN_RAPAT"] = SURAT_UNDANGAN_RAPAT;
            ViewData["SURAT_PENYERAHAN_RSNI"] = SURAT_JP;
            ViewData["BERITA_ACARA_RAPAT"] = BERITA_ACARA_RAPAT;
            ViewData["DAFTAR_HADIR_RAPAT"] = DAFTAR_HADIR_RAPAT;
            ViewData["NOTULEN_RAPAT"] = NOTULEN_RAPAT;

            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 12").SingleOrDefault();
            var DetailRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 12 AND PROPOSAL_RAPAT_VERSION = " + (VERSION_RATEK - 1)).FirstOrDefault();
            var DetailHistoryRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS IN (12) ").ToList();



            ViewData["Komtek"] = DataKomtek;
            ViewData["DataProposal"] = DataProposal;
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            ViewData["AdopsiList"] = AdopsiList;
            ViewData["RevisiList"] = RevisiList;
            ViewData["Lampiran"] = Lampiran;
            ViewData["Bukti"] = Bukti;
            ViewData["Surat"] = Surat;
            ViewData["Outline"] = Outline;
            ViewData["IsKetua"] = ((IsKetua == "Ketua" || IsKetua == "Sekretariat") ? 1 : 0);
            ViewData["VERSION_RATEK"] = VERSION_RATEK;
            ViewData["DetailRatek"] = DetailRatek;
            ViewData["DetailHistoryRatek"] = DetailHistoryRatek;

            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_RELATED_ID = " + id).ToList();
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 17 AND DOC_FOLDER_ID = 15 AND DOC_STATUS = 1  AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
            if (DefaultDokumen == null)
            {
                var DefaultDokumenRSNI1 = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 11 AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1  AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
                ViewData["DefaultDokumen"] = DefaultDokumenRSNI1;
            }
            else
            {
                ViewData["DefaultDokumen"] = DefaultDokumen;
            }
            ViewData["Dokumen"] = Dokumen;

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
            ViewData["RefLain"] = RefLain;
            return View();
        }
        [Auth(RoleTipe = 5)]
        public ActionResult Pengesahan(int id = 0)
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            var Lampiran = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
            var Bukti = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
            var Surat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
            var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 && komtek.KOMTEK_ID == DataProposal.KOMTEK_ID select komtek).SingleOrDefault();
            var IsKetua = db.Database.SqlQuery<string>("SELECT JABATAN FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + DataProposal.KOMTEK_ID + " AND USER_ID = " + USER_ID).SingleOrDefault();

            var SURAT_UNDANGAN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 43 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var SURAT_JP = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 45 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var BERITA_ACARA_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 42 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DAFTAR_HADIR_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 16 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var NOTULEN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 44 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DataPolling = db.Database.SqlQuery<VIEW_POLLING_SINGLE>("SELECT ZZ.* FROM VIEW_POLLING_SINGLE ZZ WHERE ZZ.POLLING_PROPOSAL_ID = " + id + " AND ZZ.POLLING_TYPE = 12 AND ROWNUM = 1 ORDER BY ZZ.POLLING_ID DESC").SingleOrDefault();

            ViewData["DataPolling"] = DataPolling;
            ViewData["SURAT_UNDANGAN_RAPAT"] = SURAT_UNDANGAN_RAPAT;
            ViewData["SURAT_PENYERAHAN_RSNI"] = SURAT_JP;
            ViewData["BERITA_ACARA_RAPAT"] = BERITA_ACARA_RAPAT;
            ViewData["DAFTAR_HADIR_RAPAT"] = DAFTAR_HADIR_RAPAT;
            ViewData["NOTULEN_RAPAT"] = NOTULEN_RAPAT;

            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 12").SingleOrDefault();
            var DetailRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 12 AND PROPOSAL_RAPAT_VERSION = " + (VERSION_RATEK - 1)).FirstOrDefault();
            var DetailHistoryRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS IN (12) ").ToList();



            ViewData["Komtek"] = DataKomtek;
            ViewData["DataProposal"] = DataProposal;
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            ViewData["AdopsiList"] = AdopsiList;
            ViewData["RevisiList"] = RevisiList;
            ViewData["Lampiran"] = Lampiran;
            ViewData["Bukti"] = Bukti;
            ViewData["Surat"] = Surat;
            ViewData["Outline"] = Outline;
            ViewData["IsKetua"] = ((IsKetua == "Ketua" || IsKetua == "Sekretariat") ? 1 : 0);
            ViewData["VERSION_RATEK"] = VERSION_RATEK;
            ViewData["DetailRatek"] = DetailRatek;
            ViewData["DetailHistoryRatek"] = DetailHistoryRatek;

            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_RELATED_ID = " + id).ToList();
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 17 AND DOC_FOLDER_ID = 15 AND DOC_STATUS = 1  AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
            if (DefaultDokumen == null)
            {
                var DefaultDokumenRSNI1 = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 11 AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1  AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
                ViewData["DefaultDokumen"] = DefaultDokumenRSNI1;
            }
            else
            {
                ViewData["DefaultDokumen"] = DefaultDokumen;
            }
            ViewData["Dokumen"] = Dokumen;

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
            ViewData["RefLain"] = RefLain;
            return View();
        }
        [HttpPost]
        public ActionResult Pengesahan(string PROPOSAL_ABSTRAK = "", int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, string PROPOSAL_NO_SNI_PROPOSAL = "", string PROPOSAL_JUDUL_SNI_PROPOSAL = "", string POLLING_START_DATE = "", string POLLING_END_DATE = "", int APPROVAL_TYPE = 1, string NO_RAKOR = "", string TGL_RAKOR = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
            int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            int PROPOSAL_RAPAT_ID = MixHelper.GetSequence("TRX_PROPOSAL_RAPAT");
            var VERSION_RAKOR = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 8").SingleOrDefault();
            int LASTID_PROPOSAL_RAPAT = MixHelper.GetSequence("TRX_DOCUMENTS");
            var LOGCODE_PROPOSAL_RAPAT = MixHelper.GetLogCode();
            String TGL_RAKOR_CONVERT = "TO_DATE('" + TGL_RAKOR + "', 'yyyy-mm-dd hh24:mi:ss')";
            var FNAME_PROPOSAL_RAPAT = "PROPOSAL_RAPAT_ID,PROPOSAL_RAPAT_PROPOSAL_ID,PROPOSAL_RAPAT_NOMOR,PROPOSAL_RAPAT_DATE,PROPOSAL_RAPAT_VERSION,PROPOSAL_RAPAT_APPROVAL_ID,PROPOSAL_RAPAT_PROPOSAL_STATUS";
            var FVALUE_PROPOSAL_RAPAT = "'" + LASTID_PROPOSAL_RAPAT + "', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'" + NO_RAKOR + "', " +
                                        TGL_RAKOR_CONVERT + ", " +
                                        "'" + VERSION_RAKOR + "', " +
                                        "'" + PROPOSAL_RAPAT_ID + "', " +
                                        "'8'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_RAPAT (" + FNAME_PROPOSAL_RAPAT + ") VALUES (" + FVALUE_PROPOSAL_RAPAT.Replace("''", "NULL") + ")");


            HttpPostedFileBase FILE_SURAT_UNDANGAN_RAPAT = Request.Files["SURAT_UNDANGAN_RAPAT"];
            if (FILE_SURAT_UNDANGAN_RAPAT.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_SURAT_UNDANGAN_RAPAT = FILE_SURAT_UNDANGAN_RAPAT.InputStream;

                string EXT_SURAT_UNDANGAN_RAPAT = Path.GetExtension(FILE_SURAT_UNDANGAN_RAPAT.FileName);
                if (EXT_SURAT_UNDANGAN_RAPAT.ToLower() == ".docx" || EXT_SURAT_UNDANGAN_RAPAT.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_SURAT_UNDANGAN_RAPAT);
                    string filePathdoc = path + "SURAT_UNDANGAN_RAPAT_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "SURAT_UNDANGAN_RAPAT_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "SURAT_UNDANGAN_RAPAT_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_SURAT_UNDANGAN_RAPAT = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_SURAT_UNDANGAN_RAPAT = MixHelper.GetLogCode();
                    var FNAME_SURAT_UNDANGAN_RAPAT = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_SURAT_UNDANGAN_RAPAT = "'" + LASTID_SURAT_UNDANGAN_RAPAT + "', " +
                                "'16', " +
                                "'46', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Undangan Rapat Koordinasi RSNI 5 Ver " + VERSION_RAKOR + "', " +
                                "'Surat Undangan Rapat Koordinasi RSNI 5 Ver " + VERSION_RAKOR + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_UNDANGAN_RAPAT_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_SURAT_UNDANGAN_RAPAT.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_SURAT_UNDANGAN_RAPAT + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_SURAT_UNDANGAN_RAPAT + ") VALUES (" + FVALUE_SURAT_UNDANGAN_RAPAT.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_SURAT_UNDANGAN_RAPAT.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_SURAT_UNDANGAN_RAPAT, objekTanggapan, 1);
                }
                else
                {
                    FILE_SURAT_UNDANGAN_RAPAT.SaveAs(path + "SURAT_UNDANGAN_RAPAT_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_SURAT_UNDANGAN_RAPAT.ToLower());
                    int LASTID_SURAT_UNDANGAN_RAPAT = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_SURAT_UNDANGAN_RAPAT = MixHelper.GetLogCode();
                    var FNAME_SURAT_UNDANGAN_RAPAT = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_SURAT_UNDANGAN_RAPAT = "'" + LASTID_SURAT_UNDANGAN_RAPAT + "', " +
                                "'16', " +
                                "'46', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Undangan Rapat Koordinasi RSNI 5 Ver " + VERSION_RAKOR + "', " +
                                "'Surat Undangan Rapat Koordinasi RSNI 5 Ver " + VERSION_RAKOR + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_UNDANGAN_RAPAT_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_SURAT_UNDANGAN_RAPAT.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_SURAT_UNDANGAN_RAPAT + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_SURAT_UNDANGAN_RAPAT + ") VALUES (" + FVALUE_SURAT_UNDANGAN_RAPAT.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_SURAT_UNDANGAN_RAPAT.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_SURAT_UNDANGAN_RAPAT, objekTanggapan, 1);
                }
            }

            HttpPostedFileBase FILE_BERITA_ACARA = Request.Files["BERITA_ACARA"];
            if (FILE_BERITA_ACARA.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_BERITA_ACARA = FILE_BERITA_ACARA.InputStream;

                string EXT_BERITA_ACARA = Path.GetExtension(FILE_BERITA_ACARA.FileName);
                if (EXT_BERITA_ACARA.ToLower() == ".docx" || EXT_BERITA_ACARA.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_BERITA_ACARA);
                    string filePathdoc = path + "BERITA_ACARA_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "BERITA_ACARA_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "BERITA_ACARA_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_BERITA_ACARA = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_BERITA_ACARA = MixHelper.GetLogCode();
                    var FNAME_BERITA_ACARA = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_BERITA_ACARA = "'" + LASTID_BERITA_ACARA + "', " +
                                "'16', " +
                                "'12', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara RSNI 5 Ver " + VERSION_RAKOR + "', " +
                                "'Berita Acara RSNI 5 Ver " + VERSION_RAKOR + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "BERITA_ACARA_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_BERITA_ACARA.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_BERITA_ACARA + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_BERITA_ACARA + ") VALUES (" + FVALUE_BERITA_ACARA.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_BERITA_ACARA.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_BERITA_ACARA, objekTanggapan, 1);
                }
                else
                {
                    FILE_BERITA_ACARA.SaveAs(path + "BERITA_ACARA_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_BERITA_ACARA.ToLower());
                    int LASTID_BERITA_ACARA = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_BERITA_ACARA = MixHelper.GetLogCode();
                    var FNAME_BERITA_ACARA = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_BERITA_ACARA = "'" + LASTID_BERITA_ACARA + "', " +
                                "'16', " +
                                "'12', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara RSNI 5 Ver " + VERSION_RAKOR + "', " +
                                "'Berita Acara RSNI 5 Ver " + VERSION_RAKOR + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "BERITA_ACARA_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_BERITA_ACARA.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_BERITA_ACARA + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_BERITA_ACARA + ") VALUES (" + FVALUE_BERITA_ACARA.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_BERITA_ACARA.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_BERITA_ACARA, objekTanggapan, 1);
                }
            }

            HttpPostedFileBase FILE_DAFTAR_HADIR = Request.Files["DAFTAR_HADIR"];
            if (FILE_DAFTAR_HADIR.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_DAFTAR_HADIR = FILE_DAFTAR_HADIR.InputStream;

                string EXT_DAFTAR_HADIR = Path.GetExtension(FILE_DAFTAR_HADIR.FileName);
                if (EXT_DAFTAR_HADIR.ToLower() == ".docx" || EXT_DAFTAR_HADIR.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_DAFTAR_HADIR);
                    string filePathdoc = path + "DAFTAR_HADIR_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "DAFTAR_HADIR_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "DAFTAR_HADIR_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                    var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                                "'15', " +
                                "'13', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Rapat Koordinasi RSNI 5 Ver " + VERSION_RAKOR + "', " +
                                "'Daftar Hadir Rapat Koordinasi RSNI 5 Ver " + VERSION_RAKOR + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "DAFTAR_HADIR_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_DAFTAR_HADIR.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_DAFTAR_HADIR + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DAFTAR_HADIR + ") VALUES (" + FVALUE_DAFTAR_HADIR.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_DAFTAR_HADIR.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_DAFTAR_HADIR, objekTanggapan, 1);
                }
                else
                {
                    FILE_DAFTAR_HADIR.SaveAs(path + "DAFTAR_HADIR_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_DAFTAR_HADIR.ToLower());
                    int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                    var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                                "'15', " +
                                "'13', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Rapat Koordinasi RSNI 5 Ver " + VERSION_RAKOR + "', " +
                                "'Daftar Hadir Rapat Koordinasi RSNI 5 Ver " + VERSION_RAKOR + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "DAFTAR_HADIR_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_DAFTAR_HADIR.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_DAFTAR_HADIR + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DAFTAR_HADIR + ") VALUES (" + FVALUE_DAFTAR_HADIR.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_DAFTAR_HADIR.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_DAFTAR_HADIR, objekTanggapan, 1);
                }
            }

            HttpPostedFileBase FILE_NOTULEN = Request.Files["NOTULEN"];
            if (FILE_NOTULEN.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_NOTULEN = FILE_NOTULEN.InputStream;

                string EXT_NOTULEN = Path.GetExtension(FILE_NOTULEN.FileName);
                if (EXT_NOTULEN.ToLower() == ".docx" || EXT_NOTULEN.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_NOTULEN);
                    string filePathdoc = path + "NOTULEN_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "NOTULEN_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "NOTULEN_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                    var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                                "'16', " +
                                "'14', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Rapat Koordinasi RSNI 5 Ver " + VERSION_RAKOR + "', " +
                                "'Notulen Rapat Koordinasi RSNI 5 Ver " + VERSION_RAKOR + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "NOTULEN_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_NOTULEN.ToLower().Replace(".", "") + "', " +
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
                    FILE_NOTULEN.SaveAs(path + "NOTULEN_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_NOTULEN.ToLower());
                    int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                    var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                                "'16', " +
                                "'14', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Rapat Koordinasi RSNI 5 Ver " + VERSION_RAKOR + "', " +
                                "'Notulen Rapat Koordinasi RSNI 5 Ver " + VERSION_RAKOR + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "NOTULEN_RSNI5_Ver_" + VERSION_RAKOR + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_NOTULEN.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_NOTULEN + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_NOTULEN + ") VALUES (" + FVALUE_NOTULEN.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_NOTULEN.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_NOTULEN, objekTanggapan, 1);
                }
            }

            HttpPostedFileBase FILE_DATA_RSNI = Request.Files["DATA_RSNI"];
            if (FILE_DATA_RSNI.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                string PATH_SNI_DOC = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                Stream STREAM_DOC_DATA_RSNI = FILE_DATA_RSNI.InputStream;

                string EXT_DATA_RSNI = Path.GetExtension(FILE_DATA_RSNI.FileName);
                if (EXT_DATA_RSNI.ToLower() == ".docx" || EXT_DATA_RSNI.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_DATA_RSNI);
                    string filePathdoc = path + "RSNI5_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "RSNI5_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "RSNI5_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);

                    int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                    var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                                "'16', " +
                                "'15', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") RSNI 5', " +
                                "'RSNI 5 " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI5/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "RSNI5_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_DATA_RSNI.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_DATA_RSNI + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DATA_RSNI + ") VALUES (" + FVALUE_DATA_RSNI.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_DATA_RSNI.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_DATA_RSNI, objekTanggapan, 1);
                }
            }


            var APPROVAL_REASON = "";
            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
            var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
            MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
            var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 8").SingleOrDefault();
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1,'" + APPROVAL_REASON + "'," + DATENOW + "," + USER_ID + ",1,8," + APPROVAL_STATUS_SESSION + ")");

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult DataRSNI5PPS(DataTables param)
        {
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;
            var BIDANG_ID = Convert.ToInt32(Session["BIDANG_ID"]);

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("PROPOSAL_PNPS_CODE");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_IS_URGENT_NAME");
            order_field.Add("PROPOSAL_TAHAPAN");
            order_field.Add("PROPOSAL_STATUS_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "PROPOSAL_STATUS = 8 AND PROPOSAL_STATUS_PROSES = 1  " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");

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
                        search_clause += "LOWER("+fields + ")  LIKE LOWER('%" + search + "%')";
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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " AND (PROPOSAL_IS_BATAL <> '1' OR PROPOSAL_IS_BATAL IS NULL) " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            {
                Convert.ToString("<center>"+list.PROPOSAL_CREATE_DATE_NAME+"</center>"),
                Convert.ToString(list.PROPOSAL_PNPS_CODE),
                Convert.ToString(list.KOMTEK_CODE),
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString("<center>"+list.PROPOSAL_IS_URGENT_NAME+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                Convert.ToString("<center><a href='/Perumusan/RSNI5/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 8 && list.PROPOSAL_STATUS_PROSES == 1)?"<a href='/Perumusan/RSNI5/Pengesahan/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Pengesahan RSNI 5'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
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
