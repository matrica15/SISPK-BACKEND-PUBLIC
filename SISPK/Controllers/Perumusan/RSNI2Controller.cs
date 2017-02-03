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
    public class RSNI2Controller : Controller
    {
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            var IS_KOMTEK = Convert.ToInt32(Session["IS_KOMTEK"]);
            string ViewName = ((IS_KOMTEK == 1) ? "DaftarPenyusunanRSNI2" : "DaftarPengesahanRSNI2");
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

            var BA = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 4 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DH = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 5 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var NT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 6 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var SRT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 34 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 5").SingleOrDefault();
            var DetailRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 5 AND PROPOSAL_RAPAT_VERSION = " + (VERSION_RATEK - 1)).FirstOrDefault();
            var DetailHistoryRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 5 ").ToList();

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

            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_RELATED_ID = " + id).ToList();
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 7 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            if (DefaultDokumen == null)
            {
                var DefaultDokumenRSNI1 = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 3 AND DOC_FOLDER_ID = 12 AND DOC_STATUS = 1").SingleOrDefault();
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
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                    Stream stremdokumen = DATA_RSNI.InputStream;
                    byte[] appData = new byte[DATA_RSNI.ContentLength + 1];
                    stremdokumen.Read(appData, 0, DATA_RSNI.ContentLength);
                    string Extension = Path.GetExtension(DATA_RSNI.FileName);
                    if (Extension.ToLower() == ".docx" || Extension.ToLower() == ".doc")
                    {

                        Aspose.Words.Document doc = new Aspose.Words.Document(stremdokumen);
                        string filePathdoc = path + "RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                        string filePathpdf = path + "RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                        string filePathxml = path + "RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                        doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                        doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                        doc.Save(@"" + filePathxml);

                        var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 7 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 13 AND DOC_STATUS = 1").SingleOrDefault();
                        if (CEKDOKUMEN != null)
                        {
                            db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                        }
                        int LASTID = MixHelper.GetSequence("TRX_DOCUMENTS");
                        var DATENOW = MixHelper.ConvertDateNow();
                        var LOGCODE_RSNI1 = MixHelper.GetLogCode();
                        var FNAME_RSNI1 = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                        var FVALUE_RSNI1 = "'" + LASTID + "', " +
                                    "'13', " +
                                    "'7', " +
                                    "'" + PROPOSAL_ID + "', " +
                                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") RSNI 2" + "', " +
                                    "'Hasil Rancangan SNI 2 " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                    "'" + "RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
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
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT"));
                    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                    Stream stremdokumen = DATA_RSNI.InputStream;
                    byte[] appData = new byte[DATA_RSNI.ContentLength + 1];
                    stremdokumen.Read(appData, 0, DATA_RSNI.ContentLength);
                    string Extension = Path.GetExtension(DATA_RSNI.FileName);
                    if (Extension.ToLower() == ".docx" || Extension.ToLower() == ".doc")
                    {

                        Aspose.Words.Document doc = new Aspose.Words.Document(stremdokumen);
                        var SNI_DOC_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0)+1 AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.SNI_DOC_TYPE = 2").SingleOrDefault();
                        string filePathdoc = path + "DRAFT_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX";
                        string filePathpdf = path + "DRAFT_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".PDF";
                        string filePathxml = path + "DRAFT_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".XML";
                        doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                        doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                        doc.Save(@"" + filePathxml);
                        doc.Save(@"" + path + "DRAFT_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + ".DOCX", Aspose.Words.SaveFormat.Docx);

                        var DATENOW = MixHelper.ConvertDateNow();
                        var LOGCODE_SNI_DOC = MixHelper.GetLogCode();
                        int LASTID_SNI_DOC = MixHelper.GetSequence("TRX_SNI_DOC");

                        var fname = "SNI_DOC_ID,SNI_DOC_PROPOSAL_ID,SNI_DOC_TYPE,SNI_DOC_FILE_PATH,SNI_DOC_VERSION,SNI_DOC_CREATE_BY,SNI_DOC_CREATE_DATE,SNI_DOC_LOG_CODE,SNI_DOC_STATUS";
                        var fvalue = "'" + LASTID_SNI_DOC + "', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'" + 2 + "', " +
                                        "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/DRAFT/DRAFT_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX" + "', " +
                                        "'" + SNI_DOC_VERSION + "', " +
                                        "'" + USER_ID + "', " +
                                        DATENOW + "," +
                                        "'" + LOGCODE_SNI_DOC + "', " +
                                        "'1'";
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_SNI_DOC (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");


                        var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 7 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 13 AND DOC_STATUS = 1").SingleOrDefault();
                        if (CEKDOKUMEN == null)
                        {
                            int LASTID = MixHelper.GetSequence("TRX_DOCUMENTS");
                            var LOGCODE_RSNI1 = MixHelper.GetLogCode();
                            var FNAME_RSNI1 = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                            var FVALUE_RSNI1 = "'" + LASTID + "', " +
                                        "'13', " +
                                        "'7', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Draft RSNI 2" + "', " +
                                        "'Draft Rancangan SNI 2 " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                        "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/" + "', " +
                                        "'" + "DRAFT_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
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
        public ActionResult CreateRatek(HttpPostedFileBase DATA_RSNI, int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, int APPROVAL_TYPE = 0, string NO_RATEK = "", string TGL_RATEK = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
            int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 5").SingleOrDefault();
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
                                        "'5'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_RAPAT (" + FNAME_PROPOSAL_RAPAT + ") VALUES (" + FVALUE_PROPOSAL_RAPAT.Replace("''", "NULL") + ")");



            HttpPostedFileBase FILE_SURAT_RAPAT_TEKNIS = Request.Files["SURAT_RAPAT_TEKNIS"];
            if (FILE_SURAT_RAPAT_TEKNIS.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_SURAT_RAPAT_TEKNIS = FILE_SURAT_RAPAT_TEKNIS.InputStream;

                string EXT_SURAT_RAPAT_TEKNIS = Path.GetExtension(FILE_SURAT_RAPAT_TEKNIS.FileName);
                if (EXT_SURAT_RAPAT_TEKNIS.ToLower() == ".docx" || EXT_SURAT_RAPAT_TEKNIS.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_SURAT_RAPAT_TEKNIS);
                    string filePathdoc = path + "SURAT_RAPAT_TEKNIS_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "SURAT_RAPAT_TEKNIS_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "SURAT_RAPAT_TEKNIS_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_SURAT_RAPAT_TEKNIS = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_SURAT_RAPAT_TEKNIS = MixHelper.GetLogCode();
                    var FNAME_SURAT_RAPAT_TEKNIS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_SURAT_RAPAT_TEKNIS = "'" + LASTID_SURAT_RAPAT_TEKNIS + "', " +
                                "'13', " +
                                "'34', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + "', " +
                                "'Surat Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_RAPAT_TEKNIS_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
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
                    FILE_SURAT_RAPAT_TEKNIS.SaveAs(path + "SURAT_RAPAT_TEKNIS_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_SURAT_RAPAT_TEKNIS.ToLower());
                    int LASTID_SURAT_RAPAT_TEKNIS = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_SURAT_RAPAT_TEKNIS = MixHelper.GetLogCode();
                    var FNAME_SURAT_RAPAT_TEKNIS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_SURAT_RAPAT_TEKNIS = "'" + LASTID_SURAT_RAPAT_TEKNIS + "', " +
                                "'13', " +
                                "'34', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + "', " +
                                "'Surat Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_RAPAT_TEKNIS_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_SURAT_RAPAT_TEKNIS.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_SURAT_RAPAT_TEKNIS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_SURAT_RAPAT_TEKNIS + ") VALUES (" + FVALUE_SURAT_RAPAT_TEKNIS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_SURAT_RAPAT_TEKNIS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_SURAT_RAPAT_TEKNIS, objekTanggapan, 1);
                }
            }

            HttpPostedFileBase FILE_SURAT_UNDANGAN_RAPAT = Request.Files["SURAT_UNDANGAN_RAPAT"];
            if (FILE_SURAT_UNDANGAN_RAPAT.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_SURAT_UNDANGAN_RAPAT = FILE_SURAT_UNDANGAN_RAPAT.InputStream;

                string EXT_SURAT_UNDANGAN_RAPAT = Path.GetExtension(FILE_SURAT_UNDANGAN_RAPAT.FileName);
                if (EXT_SURAT_UNDANGAN_RAPAT.ToLower() == ".docx" || EXT_SURAT_UNDANGAN_RAPAT.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_SURAT_UNDANGAN_RAPAT);
                    string filePathdoc = path + "SURAT_UNDANGAN_RAPAT_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "SURAT_UNDANGAN_RAPAT_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "SURAT_UNDANGAN_RAPAT_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_SURAT_UNDANGAN_RAPAT = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_SURAT_UNDANGAN_RAPAT = MixHelper.GetLogCode();
                    var FNAME_SURAT_UNDANGAN_RAPAT = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_SURAT_UNDANGAN_RAPAT = "'" + LASTID_SURAT_UNDANGAN_RAPAT + "', " +
                                "'13', " +
                                "'40', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Undangan Rapat RSNI 2 Ver " + VERSION_RATEK + "', " +
                                "'Surat Undangan Rapat RSNI 2 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_UNDANGAN_RAPAT_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
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
                    FILE_SURAT_UNDANGAN_RAPAT.SaveAs(path + "SURAT_UNDANGAN_RAPAT_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_SURAT_UNDANGAN_RAPAT.ToLower());
                    int LASTID_SURAT_UNDANGAN_RAPAT = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_SURAT_UNDANGAN_RAPAT = MixHelper.GetLogCode();
                    var FNAME_SURAT_UNDANGAN_RAPAT = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_SURAT_UNDANGAN_RAPAT = "'" + LASTID_SURAT_UNDANGAN_RAPAT + "', " +
                                "'13', " +
                                "'40', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Undangan Rapat RSNI 2 Ver " + VERSION_RATEK + "', " +
                                "'Surat Undangan Rapat RSNI 2 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_UNDANGAN_RAPAT_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
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
            HttpPostedFileBase FILE_BERITA_ACARA_RATEK = Request.Files["BERITA_ACARA_RATEK"];
            if (FILE_BERITA_ACARA_RATEK.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_BERITA_ACARA_RATEK = FILE_BERITA_ACARA_RATEK.InputStream;

                string EXT_BERITA_ACARA_RATEK = Path.GetExtension(FILE_BERITA_ACARA_RATEK.FileName);
                if (EXT_BERITA_ACARA_RATEK.ToLower() == ".docx" || EXT_BERITA_ACARA_RATEK.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_BERITA_ACARA_RATEK);
                    string filePathdoc = path + "BERITA_ACARA_RATEK_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "BERITA_ACARA_RATEK_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "BERITA_ACARA_RATEK_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_BERITA_ACARA_RATEK = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_BERITA_ACARA_RATEK = MixHelper.GetLogCode();
                    var FNAME_BERITA_ACARA_RATEK = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_BERITA_ACARA_RATEK = "'" + LASTID_BERITA_ACARA_RATEK + "', " +
                                "'13', " +
                                "'4', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + "', " +
                                "'Berita Acara Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "BERITA_ACARA_RATEK_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_BERITA_ACARA_RATEK + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_BERITA_ACARA_RATEK + ") VALUES (" + FVALUE_BERITA_ACARA_RATEK.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_BERITA_ACARA_RATEK.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_BERITA_ACARA_RATEK, objekTanggapan, 1);
                }
                else
                {
                    FILE_BERITA_ACARA_RATEK.SaveAs(path + "BERITA_ACARA_RATEK_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_BERITA_ACARA_RATEK.ToLower());
                    int LASTID_BERITA_ACARA_RATEK = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_BERITA_ACARA_RATEK = MixHelper.GetLogCode();
                    var FNAME_BERITA_ACARA_RATEK = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_BERITA_ACARA_RATEK = "'" + LASTID_BERITA_ACARA_RATEK + "', " +
                                "'13', " +
                                "'4', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + "', " +
                                "'Berita Acara Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "BERITA_ACARA_RATEK_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_BERITA_ACARA_RATEK.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_BERITA_ACARA_RATEK + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_BERITA_ACARA_RATEK + ") VALUES (" + FVALUE_BERITA_ACARA_RATEK.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_BERITA_ACARA_RATEK.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_BERITA_ACARA_RATEK, objekTanggapan, 1);
                }
            }
            HttpPostedFileBase FILE_DAFTAR_HADIR = Request.Files["DAFTAR_HADIR"];
            if (FILE_DAFTAR_HADIR.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_DAFTAR_HADIR = FILE_DAFTAR_HADIR.InputStream;

                string EXT_DAFTAR_HADIR = Path.GetExtension(FILE_DAFTAR_HADIR.FileName);
                if (EXT_DAFTAR_HADIR.ToLower() == ".docx" || EXT_DAFTAR_HADIR.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_DAFTAR_HADIR);
                    string filePathdoc = path + "DAFTAR_HADIR_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "DAFTAR_HADIR_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "DAFTAR_HADIR_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                    var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                                "'13', " +
                                "'5', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + "', " +
                                "'Daftar Hadir Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "DAFTAR_HADIR_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
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
                    FILE_DAFTAR_HADIR.SaveAs(path + "DAFTAR_HADIR_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_DAFTAR_HADIR.ToLower());
                    int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                    var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                                "'13', " +
                                "'5', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + "', " +
                                "'Daftar Hadir Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "DAFTAR_HADIR_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_NOTULEN = FILE_NOTULEN.InputStream;

                string EXT_NOTULEN = Path.GetExtension(FILE_NOTULEN.FileName);
                if (EXT_NOTULEN.ToLower() == ".docx" || EXT_NOTULEN.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_NOTULEN);
                    string filePathdoc = path + "NOTULEN_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "NOTULEN_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "NOTULEN_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                    var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                                "'13', " +
                                "'6', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + "', " +
                                "'Notulen Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "NOTULEN_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
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
                    FILE_NOTULEN.SaveAs(path + "NOTULEN_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + EXT_NOTULEN.ToLower());
                    int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                    var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                                "'13', " +
                                "'6', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + "', " +
                                "'Notulen Rapat Teknis RSNI 2 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "NOTULEN_RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                string PATH_SNI_DOC = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                Stream STREAM_DOC_DATA_RSNI = FILE_DATA_RSNI.InputStream;

                string EXT_DATA_RSNI = Path.GetExtension(FILE_DATA_RSNI.FileName);
                if (EXT_DATA_RSNI.ToLower() == ".docx" || EXT_DATA_RSNI.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_DATA_RSNI);
                    var DRAFT_NAME = ((APPROVAL_TYPE == 1) ? "" : "DRAFT_");
                    string filePathdoc = path + DRAFT_NAME + "RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + DRAFT_NAME + "RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + DRAFT_NAME + "RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);

                    int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                    var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                                "'13', " +
                                "'7', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") " + DRAFT_NAME + "RSNI 2 Ver " + VERSION_RATEK + "', " +
                                "'RSNI 2 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + DRAFT_NAME + "RSNI2_Ver_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
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
            }

            if (APPROVAL_TYPE == 1)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);


                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 5").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,5," + APPROVAL_STATUS_SESSION + ")");


                if (VERSION_RATEK > 5)
                {
                    VERSION_RATEK = 5;
                }
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_RATEK_" + Convert.ToString(VERSION_RATEK) + " = " + TGL_RATEK_CONVERT + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);


                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 5").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0," + DATENOW + "," + USER_ID + ",1,5," + APPROVAL_STATUS_SESSION + ")");

                if (VERSION_RATEK > 5)
                {
                    VERSION_RATEK = 5;
                }
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_RATEK_" + Convert.ToString(VERSION_RATEK) + " = " + TGL_RATEK_CONVERT + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_RATEK_" + Convert.ToString(VERSION_RATEK) + " = " + DATENOW + ", MONITORING_HASIL_APP_RATEK_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = 0 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");

        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateRatekBackUp(HttpPostedFileBase DATA_RSNI, HttpPostedFileBase BA_RATEK, HttpPostedFileBase DAFTAR_HADIR, HttpPostedFileBase NOTULEN, HttpPostedFileBase SURAT_RAPAT_TEKNIS, int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, int APPROVAL_TYPE = 0, string NO_RATEK = "", string TGL_RATEK = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");

            Stream STREAM_DATA_RSNI = DATA_RSNI.InputStream;
            byte[] APPDATA_DATA_RSNI = new byte[DATA_RSNI.ContentLength + 1];
            STREAM_DATA_RSNI.Read(APPDATA_DATA_RSNI, 0, DATA_RSNI.ContentLength);
            string Extension_DATA_RSNI = Path.GetExtension(DATA_RSNI.FileName);

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
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 5").SingleOrDefault();
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
                                        "'5'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_RAPAT (" + FNAME_PROPOSAL_RAPAT + ") VALUES (" + FVALUE_PROPOSAL_RAPAT.Replace("''", "NULL") + ")");

            if (Extension_SURAT_RAPAT_TEKNIS.ToLower() == ".docx" || Extension_SURAT_RAPAT_TEKNIS.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_SURAT_RAPAT_TEKNIS);
                string filePathdoc = path + "SURAT_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".docx";
                string filePathpdf = path + "SURAT_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                string filePathxml = path + "SURAT_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                int LASTID_SURAT_RAPAT_TEKNIS = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_SURAT_RAPAT_TEKNIS = MixHelper.GetLogCode();
                var FNAME_SURAT_RAPAT_TEKNIS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_SURAT_RAPAT_TEKNIS = "'" + LASTID_SURAT_RAPAT_TEKNIS + "', " +
                            "'13', " +
                            "'34', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Rapat Teknis " + VERSION_RATEK + " RSNI 2" + "', " +
                            "'Surat Rapat Teknis " + VERSION_RATEK + " RSNI 2" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "SURAT_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
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
                SURAT_RAPAT_TEKNIS.SaveAs(path + "SURAT_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + Extension_SURAT_RAPAT_TEKNIS.ToUpper());

                int LASTID_SURAT_RAPAT_TEKNIS = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_SURAT_RAPAT_TEKNIS = MixHelper.GetLogCode();
                var FNAME_SURAT_RAPAT_TEKNIS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_SURAT_RAPAT_TEKNIS = "'" + LASTID_SURAT_RAPAT_TEKNIS + "', " +
                            "'13', " +
                            "'34', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Surat Rapat Teknis " + VERSION_RATEK + " RSNI 2" + "', " +
                            "'Surat Rapat Teknis " + VERSION_RATEK + " RSNI 2" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "SURAT_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_BA_RATEK);
                string filePathdoc = path + "BERITA_ACARA_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".docx";
                string filePathpdf = path + "BERITA_ACARA_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                string filePathxml = path + "BERITA_ACARA_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                int LASTID_BA_RATEK = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_BA_RATEK = MixHelper.GetLogCode();
                var FNAME_BA_RATEK = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_BA_RATEK = "'" + LASTID_BA_RATEK + "', " +
                            "'13', " +
                            "'4', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Rapat Teknis " + VERSION_RATEK + " RSNI 2" + "', " +
                            "'Berita Acara Rapat Teknis " + VERSION_RATEK + " RSNI 1" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "BERITA_ACARA_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                BA_RATEK.SaveAs(path + "BERITA_ACARA_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + Extension_BA_RATEK.ToUpper());
                
                int LASTID_BA_RATEK = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_BA_RATEK = MixHelper.GetLogCode();
                var FNAME_BA_RATEK = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_BA_RATEK = "'" + LASTID_BA_RATEK + "', " +
                            "'13', " +
                            "'4', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Rapat Teknis " + VERSION_RATEK + " RSNI 2" + "', " +
                            "'Berita Acara Rapat Teknis " + VERSION_RATEK + " RSNI 2" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "BERITA_ACARA_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DAFTAR_HADIR);
                string filePathdoc = path + "DAFTAR_HADIR_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".docx";
                string filePathpdf = path + "DAFTAR_HADIR_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                string filePathxml = path + "DAFTAR_HADIR_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                            "'13', " +
                            "'5', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Rapat Teknis " + VERSION_RATEK + " RSNI 2" + "', " +
                            "'Daftar Hadir Rapat Teknis " + VERSION_RATEK + " RSNI 1" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "DAFTAR_HADIR_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                DAFTAR_HADIR.SaveAs(path + "DAFTAR_HADIR_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + Extension_DAFTAR_HADIR.ToUpper());
              
                int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                            "'13', " +
                            "'5', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Rapat Teknis " + VERSION_RATEK + " RSNI 2" + "', " +
                            "'Daftar Hadir Rapat Teknis " + VERSION_RATEK + " RSNI 2" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "DAFTAR_HADIR_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_NOTULEN);
                string filePathdoc = path + "NOTULEN_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".docx";
                string filePathpdf = path + "NOTULEN_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                string filePathxml = path + "NOTULEN_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

               
                int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                            "'13', " +
                            "'6', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Rapat Teknis " + VERSION_RATEK + " RSNI 2" + "', " +
                            "'Notulen Rapat Teknis " + VERSION_RATEK + " RSNI 1" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "NOTULEN_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                NOTULEN.SaveAs(path + "NOTULEN_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + Extension_NOTULEN.ToUpper());
               
                int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                            "'13', " +
                            "'6', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Rapat Teknis " + VERSION_RATEK + " RSNI 2" + "', " +
                            "'Notulen Rapat Teknis " + VERSION_RATEK + " RSNI 2" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "NOTULEN_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DATA_RSNI);
                string filePathdoc = path + "RSNI2_RAPAT_TEKNIS_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".docx";
                string filePathpdf = path + "RSNI2_RAPAT_TEKNIS_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                string filePathxml = path + "RSNI2_RAPAT_TEKNIS_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                string pathSNIDOC = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                var SNI_DOC_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0)+1 AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.SNI_DOC_TYPE = 2").SingleOrDefault();
                string filePathdocSNI_DOC = pathSNIDOC + "DRAFT_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX";
                string filePathpdfSNI_DOC = pathSNIDOC + "DRAFT_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".PDF";
                string filePathxmlSNI_DOC = pathSNIDOC + "DRAFT_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".XML";
                doc.Save(@"" + filePathdocSNI_DOC, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdfSNI_DOC, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxmlSNI_DOC);
                doc.Save(@"" + pathSNIDOC + "RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG +(Extension_DATA_RSNI.ToUpper()), Aspose.Words.SaveFormat.Docx);

                var LOGCODE_SNI_DOC = MixHelper.GetLogCode();
                int LASTID_SNI_DOC = MixHelper.GetSequence("TRX_SNI_DOC");

                var fname = "SNI_DOC_ID,SNI_DOC_PROPOSAL_ID,SNI_DOC_TYPE,SNI_DOC_FILE_PATH,SNI_DOC_VERSION,SNI_DOC_CREATE_BY,SNI_DOC_CREATE_DATE,SNI_DOC_LOG_CODE,SNI_DOC_STATUS";
                var fvalue = "'" + LASTID_SNI_DOC + "', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + 2 + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/DRAFT/DRAFT_RSNI2_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX" + "', " +
                                "'" + SNI_DOC_VERSION + "', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'" + LOGCODE_SNI_DOC + "', " +
                                "'1'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_SNI_DOC (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

                var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 7 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 13 AND DOC_STATUS = 1").SingleOrDefault();
                if (CEKDOKUMEN != null)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                }
                int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                            "'13', " +
                            "'7', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") RSNI 2 Rapat Teknis " + VERSION_RATEK + "" + "', " +
                            "'RSNI 2 Rapat Teknis " + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI2/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "RSNI2_RAPAT_TEKNIS_" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
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
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);


                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 5").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,5," + APPROVAL_STATUS_SESSION + ")");


                if (VERSION_RATEK > 5)
                {
                    VERSION_RATEK = 5;
                }
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_RATEK_" + Convert.ToString(VERSION_RATEK) + " = " + TGL_RATEK_CONVERT + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);


                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 5").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0," + DATENOW + "," + USER_ID + ",1,5," + APPROVAL_STATUS_SESSION + ")");

                if (VERSION_RATEK > 5)
                {
                    VERSION_RATEK = 5;
                }
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_RATEK_" + Convert.ToString(VERSION_RATEK) + " = " + TGL_RATEK_CONVERT + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_RATEK_" + Convert.ToString(VERSION_RATEK) + " = " + DATENOW + ", MONITORING_HASIL_APP_RATEK_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = 0 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
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
            var SURAT_RAPAT_TEKNIS = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 34 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var SURAT_UNDANGAN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 40 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var BERITA_ACARA_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 4 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DAFTAR_HADIR_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 5 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var NOTULEN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 6 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            ViewData["SURAT_RAPAT_TEKNIS"] = SURAT_RAPAT_TEKNIS;
            ViewData["SURAT_UNDANGAN_RAPAT"] = SURAT_UNDANGAN_RAPAT;
            ViewData["BERITA_ACARA_RAPAT"] = BERITA_ACARA_RAPAT;
            ViewData["DAFTAR_HADIR_RAPAT"] = DAFTAR_HADIR_RAPAT;
            ViewData["NOTULEN_RAPAT"] = NOTULEN_RAPAT;

            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 5").SingleOrDefault();
            var DetailRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 5 AND PROPOSAL_RAPAT_VERSION = " + (VERSION_RATEK - 1)).FirstOrDefault();
            var DetailHistoryRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 5 ").ToList();

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
            ViewData["DetailRatek"] = (DetailRatek == null) ? null : DetailRatek;

            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_RELATED_ID = " + id).ToList();
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 7 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            ViewData["Dokumen"] = Dokumen;
            ViewData["DefaultDokumen"] = DefaultDokumen;

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
            var SURAT_RAPAT_TEKNIS = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 34 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var SURAT_UNDANGAN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 40 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var BERITA_ACARA_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 4 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DAFTAR_HADIR_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 5 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var NOTULEN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 6 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            ViewData["SURAT_RAPAT_TEKNIS"] = SURAT_RAPAT_TEKNIS;
            ViewData["SURAT_UNDANGAN_RAPAT"] = SURAT_UNDANGAN_RAPAT;
            ViewData["BERITA_ACARA_RAPAT"] = BERITA_ACARA_RAPAT;
            ViewData["DAFTAR_HADIR_RAPAT"] = DAFTAR_HADIR_RAPAT;
            ViewData["NOTULEN_RAPAT"] = NOTULEN_RAPAT;
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 5").SingleOrDefault();
            var DetailRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 5 AND PROPOSAL_RAPAT_VERSION = " + (VERSION_RATEK - 1)).FirstOrDefault();
            var DetailHistoryRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 5 ").ToList();

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
            ViewData["DetailRatek"] = (DetailRatek == null) ? null : DetailRatek;

            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_RELATED_ID = " + id).ToList();
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 7 AND AA.DOC_FOLDER_ID = 13 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            ViewData["Dokumen"] = Dokumen;
            ViewData["DefaultDokumen"] = DefaultDokumen;

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
        public ActionResult Pengesahan(int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, string PROPOSAL_PNPS_CODE = "", int APPROVAL_TYPE = 0, int APPROVAL_STATUS = 1, string APPROVAL_REASON = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            if (APPROVAL_TYPE == 1)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 6, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 6, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 5").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,5," + APPROVAL_STATUS_SESSION + ")");
                if (APPROVAL_STATUS_SESSION > 5) {
                    APPROVAL_STATUS_SESSION = 5;
                }
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_RATEK_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = " + DATENOW + ", MONITORING_HASIL_APP_RATEK_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = 1 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = " + APPROVAL_STATUS + ", PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = " + APPROVAL_STATUS + ", PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 5").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0,'" + APPROVAL_REASON + "'," + DATENOW + "," + USER_ID + ",1,5," + APPROVAL_STATUS_SESSION + ")");
                if (APPROVAL_STATUS_SESSION > 5)
                {
                    APPROVAL_STATUS_SESSION = 5;
                }
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_RATEK_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = " + DATENOW + ", MONITORING_HASIL_APP_RATEK_" + Convert.ToString(APPROVAL_STATUS_SESSION) + " = 0 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        
        [HttpPost]
        public ActionResult SetVoteUser(string POLLING_REASON = "", int PROPOSAL_ID = 0, int POLLING_ID = 0, int POLLING_OPTION = 0)
        {
            var UserId = Convert.ToInt32(Session["USER_ID"]);
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("TRX_POLLING_DETAILS");
            var datenow = MixHelper.ConvertDateNow();


            var fname = "POLLING_DETAIL_ID,POLLING_DETAIL_POLLING_ID,POLLING_DETAIL_OPTION,POLLING_DETAIL_REASON,POLLING_DETAIL_IS_APPROVE,POLLING_DETAIL_CREATE_BY,POLLING_DETAIL_CREATE_DATE,POLLING_DETAIL_STATUS";
            var fvalue = "'" + lastid + "', " +
                            "'" + POLLING_ID + "', " +
                            "'" + POLLING_OPTION + "', " +
                            "'" + POLLING_REASON + "', " +
                            1 + "," +
                            "'" + UserId + "', " +
                            datenow + "," +
                            "'" + 1 + "'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_POLLING_DETAILS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            if (POLLING_OPTION == 1)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING SET POLLING_SETUJU = (POLLING_SETUJU+1) WHERE POLLING_ID = " + POLLING_ID);
                db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING SET POLLING_SETUJU_PERSEN = (SELECT ROUND((AA.POLLING_SETUJU / AA.POLLING_JML_PARTISIPAN * 100),2) AS SETUJU_PERSEN FROM TRX_POLLING AA WHERE POLLING_ID = " + +POLLING_ID + ") WHERE POLLING_ID = " + POLLING_ID);
                db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING SET POLLING_ABSTAIN = (POLLING_JML_PARTISIPAN-(SELECT COUNT(*) FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = " + POLLING_ID + ")) WHERE POLLING_ID = " + POLLING_ID);
                db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING SET POLLING_ABSTAIN_PERSEN = (SELECT ROUND((AA.POLLING_ABSTAIN / AA.POLLING_JML_PARTISIPAN * 100),2) AS POLLING_ABSTAIN_PERSEN FROM TRX_POLLING AA WHERE POLLING_ID = " + +POLLING_ID + ") WHERE POLLING_ID = " + POLLING_ID);
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING SET POLLING_TDK_SETUJU = (POLLING_TDK_SETUJU+1) WHERE POLLING_ID = " + POLLING_ID);
                db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING SET POLLING_TDK_SETUJU_PERSEN = (SELECT ROUND((AA.POLLING_TDK_SETUJU / AA.POLLING_JML_PARTISIPAN * 100),2) AS SETUJU_PERSEN FROM TRX_POLLING AA WHERE POLLING_ID = " + +POLLING_ID + ") WHERE POLLING_ID = " + POLLING_ID);
                db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING SET POLLING_ABSTAIN = (POLLING_JML_PARTISIPAN-(SELECT COUNT(*) FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = " + POLLING_ID + ")) WHERE POLLING_ID = " + POLLING_ID);
                db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING SET POLLING_ABSTAIN_PERSEN = (SELECT ROUND((AA.POLLING_ABSTAIN / AA.POLLING_JML_PARTISIPAN * 100),2) AS POLLING_ABSTAIN_PERSEN FROM TRX_POLLING AA WHERE POLLING_ID = " + +POLLING_ID + ") WHERE POLLING_ID = " + POLLING_ID);
            }

            db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING SET POLLING_IS_KUORUM = (CASE WHEN POLLING_SETUJU >= 2/3*POLLING_JML_PARTISIPAN THEN 1 ELSE 0 END) WHERE POLLING_ID = " + POLLING_ID);


            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            return Json(new
            {
                data = true
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Polling(int id = 0)
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
            var IsKetua = db.Database.SqlQuery<string>("SELECT JABATAN FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + DataProposal.KOMTEK_ID + " AND USER_ID = " + USER_ID).SingleOrDefault();
            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_RELATED_ID = " + id).ToList();
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
            var SNI_DOC_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0) AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + id + " AND T1.SNI_DOC_TYPE = 2").SingleOrDefault();
            var DefaultDokumen = db.Database.SqlQuery<TRX_SNI_DOC>("SELECT * FROM TRX_SNI_DOC WHERE SNI_DOC_PROPOSAL_ID = " + id + " AND SNI_DOC_VERSION = " + SNI_DOC_VERSION + " AND SNI_DOC_TYPE = 2").SingleOrDefault();
            string text = "";

            if (SNI_DOC_VERSION == 0)
            {
                var SNI_DOC_VERSION_LAST_RSNI = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0) AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + id + " AND T1.SNI_DOC_TYPE = 1").SingleOrDefault();
                var DefaultDokumen_LAST_RSNI = db.Database.SqlQuery<TRX_SNI_DOC>("SELECT * FROM TRX_SNI_DOC WHERE SNI_DOC_PROPOSAL_ID = " + id + " AND SNI_DOC_VERSION = " + SNI_DOC_VERSION_LAST_RSNI + " AND SNI_DOC_TYPE = 1").SingleOrDefault();
                if (DefaultDokumen_LAST_RSNI != null)
                {
                    string path = Server.MapPath("~" + DefaultDokumen_LAST_RSNI.SNI_DOC_FILE_PATH);
                    text = System.IO.File.ReadAllText(@"" + path);
                }
                else
                {
                    text = "";
                }
            }
            else
            {
                if (DefaultDokumen != null)
                {
                    string path = Server.MapPath("~" + DefaultDokumen.SNI_DOC_FILE_PATH);
                    text = System.IO.File.ReadAllText(@"" + path);
                }
                else
                {
                    text = "";
                }
            }

            ViewData["DefaultDokumen"] = text;
            ViewData["IsKetua"] = ((IsKetua == "Ketua") ? 1 : 0);
            ViewData["Dokumen"] = Dokumen;
            ViewData["RefLain"] = RefLain;
            ViewData["IsPolling"] = ((DataProposal.PROPOSAL_IS_POLLING == null || DataProposal.PROPOSAL_IS_POLLING == 0) ? 0 : 1);
            var CekPolling = db.Database.SqlQuery<TRX_POLLING_DETAILS>("SELECT * FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_CREATE_BY = " + USER_ID + " AND POLLING_DETAIL_POLLING_ID = " + DataProposal.PROPOSAL_POLLING_ID).SingleOrDefault();
            var DataPolling = db.Database.SqlQuery<VIEW_POLLING_SINGLE>("SELECT * FROM VIEW_POLLING_SINGLE WHERE POLLING_ID = " + DataProposal.PROPOSAL_POLLING_ID).SingleOrDefault();
            if (CekPolling == null)
            {
                ViewData["LakukanPolling"] = true;
            }
            else
            {
                ViewData["LakukanPolling"] = false;
            }
            ViewData["IsKetua"] = ((IsKetua == "Ketua") ? 1 : 0);
            ViewData["DataPolling"] = DataPolling;
            ViewData["Dokumen"] = Dokumen;
            ViewData["RefLain"] = RefLain;
            ViewData["Komtek"] = DataKomtek;
            ViewData["DataProposal"] = DataProposal;
            return View();

        }
        [HttpPost]
        public ActionResult Polling(int PROPOSAL_ID = 0, int POLLING_OPTION = 0, string POLLING_REASON = "")
        {
            var UserId = Convert.ToInt32(Session["USER_ID"]);
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("TRX_POLLING");
            var datenow = MixHelper.ConvertDateNow();
            var PollingVersion = db.Database.SqlQuery<int>("SELECT NVL(CAST(MAX(POLLING_VERSION) AS INT),1) FROM TRX_POLLING WHERE POLLING_TYPE = 2 AND POLLING_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();

            var CekPolling = db.Database.SqlQuery<TRX_POLLING>("SELECT * FROM TRX_POLLING WHERE POLLING_VERSION = " + PollingVersion + " AND POLLING_TYPE = 2 AND POLLING_PROPOSAL_ID = " + PROPOSAL_ID + " AND POLLING_CREATE_BY = " + UserId).SingleOrDefault();
            if (CekPolling == null)
            {
                var fname = "POLLING_ID,POLLING_PROPOSAL_ID,POLLING_TYPE,POLLING_OPTION,POLLING_REASON,POLLING_CREATE_BY,POLLING_CREATE_DATE,POLLING_STATUS,POLLING_LOGCODE,POLLING_VERSION";
                var fvalue = "'" + lastid + "', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + 2 + "', " +
                                "'" + POLLING_OPTION + "', " +
                                "'" + POLLING_REASON + "', " +
                                "'" + UserId + "', " +
                                datenow + "," +
                                "'1', " +
                                "'" + logcode + "'," +
                                "'" + PollingVersion + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_POLLING (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");


                String objek = fvalue.Replace("'", "-");
                MixHelper.InsertLog(logcode, objek, 1);
                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            }
            return Json(new
            {
                PROPOSAL_ID,
                POLLING_OPTION,
                POLLING_REASON
            }, JsonRequestBehavior.AllowGet);

        }
        [HttpPost]
        public ActionResult TutupPolling(int POLLING_ID = 0, int PROPOSAL_ID = 0)
        {
            var UserId = Convert.ToInt32(Session["USER_ID"]);
            var logcode = MixHelper.GetLogCode();
            var datenow = MixHelper.ConvertDateNow();
            var CekKourum = db.Database.SqlQuery<int>("SELECT POLLING_IS_KUORUM FROM TRX_POLLING WHERE POLLING_ID =" + POLLING_ID).SingleOrDefault();
            //var DataPolling = db.Database.SqlQuery<VIEW_POLLING>("SELECT * FROM VIEW_POLLING WHERE POLLING_ID = " + POLLING_ID).SingleOrDefault();
            if (CekKourum == 1)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 3,PROPOSAL_IS_POLLING = 0,PROPOSAL_STATUS_PROSES = 1,PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                String objek = "PROPOSAL_STATUS = 3,PROPOSAL_IS_POLLING = 0,PROPOSAL_STATUS_PROSES = 1, PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId;
                MixHelper.InsertLog(logcode, objek.Replace("'", "-"), 2);

                int lastid = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + lastid + "," + PROPOSAL_ID + ",1," + datenow + "," + UserId + ",1,3,1)");
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 3,PROPOSAL_IS_POLLING = 0,PROPOSAL_STATUS_PROSES = 0,PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                String objek = "PROPOSAL_STATUS = 3,PROPOSAL_IS_POLLING = 0,PROPOSAL_STATUS_PROSES = 0, PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId;
                MixHelper.InsertLog(logcode, objek.Replace("'", "-"), 2);

                int lastid = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + lastid + "," + PROPOSAL_ID + ",0," + datenow + "," + UserId + ",1,3,1)");
            }
            return Json(new
            {
                POLLING_ID
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SetPolling(string POLLING_START_DATE = "", string POLLING_END_DATE = "", int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0)
        {
            var UserId = Convert.ToInt32(Session["USER_ID"]);
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("TRX_POLLING");
            var datenow = MixHelper.ConvertDateNow();

            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_IS_POLLING = 1 WHERE PROPOSAL_ID = " + PROPOSAL_ID);
            var POLLING_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(POLLING_VERSION),0) AS NUMBER) AS SNI_DOC_VERSION FROM TRX_POLLING WHERE POLLING_TYPE = 2 AND POLLING_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            var POLLING_JML_PARTISIPAN = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(COUNT(*),0) AS NUMBER)  FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + PROPOSAL_KOMTEK_ID).SingleOrDefault();
            String POLLING_START_DATE_FIX = "TO_DATE('" + POLLING_START_DATE + "', 'yyyy-mm-dd hh24:mi:ss')";
            String POLLING_END_DATE_FIX = "TO_DATE('" + POLLING_END_DATE + "', 'yyyy-mm-dd hh24:mi:ss')";
            var fname = "POLLING_ID,POLLING_PROPOSAL_ID,POLLING_TYPE,POLLING_START_DATE,POLLING_END_DATE,POLLING_VERSION,POLLING_IS_KUORUM,POLLING_JML_PARTISIPAN,POLLING_CREATE_BY,POLLING_CREATE_DATE,POLLING_STATUS,POLLING_LOGCODE";
            var fvalue = "'" + lastid + "', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + 2 + "', " +
                            POLLING_START_DATE_FIX + "," +
                            POLLING_END_DATE_FIX + "," +
                            "'" + ((POLLING_VERSION == 0) ? 1 : POLLING_VERSION) + "', " +
                            "'" + 0 + "', " +
                            "'" + POLLING_JML_PARTISIPAN + "', " +
                            "'" + UserId + "', " +
                            datenow + "," +
                            "'" + 1 + "', " +
                            "'" + logcode + "'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_POLLING (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");


            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return Json(new
            {
                data = true
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DataRSNI2Komtek(DataTables param)
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

            string where_clause = "PROPOSAL_STATUS = 5 AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID;

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
                Convert.ToString("<center><a href='/Perumusan/RSNI2/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 5 && list.PROPOSAL_STATUS_PROSES == 0)?"<a href='/Perumusan/RSNI2/Create/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Susun RSNI 2'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DataRSNI2PPS(DataTables param)
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


            string where_clause = "PROPOSAL_STATUS = 5 AND PROPOSAL_STATUS_PROSES = 1 " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");

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
                Convert.ToString("<center><a href='/Perumusan/RSNI2/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 5 && list.PROPOSAL_STATUS_PROSES == 1)?"<a href='/Perumusan/RSNI2/Pengesahan/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Pengesahan RSNI 2'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
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
