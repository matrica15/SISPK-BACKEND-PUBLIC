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

namespace SISPK.Controllers.Perumusan
{
    [Auth(RoleTipe = 1)]
    public class RSNI3Controller : Controller
    {
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            var IS_KOMTEK = Convert.ToInt32(Session["IS_KOMTEK"]);
            string ViewName = ((IS_KOMTEK == 1) ? "DaftarPenyusunanRSNI3" : "DaftarPengesahanRSNI3");
            return View(ViewName);
        }
        [Auth(RoleTipe = 2)]
        public ActionResult Create(int id = 0)
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
            var BA = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 8 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DH = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 9 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var NT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 10 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var SRT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 35 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var SRTP = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 35 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 6").SingleOrDefault();
            var DetailRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 6 AND PROPOSAL_RAPAT_VERSION = " + (VERSION_RATEK - 1)).FirstOrDefault();
            var DetailHistoryRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS IN (5,6) ").ToList();

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
            ViewData["Notulen"] = NT;
            ViewData["DaftarHadir"] = DH;
            ViewData["Berita"] = BA;
            ViewData["SuratRT"] = SRT;
            ViewData["SRTP"] = SRTP;

            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_RELATED_ID = " + id).ToList();
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 11 AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1 AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
            if (DefaultDokumen == null)
            {
                var DefaultDokumenRSNI1 = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 7 AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1 AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
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
            //string lastItem = Name.Last();
            int lastNameIndex = Name.Length;
            int count = 1;

            foreach (string Res in Name)
            {
                //if (!object.ReferenceEquals(Res, lastItem))
                //{
                //    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' OR ";
                //}
                //else
                //{
                //    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' )";
                //}
                if (count != lastNameIndex)
                {
                    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' OR ";
                }
                else
                {
                    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' )";
                }
                count++;
            }
            var RefLain = db.Database.SqlQuery<VIEW_DOCUMENTS>(QueryRefLain).ToList();
            ViewData["RefLain"] = RefLain;
            return View();
        }
        [HttpPost]
        public ActionResult CreateAnggota(HttpPostedFileBase DATA_RSNI, int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, int SUBMIT_TIPE = 0)
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            if (SUBMIT_TIPE == 1)
            {
                if (DATA_RSNI.ContentLength > 0)
                {
                    var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
                    var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                    Stream stremdokumen = DATA_RSNI.InputStream;
                    byte[] appData = new byte[DATA_RSNI.ContentLength + 1];
                    stremdokumen.Read(appData, 0, DATA_RSNI.ContentLength);
                    string Extension = Path.GetExtension(DATA_RSNI.FileName);
                    if (Extension.ToLower() == ".docx" || Extension.ToLower() == ".doc")
                    {

                        Aspose.Words.Document doc = new Aspose.Words.Document(stremdokumen);
                        doc.RemoveMacros();
                        string filePathdoc = path + "RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                        string filePathpdf = path + "RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                        string filePathxml = path + "RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                        doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                        doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                        doc.Save(@"" + filePathxml);

                        var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 11 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1").SingleOrDefault();
                        if (CEKDOKUMEN != null)
                        {
                            db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                        }
                        int LASTID = MixHelper.GetSequence("TRX_DOCUMENTS");
                        var DATENOW = MixHelper.ConvertDateNow();
                        var LOGCODE_RSNI1 = MixHelper.GetLogCode();
                        var FNAME_RSNI1 = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                        var FVALUE_RSNI1 = "'" + LASTID + "', " +
                                    "'14', " +
                                    "'7', " +
                                    "'" + PROPOSAL_ID + "', " +
                                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") RSNI 3" + "', " +
                                    "'Hasil Rancangan SNI 3 " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                    "'" + "RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                                    "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                    "'0', " +
                                    "'" + USER_ID + "', " +
                                    DATENOW + "," +
                                    "'1', " +
                                    "'" + LOGCODE_RSNI1 + "'";
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_RSNI1 + ") VALUES (" + FVALUE_RSNI1.Replace("''", "NULL") + ")");
                        String objekTanggapan = FVALUE_RSNI1.Replace("'", "-");
                        MixHelper.InsertLog(LOGCODE_RSNI1, objekTanggapan, 1);

                        db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 4, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                        var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                        String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 4, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                        MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                        int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                        db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                        var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 4").SingleOrDefault();
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,4," + APPROVAL_STATUS_SESSION + ")");
                    }
                }
            }
            else
            {
                if (DATA_RSNI.ContentLength > 0)
                {
                    var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
                    var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT"));
                    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                    Stream stremdokumen = DATA_RSNI.InputStream;
                    byte[] appData = new byte[DATA_RSNI.ContentLength + 1];
                    stremdokumen.Read(appData, 0, DATA_RSNI.ContentLength);
                    string Extension = Path.GetExtension(DATA_RSNI.FileName);
                    if (Extension.ToLower() == ".docx" || Extension.ToLower() == ".doc")
                    {

                        Aspose.Words.Document doc = new Aspose.Words.Document(stremdokumen);
                        doc.RemoveMacros();
                        var SNI_DOC_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0)+1 AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.SNI_DOC_TYPE = 2").SingleOrDefault();
                        string filePathdoc = path + "DRAFT_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX";
                        string filePathpdf = path + "DRAFT_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".PDF";
                        string filePathxml = path + "DRAFT_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".XML";
                        doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                        doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                        doc.Save(@"" + filePathxml);
                        doc.Save(@"" + path + "DRAFT_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + ".DOCX", Aspose.Words.SaveFormat.Docx);

                        var DATENOW = MixHelper.ConvertDateNow();
                        var LOGCODE_SNI_DOC = MixHelper.GetLogCode();
                        int LASTID_SNI_DOC = MixHelper.GetSequence("TRX_SNI_DOC");

                        var fname = "SNI_DOC_ID,SNI_DOC_PROPOSAL_ID,SNI_DOC_TYPE,SNI_DOC_FILE_PATH,SNI_DOC_VERSION,SNI_DOC_CREATE_BY,SNI_DOC_CREATE_DATE,SNI_DOC_LOG_CODE,SNI_DOC_STATUS";
                        var fvalue = "'" + LASTID_SNI_DOC + "', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'" + 3 + "', " +
                                        "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/DRAFT/DRAFT_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX" + "', " +
                                        "'" + SNI_DOC_VERSION + "', " +
                                        "'" + USER_ID + "', " +
                                        DATENOW + "," +
                                        "'" + LOGCODE_SNI_DOC + "', " +
                                        "'1'";
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_SNI_DOC (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");


                        var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 11 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1").SingleOrDefault();
                        if (CEKDOKUMEN == null)
                        {
                            int LASTID = MixHelper.GetSequence("TRX_DOCUMENTS");
                            var LOGCODE_RSNI1 = MixHelper.GetLogCode();
                            var FNAME_RSNI1 = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                            var FVALUE_RSNI1 = "'" + LASTID + "', " +
                                        "'14', " +
                                        "'11', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Draft RSNI 3" + "', " +
                                        "'Draft Rancangan SNI 3 " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                        "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/" + "', " +
                                        "'" + "DRAFT_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                                        "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                        "'0', " +
                                        "'" + USER_ID + "', " +
                                        DATENOW + "," +
                                        "'1', " +
                                        "'" + LOGCODE_RSNI1 + "'";
                            db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_RSNI1 + ") VALUES (" + FVALUE_RSNI1.Replace("''", "NULL") + ")");
                            String objekTanggapan = FVALUE_RSNI1.Replace("'", "-");
                            MixHelper.InsertLog(LOGCODE_RSNI1, objekTanggapan, 1);
                        }
                    }
                }
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateRatek(int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, int APPROVAL_TYPE = 0, string NO_RAKON = "", string TGL_RAKON = "", string APPROVAL_REASON = "", string PROPOSAL_JUDUL_PNPS_ENG = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
            int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            var VERSION_RAKON = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 6").SingleOrDefault();
            int LASTID_PROPOSAL_RAPAT = MixHelper.GetSequence("TRX_DOCUMENTS");
            var LOGCODE_PROPOSAL_RAPAT = MixHelper.GetLogCode();
            String TGL_RAKON_CONVERT = "TO_DATE('" + TGL_RAKON + "', 'yyyy-mm-dd hh24:mi:ss')";
            var FNAME_PROPOSAL_RAPAT = "PROPOSAL_RAPAT_ID,PROPOSAL_RAPAT_PROPOSAL_ID,PROPOSAL_RAPAT_NOMOR,PROPOSAL_RAPAT_DATE,PROPOSAL_RAPAT_VERSION,PROPOSAL_RAPAT_APPROVAL_ID,PROPOSAL_RAPAT_PROPOSAL_STATUS";
            var FVALUE_PROPOSAL_RAPAT = "'" + LASTID_PROPOSAL_RAPAT + "', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'" + NO_RAKON + "', " +
                                        TGL_RAKON_CONVERT + ", " +
                                        "'" + VERSION_RAKON + "', " +
                                        "'" + APPROVAL_ID + "', " +
                                        "'6'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_RAPAT (" + FNAME_PROPOSAL_RAPAT + ") VALUES (" + FVALUE_PROPOSAL_RAPAT.Replace("''", "NULL") + ")");


            HttpPostedFileBase FILE_SURAT_UNDANGAN_RAPAT = Request.Files["SURAT_UNDANGAN_RAPAT"];
            if (FILE_SURAT_UNDANGAN_RAPAT.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_SURAT_UNDANGAN_RAPAT = FILE_SURAT_UNDANGAN_RAPAT.InputStream;

                string EXT_SURAT_UNDANGAN_RAPAT = Path.GetExtension(FILE_SURAT_UNDANGAN_RAPAT.FileName);
                if (EXT_SURAT_UNDANGAN_RAPAT.ToLower() == ".docx" || EXT_SURAT_UNDANGAN_RAPAT.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_SURAT_UNDANGAN_RAPAT);
                    doc.RemoveMacros();
                    string filePathdoc = path + "SURAT_UNDANGAN_RAPAT_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "SURAT_UNDANGAN_RAPAT_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "SURAT_UNDANGAN_RAPAT_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_SURAT_UNDANGAN_RAPAT = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_SURAT_UNDANGAN_RAPAT = MixHelper.GetLogCode();
                    var FNAME_SURAT_UNDANGAN_RAPAT = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_SURAT_UNDANGAN_RAPAT = "'" + LASTID_SURAT_UNDANGAN_RAPAT + "', " +
                                "'14', " +
                                "'35', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Undangan Rapat RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Surat Undangan Rapat RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_UNDANGAN_RAPAT_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
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
                    FILE_SURAT_UNDANGAN_RAPAT.SaveAs(path + "SURAT_UNDANGAN_RAPAT_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_SURAT_UNDANGAN_RAPAT.ToLower());
                    int LASTID_SURAT_UNDANGAN_RAPAT = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_SURAT_UNDANGAN_RAPAT = MixHelper.GetLogCode();
                    var FNAME_SURAT_UNDANGAN_RAPAT = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_SURAT_UNDANGAN_RAPAT = "'" + LASTID_SURAT_UNDANGAN_RAPAT + "', " +
                                "'14', " +
                                "'35', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Undangan Rapat RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Surat Undangan Rapat RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_UNDANGAN_RAPAT_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
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

            HttpPostedFileBase FILE_SURAT_PENYERAHAN = Request.Files["SURAT_PENYERAHAN"];
            if (FILE_SURAT_PENYERAHAN.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_SURAT_PENYERAHAN = FILE_SURAT_PENYERAHAN.InputStream;

                string EXT_SURAT_PENYERAHAN = Path.GetExtension(FILE_SURAT_PENYERAHAN.FileName);
                if (EXT_SURAT_PENYERAHAN.ToLower() == ".docx" || EXT_SURAT_PENYERAHAN.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_SURAT_PENYERAHAN);
                    doc.RemoveMacros();
                    string filePathdoc = path + "SURAT_PENYERAHAN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "SURAT_PENYERAHAN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "SURAT_PENYERAHAN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_SURAT_PENYERAHAN = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_SURAT_PENYERAHAN = MixHelper.GetLogCode();
                    var FNAME_SURAT_PENYERAHAN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_SURAT_PENYERAHAN = "'" + LASTID_SURAT_PENYERAHAN + "', " +
                                "'14', " +
                                "'47', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Penyerahan RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Surat Penyerahan RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_PENYERAHAN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_SURAT_PENYERAHAN + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_SURAT_PENYERAHAN + ") VALUES (" + FVALUE_SURAT_PENYERAHAN.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_SURAT_PENYERAHAN.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_SURAT_PENYERAHAN, objekTanggapan, 1);
                }
                else
                {
                    FILE_SURAT_PENYERAHAN.SaveAs(path + "SURAT_PENYERAHAN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_SURAT_PENYERAHAN.ToLower());
                    int LASTID_SURAT_PENYERAHAN = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_SURAT_PENYERAHAN = MixHelper.GetLogCode();
                    var FNAME_SURAT_PENYERAHAN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_SURAT_PENYERAHAN = "'" + LASTID_SURAT_PENYERAHAN + "', " +
                                "'14', " +
                                "'47', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Penyerahan RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Surat Penyerahan RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_PENYERAHAN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_SURAT_PENYERAHAN.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_SURAT_PENYERAHAN + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_SURAT_PENYERAHAN + ") VALUES (" + FVALUE_SURAT_PENYERAHAN.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_SURAT_PENYERAHAN.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_SURAT_PENYERAHAN, objekTanggapan, 1);
                }
            }

            HttpPostedFileBase FILE_BERITA_ACARA_RAKON = Request.Files["BERITA_ACARA_RAKON"];
            if (FILE_BERITA_ACARA_RAKON.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_BERITA_ACARA_RAKON = FILE_BERITA_ACARA_RAKON.InputStream;

                string EXT_BERITA_ACARA_RAKON = Path.GetExtension(FILE_BERITA_ACARA_RAKON.FileName);
                if (EXT_BERITA_ACARA_RAKON.ToLower() == ".docx" || EXT_BERITA_ACARA_RAKON.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_BERITA_ACARA_RAKON);
                    doc.RemoveMacros();
                    string filePathdoc = path + "BERITA_ACARA_RAKON_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "BERITA_ACARA_RAKON_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "BERITA_ACARA_RAKON_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_BERITA_ACARA_RAKON = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_BERITA_ACARA_RAKON = MixHelper.GetLogCode();
                    var FNAME_BERITA_ACARA_RAKON = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_BERITA_ACARA_RAKON = "'" + LASTID_BERITA_ACARA_RAKON + "', " +
                                "'14', " +
                                "'8', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Berita Acara Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "BERITA_ACARA_RAKON_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_BERITA_ACARA_RAKON + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_BERITA_ACARA_RAKON + ") VALUES (" + FVALUE_BERITA_ACARA_RAKON.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_BERITA_ACARA_RAKON.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_BERITA_ACARA_RAKON, objekTanggapan, 1);
                }
                else
                {
                    FILE_BERITA_ACARA_RAKON.SaveAs(path + "BERITA_ACARA_RAKON_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_BERITA_ACARA_RAKON.ToLower());
                    int LASTID_BERITA_ACARA_RAKON = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_BERITA_ACARA_RAKON = MixHelper.GetLogCode();
                    var FNAME_BERITA_ACARA_RAKON = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_BERITA_ACARA_RAKON = "'" + LASTID_BERITA_ACARA_RAKON + "', " +
                                "'14', " +
                                "'8', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Berita Acara Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "BERITA_ACARA_RAKON_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_BERITA_ACARA_RAKON.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_BERITA_ACARA_RAKON + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_BERITA_ACARA_RAKON + ") VALUES (" + FVALUE_BERITA_ACARA_RAKON.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_BERITA_ACARA_RAKON.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_BERITA_ACARA_RAKON, objekTanggapan, 1);
                }
            }

            HttpPostedFileBase FILE_DAFTAR_HADIR = Request.Files["DAFTAR_HADIR"];
            if (FILE_DAFTAR_HADIR.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_DAFTAR_HADIR = FILE_DAFTAR_HADIR.InputStream;

                string EXT_DAFTAR_HADIR = Path.GetExtension(FILE_DAFTAR_HADIR.FileName);
                if (EXT_DAFTAR_HADIR.ToLower() == ".docx" || EXT_DAFTAR_HADIR.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_DAFTAR_HADIR);
                    doc.RemoveMacros();
                    string filePathdoc = path + "DAFTAR_HADIR_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "DAFTAR_HADIR_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "DAFTAR_HADIR_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                    var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                                "'14', " +
                                "'9', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Daftar Hadir Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "DAFTAR_HADIR_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
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
                    FILE_DAFTAR_HADIR.SaveAs(path + "DAFTAR_HADIR_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_DAFTAR_HADIR.ToLower());
                    int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                    var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                                "'14', " +
                                "'9', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Daftar Hadir Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "DAFTAR_HADIR_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_NOTULEN = FILE_NOTULEN.InputStream;

                string EXT_NOTULEN = Path.GetExtension(FILE_NOTULEN.FileName);
                if (EXT_NOTULEN.ToLower() == ".docx" || EXT_NOTULEN.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_NOTULEN);
                    doc.RemoveMacros();
                    string filePathdoc = path + "NOTULEN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "NOTULEN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "NOTULEN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
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
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Notulen Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "NOTULEN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
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
                    FILE_NOTULEN.SaveAs(path + "NOTULEN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_NOTULEN.ToLower());
                    int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                    var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                                "'14', " +
                                "'10', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Notulen Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "NOTULEN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                string PATH_SNI_DOC = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                Stream STREAM_DOC_DATA_RSNI = FILE_DATA_RSNI.InputStream;

                string EXT_DATA_RSNI = Path.GetExtension(FILE_DATA_RSNI.FileName);
                if (EXT_DATA_RSNI.ToLower() == ".docx" || EXT_DATA_RSNI.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_DATA_RSNI);
                    doc.RemoveMacros();
                    var DRAFT_NAME = ((APPROVAL_TYPE == 1) ? "" : "DRAFT_");
                    string filePathdoc = path + DRAFT_NAME + "RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + DRAFT_NAME + "RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + DRAFT_NAME + "RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);

                    int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                    var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                                "'14', " +
                                "'11', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") " + DRAFT_NAME + "RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + DRAFT_NAME + "RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_DATA_RSNI + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DATA_RSNI + ") VALUES (" + FVALUE_DATA_RSNI.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_DATA_RSNI.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_DATA_RSNI, objekTanggapan, 1);
                }
                else
                {
                    //FILE_DATA_RSNI.SaveAs(path + "NOTULEN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_NOTULEN.ToLower());
                    //int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                    //var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                    //var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    //var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                    //            "'14', " +
                    //            "'10', " +
                    //            "'" + PROPOSAL_ID + "', " +
                    //            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + "', " +
                    //            "'Notulen Rapat Konsensus RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                    //            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                    //            "'" + "NOTULEN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                    //            "'" + EXT_NOTULEN.ToLower().Replace(".", "") + "', " +
                    //            "'0', " +
                    //            "'" + USER_ID + "', " +
                    //            DATENOW + "," +
                    //            "'1', " +
                    //            "'" + LOGCODE_NOTULEN + "'";
                    //db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_NOTULEN + ") VALUES (" + FVALUE_NOTULEN.Replace("''", "NULL") + ")");
                    //String objekTanggapan = FVALUE_NOTULEN.Replace("'", "-");
                    //MixHelper.InsertLog(LOGCODE_NOTULEN, objekTanggapan, 1);
                }
            }

            if (APPROVAL_TYPE == 1)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 6, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_JUDUL_PNPS_ENG = '" + PROPOSAL_JUDUL_PNPS_ENG + "', PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 6, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_JUDUL_PNPS_ENG = '" + PROPOSAL_JUDUL_PNPS_ENG + "', PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);


                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 6").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,6," + APPROVAL_STATUS_SESSION + ")");


                if (APPROVAL_STATUS_SESSION > 5)
                {
                    APPROVAL_STATUS_SESSION = 5;
                }
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_RAKON_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = " + TGL_RAKON_CONVERT + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 6, PROPOSAL_JUDUL_PNPS_ENG = '" + PROPOSAL_JUDUL_PNPS_ENG + "', PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 6, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_JUDUL_PNPS_ENG = '" + PROPOSAL_JUDUL_PNPS_ENG + "', PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);


                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 6").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION,APPROVAL_REASON) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0," + DATENOW + "," + USER_ID + ",1,6," + APPROVAL_STATUS_SESSION + ",'" + APPROVAL_REASON + "')");
                if (APPROVAL_STATUS_SESSION > 5)
                {
                    APPROVAL_STATUS_SESSION = 5;
                }
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_RAKON_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = " + TGL_RAKON_CONVERT + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_RAKON_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = " + DATENOW + ", MONITORING_HASIL_APP_RAKON_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = 0 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");


        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateRateksssss(HttpPostedFileBase DATA_RSNI, HttpPostedFileBase BA_RATEK, HttpPostedFileBase SURAT_PENYERAHAN, HttpPostedFileBase DAFTAR_HADIR, HttpPostedFileBase SURAT_RAPAT_TEKNIS, HttpPostedFileBase NOTULEN, int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, int APPROVAL_TYPE = 0, string NO_RATEK = "", string TGL_RATEK = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;

            Stream STREAM_DATA_RSNI = DATA_RSNI.InputStream;
            byte[] APPDATA_DATA_RSNI = new byte[DATA_RSNI.ContentLength + 1];
            STREAM_DATA_RSNI.Read(APPDATA_DATA_RSNI, 0, DATA_RSNI.ContentLength);
            string Extension_DATA_RSNI = Path.GetExtension(DATA_RSNI.FileName);

            Stream STREAM_SURAT_PENYERAHAN = SURAT_PENYERAHAN.InputStream;
            byte[] APPDATA_SURAT_PENYERAHAN = new byte[SURAT_PENYERAHAN.ContentLength + 1];
            STREAM_DATA_RSNI.Read(APPDATA_SURAT_PENYERAHAN, 0, SURAT_PENYERAHAN.ContentLength);
            string Extension_SURAT_PENYERAHAN = Path.GetExtension(SURAT_PENYERAHAN.FileName);

            Stream STREAM_SURAT_RAPAT_TEKNIS = SURAT_RAPAT_TEKNIS.InputStream;
            byte[] APPDATA_SURAT_RAPAT_TEKNIS = new byte[SURAT_RAPAT_TEKNIS.ContentLength + 1];
            STREAM_DATA_RSNI.Read(APPDATA_SURAT_RAPAT_TEKNIS, 0, SURAT_RAPAT_TEKNIS.ContentLength);
            string Extension_SURAT_RAPAT_TEKNIS = Path.GetExtension(SURAT_RAPAT_TEKNIS.FileName);

            Stream STREAM_BA_RATEK = BA_RATEK.InputStream;
            byte[] APPDATA_BA_RATEK = new byte[BA_RATEK.ContentLength + 1];
            STREAM_BA_RATEK.Read(APPDATA_BA_RATEK, 0, BA_RATEK.ContentLength);
            string Extension_BA_RATEK = Path.GetExtension(BA_RATEK.FileName);

            Stream STREAM_DAFTAR_HADIR = DAFTAR_HADIR.InputStream;
            byte[] APPDATA_DAFTAR_HADIR = new byte[DAFTAR_HADIR.ContentLength + 1];
            STREAM_DAFTAR_HADIR.Read(APPDATA_DAFTAR_HADIR, 0, DAFTAR_HADIR.ContentLength);
            string Extension_DAFTAR_HADIR = Path.GetExtension(DAFTAR_HADIR.FileName);

            Stream STREAM_NOTULEN = NOTULEN.InputStream;
            byte[] APPDATA_NOTULEN = new byte[NOTULEN.ContentLength + 1];
            STREAM_NOTULEN.Read(APPDATA_NOTULEN, 0, NOTULEN.ContentLength);
            string Extension_NOTULEN = Path.GetExtension(NOTULEN.FileName);

            int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 6").SingleOrDefault();
            int LASTID_PROPOSAL_RAPAT = MixHelper.GetSequence("TRX_DOCUMENTS");
            var LOGCODE_PROPOSAL_RAPAT = MixHelper.GetLogCode();
            String TGL_RATEK_CONVERT = "TO_DATE('" + TGL_RATEK + "', 'yyyy-mm-dd hh24:mi:ss')";
            var FNAME_PROPOSAL_RAPAT = "PROPOSAL_RAPAT_ID,PROPOSAL_RAPAT_PROPOSAL_ID,PROPOSAL_RAPAT_NOMOR,PROPOSAL_RAPAT_DATE,PROPOSAL_RAPAT_VERSION,PROPOSAL_RAPAT_APPROVAL_ID,PROPOSAL_RAPAT_PROPOSAL_STATUS";
            var FVALUE_PROPOSAL_RAPAT = "'" + LASTID_PROPOSAL_RAPAT + "', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'" + NO_RATEK + "', " +
                                        TGL_RATEK_CONVERT + ", " +
                                        "'" + VERSION_RATEK + "', " +
                                        "'" + APPROVAL_ID + "', " +
                                        "'6'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_RAPAT (" + FNAME_PROPOSAL_RAPAT + ") VALUES (" + FVALUE_PROPOSAL_RAPAT.Replace("''", "NULL") + ")");

            if (Extension_SURAT_PENYERAHAN.ToLower() == ".docx" || Extension_SURAT_PENYERAHAN.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_SURAT_PENYERAHAN);
                doc.RemoveMacros();
                string filePathdoc = path + "SURAT_PENYERAHAN_" + VERSION_RATEK + "_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "SURAT_PENYERAHAN_" + VERSION_RATEK + "_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "SURAT_PENYERAHAN_" + VERSION_RATEK + "_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                int LASTID_SURAT_PENYERAHAN = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_SURAT_PENYERAHAN = MixHelper.GetLogCode();
                var FNAME_SURAT_PENYERAHAN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_SURAT_PENYERAHAN = "'" + LASTID_SURAT_PENYERAHAN + "', " +
                            "'14', " +
                            "'47', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Penyerahan RSNI 3 " + VERSION_RATEK + "" + "', " +
                            "'Surat Penyerahan RSNI 3 " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "SURAT_PENYERAHAN_" + VERSION_RATEK + "_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_SURAT_PENYERAHAN.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_SURAT_PENYERAHAN + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_SURAT_PENYERAHAN + ") VALUES (" + FVALUE_SURAT_PENYERAHAN.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_SURAT_PENYERAHAN.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_SURAT_PENYERAHAN, objekTanggapan, 1);
            }
            else
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                SURAT_PENYERAHAN.SaveAs(path + "SURAT_PENYERAHAN_" + VERSION_RATEK + "_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + Extension_SURAT_PENYERAHAN.ToUpper());

                int LASTID_SURAT_PENYERAHAN = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_SURAT_PENYERAHAN = MixHelper.GetLogCode();
                var FNAME_SURAT_PENYERAHAN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_SURAT_PENYERAHAN = "'" + LASTID_SURAT_PENYERAHAN + "', " +
                            "'14', " +
                            "'47', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Penyerahan RSNI 3 " + VERSION_RATEK + " " + "', " +
                            "'Surat Penyerahan RSNI 3 " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "SURAT_PENYERAHAN_" + VERSION_RATEK + "_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_SURAT_PENYERAHAN.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_SURAT_PENYERAHAN + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_SURAT_PENYERAHAN + ") VALUES (" + FVALUE_SURAT_PENYERAHAN.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_SURAT_PENYERAHAN.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_SURAT_PENYERAHAN, objekTanggapan, 1);
            }

            if (Extension_SURAT_RAPAT_TEKNIS.ToLower() == ".docx" || Extension_SURAT_RAPAT_TEKNIS.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_SURAT_RAPAT_TEKNIS);
                doc.RemoveMacros();
                string filePathdoc = path + "SURAT_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "SURAT_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "SURAT_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                int LASTID_SURAT_RAPAT_TEKNIS = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_SURAT_RAPAT_TEKNIS = MixHelper.GetLogCode();
                var FNAME_SURAT_RAPAT_TEKNIS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_SURAT_RAPAT_TEKNIS = "'" + LASTID_SURAT_RAPAT_TEKNIS + "', " +
                            "'14', " +
                            "'35', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Rapat Teknis " + VERSION_RATEK + " RSNI 3" + "', " +
                            "'Surat Rapat Teknis " + VERSION_RATEK + " RSNI 3" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "SURAT_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_SURAT_RAPAT_TEKNIS.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_SURAT_RAPAT_TEKNIS + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_SURAT_RAPAT_TEKNIS + ") VALUES (" + FVALUE_SURAT_RAPAT_TEKNIS.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_SURAT_RAPAT_TEKNIS.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_SURAT_RAPAT_TEKNIS, objekTanggapan, 1);
            }
            else
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                SURAT_RAPAT_TEKNIS.SaveAs(path + "SURAT_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + Extension_SURAT_RAPAT_TEKNIS.ToUpper());

                int LASTID_SURAT_RAPAT_TEKNIS = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_SURAT_RAPAT_TEKNIS = MixHelper.GetLogCode();
                var FNAME_SURAT_RAPAT_TEKNIS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_SURAT_RAPAT_TEKNIS = "'" + LASTID_SURAT_RAPAT_TEKNIS + "', " +
                            "'14', " +
                            "'35', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Rapat Teknis " + VERSION_RATEK + " RSNI 3" + "', " +
                            "'Surat Rapat Teknis " + VERSION_RATEK + " RSNI 3" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "SURAT_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_SURAT_RAPAT_TEKNIS.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_SURAT_RAPAT_TEKNIS + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_SURAT_RAPAT_TEKNIS + ") VALUES (" + FVALUE_SURAT_RAPAT_TEKNIS.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_SURAT_RAPAT_TEKNIS.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_SURAT_RAPAT_TEKNIS, objekTanggapan, 1);
            }

            if (Extension_BA_RATEK.ToLower() == ".docx" || Extension_BA_RATEK.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_BA_RATEK);
                doc.RemoveMacros();
                string filePathdoc = path + "BERITA_ACARA_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "BERITA_ACARA_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "BERITA_ACARA_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                int LASTID_BA_RATEK = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_BA_RATEK = MixHelper.GetLogCode();
                var FNAME_BA_RATEK = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_BA_RATEK = "'" + LASTID_BA_RATEK + "', " +
                            "'14', " +
                            "'8', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Ver " + VERSION_RATEK + "', " +
                            "'Berita Acara Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "BERITA_ACARA_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_BA_RATEK.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_BA_RATEK + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_BA_RATEK + ") VALUES (" + FVALUE_BA_RATEK.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_BA_RATEK.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_BA_RATEK, objekTanggapan, 1);
            }
            else
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                BA_RATEK.SaveAs(path + "BERITA_ACARA_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + Extension_BA_RATEK.ToUpper());

                int LASTID_BA_RATEK = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_BA_RATEK = MixHelper.GetLogCode();
                var FNAME_BA_RATEK = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_BA_RATEK = "'" + LASTID_BA_RATEK + "', " +
                            "'14', " +
                            "'8', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Ver " + VERSION_RATEK + "', " +
                            "'Berita Acara Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "BERITA_ACARA_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_BA_RATEK.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_BA_RATEK + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_BA_RATEK + ") VALUES (" + FVALUE_BA_RATEK.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_BA_RATEK.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_BA_RATEK, objekTanggapan, 1);
            }

            if (Extension_DAFTAR_HADIR.ToLower() == ".docx" || Extension_DAFTAR_HADIR.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DAFTAR_HADIR);
                doc.RemoveMacros();
                string filePathdoc = path + "DAFTAR_HADIR_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "DAFTAR_HADIR_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "DAFTAR_HADIR_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                            "'14', " +
                            "'9', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Ver " + VERSION_RATEK + "', " +
                            "'Daftar Hadir Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "DAFTAR_HADIR_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_DAFTAR_HADIR.ToUpper().Replace(".", "") + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                DAFTAR_HADIR.SaveAs(path + "DAFTAR_HADIR_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + Extension_DAFTAR_HADIR.ToUpper());

                int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                            "'14', " +
                            "'9', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Ver " + VERSION_RATEK + "', " +
                            "'Daftar Hadir Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "DAFTAR_HADIR_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_DAFTAR_HADIR.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_DAFTAR_HADIR + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DAFTAR_HADIR + ") VALUES (" + FVALUE_DAFTAR_HADIR.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_DAFTAR_HADIR.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_DAFTAR_HADIR, objekTanggapan, 1);
            }

            if (Extension_NOTULEN.ToLower() == ".docx" || Extension_NOTULEN.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_NOTULEN);
                doc.RemoveMacros();
                string filePathdoc = path + "NOTULEN_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "NOTULEN_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "NOTULEN_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
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
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Ver " + VERSION_RATEK + "', " +
                            "'Notulen Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "NOTULEN_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
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
                NOTULEN.SaveAs(path + "NOTULEN_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + Extension_NOTULEN.ToUpper());

                int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                            "'14', " +
                            "'10', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Ver " + VERSION_RATEK + "', " +
                            "'Notulen Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "NOTULEN_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
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

            if (Extension_DATA_RSNI.ToLower() == ".docx" || Extension_DATA_RSNI.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");

                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DATA_RSNI);
                doc.RemoveMacros();
                string filePathdoc = path + "RSNI3_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "RSNI3_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "RSNI3_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                string pathSNIDOC = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                var SNI_DOC_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0)+1 AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.SNI_DOC_TYPE = 3").SingleOrDefault();
                string filePathdocSNI_DOC = pathSNIDOC + "DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX";
                string filePathpdfSNI_DOC = pathSNIDOC + "DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".PDF";
                string filePathxmlSNI_DOC = pathSNIDOC + "DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".XML";
                doc.Save(@"" + filePathdocSNI_DOC, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdfSNI_DOC, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxmlSNI_DOC);
                doc.Save(@"" + pathSNIDOC + "RSNI3_" + PROPOSAL_PNPS_CODE_FIXER + ".DOCX", Aspose.Words.SaveFormat.Docx);

                var LOGCODE_SNI_DOC = MixHelper.GetLogCode();
                int LASTID_SNI_DOC = MixHelper.GetSequence("TRX_SNI_DOC");

                var fname = "SNI_DOC_ID,SNI_DOC_PROPOSAL_ID,SNI_DOC_TYPE,SNI_DOC_FILE_PATH,SNI_DOC_VERSION,SNI_DOC_CREATE_BY,SNI_DOC_CREATE_DATE,SNI_DOC_LOG_CODE,SNI_DOC_STATUS";
                var fvalue = "'" + LASTID_SNI_DOC + "', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'3', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/DRAFT/DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX" + "', " +
                                "'" + SNI_DOC_VERSION + "', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'" + LOGCODE_SNI_DOC + "', " +
                                "'1'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_SNI_DOC (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

                var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 11 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1").SingleOrDefault();
                if (CEKDOKUMEN != null)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                }
                int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                            "'14', " +
                            "'11', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") RSNI 3 Ver " + VERSION_RATEK + "" + "', " +
                            "'RSNI 3 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "RSNI3_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_DATA_RSNI.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_DATA_RSNI + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DATA_RSNI + ") VALUES (" + FVALUE_DATA_RSNI.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_DATA_RSNI.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_DATA_RSNI, objekTanggapan, 1);
            }

            if (APPROVAL_TYPE == 1)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 6, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 6, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);


                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 6").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,6," + APPROVAL_STATUS_SESSION + ")");


                if (APPROVAL_STATUS_SESSION > 5)
                {
                    APPROVAL_STATUS_SESSION = 5;
                }
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_RAKON_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = " + TGL_RATEK_CONVERT + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 6, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 6, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);


                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 6").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0," + DATENOW + "," + USER_ID + ",1,6," + APPROVAL_STATUS_SESSION + ")");
                if (APPROVAL_STATUS_SESSION > 5)
                {
                    APPROVAL_STATUS_SESSION = 5;
                }
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_RAKON_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = " + TGL_RATEK_CONVERT + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_RAKON_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = " + DATENOW + ", MONITORING_HASIL_APP_RAKON_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = 0 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
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

            var SURAT_UNDANGAN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 35 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var SURAT_PENYERAHAN_RSNI = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 47 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var BERITA_ACARA_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 8 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DAFTAR_HADIR_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 9 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var NOTULEN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 10 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var REF_LAIN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 39 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();

            ViewData["SURAT_UNDANGAN_RAPAT"] = SURAT_UNDANGAN_RAPAT;
            ViewData["SURAT_PENYERAHAN_RSNI"] = SURAT_PENYERAHAN_RSNI;
            ViewData["BERITA_ACARA_RAPAT"] = BERITA_ACARA_RAPAT;
            ViewData["DAFTAR_HADIR_RAPAT"] = DAFTAR_HADIR_RAPAT;
            ViewData["NOTULEN_RAPAT"] = NOTULEN_RAPAT;
            ViewData["REF_LAIN"] = REF_LAIN;

            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 6").SingleOrDefault();
            var DetailRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 6 AND PROPOSAL_RAPAT_VERSION = " + (VERSION_RATEK - 1)).FirstOrDefault();
            var DetailHistoryRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS IN (5,6) ").ToList();
            ViewData["ListTas"] = (from t in db.VIEW_MASTER_TAS orderby t.TAS_NAME select t).ToList();
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
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 11 AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1 AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
            if (DefaultDokumen == null)
            {
                var DefaultDokumenRSNI2 = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 7 AND DOC_FOLDER_ID = 13 AND DOC_STATUS = 1 AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
                ViewData["DefaultDokumen"] = DefaultDokumenRSNI2;
            }
            else
            {
                ViewData["DefaultDokumen"] = DefaultDokumen;
            }
            ViewData["Dokumen"] = Dokumen;

            string SearchName = DataProposal.PROPOSAL_JUDUL_PNPS;
            string[] Name = SearchName.Split(' ');
            string QueryRefLain = "SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND (DOC_RELATED_ID <> " + id + " OR DOC_RELATED_ID IS NULL) AND ( ";
            //string lastItem = Name.Last();
            int lastNameIndex = Name.Length;
            int count = 1;

            foreach (string Res in Name)
            {
                //if (!object.ReferenceEquals(Res, lastItem))
                //{
                //    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' OR ";
                //}
                //else
                //{
                //    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' )";
                //}
                if (count != lastNameIndex)
                {
                    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' OR ";
                }
                else
                {
                    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' )";
                }
                count++;
            }
            var RefLain = db.Database.SqlQuery<VIEW_DOCUMENTS>(QueryRefLain).ToList();
            ViewData["RefLain"] = RefLain;

            return View();
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

            var SURAT_UNDANGAN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 35 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var SURAT_PENYERAHAN_RSNI = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 47 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var BERITA_ACARA_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 8 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DAFTAR_HADIR_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 9 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var NOTULEN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 10 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var REF_LAIN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 39 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();

            ViewData["SURAT_UNDANGAN_RAPAT"] = SURAT_UNDANGAN_RAPAT;
            ViewData["SURAT_PENYERAHAN_RSNI"] = SURAT_PENYERAHAN_RSNI;
            ViewData["BERITA_ACARA_RAPAT"] = BERITA_ACARA_RAPAT;
            ViewData["DAFTAR_HADIR_RAPAT"] = DAFTAR_HADIR_RAPAT;
            ViewData["NOTULEN_RAPAT"] = NOTULEN_RAPAT;
            ViewData["REF_LAIN"] = REF_LAIN;

            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 6").SingleOrDefault();
            var DetailRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 6 AND PROPOSAL_RAPAT_VERSION = " + (VERSION_RATEK - 1)).FirstOrDefault();
            var DetailHistoryRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS IN (5,6) ").ToList();

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
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 11 AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1 AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
            if (DefaultDokumen == null)
            {
                var DefaultDokumenRSNI2 = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 7 AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1 AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
                ViewData["DefaultDokumen"] = DefaultDokumenRSNI2;
            }
            else
            {
                ViewData["DefaultDokumen"] = DefaultDokumen;
            }
            ViewData["Dokumen"] = Dokumen;

            string SearchName = DataProposal.PROPOSAL_JUDUL_PNPS;
            string[] Name = SearchName.Split(' ');
            string QueryRefLain = "SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND (DOC_RELATED_ID <> " + id + " OR DOC_RELATED_ID IS NULL) AND ( ";
            //string lastItem = Name.Last();
            int lastNameIndex = Name.Length;
            int count = 1;

            foreach (string Res in Name)
            {
                //if (!object.ReferenceEquals(Res, lastItem))
                //{
                //    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' OR ";
                //}
                //else
                //{
                //    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' )";
                //}
                if (count != lastNameIndex)
                {
                    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' OR ";
                }
                else
                {
                    QueryRefLain += " DOC_NAME_LOWER LIKE '%" + Res.ToLower() + "%' )";
                }
                count++;
            }
            var RefLain = db.Database.SqlQuery<VIEW_DOCUMENTS>(QueryRefLain).ToList();
            ViewData["RefLain"] = RefLain;
            return View();

        }
        [HttpPost]
        public ActionResult Pengesahan(TRX_PROPOSAL input, string PROPOSAL_TAS_DATE, int[] PROPOSAL_REV_MERIVISI_ID, string[] PROPOSAL_ADOPSI_NOMOR_JUDUL, int[] PROPOSAL_REF_SNI_ID, string[] PROPOSAL_REF_NON_SNI, string[] BIBLIOGRAFI, int PROPOSAL_JENIS_ADOPSI_TERJEMAHAN = 0, int PROPOSAL_METODE_ADOPSI_TERJEMAHAN = 0)
        {
            //declare
            int PROPOSAL_ID = 0;
            int PROPOSAL_TAS_ID = 0;
            //string PROPOSAL_TAS_DATE = "";
            int PROPOSAL_KOMTEK_ID = 0;
            string PROPOSAL_PNPS_CODE = "";
            PROPOSAL_ID = Convert.ToInt32(input.PROPOSAL_ID);
            PROPOSAL_TAS_ID = Convert.ToInt32(input.PROPOSAL_TAS_ID);
            //PROPOSAL_TAS_DATE = Convert.ToString(PROPOSAL_TAS_DATE);
            PROPOSAL_KOMTEK_ID = Convert.ToInt32(input.PROPOSAL_KOMTEK_ID);
            PROPOSAL_PNPS_CODE = input.PROPOSAL_PNPS_CODE;
            if (input.PROPOSAL_JENIS_PERUMUSAN == 5)
            {
                input.PROPOSAL_JENIS_ADOPSI = PROPOSAL_JENIS_ADOPSI_TERJEMAHAN;
                input.PROPOSAL_METODE_ADOPSI = PROPOSAL_METODE_ADOPSI_TERJEMAHAN;
            }
            int PROPOSAL_CLASIFICATION_ID = 0;
            int APPROVAL_TYPE = 0;
            int APPROVAL_STATUS = 1;
            string APPROVAL_REASON = "";
            string KLASIFIKASI_SNI = "";

            APPROVAL_TYPE = Convert.ToInt32(Request.Form["APPROVAL_TYPE"]);
            APPROVAL_STATUS = Convert.ToInt32(Request.Form["APPROVAL_STATUS"]);
            APPROVAL_REASON = Request.Form["APPROVAL_REASON"];
            KLASIFIKASI_SNI = Request.Form["KLASIFIKASI_SNI"];

            //return Content("PROPOSAL_ID : " + PROPOSAL_ID + ", PROPOSAL_TAS_ID :" + PROPOSAL_TAS_ID + ", PROPOSAL_TAS_DATE :" + PROPOSAL_TAS_DATE + ", PROPOSAL_KOMTEK_ID :" + PROPOSAL_KOMTEK_ID + ", PROPOSAL_PNPS_CODE :" + PROPOSAL_PNPS_CODE);

            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            String PROPOSAL_TAS_DATE_CONVERT = "TO_DATE('" + PROPOSAL_TAS_DATE + "', 'yyyy-mm-dd hh24:mi:ss')";
            var update = new SISPK.Controllers.Pengajuan.UsulanController();
            var KLAS_JNS_SNI = KLASIFIKASI_SNI;
            var hasilUpdate = update.UpdateProposal(input, Convert.ToInt32(Session["USER_ID"]), PROPOSAL_REV_MERIVISI_ID, PROPOSAL_ADOPSI_NOMOR_JUDUL, PROPOSAL_REF_SNI_ID, PROPOSAL_REF_NON_SNI, BIBLIOGRAFI,null,null);

            var test = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 7,PROPOSAL_TAS_ID = " + PROPOSAL_TAS_ID + ",PROPOSAL_TAS_DATE = " + PROPOSAL_TAS_DATE_CONVERT + ",PROPOSAL_CLASIFICATION_ID = " + PROPOSAL_CLASIFICATION_ID + ", PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", KLASIFIKASI_JNS_SNI = '" + KLAS_JNS_SNI + "', PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
            //return Content("Status Save : " + test);
            if (APPROVAL_TYPE == 1)
            {

                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 7,PROPOSAL_TAS_ID = " + PROPOSAL_TAS_ID + ",PROPOSAL_TAS_DATE = " + PROPOSAL_TAS_DATE_CONVERT + ",PROPOSAL_CLASIFICATION_ID = " + PROPOSAL_CLASIFICATION_ID + ", PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", KLASIFIKASI_JNS_SNI = '" + KLAS_JNS_SNI + "', PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 7, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", KLASIFIKASI_JNS_SNI = '" + KLAS_JNS_SNI + "', PROPOSAL_UPDATE_BY = " + USER_ID + "WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),1) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 6").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,6," + Convert.ToString(((APPROVAL_STATUS_SESSION == 0) ? 1 : APPROVAL_STATUS_SESSION)) + ")");
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_RAKON_" + Convert.ToString(((APPROVAL_STATUS_SESSION == 0) ? 1 : APPROVAL_STATUS_SESSION)) + " = " + DATENOW + ", MONITORING_HASIL_APP_RAKON_" + Convert.ToString(((APPROVAL_STATUS_SESSION == 0) ? 1 : APPROVAL_STATUS_SESSION)) + " = 1 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 6,PROPOSAL_TAS_ID = " + PROPOSAL_TAS_ID + ", PROPOSAL_TAS_DATE = " + PROPOSAL_TAS_DATE_CONVERT + ",PROPOSAL_CLASIFICATION_ID = " + PROPOSAL_CLASIFICATION_ID + ", PROPOSAL_STATUS_PROSES = " + APPROVAL_STATUS + ", PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + " , KLASIFIKASI_JNS_SNI = '" + KLAS_JNS_SNI + "', PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 6, PROPOSAL_STATUS_PROSES = " + APPROVAL_STATUS + ", PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + ", KLASIFIKASI_JNS_SNI = '" + KLAS_JNS_SNI + "' WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),1) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 6").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0,'" + APPROVAL_REASON + "'," + DATENOW + "," + USER_ID + ",1,6," + Convert.ToString(((APPROVAL_STATUS_SESSION == 0) ? 1 : APPROVAL_STATUS_SESSION)) + ")");
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_RAKON_" + Convert.ToString(((APPROVAL_STATUS_SESSION == 0) ? 1 : APPROVAL_STATUS_SESSION)) + " = " + DATENOW + ", MONITORING_HASIL_APP_RAKON_" + Convert.ToString(((APPROVAL_STATUS_SESSION == 0) ? 1 : APPROVAL_STATUS_SESSION)) + " = 0 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }



            var VERSION_RAKON = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 6").SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = PROPOSAL_PNPS_CODE;
            HttpPostedFileBase FILE_LAPORAN_TAS = Request.Files["LAPORAN_TAS"];
            if (FILE_LAPORAN_TAS.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_LAPORAN_TAS = FILE_LAPORAN_TAS.InputStream;

                string EXT_LAPORAN_TAS = Path.GetExtension(FILE_LAPORAN_TAS.FileName);
                if (EXT_LAPORAN_TAS.ToLower() == ".docx" || EXT_LAPORAN_TAS.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_LAPORAN_TAS);
                    doc.RemoveMacros();
                    string filePathdoc = path + "LAPORAN_TAS_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "LAPORAN_TAS_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "LAPORAN_TAS_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_LAPORAN_TAS = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_LAPORAN_TAS = MixHelper.GetLogCode();
                    var FNAME_LAPORAN_TAS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_LAPORAN_TAS = "'" + LASTID_LAPORAN_TAS + "', " +
                                "'14', " +
                                "'37', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Laporan TAS RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Laporan TAS RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "LAPORAN_TAS_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_LAPORAN_TAS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_LAPORAN_TAS + ") VALUES (" + FVALUE_LAPORAN_TAS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_LAPORAN_TAS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_LAPORAN_TAS, objekTanggapan, 1);
                }
                else
                {
                    FILE_LAPORAN_TAS.SaveAs(path + "LAPORAN_TAS_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_LAPORAN_TAS.ToLower());
                    int LASTID_LAPORAN_TAS = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_LAPORAN_TAS = MixHelper.GetLogCode();
                    var FNAME_LAPORAN_TAS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_LAPORAN_TAS = "'" + LASTID_LAPORAN_TAS + "', " +
                                "'14', " +
                                "'35', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Laporan TAS RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Laporan TAS RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "LAPORAN_TAS_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_LAPORAN_TAS.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_LAPORAN_TAS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_LAPORAN_TAS + ") VALUES (" + FVALUE_LAPORAN_TAS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_LAPORAN_TAS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_LAPORAN_TAS, objekTanggapan, 1);
                }
            }
            HttpPostedFileBase FILE_REF_LAIN = Request.Files["REF_LAIN"];
            if (FILE_REF_LAIN.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_REF_LAIN = FILE_REF_LAIN.InputStream;

                string EXT_REF_LAIN = Path.GetExtension(FILE_REF_LAIN.FileName);
                if (EXT_REF_LAIN.ToLower() == ".docx" || EXT_REF_LAIN.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_REF_LAIN);
                    doc.RemoveMacros();
                    string filePathdoc = path + "REF_LAIN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "REF_LAIN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "REF_LAIN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_REF_LAIN = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_REF_LAIN = MixHelper.GetLogCode();
                    var FNAME_REF_LAIN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_REF_LAIN = "'" + LASTID_REF_LAIN + "', " +
                                "'14', " +
                                "'39', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Referensi Lain RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Referensi Lain RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "REF_LAIN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_REF_LAIN + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_REF_LAIN + ") VALUES (" + FVALUE_REF_LAIN.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_REF_LAIN.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_REF_LAIN, objekTanggapan, 1);
                }
                else
                {
                    FILE_REF_LAIN.SaveAs(path + "REF_LAIN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_REF_LAIN.ToLower());
                    int LASTID_REF_LAIN = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_REF_LAIN = MixHelper.GetLogCode();
                    var FNAME_REF_LAIN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_REF_LAIN = "'" + LASTID_REF_LAIN + "', " +
                                "'14', " +
                                "'39', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Referensi Lain RSNI 3 Ver " + VERSION_RAKON + "', " +
                                "'Referensi Lain RSNI 3 Ver " + VERSION_RAKON + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI3/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "REF_LAIN_RSNI3_Ver_" + VERSION_RAKON + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_REF_LAIN.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_REF_LAIN + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_REF_LAIN + ") VALUES (" + FVALUE_REF_LAIN.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_REF_LAIN.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_REF_LAIN, objekTanggapan, 1);
                }
            }


            return RedirectToAction("Index");
        }

        public ActionResult DataRSNI3Komtek(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
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

            string where_clause = "PROPOSAL_STATUS = 6 AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID;

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
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString("<center>"+list.PROPOSAL_IS_URGENT_NAME+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                Convert.ToString("<center><a href='/Perumusan/RSNI3/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 6 && list.PROPOSAL_STATUS_PROSES == 0)?"<a href='/Perumusan/RSNI3/Create/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Susun RSNI 3'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DataRSNI3PPS(DataTables param)
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


            string where_clause = "PROPOSAL_STATUS = 6 AND (PROPOSAL_STATUS_PROSES = 1 OR PROPOSAL_STATUS_PROSES = 2)  " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");

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

            //return Content(inject_clause_select);

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
                Convert.ToString("<center><a href='/Perumusan/RSNI3/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 6 && list.PROPOSAL_STATUS_PROSES == 1)?"<a href='/Perumusan/RSNI3/Pengesahan/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Pengesahan RSNI 3'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),

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
