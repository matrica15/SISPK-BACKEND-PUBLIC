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
    public class UsulanPenetapanController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            var IS_KOMTEK = Convert.ToInt32(Session["IS_KOMTEK"]);
            string ViewName = ((IS_KOMTEK == 1) ? "DaftarPenyusunanRASNI" : "DaftarPengesahanRASNI");
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
            var Surat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 14 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();
            var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();

            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 && komtek.KOMTEK_ID == DataProposal.KOMTEK_ID select komtek).SingleOrDefault();
            var IsKetua = db.Database.SqlQuery<string>("SELECT JABATAN FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + DataProposal.KOMTEK_ID + " AND USER_ID = " + USER_ID).SingleOrDefault();

            var SURAT_UNDANGAN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 43 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var BERITA_ACARA_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 42 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DAFTAR_HADIR_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 16 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var NOTULEN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 44 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();

            ViewData["SURAT_UNDANGAN_RAPAT"] = SURAT_UNDANGAN_RAPAT;
            ViewData["BERITA_ACARA_RAPAT"] = BERITA_ACARA_RAPAT;
            ViewData["DAFTAR_HADIR_RAPAT"] = DAFTAR_HADIR_RAPAT;
            ViewData["NOTULEN_RAPAT"] = NOTULEN_RAPAT;

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

            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_RELATED_ID = " + id).ToList();
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 17 AND DOC_FOLDER_ID = 15 AND DOC_STATUS = 1 AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
            if (DefaultDokumen == null)
            {
                var DefaultDokumenRSNI2 = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 38 AND DOC_FOLDER_ID = 25 AND DOC_STATUS = 1 AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
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
            var LastNumber = db.Database.SqlQuery<string>("WITH AA AS(SELECT CAST(SUBSTR(regexp_replace(SNI_NOMOR, '[^0-9]', ''), 1, LENGTH(regexp_replace(SNI_NOMOR, '[^0-9]', '')) - 4) AS NUMBER) LAST_NOMOR FROM VIEW_SNI ) SELECT CAST(MAX(LAST_NOMOR)+1 AS VARCHAR(255)) LAST_NOMOR FROM AA").SingleOrDefault();
            ViewData["Komtek"] = DataKomtek;
            ViewData["LastNumber"] = LastNumber;
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

            var DefaultDokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_RELATED_TYPE = 18 AND DOC_STATUS = 1 AND DOC_RELATED_ID = " + id + " AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
            ViewData["DefaultDokumen"] = DefaultDokumen;
            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_RELATED_ID = " + id).ToList();
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
        public ActionResult Pengesahan(TRX_PROPOSAL tp, int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, string PROPOSAL_PNPS_CODE = "", int APPROVAL_TYPE = 0, int APPROVAL_STATUS = 1, string APPROVAL_REASON = "", string MEMO_KAPUS_DATE = "", string MEMO_DEPUTI_DATE = "", string MEMO_KAPUS_NOMOR = "", string MEMO_DEPUTI_NOMOR = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
            int PROPOSAL_RAPAT_ID = MixHelper.GetSequence("TRX_PROPOSAL_RAPAT");
            var VERSION_RAKOR = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 10").SingleOrDefault();
            int LASTID_PROPOSAL_RAPAT = MixHelper.GetSequence("TRX_DOCUMENTS");
            var LOGCODE_PROPOSAL_RAPAT = MixHelper.GetLogCode();
            //String TGL_RAPAT_CONVERT = "TO_DATE('" + TGL_RAPAT + "', 'yyyy-mm-dd hh24:mi:ss')";
            String TGL_MEMO_KAPUS_CONVERT = "TO_DATE('" + MEMO_KAPUS_DATE + "', 'yyyy-mm-dd hh24:mi:ss')";
            String TGL_MEMO_DEPUTI_CONVERT = "TO_DATE('" + MEMO_DEPUTI_DATE + "', 'yyyy-mm-dd hh24:mi:ss')";
            //var FNAME_PROPOSAL_RAPAT = "PROPOSAL_RAPAT_ID,PROPOSAL_RAPAT_PROPOSAL_ID,PROPOSAL_RAPAT_NOMOR,PROPOSAL_RAPAT_DATE,PROPOSAL_RAPAT_VERSION,PROPOSAL_RAPAT_APPROVAL_ID,PROPOSAL_RAPAT_PROPOSAL_STATUS";
            //var FVALUE_PROPOSAL_RAPAT = "'" + LASTID_PROPOSAL_RAPAT + "', " +
            //                            "'" + PROPOSAL_ID + "', " +
            //                            "'" + NO_RAKOR + "', " +
            //                            TGL_RAPAT_CONVERT + ", " +
            //                            "'" + VERSION_RAKOR + "', " +
            //                            "'" + PROPOSAL_RAPAT_ID + "', " +
            //                            "'10'";
            //db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_RAPAT (" + FNAME_PROPOSAL_RAPAT + ") VALUES (" + FVALUE_PROPOSAL_RAPAT.Replace("''", "NULL") + ")");
            string test = "UPDATE TRX_MONITORING SET MONITORING_TGL_RASNI = SYSDATE,MONITORING_TGL_MEMO_KAPUS= " + TGL_MEMO_KAPUS_CONVERT + ",MONITORING_TGL_MEMO_DEPUTI = " + TGL_MEMO_DEPUTI_CONVERT + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID;
            db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_RASNI = SYSDATE,MONITORING_TGL_MEMO_KAPUS= " + TGL_MEMO_KAPUS_CONVERT + ",MONITORING_TGL_MEMO_DEPUTI = " + TGL_MEMO_DEPUTI_CONVERT + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);

            //------------------------------------
            HttpPostedFileBase FILE_MEMO_KAPUS = Request.Files["MEMO_KAPUS"];
            if (FILE_MEMO_KAPUS.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_MEMO_KAPUS = FILE_MEMO_KAPUS.InputStream;

                string EXT_MEMO_KAPUS = Path.GetExtension(FILE_MEMO_KAPUS.FileName);
                if (EXT_MEMO_KAPUS.ToLower() == ".docx" || EXT_MEMO_KAPUS.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_MEMO_KAPUS);
                    string filePathdoc = path + "MEMO_KAPUS_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "MEMO_KAPUS_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "MEMO_KAPUS_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_MEMO_KAPUS = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_MEMO_KAPUS = MixHelper.GetLogCode();
                    var FNAME_MEMO_KAPUS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_MEMO_KAPUS = "'" + LASTID_MEMO_KAPUS + "', " +
                                "'18', " +
                                "'21', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Memo Kapus RASNI', " +
                                "'Memo Kapus RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "MEMO_KAPUS_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_MEMO_KAPUS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_MEMO_KAPUS + ") VALUES (" + FVALUE_MEMO_KAPUS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_MEMO_KAPUS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_MEMO_KAPUS, objekTanggapan, 1);
                }
                else
                {
                    FILE_MEMO_KAPUS.SaveAs(path + "MEMO_KAPUS_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + EXT_MEMO_KAPUS.ToLower());
                    int LASTID_MEMO_KAPUS = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_MEMO_KAPUS = MixHelper.GetLogCode();
                    var FNAME_MEMO_KAPUS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_MEMO_KAPUS = "'" + LASTID_MEMO_KAPUS + "', " +
                                "'18', " +
                                "'21', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Memo Kapus RASNI', " +
                                "'Memo Kapus RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "MEMO_KAPUS_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_MEMO_KAPUS.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_MEMO_KAPUS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_MEMO_KAPUS + ") VALUES (" + FVALUE_MEMO_KAPUS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_MEMO_KAPUS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_MEMO_KAPUS, objekTanggapan, 1);
                }
            }

            HttpPostedFileBase FILE_MEMO_DEPUTI = Request.Files["MEMO_DEPUTI"];
            if (FILE_MEMO_DEPUTI.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_MEMO_DEPUTI = FILE_MEMO_DEPUTI.InputStream;

                string EXT_MEMO_DEPUTI = Path.GetExtension(FILE_MEMO_DEPUTI.FileName);
                if (EXT_MEMO_DEPUTI.ToLower() == ".docx" || EXT_MEMO_DEPUTI.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_MEMO_DEPUTI);
                    string filePathdoc = path + "MEMO_DEPUTI_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "MEMO_DEPUTI_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "MEMO_DEPUTI_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_MEMO_DEPUTI = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_MEMO_DEPUTI = MixHelper.GetLogCode();
                    var FNAME_MEMO_DEPUTI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_MEMO_DEPUTI = "'" + LASTID_MEMO_DEPUTI + "', " +
                                "'18', " +
                                "'48', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Memo Deputi RASNI', " +
                                "'Memo Deputi RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "MEMO_DEPUTI_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_MEMO_DEPUTI + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_MEMO_DEPUTI + ") VALUES (" + FVALUE_MEMO_DEPUTI.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_MEMO_DEPUTI.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_MEMO_DEPUTI, objekTanggapan, 1);
                }
                else
                {
                    FILE_MEMO_DEPUTI.SaveAs(path + "MEMO_DEPUTI_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + EXT_MEMO_DEPUTI.ToLower());
                    int LASTID_MEMO_DEPUTI = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_MEMO_DEPUTI = MixHelper.GetLogCode();
                    var FNAME_MEMO_DEPUTI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_MEMO_DEPUTI = "'" + LASTID_MEMO_DEPUTI + "', " +
                                "'18', " +
                                "'48', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Memo Deputi RASNI', " +
                                "'Memo Deputi RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "MEMO_DEPUTI_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_MEMO_DEPUTI.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_MEMO_DEPUTI + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_MEMO_DEPUTI + ") VALUES (" + FVALUE_MEMO_DEPUTI.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_MEMO_DEPUTI.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_MEMO_DEPUTI, objekTanggapan, 1);
                }
            }

            HttpPostedFileBase FILE_LEMBAR_KENDALI = Request.Files["LEMBAR_KENDALI"];
            if (FILE_LEMBAR_KENDALI.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_LEMBAR_KENDALI = FILE_LEMBAR_KENDALI.InputStream;

                string EXT_LEMBAR_KENDALI = Path.GetExtension(FILE_LEMBAR_KENDALI.FileName);
                if (EXT_LEMBAR_KENDALI.ToLower() == ".docx" || EXT_LEMBAR_KENDALI.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_LEMBAR_KENDALI);
                    string filePathdoc = path + "LEMBAR_KENDALI_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "LEMBAR_KENDALI_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "LEMBAR_KENDALI_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_LEMBAR_KENDALI = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_LEMBAR_KENDALI = MixHelper.GetLogCode();
                    var FNAME_LEMBAR_KENDALI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_LEMBAR_KENDALI = "'" + LASTID_LEMBAR_KENDALI + "', " +
                                "'18', " +
                                "'22', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lembar Kendali RASNI', " +
                                "'Lembar Kendali RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "LEMBAR_KENDALI_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_LEMBAR_KENDALI + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_LEMBAR_KENDALI + ") VALUES (" + FVALUE_LEMBAR_KENDALI.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_LEMBAR_KENDALI.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_LEMBAR_KENDALI, objekTanggapan, 1);
                }
                else
                {
                    FILE_LEMBAR_KENDALI.SaveAs(path + "LEMBAR_KENDALI_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + EXT_LEMBAR_KENDALI.ToLower());
                    int LASTID_LEMBAR_KENDALI = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_LEMBAR_KENDALI = MixHelper.GetLogCode();
                    var FNAME_LEMBAR_KENDALI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_LEMBAR_KENDALI = "'" + LASTID_LEMBAR_KENDALI + "', " +
                                "'18', " +
                                "'22', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lembar Kendali RASNI', " +
                                "'Lembar Kendali RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "LEMBAR_KENDALI_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_LEMBAR_KENDALI.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_LEMBAR_KENDALI + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_LEMBAR_KENDALI + ") VALUES (" + FVALUE_LEMBAR_KENDALI.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_LEMBAR_KENDALI.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_LEMBAR_KENDALI, objekTanggapan, 1);
                }
            }

            HttpPostedFileBase FILE_LAMPIRAN_SK = Request.Files["LAMPIRAN_SK"];
            if (FILE_LAMPIRAN_SK.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream STREAM_DOC_LAMPIRAN_SK = FILE_LAMPIRAN_SK.InputStream;

                string EXT_LAMPIRAN_SK = Path.GetExtension(FILE_LAMPIRAN_SK.FileName);
                if (EXT_LAMPIRAN_SK.ToLower() == ".docx" || EXT_LAMPIRAN_SK.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_LAMPIRAN_SK);
                    string filePathdoc = path + "LAMPIRAN_SK_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "LAMPIRAN_SK_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "LAMPIRAN_SK_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    int LASTID_LAMPIRAN_SK = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_LAMPIRAN_SK = MixHelper.GetLogCode();
                    var FNAME_LAMPIRAN_SK = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_LAMPIRAN_SK = "'" + LASTID_LAMPIRAN_SK + "', " +
                                "'18', " +
                                "'88', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran SK RASNI', " +
                                "'Lampiran SK RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "LAMPIRAN_SK_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'docx', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_LAMPIRAN_SK + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_LAMPIRAN_SK + ") VALUES (" + FVALUE_LAMPIRAN_SK.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_LAMPIRAN_SK.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_LAMPIRAN_SK, objekTanggapan, 1);
                }
                else
                {
                    FILE_LAMPIRAN_SK.SaveAs(path + "LAMPIRAN_SK_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + EXT_LAMPIRAN_SK.ToLower());
                    int LASTID_LAMPIRAN_SK = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_LAMPIRAN_SK = MixHelper.GetLogCode();
                    var FNAME_LAMPIRAN_SK = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_LAMPIRAN_SK = "'" + LASTID_LAMPIRAN_SK + "', " +
                                "'18', " +
                                "'88', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran SK RASNI', " +
                                "'Lampiran SK RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "LAMPIRAN_SK_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + EXT_LAMPIRAN_SK.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_LAMPIRAN_SK + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_LAMPIRAN_SK + ") VALUES (" + FVALUE_LAMPIRAN_SK.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_LAMPIRAN_SK.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_LAMPIRAN_SK, objekTanggapan, 1);
                }
            }
            //HttpPostedFileBase FILE_DAFTAR_HADIR = Request.Files["DAFTAR_HADIR"];
            //if (FILE_DAFTAR_HADIR.ContentLength > 0)
            //{
            //    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
            //    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
            //    Stream STREAM_DOC_DAFTAR_HADIR = FILE_DAFTAR_HADIR.InputStream;

            //    string EXT_DAFTAR_HADIR = Path.GetExtension(FILE_DAFTAR_HADIR.FileName);
            //    if (EXT_DAFTAR_HADIR.ToLower() == ".docx" || EXT_DAFTAR_HADIR.ToLower() == ".doc")
            //    {
            //        Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_DAFTAR_HADIR);
            //        string filePathdoc = path + "DAFTAR_HADIR_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
            //        string filePathpdf = path + "DAFTAR_HADIR_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
            //        string filePathxml = path + "DAFTAR_HADIR_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
            //        doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
            //        doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
            //        doc.Save(@"" + filePathxml);
            //        int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
            //        var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
            //        var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
            //        var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
            //                    "'18', " +
            //                    "'20', " +
            //                    "'" + PROPOSAL_ID + "', " +
            //                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir RASNI', " +
            //                    "'Daftar Hadir RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
            //                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
            //                    "'" + "DAFTAR_HADIR_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
            //                    "'" + EXT_DAFTAR_HADIR.ToLower().Replace(".", "") + "', " +
            //                    "'0', " +
            //                    "'" + USER_ID + "', " +
            //                    DATENOW + "," +
            //                    "'1', " +
            //                    "'" + LOGCODE_DAFTAR_HADIR + "'";
            //        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DAFTAR_HADIR + ") VALUES (" + FVALUE_DAFTAR_HADIR.Replace("''", "NULL") + ")");
            //        String objekTanggapan = FVALUE_DAFTAR_HADIR.Replace("'", "-");
            //        MixHelper.InsertLog(LOGCODE_DAFTAR_HADIR, objekTanggapan, 1);
            //    }
            //    else
            //    {
            //        FILE_DAFTAR_HADIR.SaveAs(path + "DAFTAR_HADIR_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + EXT_DAFTAR_HADIR.ToLower());
            //        int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
            //        var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
            //        var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
            //        var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
            //                    "'18', " +
            //                    "'20', " +
            //                    "'" + PROPOSAL_ID + "', " +
            //                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir RASNI', " +
            //                    "'Daftar Hadir RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
            //                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
            //                    "'" + "DAFTAR_HADIR_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
            //                    "'" + EXT_DAFTAR_HADIR.ToLower().Replace(".", "") + "', " +
            //                    "'0', " +
            //                    "'" + USER_ID + "', " +
            //                    DATENOW + "," +
            //                    "'1', " +
            //                    "'" + LOGCODE_DAFTAR_HADIR + "'";
            //        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DAFTAR_HADIR + ") VALUES (" + FVALUE_DAFTAR_HADIR.Replace("''", "NULL") + ")");
            //        String objekTanggapan = FVALUE_DAFTAR_HADIR.Replace("'", "-");
            //        MixHelper.InsertLog(LOGCODE_DAFTAR_HADIR, objekTanggapan, 1);
            //    }
            //}
            //HttpPostedFileBase FILE_BERITA_ACARA = Request.Files["BERITA_ACARA"];
            //if (FILE_BERITA_ACARA.ContentLength > 0)
            //{
            //    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
            //    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
            //    Stream STREAM_DOC_BERITA_ACARA = FILE_BERITA_ACARA.InputStream;

            //    string EXT_BERITA_ACARA = Path.GetExtension(FILE_BERITA_ACARA.FileName);
            //    if (EXT_BERITA_ACARA.ToLower() == ".docx" || EXT_BERITA_ACARA.ToLower() == ".doc")
            //    {
            //        Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_BERITA_ACARA);
            //        string filePathdoc = path + "BERITA_ACARA_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
            //        string filePathpdf = path + "BERITA_ACARA_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
            //        string filePathxml = path + "BERITA_ACARA_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
            //        doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
            //        doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
            //        doc.Save(@"" + filePathxml);
            //        int LASTID_BERITA_ACARA = MixHelper.GetSequence("TRX_DOCUMENTS");
            //        var LOGCODE_BERITA_ACARA = MixHelper.GetLogCode();
            //        var FNAME_BERITA_ACARA = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
            //        var FVALUE_BERITA_ACARA = "'" + LASTID_BERITA_ACARA + "', " +
            //                    "'18', " +
            //                    "'19', " +
            //                    "'" + PROPOSAL_ID + "', " +
            //                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara RASNI', " +
            //                    "'Berita Acara RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
            //                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
            //                    "'" + "BERITA_ACARA_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
            //                    "'" + EXT_BERITA_ACARA.ToLower().Replace(".", "") + "', " +
            //                    "'0', " +
            //                    "'" + USER_ID + "', " +
            //                    DATENOW + "," +
            //                    "'1', " +
            //                    "'" + LOGCODE_BERITA_ACARA + "'";
            //        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_BERITA_ACARA + ") VALUES (" + FVALUE_BERITA_ACARA.Replace("''", "NULL") + ")");
            //        String objekTanggapan = FVALUE_BERITA_ACARA.Replace("'", "-");
            //        MixHelper.InsertLog(LOGCODE_BERITA_ACARA, objekTanggapan, 1);
            //    }
            //    else
            //    {
            //        FILE_BERITA_ACARA.SaveAs(path + "BERITA_ACARA_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + EXT_BERITA_ACARA.ToLower());
            //        int LASTID_BERITA_ACARA = MixHelper.GetSequence("TRX_DOCUMENTS");
            //        var LOGCODE_BERITA_ACARA = MixHelper.GetLogCode();
            //        var FNAME_BERITA_ACARA = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
            //        var FVALUE_BERITA_ACARA = "'" + LASTID_BERITA_ACARA + "', " +
            //                    "'18', " +
            //                    "'19', " +
            //                    "'" + PROPOSAL_ID + "', " +
            //                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara RASNI', " +
            //                    "'Berita Acara RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
            //                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
            //                    "'" + "BERITA_ACARA_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
            //                    "'" + EXT_BERITA_ACARA.ToLower().Replace(".", "") + "', " +
            //                    "'0', " +
            //                    "'" + USER_ID + "', " +
            //                    DATENOW + "," +
            //                    "'1', " +
            //                    "'" + LOGCODE_BERITA_ACARA + "'";
            //        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_BERITA_ACARA + ") VALUES (" + FVALUE_BERITA_ACARA.Replace("''", "NULL") + ")");
            //        String objekTanggapan = FVALUE_BERITA_ACARA.Replace("'", "-");
            //        MixHelper.InsertLog(LOGCODE_BERITA_ACARA, objekTanggapan, 1);
            //    }
            //}
            //HttpPostedFileBase FILE_DATA_RSNI = Request.Files["DATA_RSNI"];
            //if (FILE_DATA_RSNI.ContentLength > 0)
            //{
            //    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
            //    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
            //    Stream STREAM_DOC_DATA_RSNI = FILE_DATA_RSNI.InputStream;

            //    string EXT_DATA_RSNI = Path.GetExtension(FILE_DATA_RSNI.FileName);
            //    if (EXT_DATA_RSNI.ToLower() == ".docx" || EXT_DATA_RSNI.ToLower() == ".doc")
            //    {
            //        Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_DATA_RSNI);
            //        string filePathdoc = path + "DATA_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
            //        string filePathpdf = path + "DATA_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
            //        string filePathxml = path + "DATA_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
            //        doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
            //        doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
            //        doc.Save(@"" + filePathxml);
            //        int Total_Hal = doc.PageCount;
            //        int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
            //        var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
            //        var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_INFO,DOC_LOG_CODE";
            //        var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
            //                    "'18', " +
            //                    "'18', " +
            //                    "'" + PROPOSAL_ID + "', " +
            //                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Data RASNI', " +
            //                    "'Data RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
            //                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
            //                    "'" + "DATA_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
            //                    "'" + EXT_DATA_RSNI.ToLower().Replace(".", "") + "', " +
            //                    "'0', " +
            //                    "'" + USER_ID + "', " +
            //                    DATENOW + "," +
            //                    "'1', " +
            //                    "'" + Total_Hal + "', " +
            //                    "'" + LOGCODE_DATA_RSNI + "'";
            //        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DATA_RSNI + ") VALUES (" + FVALUE_DATA_RSNI.Replace("''", "NULL") + ")");
            //        String objekTanggapan = FVALUE_DATA_RSNI.Replace("'", "-");
            //        MixHelper.InsertLog(LOGCODE_DATA_RSNI, objekTanggapan, 1);
            //    }
            //    else
            //    {
            //        FILE_DATA_RSNI.SaveAs(path + "DATA_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + EXT_DATA_RSNI.ToLower());
            //        int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
            //        var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
            //        var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
            //        var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
            //                    "'18', " +
            //                    "'18', " +
            //                    "'" + PROPOSAL_ID + "', " +
            //                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Data RASNI', " +
            //                    "'Data RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
            //                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
            //                    "'" + "DATA_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
            //                    "'" + EXT_DATA_RSNI.ToLower().Replace(".", "") + "', " +
            //                    "'0', " +
            //                    "'" + USER_ID + "', " +
            //                    DATENOW + "," +
            //                    "'1', " +
            //                    "'" + LOGCODE_DATA_RSNI + "'";
            //        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DATA_RSNI + ") VALUES (" + FVALUE_DATA_RSNI.Replace("''", "NULL") + ")");
            //        String objekTanggapan = FVALUE_DATA_RSNI.Replace("'", "-");
            //        MixHelper.InsertLog(LOGCODE_DATA_RSNI, objekTanggapan, 1);
            //    }
            //}
            //if (DataProposal.PROPOSAL_JENIS_PERUMUSAN == 3)
            //{
            //    var update_PROPOSAL = "UPDATE TRX_PROPOSAL SET PROPOSAL_ABSTRAK = '" + tp.PROPOSAL_ABSTRAK + "',  PROPOSAL_RUANG_LINGKUP = '" + tp.PROPOSAL_RUANG_LINGKUP + "', PROPOSAL_NO_SNI_PROPOSAL = '" + tp.PROPOSAL_NO_SNI_PROPOSAL + "' , PROPOSAL_JUDUL_SNI_PROPOSAL = '" + tp.PROPOSAL_JUDUL_SNI_PROPOSAL + "' , PROPOSAL_JUDUL_PNPS = '" + tp.PROPOSAL_JUDUL_SNI_PROPOSAL + "' WHERE PROPOSAL_ID = " + PROPOSAL_ID;
            //    db.Database.ExecuteSqlCommand(update_PROPOSAL);
            //}
            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 3, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + ",MONITORING_NO_MEMO_KAPUS= '" + MEMO_KAPUS_NOMOR + "',MONITORING_NO_MEMO_DEPUTI = '" + MEMO_DEPUTI_NOMOR + "' WHERE PROPOSAL_ID = " + PROPOSAL_ID);
            var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 3, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
            MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);
            int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 15").SingleOrDefault();
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,15," + APPROVAL_STATUS_SESSION + ")");


            //------------------------------------
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan dan Diteruskan ke Penetapan SNI";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult PengesahanBackup(HttpPostedFileBase DATA_RSNI, HttpPostedFileBase MEMO_KAPUS, HttpPostedFileBase LEMBAR_KENDALI, int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, string PROPOSAL_PNPS_CODE = "", int APPROVAL_TYPE = 0, int APPROVAL_STATUS = 1, string APPROVAL_REASON = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            Stream STREAM_DATA_MEMO_KAPUS = MEMO_KAPUS.InputStream;
            byte[] APPDATA_DATA_MEMO_KAPUS = new byte[MEMO_KAPUS.ContentLength + 1];
            STREAM_DATA_MEMO_KAPUS.Read(APPDATA_DATA_MEMO_KAPUS, 0, MEMO_KAPUS.ContentLength);
            string Extension_DATA_MEMO_KAPUS = Path.GetExtension(MEMO_KAPUS.FileName);

            Stream STREAM_LEMBAR_KENDALI = LEMBAR_KENDALI.InputStream;
            byte[] APPDATA_LEMBAR_KENDALI = new byte[LEMBAR_KENDALI.ContentLength + 1];
            STREAM_LEMBAR_KENDALI.Read(APPDATA_LEMBAR_KENDALI, 0, LEMBAR_KENDALI.ContentLength);
            string Extension_LEMBAR_KENDALI = Path.GetExtension(LEMBAR_KENDALI.FileName);

            Stream STREAM_DATA_RSNI = DATA_RSNI.InputStream;
            byte[] APPDATA_DATA_RSNI = new byte[DATA_RSNI.ContentLength + 1];
            STREAM_DATA_RSNI.Read(APPDATA_DATA_RSNI, 0, DATA_RSNI.ContentLength);
            string Extension_DATA_RSNI = Path.GetExtension(DATA_RSNI.FileName);

            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;

            if (Extension_DATA_RSNI.ToLower() == ".docx" || Extension_DATA_RSNI.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                var VERSION_RATEK = 1;
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DATA_RSNI);
                string filePathdoc = path + "RASNI_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "RASNI_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "RASNI_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                string pathSNIDOC = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                var SNI_DOC_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0)+1 AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.SNI_DOC_TYPE = 7").SingleOrDefault();
                string filePathdocSNI_DOC = pathSNIDOC + "DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX";
                string filePathpdfSNI_DOC = pathSNIDOC + "DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".PDF";
                string filePathxmlSNI_DOC = pathSNIDOC + "DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".XML";
                doc.Save(@"" + filePathdocSNI_DOC, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdfSNI_DOC, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxmlSNI_DOC);
                doc.Save(@"" + pathSNIDOC + "RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".DOCX", Aspose.Words.SaveFormat.Docx);

                var LOGCODE_SNI_DOC = MixHelper.GetLogCode();
                int LASTID_SNI_DOC = MixHelper.GetSequence("TRX_SNI_DOC");

                var fname = "SNI_DOC_ID,SNI_DOC_PROPOSAL_ID,SNI_DOC_TYPE,SNI_DOC_FILE_PATH,SNI_DOC_VERSION,SNI_DOC_CREATE_BY,SNI_DOC_CREATE_DATE,SNI_DOC_LOG_CODE,SNI_DOC_STATUS";
                var fvalue = "'" + LASTID_SNI_DOC + "', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'7', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/DRAFT/DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX" + "', " +
                                "'" + SNI_DOC_VERSION + "', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'" + LOGCODE_SNI_DOC + "', " +
                                "'1'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_SNI_DOC (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

                var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 18 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 18 AND DOC_STATUS = 1").SingleOrDefault();
                if (CEKDOKUMEN != null)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                }
                int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                            "'18', " +
                            "'18', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") RASNI Ver " + VERSION_RATEK + "" + "', " +
                            "'RASNI Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "RASNI_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
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
            if (Extension_DATA_MEMO_KAPUS.ToLower() == ".docx" || Extension_DATA_MEMO_KAPUS.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DATA_MEMO_KAPUS);
                string filePathdoc = path + "MEMO_KAPUS_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "MEMO_KAPUS_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "MEMO_KAPUS_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                int LASTID_MEMO_KAPUS = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_MEMO_KAPUS = MixHelper.GetLogCode();
                var FNAME_MEMO_KAPUS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_MEMO_KAPUS = "'" + LASTID_MEMO_KAPUS + "', " +
                            "'18', " +
                            "'21', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Memo Kapus', " +
                            "'Memo Kapus " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "MEMO_KAPUS_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_DATA_MEMO_KAPUS.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_MEMO_KAPUS + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_MEMO_KAPUS + ") VALUES (" + FVALUE_MEMO_KAPUS.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_MEMO_KAPUS.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_MEMO_KAPUS, objekTanggapan, 1);
            }
            else
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                MEMO_KAPUS.SaveAs(path + "MEMO_KAPUS_" + PROPOSAL_PNPS_CODE_FIXER + Extension_DATA_MEMO_KAPUS.ToUpper());

                int LASTID_MEMO_KAPUS = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_MEMO_KAPUS = MixHelper.GetLogCode();
                var FNAME_MEMO_KAPUS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_MEMO_KAPUS = "'" + LASTID_MEMO_KAPUS + "', " +
                            "'18', " +
                            "'21', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Memo Kapus', " +
                            "'Memo Kapus " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "MEMO_KAPUS_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_DATA_MEMO_KAPUS.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_MEMO_KAPUS + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_MEMO_KAPUS + ") VALUES (" + FVALUE_MEMO_KAPUS.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_MEMO_KAPUS.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_MEMO_KAPUS, objekTanggapan, 1);
            }

            if (Extension_LEMBAR_KENDALI.ToLower() == ".docx" || Extension_LEMBAR_KENDALI.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_LEMBAR_KENDALI);
                string filePathdoc = path + "LEMBAR_KENDALI_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "LEMBAR_KENDALI_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "LEMBAR_KENDALI_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                int LASTID_LEMBAR_KENDALI = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_LEMBAR_KENDALI = MixHelper.GetLogCode();
                var FNAME_LEMBAR_KENDALI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_LEMBAR_KENDALI = "'" + LASTID_LEMBAR_KENDALI + "', " +
                            "'18', " +
                            "'22', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lembar Kendali', " +
                            "'Lembar Kendali " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "LEMBAR_KENDALI_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_LEMBAR_KENDALI.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_LEMBAR_KENDALI + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_LEMBAR_KENDALI + ") VALUES (" + FVALUE_LEMBAR_KENDALI.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_LEMBAR_KENDALI.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_LEMBAR_KENDALI, objekTanggapan, 1);
            }
            else
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                LEMBAR_KENDALI.SaveAs(path + "LEMBAR_KENDALI_" + PROPOSAL_PNPS_CODE_FIXER + Extension_LEMBAR_KENDALI.ToUpper());

                int LASTID_LEMBAR_KENDALI = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_LEMBAR_KENDALI = MixHelper.GetLogCode();
                var FNAME_LEMBAR_KENDALI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_LEMBAR_KENDALI = "'" + LASTID_LEMBAR_KENDALI + "', " +
                            "'18', " +
                            "'22', " +
                            "'" + PROPOSAL_ID + "', " +
                              "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lembar Kendali', " +
                            "'Lembar Kendali " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "LEMBAR_KENDALI_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_LEMBAR_KENDALI.ToUpper().Replace(".", "") + "', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_LEMBAR_KENDALI + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_LEMBAR_KENDALI + ") VALUES (" + FVALUE_LEMBAR_KENDALI.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_LEMBAR_KENDALI.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_LEMBAR_KENDALI, objekTanggapan, 1);
            }
            if (APPROVAL_TYPE == 1)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 3, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 3, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 10").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,10," + APPROVAL_STATUS_SESSION + ")");
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_RASNI = " + DATENOW + ", MONITORING_HASIL_APP_RASNI = 1 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 10").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0,'" + APPROVAL_REASON + "'," + DATENOW + "," + USER_ID + ",1,10," + APPROVAL_STATUS_SESSION + ")");
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_RASNI = " + DATENOW + ", MONITORING_HASIL_APP_RASNI = 0 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult CreateAnggota(HttpPostedFileBase DATA_RSNI, int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, int SUBMIT_TIPE = 0)
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            if (SUBMIT_TIPE == 0)
            {
                if (DATA_RSNI.ContentLength > 0)
                {
                    var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
                    var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT"));
                    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                    Stream stremdokumen = DATA_RSNI.InputStream;
                    byte[] appData = new byte[DATA_RSNI.ContentLength + 1];
                    stremdokumen.Read(appData, 0, DATA_RSNI.ContentLength);
                    string Extension = Path.GetExtension(DATA_RSNI.FileName);
                    if (Extension.ToLower() == ".docx" || Extension.ToLower() == ".doc")
                    {

                        Aspose.Words.Document doc = new Aspose.Words.Document(stremdokumen);
                        var SNI_DOC_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0)+1 AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.SNI_DOC_TYPE = 7").SingleOrDefault();
                        string filePathdoc = path + "DRAFT_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX";
                        string filePathpdf = path + "DRAFT_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".PDF";
                        string filePathxml = path + "DRAFT_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".XML";
                        doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                        doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                        doc.Save(@"" + filePathxml);
                        doc.Save(@"" + path + "DRAFT_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".DOCX", Aspose.Words.SaveFormat.Docx);

                        var DATENOW = MixHelper.ConvertDateNow();
                        var LOGCODE_SNI_DOC = MixHelper.GetLogCode();
                        int LASTID_SNI_DOC = MixHelper.GetSequence("TRX_SNI_DOC");

                        var fname = "SNI_DOC_ID,SNI_DOC_PROPOSAL_ID,SNI_DOC_TYPE,SNI_DOC_FILE_PATH,SNI_DOC_VERSION,SNI_DOC_CREATE_BY,SNI_DOC_CREATE_DATE,SNI_DOC_LOG_CODE,SNI_DOC_STATUS";
                        var fvalue = "'" + LASTID_SNI_DOC + "', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'7', " +
                                        "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/DRAFT/DRAFT_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX" + "', " +
                                        "'" + SNI_DOC_VERSION + "', " +
                                        "'" + USER_ID + "', " +
                                        DATENOW + "," +
                                        "'" + LOGCODE_SNI_DOC + "', " +
                                        "'1'";
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_SNI_DOC (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");


                        var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 18 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 18 AND DOC_STATUS = 1").SingleOrDefault();
                        if (CEKDOKUMEN == null)
                        {
                            int LASTID = MixHelper.GetSequence("TRX_DOCUMENTS");
                            var LOGCODE_RASNI = MixHelper.GetLogCode();
                            var FNAME_RASNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                            var FVALUE_RASNI = "'" + LASTID + "', " +
                                        "'18', " +
                                        "'18', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Draft RSNI 5" + "', " +
                                        "'Draft RASNI " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                        "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/" + "', " +
                                        "'" + "DRAFT_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                                        "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                        "'0', " +
                                        "'" + USER_ID + "', " +
                                        DATENOW + "," +
                                        "'1', " +
                                        "'" + LOGCODE_RASNI + "'";
                            db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_RASNI + ") VALUES (" + FVALUE_RASNI.Replace("''", "NULL") + ")");
                            String objekTanggapan = FVALUE_RASNI.Replace("'", "-");
                            MixHelper.InsertLog(LOGCODE_RASNI, objekTanggapan, 1);
                        }
                    }
                }
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateRatek(HttpPostedFileBase DATA_RSNI, HttpPostedFileBase BA_RATEK, HttpPostedFileBase DAFTAR_HADIR, HttpPostedFileBase NOTULEN, int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, int SUBMIT_TIPE = 0, string NO_RATEK = "", string TGL_RATEK = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;

            Stream STREAM_DATA_RSNI = DATA_RSNI.InputStream;
            byte[] APPDATA_DATA_RSNI = new byte[DATA_RSNI.ContentLength + 1];
            STREAM_DATA_RSNI.Read(APPDATA_DATA_RSNI, 0, DATA_RSNI.ContentLength);
            string Extension_DATA_RSNI = Path.GetExtension(DATA_RSNI.FileName);

            Stream STREAM_BA_RATEK = BA_RATEK.InputStream;
            byte[] APPDATA_BA_RATEK = new byte[BA_RATEK.ContentLength + 1];
            STREAM_BA_RATEK.Read(APPDATA_BA_RATEK, 0, BA_RATEK.ContentLength);
            string Extension_BA_RATEK = Path.GetExtension(BA_RATEK.FileName);

            Stream STREAM_DAFTAR_HADIR = DAFTAR_HADIR.InputStream;
            byte[] APPDATA_DAFTAR_HADIR = new byte[DAFTAR_HADIR.ContentLength + 1];
            STREAM_DAFTAR_HADIR.Read(APPDATA_DAFTAR_HADIR, 0, DAFTAR_HADIR.ContentLength);
            string Extension_DAFTAR_HADIR = Path.GetExtension(DAFTAR_HADIR.FileName);

            int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 10").SingleOrDefault();
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
                                        "'10'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_RAPAT (" + FNAME_PROPOSAL_RAPAT + ") VALUES (" + FVALUE_PROPOSAL_RAPAT.Replace("''", "NULL") + ")");

            if (Extension_BA_RATEK.ToLower() == ".docx" || Extension_BA_RATEK.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_BA_RATEK);
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
                            "'18', " +
                            "'19', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Ver " + VERSION_RATEK + "', " +
                            "'Berita Acara Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                BA_RATEK.SaveAs(path + "BERITA_ACARA_Ver" + VERSION_RATEK + "_RASNI_" + PROPOSAL_PNPS_CODE_FIXER + Extension_BA_RATEK.ToUpper());

                int LASTID_BA_RATEK = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_BA_RATEK = MixHelper.GetLogCode();
                var FNAME_BA_RATEK = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_BA_RATEK = "'" + LASTID_BA_RATEK + "', " +
                            "'18', " +
                            "'19', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Ver " + VERSION_RATEK + "', " +
                            "'Berita Acara Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DAFTAR_HADIR);
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
                            "'18', " +
                            "'20', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Ver " + VERSION_RATEK + "', " +
                            "'Daftar Hadir Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                DAFTAR_HADIR.SaveAs(path + "DAFTAR_HADIR_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + Extension_DAFTAR_HADIR.ToUpper());

                int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                            "'18', " +
                            "'20', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Ver " + VERSION_RATEK + "', " +
                            "'Daftar Hadir Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
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

            if (Extension_DATA_RSNI.ToLower() == ".docx" || Extension_DATA_RSNI.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");

                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DATA_RSNI);
                string filePathdoc = path + "RASNI_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "RASNI_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "RASNI_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                string pathSNIDOC = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                var SNI_DOC_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0)+1 AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.SNI_DOC_TYPE = 7").SingleOrDefault();
                string filePathdocSNI_DOC = pathSNIDOC + "DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX";
                string filePathpdfSNI_DOC = pathSNIDOC + "DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".PDF";
                string filePathxmlSNI_DOC = pathSNIDOC + "DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".XML";
                doc.Save(@"" + filePathdocSNI_DOC, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdfSNI_DOC, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxmlSNI_DOC);
                doc.Save(@"" + pathSNIDOC + "RASNI_" + PROPOSAL_PNPS_CODE_FIXER + ".DOCX", Aspose.Words.SaveFormat.Docx);

                var LOGCODE_SNI_DOC = MixHelper.GetLogCode();
                int LASTID_SNI_DOC = MixHelper.GetSequence("TRX_SNI_DOC");

                var fname = "SNI_DOC_ID,SNI_DOC_PROPOSAL_ID,SNI_DOC_TYPE,SNI_DOC_FILE_PATH,SNI_DOC_VERSION,SNI_DOC_CREATE_BY,SNI_DOC_CREATE_DATE,SNI_DOC_LOG_CODE,SNI_DOC_STATUS";
                var fvalue = "'" + LASTID_SNI_DOC + "', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'7', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/DRAFT/DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX" + "', " +
                                "'" + SNI_DOC_VERSION + "', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'" + LOGCODE_SNI_DOC + "', " +
                                "'1'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_SNI_DOC (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

                var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 18 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 18 AND DOC_STATUS = 1").SingleOrDefault();
                if (CEKDOKUMEN != null)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                }
                int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                            "'18', " +
                            "'18', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") RASNI Ver " + VERSION_RATEK + "" + "', " +
                            "'RASNI Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RASNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "RASNI_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
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

            if (SUBMIT_TIPE == 1)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);


                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 10").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,8," + APPROVAL_STATUS_SESSION + ")");
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_RASNI = " + TGL_RATEK_CONVERT + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);


                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 10").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0," + DATENOW + "," + USER_ID + ",1,8," + APPROVAL_STATUS_SESSION + ")");
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        public ActionResult DataRASNIKomtek(DataTables param)
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

            string where_clause = "PROPOSAL_STATUS = 10 AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID;

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
                Convert.ToString("<center><a href='/Perumusan/RASNI/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 10 && list.PROPOSAL_STATUS_PROSES == 0)?"<a href='/Perumusan/RASNI/Create/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Susun RASNI'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DataRASNIPPS(DataTables param)
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


            string where_clause = "PROPOSAL_STATUS = 15 AND PROPOSAL_STATUS_PROSES = 1  " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");

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
                Convert.ToString("<center><a href='/Perumusan/UsulanPenetapan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 15 && list.PROPOSAL_STATUS_PROSES == 1)?"<a href='/Perumusan/UsulanPenetapan/Pengesahan/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Pengesahan RASNI'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
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
