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

namespace SISPK.Controllers.Pengajuan
{
    [Auth(RoleTipe = 1)]
    public class UsulanController : Controller
    {
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            var IS_KOMTEK = Convert.ToInt32(Session["IS_KOMTEK"]);
            string ViewName = ((IS_KOMTEK == 1) ? "UsulanBaruKomtek" : "UsulanBaruPPS");
            return View(ViewName);
            //return Content("Hello");
        }
        [Auth(RoleTipe = 2)]
        public ActionResult Create()
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var ListKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 && komtek.KOMTEK_ID == USER_KOMTEK_ID select komtek).SingleOrDefault();
            
            ViewData["Komtek"] = DataKomtek;
            ViewData["ListKomtek"] = ListKomtek;
            return View();
        }
        [Auth(RoleTipe = 3)]
        public ActionResult Update(int id = 0)
        {
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 && komtek.KOMTEK_ID == DataProposal.KOMTEK_ID select komtek).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var ListKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            var Lampiran = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();
            var Bukti = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
            var Surat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 32").FirstOrDefault();
            var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();

            ViewData["DataProposal"] = DataProposal;
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            ViewData["Komtek"] = DataKomtek;
            ViewData["ListKomtek"] = ListKomtek;
            ViewData["AdopsiList"] = AdopsiList;
            ViewData["RevisiList"] = RevisiList;
            ViewData["Lampiran"] = Lampiran;
            ViewData["Bukti"] = Bukti;
            ViewData["Surat"] = Surat;
            ViewData["Outline"] = Outline;
            return View();
        }
        public ActionResult Detail(int id = 0)
        {
            //var paten = (from ptn in db.VIEW_PROPOSAL where ptn.PROPOSAL_ID == id select ptn).SingleOrDefault();
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
            return View();

            //var HakPaten = ("SELECT SUBSTR(PROPOSAL_HAK_PATEN_LOCATION, 1, 26) AS FILE_PATCH,REGEXP_SUBSTR(PROPOSAL_HAK_PATEN_LOCATION, '[^/'']+', 1, 4) AS FILE_NAME FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + id);
            //return Content("test : "+ DataProposal);
        }
        
        public ActionResult Test() {
            return Json(new { jam = (DateTime.Now).ToString(), jam2 = DateTime.Now.ToString("yyyyMMddHHmmss") }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Create(TRX_PROPOSAL INPUT, int[] PROPOSAL_REV_MERIVISI_ID, string[] PROPOSAL_ADOPSI_NOMOR_JUDUL, int[] PROPOSAL_REF_SNI_ID, string[] PROPOSAL_REF_NON_SNI, string[] BIBLIOGRAFI, string[] PROPOSAL_LPK_ID, string[] PROPOSAL_RETEK_ID)
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var LOGCODE = MixHelper.GetLogCode();
            int LASTID = MixHelper.GetSequence("TRX_PROPOSAL");
            var DATENOW = MixHelper.ConvertDateNow();
            var PROPOSAL_CODE = db.Database.SqlQuery<String>("SELECT TO_CHAR (SYSDATE, 'YYYYMMDD-') || ( CASE WHEN LENGTH (COUNT(PROPOSAL_ID) + 1) = 1 THEN '000' || CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) WHEN LENGTH (COUNT(PROPOSAL_ID) + 1) = 2 THEN '00' || CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255)) WHEN LENGTH (COUNT(PROPOSAL_ID) + 1) = 3 THEN '0' || CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) ELSE CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) END ) PROPOSAL_CODE FROM TRX_PROPOSAL WHERE TO_CHAR (SYSDATE, 'MM-DD-YYYY') = TO_CHAR (PROPOSAL_CREATE_DATE,'MM-DD-YYYY')").SingleOrDefault();
            var PROPOSAL_LPK_ID_CONVERT = "";
            var PROPOSAL_RETEK_ID_CONVERT = "";
            if (PROPOSAL_LPK_ID == null)
            {
                PROPOSAL_LPK_ID_CONVERT = null;
            }
            else
            {
                PROPOSAL_LPK_ID_CONVERT = string.Join(";", PROPOSAL_LPK_ID);
            }

            if (PROPOSAL_RETEK_ID == null)
            {
                PROPOSAL_RETEK_ID_CONVERT = null;
            }
            else
            {
                PROPOSAL_RETEK_ID_CONVERT = string.Join(";", PROPOSAL_RETEK_ID);
            }

            

            string pathnya = Server.MapPath("~/Upload/Dokumen/HAK_PATEN/");
            HttpPostedFileBase file_paten = Request.Files["PROPOSAL_HAK_PATEN_LOCATION"];
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
                    file_name_paten = "HAK_PATEN_ID_PROPOSAL_" + LASTID + "_" + fileExtension_paten;
                    filePath_paten = pathnya + file_name_paten.Replace(" ", "_");
                    file_paten.SaveAs(filePath_paten);
                }
            }


            var fname = "";
            var fvalue = "";

            if (INPUT.PROPOSAL_HAK_PATEN_LOCATION != null)
            {
                //var PROPOSAL_LPK_ID_CONVERT = string.Join(";", PROPOSAL_LPK_ID);
                //var PROPOSAL_RETEK_ID_CONVERT = string.Join(";", PROPOSAL_RETEK_ID);
                PROPOSAL_LPK_ID_CONVERT = ((PROPOSAL_LPK_ID_CONVERT == null) ? "" : PROPOSAL_LPK_ID_CONVERT);
                PROPOSAL_RETEK_ID_CONVERT = ((PROPOSAL_RETEK_ID_CONVERT == null) ? "" : PROPOSAL_RETEK_ID_CONVERT);
                fname = "PROPOSAL_ID,PROPOSAL_TYPE,PROPOSAL_RETEK_ID,PROPOSAL_LPK_ID,PROPOSAL_YEAR,PROPOSAL_KOMTEK_ID,PROPOSAL_KONSEPTOR,PROPOSAL_INSTITUSI,PROPOSAL_JUDUL_PNPS,PROPOSAL_RUANG_LINGKUP,PROPOSAL_JENIS_PERUMUSAN,PROPOSAL_JALUR,PROPOSAL_JENIS_ADOPSI,PROPOSAL_METODE_ADOPSI,PROPOSAL_TERJEMAHAN_SNI_ID,PROPOSAL_RALAT_SNI_ID,PROPOSAL_AMD_SNI_ID,PROPOSAL_IS_URGENT,PROPOSAL_PASAL,PROPOSAL_IS_HAK_PATEN,PROPOSAL_IS_HAK_PATEN_DESC,PROPOSAL_INFORMASI,PROPOSAL_TUJUAN,PROPOSAL_PROGRAM_PEMERINTAH,PROPOSAL_PIHAK_BERKEPENTINGAN,PROPOSAL_CREATE_BY,PROPOSAL_CREATE_DATE,PROPOSAL_STATUS,PROPOSAL_STATUS_PROSES,PROPOSAL_LOG_CODE,PROPOSAL_MANFAAT_PENERAPAN,PROPOSAL_IS_ORG_MENDUKUNG,PROPOSAL_IS_DUPLIKASI_DESC,PROPOSAL_HAK_PATEN_LOCATION,PROPOSAL_CODE";
                fvalue = "'" + LASTID + "', " +
                        "'" + INPUT.PROPOSAL_TYPE + "', " +
                        "'" + PROPOSAL_RETEK_ID_CONVERT + "', " +
                        "'" + PROPOSAL_LPK_ID_CONVERT + "', " +
                        "'" + INPUT.PROPOSAL_YEAR + "', " +
                        "'" + INPUT.PROPOSAL_KOMTEK_ID + "', " +
                        "'" + INPUT.PROPOSAL_KONSEPTOR + "', " +
                        "'" + INPUT.PROPOSAL_INSTITUSI + "', " +
                        "'" + INPUT.PROPOSAL_JUDUL_PNPS + "', " +
                        "'" + INPUT.PROPOSAL_RUANG_LINGKUP + "', " +
                        "'" + INPUT.PROPOSAL_JENIS_PERUMUSAN + "', " +
                        "'" + INPUT.PROPOSAL_JALUR + "', " +
                        "'" + INPUT.PROPOSAL_JENIS_ADOPSI + "', " +
                        "'" + INPUT.PROPOSAL_METODE_ADOPSI + "', " +
                        "'" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "', " +
                        "'" + INPUT.PROPOSAL_RALAT_SNI_ID + "', " +
                        "'" + INPUT.PROPOSAL_AMD_SNI_ID + "', " +
                        "'" + INPUT.PROPOSAL_IS_URGENT + "', " +
                        "'" + INPUT.PROPOSAL_PASAL + "', " +
                        "'" + INPUT.PROPOSAL_IS_HAK_PATEN + "', " +
                        "'" + INPUT.PROPOSAL_IS_HAK_PATEN_DESC + "', " +
                        "'" + INPUT.PROPOSAL_INFORMASI + "', " +
                        "'" + INPUT.PROPOSAL_TUJUAN + "', " +
                        "'" + INPUT.PROPOSAL_PROGRAM_PEMERINTAH + "', " +
                        "'" + INPUT.PROPOSAL_PIHAK_BERKEPENTINGAN + "', " +
                        "'" + USER_ID + "', " +
                        DATENOW + "," +
                        "'0', " +
                        "'1', " +
                        "'" + LOGCODE + "'," +
                        "'" + INPUT.PROPOSAL_MANFAAT_PENERAPAN + "'," +
                        "'" + INPUT.PROPOSAL_IS_ORG_MENDUKUNG + "'," +
                        "'" + INPUT.PROPOSAL_IS_DUPLIKASI_DESC + "'," +
                        "'/Upload/Dokumen/HAK_PATEN/" + file_name_paten.Replace(" ", "_") + "'," +
                        "'" + PROPOSAL_CODE + "'";
            }
            else
            {
                //var PROPOSAL_LPK_ID_CONVERT = string.Join(";", PROPOSAL_LPK_ID);
                //var PROPOSAL_RETEK_ID_CONVERT = string.Join(";", PROPOSAL_RETEK_ID);
                PROPOSAL_LPK_ID_CONVERT = ((PROPOSAL_LPK_ID_CONVERT == null) ? "" : PROPOSAL_LPK_ID_CONVERT);
                PROPOSAL_RETEK_ID_CONVERT = ((PROPOSAL_RETEK_ID_CONVERT == null) ? "" : PROPOSAL_RETEK_ID_CONVERT);
                fname = "PROPOSAL_ID,PROPOSAL_TYPE,PROPOSAL_RETEK_ID,PROPOSAL_LPK_ID,PROPOSAL_YEAR,PROPOSAL_KOMTEK_ID,PROPOSAL_KONSEPTOR,PROPOSAL_INSTITUSI,PROPOSAL_JUDUL_PNPS,PROPOSAL_RUANG_LINGKUP,PROPOSAL_JENIS_PERUMUSAN,PROPOSAL_JALUR,PROPOSAL_JENIS_ADOPSI,PROPOSAL_METODE_ADOPSI,PROPOSAL_TERJEMAHAN_SNI_ID,PROPOSAL_RALAT_SNI_ID,PROPOSAL_AMD_SNI_ID,PROPOSAL_IS_URGENT,PROPOSAL_PASAL,PROPOSAL_IS_HAK_PATEN,PROPOSAL_IS_HAK_PATEN_DESC,PROPOSAL_INFORMASI,PROPOSAL_TUJUAN,PROPOSAL_PROGRAM_PEMERINTAH,PROPOSAL_PIHAK_BERKEPENTINGAN,PROPOSAL_CREATE_BY,PROPOSAL_CREATE_DATE,PROPOSAL_STATUS,PROPOSAL_STATUS_PROSES,PROPOSAL_LOG_CODE,PROPOSAL_MANFAAT_PENERAPAN,PROPOSAL_IS_ORG_MENDUKUNG,PROPOSAL_IS_DUPLIKASI_DESC,PROPOSAL_CODE";
                fvalue = "'" + LASTID + "', " +
                        "'" + INPUT.PROPOSAL_TYPE + "', " +
                        "'" + PROPOSAL_RETEK_ID_CONVERT + "', " +
                        "'" + PROPOSAL_LPK_ID_CONVERT + "', " +
                        "'" + INPUT.PROPOSAL_YEAR + "', " +
                        "'" + INPUT.PROPOSAL_KOMTEK_ID + "', " +
                        "'" + INPUT.PROPOSAL_KONSEPTOR + "', " +
                        "'" + INPUT.PROPOSAL_INSTITUSI + "', " +
                        "'" + INPUT.PROPOSAL_JUDUL_PNPS + "', " +
                        "'" + INPUT.PROPOSAL_RUANG_LINGKUP + "', " +
                        "'" + INPUT.PROPOSAL_JENIS_PERUMUSAN + "', " +
                        "'" + INPUT.PROPOSAL_JALUR + "', " +
                        "'" + INPUT.PROPOSAL_JENIS_ADOPSI + "', " +
                        "'" + INPUT.PROPOSAL_METODE_ADOPSI + "', " +
                        "'" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "', " +
                        "'" + INPUT.PROPOSAL_RALAT_SNI_ID + "', " +
                        "'" + INPUT.PROPOSAL_AMD_SNI_ID + "', " +
                        "'" + INPUT.PROPOSAL_IS_URGENT + "', " +
                        "'" + INPUT.PROPOSAL_PASAL + "', " +
                        "'" + INPUT.PROPOSAL_IS_HAK_PATEN + "', " +
                        "'" + INPUT.PROPOSAL_IS_HAK_PATEN_DESC + "', " +
                        "'" + INPUT.PROPOSAL_INFORMASI + "', " +
                        "'" + INPUT.PROPOSAL_TUJUAN + "', " +
                        "'" + INPUT.PROPOSAL_PROGRAM_PEMERINTAH + "', " +
                        "'" + INPUT.PROPOSAL_PIHAK_BERKEPENTINGAN + "', " +
                        "'" + USER_ID + "', " +
                        DATENOW + "," +
                        "'0', " +
                        "'1', " +
                        "'" + LOGCODE + "'," +
                        "'" + INPUT.PROPOSAL_MANFAAT_PENERAPAN + "'," +
                        "'" + INPUT.PROPOSAL_IS_ORG_MENDUKUNG + "'," +
                        "'" + INPUT.PROPOSAL_IS_DUPLIKASI_DESC + "'," +
                        "'" + PROPOSAL_CODE + "'";
            }
            

            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");
            //var tester1 = ("INSERT INTO TRX_PROPOSAL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");
            //return Content ("tes" + tester);
            var tester = "INSERT INTO TRX_PROPOSAL_FIXER (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")";
            if (PROPOSAL_REV_MERIVISI_ID != null)
            {
                foreach (var PROPOSAL_REV_MERIVISI_ID_VAL in PROPOSAL_REV_MERIVISI_ID)
                {
                    var PROPOSAL_REV_ID = MixHelper.GetSequence("TRX_PROPOSAL_REV");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REV (PROPOSAL_REV_ID,PROPOSAL_REV_PROPOSAL_ID,PROPOSAL_REV_MERIVISI_ID) VALUES (" + PROPOSAL_REV_ID + "," + LASTID + "," + PROPOSAL_REV_MERIVISI_ID_VAL + ")");
                }
            }
            if (PROPOSAL_ADOPSI_NOMOR_JUDUL != null)
            {
                foreach (var PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL in PROPOSAL_ADOPSI_NOMOR_JUDUL)
                {
                    var PROPOSAL_ADOPSI_ID = MixHelper.GetSequence("TRX_PROPOSAL_ADOPSI");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_ADOPSI (PROPOSAL_ADOPSI_ID,PROPOSAL_ADOPSI_PROPOSAL_ID,PROPOSAL_ADOPSI_NOMOR_JUDUL) VALUES (" + PROPOSAL_ADOPSI_ID + "," + LASTID + ",'" + PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL + "')");
                }
            }

            if (PROPOSAL_REF_SNI_ID != null)
            {
                foreach (var SNI_ID in PROPOSAL_REF_SNI_ID)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",1," + SNI_ID + ")");
                }
            }
            if (PROPOSAL_REF_NON_SNI != null)
            {
                foreach (var DATA_NON_SNI_VAL in PROPOSAL_REF_NON_SNI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    var CEK_PROPOSAL_REF_NON_SNI = db.Database.SqlQuery<MASTER_ACUAN_NON_SNI>("SELECT * FROM MASTER_ACUAN_NON_SNI WHERE ACUAN_NON_SNI_STATUS = 1 AND LOWER(ACUAN_NON_SNI_JUDUL) = '" + DATA_NON_SNI_VAL.ToLower() + "'").SingleOrDefault();
                    if (CEK_PROPOSAL_REF_NON_SNI != null)
                    {

                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",2,'" + CEK_PROPOSAL_REF_NON_SNI.ACUAN_NON_SNI_ID + "','" + DATA_NON_SNI_VAL + "')");
                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",2,'" + DATA_NON_SNI_VAL + "')");
                    }

                }
            }
            if (BIBLIOGRAFI != null)
            {
                foreach (var BIBLIOGRAFI_VAL in BIBLIOGRAFI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    var CEK_BIBLIOGRAFI = db.Database.SqlQuery<MASTER_BIBLIOGRAFI>("SELECT * FROM MASTER_BIBLIOGRAFI WHERE BIBLIOGRAFI_STATUS = 1 AND LOWER(BIBLIOGRAFI_JUDUL) = '" + BIBLIOGRAFI_VAL.ToLower() + "'").SingleOrDefault();
                    if (CEK_BIBLIOGRAFI != null)
                    {
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",3,'" + CEK_BIBLIOGRAFI.BIBLIOGRAFI_ID + "','" + BIBLIOGRAFI_VAL + "')");
                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",3,'" + BIBLIOGRAFI_VAL + "')");
                    }
                }
            }
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == LASTID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_CODE;
            var PROPOSAL_ID = LASTID;
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
            if (INPUT.PROPOSAL_IS_ORG_MENDUKUNG == 1)
            {
                HttpPostedFileBase file = Request.Files["PROPOSAL_DUKUNGAN_FILE_PATH"];
                if (file.ContentLength > 0)
                {
                    int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                    Stream stremdokumen = file.InputStream;
                    byte[] appData = new byte[file.ContentLength + 1];
                    stremdokumen.Read(appData, 0, file.ContentLength);
                    string Extension = Path.GetExtension(file.FileName);
                    if (Extension.ToLower() == ".pdf")
                    {
                        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                        string filePathpdf = path + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                        string filePathxml = path + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                        pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                        pdf.Save(@"" + filePathxml);
                        var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                        var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                        var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                    "'10', " +
                                    "'29', " +
                                    "'" + PROPOSAL_ID + "', " +
                                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Bukti Dukungan Usulan" + "', " +
                                    "'Bukti Dukungan Usulan dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                    "'" + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                    "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                    "'0', " +
                                    "'" + USER_ID + "', " +
                                    DATENOW + "," +
                                    "'1', " +
                                    "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                        String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                        MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                    }
                }
            }
            HttpPostedFileBase file2 = Request.Files["PROPOSAL_LAMPIRAN_FILE_PATH"];
            if (file2.ContentLength > 0)
            {
                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream stremdokumen = file2.InputStream;
                byte[] appData = new byte[file2.ContentLength + 1];
                stremdokumen.Read(appData, 0, file2.ContentLength);
                string Extension = Path.GetExtension(file2.FileName);
                if (Extension.ToLower() == ".pdf")
                {
                    Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                    string filePathpdf = path + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                    string filePathxml = path + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                    pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    pdf.Save(@"" + filePathxml);
                    var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                    var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                "'10', " +
                                "'30', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Pendukung Usulan" + "', " +
                                "'Lampiran Pendukung Usulan dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                }
            }
            HttpPostedFileBase file3 = Request.Files["PROPOSAL_SURAT_PENGAJUAN_PNPS"];
            if (file3.ContentLength > 0)
            {
                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream stremdokumen = file3.InputStream;
                byte[] appData = new byte[file3.ContentLength + 1];
                stremdokumen.Read(appData, 0, file3.ContentLength);
                string Extension = Path.GetExtension(file3.FileName);
                if (Extension.ToLower() == ".pdf")
                {
                    Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                    string filePathpdf = path + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                    string filePathxml = path + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                    pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    pdf.Save(@"" + filePathxml);
                    var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                    var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                "'10', " +
                                "'32', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Surat Pengajuan PNPS" + "', " +
                                "'Lampiran Surat Pengajuan PNPS dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                }
            }
            HttpPostedFileBase file4 = Request.Files["PROPOSAL_OUTLINE_RSNI"];
            if (file4.ContentLength > 0)
            {
                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream stremdokumen = file4.InputStream;
                byte[] appData = new byte[file4.ContentLength + 1];
                stremdokumen.Read(appData, 0, file4.ContentLength);
                string Extension = Path.GetExtension(file4.FileName);
                if (Extension.ToLower() == ".pdf")
                {
                    Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                    string filePathpdf = path + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                    string filePathxml = path + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                    pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    pdf.Save(@"" + filePathxml);
                    var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                    var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                "'10', " +
                                "'36', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Outline RSNI" + "', " +
                                "'Lampiran Outline RSNI dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                }
            }
            //db.Database.ExecuteSqlCommand("INSERT INTO SYS_NOTIF (NOTIF_ID,NOTIF_USER_ID,NOTIF_LINK,NOTIF_MESSAGE,NOTIF_DATE,NOTIF_TYPE,NOTIF_IS_FLASH,NOTIF_IS_COUNT,NOTIF_CATEGORY,NOTIF_DATA_ID,NOTIF_STATUS) SELECT SEQ_SYS_NOTIF.NEXTVAL, USER_ID,'/Pengajuan/Usulan/ApprovalUsulan/" + LASTID + "','Usulan Baru',SYSDATE,1,1,1,1," + LASTID + ",1 FROM VIEW_USERS WHERE USER_ACCESS_ID = 5 AND USER_TYPE_ID = 1 AND USER_STATUS = 1");

            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(LOGCODE, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
            //return Json(new { INPUT, PROPOSAL_REV_MERIVISI_ID, PROPOSAL_ADOPSI_NOMOR_JUDUL, PROPOSAL_REF_SNI_ID, PROPOSAL_REF_NON_SNI, BIBLIOGRAFI }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult CreateBackup(TRX_PROPOSAL INPUT, int[] PROPOSAL_REV_MERIVISI_ID, string[] PROPOSAL_ADOPSI_NOMOR_JUDUL, int[] PROPOSAL_REF_SNI_ID, int[] PROPOSAL_REF_NON_SNI, int[] BIBLIOGRAFI)
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var LOGCODE = MixHelper.GetLogCode();
            int LASTID = MixHelper.GetSequence("TRX_PROPOSAL");
            var DATENOW = MixHelper.ConvertDateNow();
            var fname = "PROPOSAL_ID,PROPOSAL_TYPE,PROPOSAL_YEAR,PROPOSAL_KOMTEK_ID,PROPOSAL_KONSEPTOR,PROPOSAL_INSTITUSI,PROPOSAL_JUDUL_PNPS,PROPOSAL_RUANG_LINGKUP,PROPOSAL_JENIS_PERUMUSAN,PROPOSAL_JALUR,PROPOSAL_JENIS_ADOPSI,PROPOSAL_METODE_ADOPSI,PROPOSAL_TERJEMAHAN_SNI_ID,PROPOSAL_RALAT_SNI_ID,PROPOSAL_AMD_SNI_ID,PROPOSAL_IS_URGENT,PROPOSAL_PASAL,PROPOSAL_IS_HAK_PATEN,PROPOSAL_IS_HAK_PATEN_DESC,PROPOSAL_INFORMASI,PROPOSAL_TUJUAN,PROPOSAL_PROGRAM_PEMERINTAH,PROPOSAL_PIHAK_BERKEPENTINGAN,PROPOSAL_CREATE_BY,PROPOSAL_CREATE_DATE,PROPOSAL_STATUS,PROPOSAL_STATUS_PROSES,PROPOSAL_LOG_CODE";
            var fvalue = "'" + LASTID + "', " +
                        "'" + INPUT.PROPOSAL_TYPE + "', " +
                        "'" + INPUT.PROPOSAL_YEAR + "', " +
                        "'" + INPUT.PROPOSAL_KOMTEK_ID + "', " +
                        "'" + INPUT.PROPOSAL_KONSEPTOR + "', " +
                        "'" + INPUT.PROPOSAL_INSTITUSI + "', " +
                        "'" + INPUT.PROPOSAL_JUDUL_PNPS + "', " +
                        "'" + INPUT.PROPOSAL_RUANG_LINGKUP + "', " +
                        "'" + INPUT.PROPOSAL_JENIS_PERUMUSAN + "', " +
                        "'" + INPUT.PROPOSAL_JALUR + "', " +
                        "'" + INPUT.PROPOSAL_JENIS_ADOPSI + "', " +
                        "'" + INPUT.PROPOSAL_METODE_ADOPSI + "', " +
                        "'" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "', " +
                        "'" + INPUT.PROPOSAL_RALAT_SNI_ID + "', " +
                        "'" + INPUT.PROPOSAL_AMD_SNI_ID + "', " +
                        "'" + INPUT.PROPOSAL_IS_URGENT + "', " +
                        "'" + INPUT.PROPOSAL_PASAL + "', " +
                        "'" + INPUT.PROPOSAL_IS_HAK_PATEN + "', " +
                        "'" + INPUT.PROPOSAL_IS_HAK_PATEN_DESC + "', " +
                        "'" + INPUT.PROPOSAL_INFORMASI + "', " +
                        "'" + INPUT.PROPOSAL_TUJUAN + "', " +
                        "'" + INPUT.PROPOSAL_PROGRAM_PEMERINTAH + "', " +
                        "'" + INPUT.PROPOSAL_PIHAK_BERKEPENTINGAN + "', " +
                        "'" + USER_ID + "', " +
                        DATENOW + "," +
                        "'0', " +
                        "'1', " +
                        "'" + LOGCODE + "'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");
            var tester = "INSERT INTO TRX_PROPOSAL_FIXER (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")";
            if (PROPOSAL_REV_MERIVISI_ID != null)
            {
                foreach (var PROPOSAL_REV_MERIVISI_ID_VAL in PROPOSAL_REV_MERIVISI_ID)
                {
                    var PROPOSAL_REV_ID = MixHelper.GetSequence("TRX_PROPOSAL_REV");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REV (PROPOSAL_REV_ID,PROPOSAL_REV_PROPOSAL_ID,PROPOSAL_REV_MERIVISI_ID) VALUES (" + PROPOSAL_REV_ID + "," + LASTID + "," + PROPOSAL_REV_MERIVISI_ID_VAL + ")");
                }
            }
            if (PROPOSAL_ADOPSI_NOMOR_JUDUL != null)
            {
                foreach (var PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL in PROPOSAL_ADOPSI_NOMOR_JUDUL)
                {
                    var PROPOSAL_ADOPSI_ID = MixHelper.GetSequence("TRX_PROPOSAL_ADOPSI");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_ADOPSI (PROPOSAL_ADOPSI_ID,PROPOSAL_ADOPSI_PROPOSAL_ID,PROPOSAL_ADOPSI_NOMOR_JUDUL) VALUES (" + PROPOSAL_ADOPSI_ID + "," + LASTID + ",'" + PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL + "')");
                }
            }

            if (PROPOSAL_REF_SNI_ID != null)
            {
                foreach (var SNI_ID in PROPOSAL_REF_SNI_ID)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",1," + SNI_ID + ")");
                }
            }
            if (PROPOSAL_REF_NON_SNI != null)
            {
                foreach (var DATA_NON_SNI_VAL in PROPOSAL_REF_NON_SNI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",2,'" + DATA_NON_SNI_VAL + "')");
                }
            }
            if (BIBLIOGRAFI != null)
            {
                foreach (var BIBLIOGRAFI_VAL in BIBLIOGRAFI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",3,'" + BIBLIOGRAFI_VAL + "')");
                }
            }
            //return Json(new { tester,INPUT, PROPOSAL_REV_MERIVISI_ID, PROPOSAL_ADOPSI_NOMOR_JUDUL, PROPOSAL_REF_SNI_ID, PROPOSAL_REF_NON_SNI, BIBLIOGRAFI }, JsonRequestBehavior.AllowGet);
            //return Json(new { data11 = INPUT.PROPOSAL_JENIS_ADOPSI, data112 = INPUT.PROPOSAL_METODE_ADOPSI, tester }, JsonRequestBehavior.AllowGet);
            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(LOGCODE, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult CreateXXX(TRX_PROPOSAL INPUT, int[] PROPOSAL_REF_SNI_ID, int[] PROPOSAL_REF_NON_SNI, int[] BIBLIOGRAFI, string nomorstandaradopsi = "", string tahunstandaradopsi = "", string judulstandaradopsi = "", string nomorstandarmodifikasi = "", string judulstandarmodifikasi = "", string tahunstandarmodifikasi = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var LOGCODE = MixHelper.GetLogCode();
            int LASTID = MixHelper.GetSequence("TRX_PROPOSAL");
            var DATENOW = MixHelper.ConvertDateNow();


            var PROPOSAL_PNPS_CODE = db.Database.SqlQuery<string>("SELECT CAST(TO_CHAR (SYSDATE, 'YYYY') || '.' || KOMTEK_CODE || '.' || ( SELECT CAST ( ( CASE WHEN LENGTH (COUNT(PROPOSAL_ID) + 1) = 1 THEN '0' || CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) ELSE CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) END ) AS VARCHAR2 (255) ) PNPSCODE FROM TRX_PROPOSAL WHERE PROPOSAL_KOMTEK_ID = KOMTEK_ID ) AS VARCHAR2(255)) AS PNPSCODE FROM MASTER_KOMITE_TEKNIS WHERE KOMTEK_ID = " + INPUT.PROPOSAL_KOMTEK_ID).SingleOrDefault();
            var PROPOSAL_NO_SNI_PROPOSAL = "";
            var PROPOSAL_JUDUL_SNI_PROPOSAL = "";
            //if (INPUT.PROPOSAL_JENIS_PERUMUSAN != 1)
            //{
            //    var GETDATASNI = db.Database.SqlQuery<VIEW_SNI>("SELECT * FROM VIEW_SNI WHERE SNI_ID = " + INPUT.PROPOSAL_REVISI_SNI_ID).SingleOrDefault();
            //    if (GETDATASNI != null)
            //    {
            //        PROPOSAL_NO_SNI_PROPOSAL = GETDATASNI.SNI_NOMOR;
            //        PROPOSAL_JUDUL_SNI_PROPOSAL = GETDATASNI.SNI_JUDUL;
            //    }
            //    else
            //    {
            //        if (nomorstandaradopsi != "")
            //        {
            //            PROPOSAL_NO_SNI_PROPOSAL = nomorstandaradopsi + ":" + INPUT.PROPOSAL_YEAR;
            //            PROPOSAL_JUDUL_SNI_PROPOSAL = judulstandaradopsi;
            //        }
            //        else
            //        {
            //            PROPOSAL_NO_SNI_PROPOSAL = nomorstandarmodifikasi + ":" + INPUT.PROPOSAL_YEAR;
            //            PROPOSAL_JUDUL_SNI_PROPOSAL = judulstandarmodifikasi;
            //        }

            //    }
            //}

            var fname = "PROPOSAL_ID,PROPOSAL_TYPE,PROPOSAL_YEAR,PROPOSAL_KOMTEK_ID,PROPOSAL_KONSEPTOR,PROPOSAL_INSTITUSI,PROPOSAL_NO_SNI_PROPOSAL,PROPOSAL_JUDUL_SNI_PROPOSAL,PROPOSAL_JUDUL_PNPS,PROPOSAL_RUANG_LINGKUP,PROPOSAL_JENIS_PERUMUSAN,PROPOSAL_JALUR,PROPOSAL_METODE_ADOPSI,PROPOSAL_NO_STD_ADOPSI_IDENTIK,PROPOSAL_TERJEMAHAN_SNI_ID,PROPOSAL_IS_ADOPSI_MOD,PROPOSAL_NO_STANDAR_MODIFIKASI,PROPOSAL_IS_URGENT,PROPOSAL_REVISI_SNI_ID,PROPOSAL_REVISI_PASAL,PROPOSAL_IS_HAK_PATEN,PROPOSAL_IS_HAK_PATEN_DESC,PROPOSAL_INFORMASI,PROPOSAL_TUJUAN,PROPOSAL_PROGRAM_PEMERINTAH,PROPOSAL_PIHAK_BERKEPENTINGAN,PROPOSAL_LAMPIRAN_FILE_PATH,PROPOSAL_CREATE_BY,PROPOSAL_CREATE_DATE,PROPOSAL_STATUS,PROPOSAL_STATUS_PROSES,PROPOSAL_LOG_CODE";
            var fvalue = "'" + LASTID + "', " +
                        "'" + INPUT.PROPOSAL_TYPE + "', " +
                        "'" + INPUT.PROPOSAL_YEAR + "', " +
                        "'" + INPUT.PROPOSAL_KOMTEK_ID + "', " +
                        "'" + INPUT.PROPOSAL_KONSEPTOR + "', " +
                        "'" + INPUT.PROPOSAL_INSTITUSI + "', " +
                        "'" + PROPOSAL_NO_SNI_PROPOSAL + "', " +
                        "'" + PROPOSAL_JUDUL_SNI_PROPOSAL + "', " +
                //"'" + PROPOSAL_PNPS_CODE + "', " +
                        "'" + INPUT.PROPOSAL_JUDUL_PNPS + "', " +
                        "'" + INPUT.PROPOSAL_RUANG_LINGKUP + "', " +
                        "'" + INPUT.PROPOSAL_JENIS_PERUMUSAN + "', " +
                        "'" + INPUT.PROPOSAL_JALUR + "', " +
                        "'" + INPUT.PROPOSAL_METODE_ADOPSI + "', " +
                //"'" + INPUT.PROPOSAL_NO_STD_ADOPSI_IDENTIK + "', " +
                        "'" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "', " +
                //"'" + INPUT.PROPOSAL_IS_ADOPSI_MOD + "', " +
                //"'" + INPUT.PROPOSAL_NO_STANDAR_MODIFIKASI + "', " +
                        "'" + INPUT.PROPOSAL_IS_URGENT + "', " +
                //"'" + INPUT.PROPOSAL_REVISI_SNI_ID + "', " +
                //"'" + INPUT.PROPOSAL_REVISI_PASAL + "', " +
                        "'" + INPUT.PROPOSAL_IS_HAK_PATEN + "', " +
                        "'" + INPUT.PROPOSAL_IS_HAK_PATEN_DESC + "', " +
                        "'" + INPUT.PROPOSAL_INFORMASI + "', " +
                        "'" + INPUT.PROPOSAL_TUJUAN + "', " +
                        "'" + INPUT.PROPOSAL_PROGRAM_PEMERINTAH + "', " +
                        "'" + INPUT.PROPOSAL_PIHAK_BERKEPENTINGAN + "', " +
                //"'" + ((file_att != null) ? file_name_att : "") + "', " +
                        "NULL, " +
                        "'" + USER_ID + "', " +
                        DATENOW + "," +
                        "'0', " +
                        "'1', " +
                        "'" + LOGCODE + "'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            if (PROPOSAL_REF_SNI_ID != null)
            {
                foreach (var SNI_ID in PROPOSAL_REF_SNI_ID)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",1," + SNI_ID + ")");
                }
            }
            if (PROPOSAL_REF_NON_SNI != null)
            {
                foreach (var DATA_NON_SNI_VAL in PROPOSAL_REF_NON_SNI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",2,'" + DATA_NON_SNI_VAL + "')");
                }
            }
            if (BIBLIOGRAFI != null)
            {
                foreach (var BIBLIOGRAFI_VAL in BIBLIOGRAFI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + LASTID + ",3,'" + BIBLIOGRAFI_VAL + "')");
                }
            }


            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(LOGCODE, objek, 1);

            //return Json(new { INPUT }, JsonRequestBehavior.AllowGet);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Update(TRX_PROPOSAL INPUT, int[] PROPOSAL_REV_MERIVISI_ID, string[] PROPOSAL_ADOPSI_NOMOR_JUDUL, int[] PROPOSAL_REF_SNI_ID, string[] PROPOSAL_REF_NON_SNI, string[] BIBLIOGRAFI, string[] PROPOSAL_LPK_ID, string[] PROPOSAL_RETEK_ID)
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var LOGCODE = MixHelper.GetLogCode();
            int LASTID = MixHelper.GetSequence("TRX_PROPOSAL");
            var DATENOW = MixHelper.ConvertDateNow();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == INPUT.PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_LPK_ID_CONVERT = "";
            var PROPOSAL_RETEK_ID_CONVERT = "";
            if (PROPOSAL_LPK_ID == null)
            {
                PROPOSAL_LPK_ID_CONVERT = null;
            }
            else
            {
                PROPOSAL_LPK_ID_CONVERT = string.Join(";", PROPOSAL_LPK_ID);
            }

            if (PROPOSAL_RETEK_ID == null)
            {
                PROPOSAL_RETEK_ID_CONVERT = null;
            }
            else
            {
                PROPOSAL_RETEK_ID_CONVERT = string.Join(";", PROPOSAL_RETEK_ID);
            }
            //var PROPOSAL_LPK_ID_CONVERT = string.Join(";", PROPOSAL_LPK_ID);
            //var PROPOSAL_RETEK_ID_CONVERT = string.Join(";", PROPOSAL_RETEK_ID);
            PROPOSAL_LPK_ID_CONVERT = ((PROPOSAL_LPK_ID_CONVERT == null) ? "" : PROPOSAL_LPK_ID_CONVERT);
            PROPOSAL_RETEK_ID_CONVERT = ((PROPOSAL_RETEK_ID_CONVERT == null) ? "" : PROPOSAL_RETEK_ID_CONVERT);
            var fupdate = "UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID = '" + INPUT.PROPOSAL_KOMTEK_ID + "'," +
                            "PROPOSAL_KONSEPTOR = '" + INPUT.PROPOSAL_KONSEPTOR + "'," +
                            "PROPOSAL_INSTITUSI = '" + INPUT.PROPOSAL_INSTITUSI + "'," +
                            "PROPOSAL_JUDUL_PNPS = '" + INPUT.PROPOSAL_JUDUL_PNPS + "'," +
                            "PROPOSAL_LPK_ID = '" + PROPOSAL_LPK_ID_CONVERT + "'," +
                            "PROPOSAL_RETEK_ID = '" + PROPOSAL_RETEK_ID_CONVERT + "'," +
                            "PROPOSAL_RUANG_LINGKUP = '" + INPUT.PROPOSAL_RUANG_LINGKUP + "'," +
                            "PROPOSAL_JENIS_PERUMUSAN = '" + INPUT.PROPOSAL_JENIS_PERUMUSAN + "'," +
                            "PROPOSAL_JALUR = '" + INPUT.PROPOSAL_JALUR + "'," +
                            "PROPOSAL_JENIS_ADOPSI = '" + INPUT.PROPOSAL_JENIS_ADOPSI + "'," +
                            "PROPOSAL_METODE_ADOPSI = '" + INPUT.PROPOSAL_METODE_ADOPSI + "'," +
                            "PROPOSAL_TERJEMAHAN_SNI_ID = '" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "'," +
                            "PROPOSAL_RALAT_SNI_ID = '" + INPUT.PROPOSAL_RALAT_SNI_ID + "'," +
                            "PROPOSAL_AMD_SNI_ID = '" + INPUT.PROPOSAL_AMD_SNI_ID + "'," +
                            "PROPOSAL_IS_URGENT = '" + INPUT.PROPOSAL_IS_URGENT + "'," +
                            "PROPOSAL_PASAL = '" + INPUT.PROPOSAL_PASAL + "'," +
                            "PROPOSAL_IS_HAK_PATEN = '" + INPUT.PROPOSAL_IS_HAK_PATEN + "'," +
                            "PROPOSAL_IS_HAK_PATEN_DESC = '" + INPUT.PROPOSAL_IS_HAK_PATEN_DESC + "'," +
                            "PROPOSAL_INFORMASI = '" + INPUT.PROPOSAL_INFORMASI + "'," +
                            "PROPOSAL_TUJUAN = '" + INPUT.PROPOSAL_TUJUAN + "'," +
                            "PROPOSAL_PROGRAM_PEMERINTAH = '" + INPUT.PROPOSAL_PROGRAM_PEMERINTAH + "'," +
                            "PROPOSAL_PIHAK_BERKEPENTINGAN = '" + INPUT.PROPOSAL_PIHAK_BERKEPENTINGAN + "'," +
                            "PROPOSAL_MANFAAT_PENERAPAN = '" + INPUT.PROPOSAL_MANFAAT_PENERAPAN + "'," +
                            "PROPOSAL_IS_ORG_MENDUKUNG = '" + INPUT.PROPOSAL_IS_ORG_MENDUKUNG + "'," +
                            "PROPOSAL_IS_DUPLIKASI_DESC = '" + INPUT.PROPOSAL_IS_DUPLIKASI_DESC + "'," +
                            "PROPOSAL_UPDATE_BY = '" + USER_ID + "'," +
                            "PROPOSAL_UPDATE_DATE = " + DATENOW + "," +
                            "PROPOSAL_STATUS_PROSES = " + ((DataProposal.APPROVAL_TYPE == 0) ? 1 : DataProposal.PROPOSAL_STATUS_PROSES) + "," +
                            "PROPOSAL_LOG_CODE = '" + LOGCODE + "' WHERE PROPOSAL_ID = " + INPUT.PROPOSAL_ID;

            db.Database.ExecuteSqlCommand(fupdate);

            var tester = fupdate;
            if (PROPOSAL_REV_MERIVISI_ID != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REV WHERE PROPOSAL_REV_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var PROPOSAL_REV_MERIVISI_ID_VAL in PROPOSAL_REV_MERIVISI_ID)
                {
                    var PROPOSAL_REV_ID = MixHelper.GetSequence("TRX_PROPOSAL_REV");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REV (PROPOSAL_REV_ID,PROPOSAL_REV_PROPOSAL_ID,PROPOSAL_REV_MERIVISI_ID) VALUES (" + PROPOSAL_REV_ID + "," + INPUT.PROPOSAL_ID + "," + PROPOSAL_REV_MERIVISI_ID_VAL + ")");
                }
            }
            if (PROPOSAL_ADOPSI_NOMOR_JUDUL != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_ADOPSI WHERE PROPOSAL_ADOPSI_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL in PROPOSAL_ADOPSI_NOMOR_JUDUL)
                {
                    var PROPOSAL_ADOPSI_ID = MixHelper.GetSequence("TRX_PROPOSAL_ADOPSI");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_ADOPSI (PROPOSAL_ADOPSI_ID,PROPOSAL_ADOPSI_PROPOSAL_ID,PROPOSAL_ADOPSI_NOMOR_JUDUL) VALUES (" + PROPOSAL_ADOPSI_ID + "," + INPUT.PROPOSAL_ID + ",'" + PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL + "')");
                }
            }

            if (PROPOSAL_REF_SNI_ID != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 1 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var SNI_ID in PROPOSAL_REF_SNI_ID)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",1," + SNI_ID + ")");
                }
            }
            if (PROPOSAL_REF_NON_SNI != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 2 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var DATA_NON_SNI_VAL in PROPOSAL_REF_NON_SNI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    var CEK_PROPOSAL_REF_NON_SNI = db.Database.SqlQuery<MASTER_ACUAN_NON_SNI>("SELECT * FROM MASTER_ACUAN_NON_SNI WHERE ACUAN_NON_SNI_STATUS = 1 AND LOWER(ACUAN_NON_SNI_JUDUL) = '" + DATA_NON_SNI_VAL.ToLower() + "'").SingleOrDefault();
                    if (CEK_PROPOSAL_REF_NON_SNI != null)
                    {

                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",2,'" + CEK_PROPOSAL_REF_NON_SNI.ACUAN_NON_SNI_ID + "','" + DATA_NON_SNI_VAL + "')");
                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",2,'" + DATA_NON_SNI_VAL + "')");
                    }
                }
            }
            if (BIBLIOGRAFI != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 3 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var BIBLIOGRAFI_VAL in BIBLIOGRAFI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    //db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + BIBLIOGRAFI_VAL + "')");
                    var CEK_BIBLIOGRAFI = db.Database.SqlQuery<MASTER_BIBLIOGRAFI>("SELECT * FROM MASTER_BIBLIOGRAFI WHERE BIBLIOGRAFI_STATUS = 1 AND LOWER(BIBLIOGRAFI_JUDUL) = '" + BIBLIOGRAFI_VAL.ToLower() + "'").SingleOrDefault();
                    if (CEK_BIBLIOGRAFI != null)
                    {
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + CEK_BIBLIOGRAFI.BIBLIOGRAFI_ID + "','" + BIBLIOGRAFI_VAL + "')");
                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + BIBLIOGRAFI_VAL + "')");
                    }
                }
            }

            
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_CODE;
            var PROPOSAL_ID = INPUT.PROPOSAL_ID;
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
            if (INPUT.PROPOSAL_IS_ORG_MENDUKUNG == 1)
            {
                HttpPostedFileBase file = Request.Files["PROPOSAL_DUKUNGAN_FILE_PATH"];
                if (file.ContentLength > 0)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 29");
                    int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                    Stream stremdokumen = file.InputStream;
                    byte[] appData = new byte[file.ContentLength + 1];
                    stremdokumen.Read(appData, 0, file.ContentLength);
                    string Extension = Path.GetExtension(file.FileName);
                    if (Extension.ToLower() == ".pdf")
                    {
                        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                        string filePathpdf = path + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                        string filePathxml = path + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                        pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                        pdf.Save(@"" + filePathxml);
                        var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                        var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                        var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                    "'10', " +
                                    "'29', " +
                                    "'" + PROPOSAL_ID + "', " +
                                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Bukti Dukungan Usulan" + "', " +
                                    "'Bukti Dukungan Usulan dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                    "'" + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                    "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                    "'0', " +
                                    "'" + USER_ID + "', " +
                                    DATENOW + "," +
                                    "'1', " +
                                    "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                        String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                        MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                    }
                }
            }
            HttpPostedFileBase file2 = Request.Files["PROPOSAL_LAMPIRAN_FILE_PATH"];
            if (file2.ContentLength > 0)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 30");
                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream stremdokumen = file2.InputStream;
                byte[] appData = new byte[file2.ContentLength + 1];
                stremdokumen.Read(appData, 0, file2.ContentLength);
                string Extension = Path.GetExtension(file2.FileName);
                if (Extension.ToLower() == ".pdf")
                {
                    Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                    string filePathpdf = path + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                    string filePathxml = path + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                    pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    pdf.Save(@"" + filePathxml);
                    var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                    var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                "'10', " +
                                "'30', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Pendukung Usulan" + "', " +
                                "'Lampiran Pendukung Usulan dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                }
            }
            HttpPostedFileBase file3 = Request.Files["PROPOSAL_SURAT_PENGAJUAN_PNPS"];
            if (file3.ContentLength > 0)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 32");
                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream stremdokumen = file3.InputStream;
                byte[] appData = new byte[file3.ContentLength + 1];
                stremdokumen.Read(appData, 0, file3.ContentLength);
                string Extension = Path.GetExtension(file3.FileName);
                if (Extension.ToLower() == ".pdf")
                {
                    Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                    string filePathpdf = path + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                    string filePathxml = path + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                    pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    pdf.Save(@"" + filePathxml);
                    var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                    var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                "'10', " +
                                "'32', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Surat Pengajuan PNPS" + "', " +
                                "'Lampiran Surat Pengajuan PNPS dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                }
            }

            HttpPostedFileBase file4 = Request.Files["PROPOSAL_OUTLINE_RSNI"];
            if (file4.ContentLength > 0)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 36");
                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Stream stremdokumen = file4.InputStream;
                byte[] appData = new byte[file4.ContentLength + 1];
                stremdokumen.Read(appData, 0, file4.ContentLength);
                string Extension = Path.GetExtension(file4.FileName);
                if (Extension.ToLower() == ".pdf")
                {
                    Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                    string filePathpdf = path + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                    string filePathxml = path + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                    pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    pdf.Save(@"" + filePathxml);
                    var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                    var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                "'10', " +
                                "'36', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Outline RSNI" + "', " +
                                "'Lampiran Outline RSNI dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                }
            }

            if(DataProposal.APPROVAL_TYPE == 0){
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND APPROVAL_STATUS_PROPOSAL = 0 AND APPROVAL_TYPE = 0");
            }
            
            //return Json(new { tester,INPUT, PROPOSAL_REV_MERIVISI_ID, PROPOSAL_ADOPSI_NOMOR_JUDUL, PROPOSAL_REF_SNI_ID, PROPOSAL_REF_NON_SNI, BIBLIOGRAFI }, JsonRequestBehavior.AllowGet);
            String objek = fupdate.Replace("'", "-");
            MixHelper.InsertLog(LOGCODE, objek, 2);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult UpdateXXXX(TRX_PROPOSAL INPUT, int[] PROPOSAL_REF_SNI_ID, int[] PROPOSAL_REF_NON_SNI, int[] BIBLIOGRAFI, string nomorstandaradopsi = "", string tahunstandaradopsi = "", string judulstandaradopsi = "", string nomorstandarmodifikasi = "", string judulstandarmodifikasi = "", string tahunstandarmodifikasi = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var LOGCODE = INPUT.PROPOSAL_LOG_CODE;
            int LASTID = MixHelper.GetSequence("TRX_PROPOSAL");
            var DATENOW = MixHelper.ConvertDateNow();
            //var GETDATASNI = db.Database.SqlQuery<VIEW_SNI>("SELECT * FROM VIEW_SNI WHERE SNI_ID = " + INPUT.PROPOSAL_REVISI_SNI_ID).SingleOrDefault();
            var PROPOSAL_NO_SNI_PROPOSAL = "";
            var PROPOSAL_JUDUL_SNI_PROPOSAL = "";
            //if (GETDATASNI != null)
            //{
            //    PROPOSAL_NO_SNI_PROPOSAL = GETDATASNI.SNI_NOMOR;
            //    PROPOSAL_JUDUL_SNI_PROPOSAL = GETDATASNI.SNI_JUDUL;
            //}
            //else
            //{
            //    if (nomorstandaradopsi != "")
            //    {
            //        PROPOSAL_NO_SNI_PROPOSAL = nomorstandaradopsi + ":" + INPUT.PROPOSAL_YEAR;
            //        PROPOSAL_JUDUL_SNI_PROPOSAL = judulstandaradopsi;
            //    }
            //    else
            //    {
            //        PROPOSAL_NO_SNI_PROPOSAL = nomorstandarmodifikasi + ":" + INPUT.PROPOSAL_YEAR;
            //        PROPOSAL_JUDUL_SNI_PROPOSAL = judulstandarmodifikasi;
            //    }

            //}
            var fupdate = "UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID = '" + INPUT.PROPOSAL_KOMTEK_ID + "'," +
                            "PROPOSAL_KONSEPTOR = '" + INPUT.PROPOSAL_KONSEPTOR + "'," +
                            "PROPOSAL_INSTITUSI = '" + INPUT.PROPOSAL_INSTITUSI + "'," +
                            "PROPOSAL_NO_SNI_PROPOSAL = '" + PROPOSAL_NO_SNI_PROPOSAL + "'," +
                            "PROPOSAL_JUDUL_SNI_PROPOSAL = '" + PROPOSAL_JUDUL_SNI_PROPOSAL + "'," +
                            "PROPOSAL_JUDUL_PNPS = '" + INPUT.PROPOSAL_JUDUL_PNPS + "'," +
                            "PROPOSAL_RUANG_LINGKUP = '" + INPUT.PROPOSAL_RUANG_LINGKUP + "'," +
                            "PROPOSAL_JENIS_PERUMUSAN = '" + INPUT.PROPOSAL_JALUR + "'," +
                            "PROPOSAL_METODE_ADOPSI = '" + INPUT.PROPOSAL_METODE_ADOPSI + "'," +
                //"PROPOSAL_NO_STD_ADOPSI_IDENTIK = '" + INPUT.PROPOSAL_NO_STD_ADOPSI_IDENTIK + "'," +
                            "PROPOSAL_TERJEMAHAN_SNI_ID = '" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "'," +
                //"PROPOSAL_IS_ADOPSI_MOD = '" + INPUT.PROPOSAL_IS_ADOPSI_MOD + "'," +
                //"PROPOSAL_NO_STANDAR_MODIFIKASI = '" + INPUT.PROPOSAL_NO_STANDAR_MODIFIKASI + "'," +
                            "PROPOSAL_IS_URGENT = '" + INPUT.PROPOSAL_IS_URGENT + "'," +
                //"PROPOSAL_REVISI_SNI_ID = '" + INPUT.PROPOSAL_REVISI_SNI_ID + "'," +
                //"PROPOSAL_REVISI_PASAL = '" + INPUT.PROPOSAL_REVISI_PASAL + "'," +
                            "PROPOSAL_IS_HAK_PATEN_DESC = '" + INPUT.PROPOSAL_IS_HAK_PATEN_DESC + "'," +
                            "PROPOSAL_INFORMASI = '" + INPUT.PROPOSAL_INFORMASI + "'," +
                            "PROPOSAL_TUJUAN = '" + INPUT.PROPOSAL_TUJUAN + "'," +
                            "PROPOSAL_PROGRAM_PEMERINTAH = '" + INPUT.PROPOSAL_PROGRAM_PEMERINTAH + "'," +
                            "PROPOSAL_PIHAK_BERKEPENTINGAN = '" + INPUT.PROPOSAL_PIHAK_BERKEPENTINGAN + "'," +
                            "PROPOSAL_LAMPIRAN_FILE_PATH = NULL," +
                            "PROPOSAL_UPDATE_BY = '" + USER_ID + "'," +
                            "PROPOSAL_UPDATE_DATE = " + DATENOW + "," +
                            "PROPOSAL_LOG_CODE = '" + LOGCODE + "' WHERE PROPOSAL_ID = " + INPUT.PROPOSAL_ID;

            db.Database.ExecuteSqlCommand(fupdate);


            if (PROPOSAL_REF_SNI_ID != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 1 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var SNI_ID in PROPOSAL_REF_SNI_ID)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",1," + SNI_ID + ")");
                }
            }
            if (PROPOSAL_REF_NON_SNI != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 2 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var DATA_NON_SNI_VAL in PROPOSAL_REF_NON_SNI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",2,'" + DATA_NON_SNI_VAL + "')");
                }
            }
            if (BIBLIOGRAFI != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 3 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);

                foreach (var BIBLIOGRAFI_VAL in BIBLIOGRAFI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + BIBLIOGRAFI_VAL + "')");
                }
            }

            String objek = fupdate.Replace("'", "-");
            MixHelper.InsertLog(LOGCODE, objek, 2);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";

            return Json(new { res = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Hapus(int id = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var USER_KOMTEK_CODE = Session["KOMTEK_CODE"];
            var UserId = Session["USER_ID"];
            var logcode = db.Database.SqlQuery<String>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + id).SingleOrDefault();

            var datenow = MixHelper.ConvertDateNow();

            var year_now = DateTime.Now.Year;

            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = -1,PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId + " WHERE PROPOSAL_ID = " + id);

            String objek = "PROPOSAL_STATUS = -1, PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId;
            MixHelper.InsertLog(logcode, objek.Replace("'", "-"), 2);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            //return Json(new { query = "INSERT INTO TRANSACTION_PROPOSAL (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            return RedirectToAction("Index");
        }
        public ActionResult ApprovalUsulan(int id = 0)
        {
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
            return View();
        }
        [HttpPost]
        public ActionResult ApprovalUsulanSave(int PROPOSAL_ID = 0)
        {
            var UserId = Session["USER_ID"];
            var logcode = db.Database.SqlQuery<String>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            var datenow = MixHelper.ConvertDateNow();
            var year_now = DateTime.Now.Year;
            var TANGGAL_SKR = MixHelper.ConvertDateNow();
            var LOGCODE_POLLING = MixHelper.GetLogCode();
            int LASTID_POLLING = MixHelper.GetSequence("TRX_POLLING");
            var LAST_POLLING_VERSION = db.Database.SqlQuery<int>("SELECT NVL(CAST(MAX(POLLING_VERSION) AS INT),1) FROM TRX_POLLING WHERE POLLING_TYPE = 2 AND POLLING_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();

            var POLLING_IS_EXIST = db.Database.SqlQuery<TRX_POLLING>("SELECT * FROM TRX_POLLING WHERE POLLING_VERSION = " + LAST_POLLING_VERSION + " AND POLLING_TYPE = 2 AND POLLING_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            var JML_HARI = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 8").SingleOrDefault();
            //return RedirectToAction("Index");
            if (POLLING_IS_EXIST == null)
            {
                var fname = "POLLING_ID,POLLING_PROPOSAL_ID,POLLING_TYPE,POLLING_START_DATE,POLLING_END_DATE,POLLING_VERSION,POLLING_CREATE_BY,POLLING_CREATE_DATE,POLLING_STATUS,POLLING_LOGCODE";
                var fvalue = "'" + LASTID_POLLING + "', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + 2 + "', " +
                                "TO_DATE(SYSDATE), " +
                                "TO_DATE(SYSDATE)+" + ((JML_HARI != null) ? JML_HARI.CONFIG_VALUE : "14") + ", " +
                                "'" + LAST_POLLING_VERSION + "', " +
                                "'" + UserId + "'," +
                                TANGGAL_SKR + "," +
                                "'1', " +
                                "'" + LOGCODE_POLLING + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_POLLING (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

                String objek2 = fvalue.Replace("'", "-");
                MixHelper.InsertLog(logcode, objek2, 1);
            }
            var NEW_LASTID_POLLING = db.Database.SqlQuery<int>("SELECT POLLING_ID FROM TRX_POLLING WHERE POLLING_PROPOSAL_ID =" + PROPOSAL_ID + " AND POLLING_VERSION = " + LAST_POLLING_VERSION).SingleOrDefault();
            
            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 2, PROPOSAL_IS_POLLING = 1, PROPOSAL_POLLING_ID = " + NEW_LASTID_POLLING + ", PROPOSAL_UPDATE_DATE = " + TANGGAL_SKR + ", PROPOSAL_UPDATE_BY = " + UserId + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
            String objek = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 2, PROPOSAL_IS_POLLING = 1, PROPOSAL_POLLING_ID = " + NEW_LASTID_POLLING + ", PROPOSAL_UPDATE_DATE = " + TANGGAL_SKR + ", PROPOSAL_UPDATE_BY = " + UserId + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
            MixHelper.InsertLog(logcode, objek.Replace("'", "-"), 2);

            int lastid = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + lastid + "," + PROPOSAL_ID + ",1," + TANGGAL_SKR + "," + UserId + ",1,1,1)");
            

            var Data = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();


            if (Data != null)
            {
                string dataDir = Server.MapPath("~/Format/Laporan/");
                Stream stream = System.IO.File.OpenRead(dataDir + "FORMULIR_PENGAJUAN_USULAN_PERUMUSAN_SNI.docx");

                Aspose.Words.Document doc = new Aspose.Words.Document(stream);
                stream.Close();
                ReplaceHelper helper = new ReplaceHelper(doc);
                var dt = Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE);
                string time = dt.ToString("hh:mm tt");
                var PROPOSAL_PNPS_CODE_FIXER = Data.PROPOSAL_CODE;
                var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
                string PROPOSAL_CREATE_DATE = Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE).ToString("dd-MM-yyyy");
                helper.Replace("data_tgl", PROPOSAL_CREATE_DATE);
                helper.Replace("data_01", Data.KOMTEK_CODE + " " + Data.KOMTEK_NAME);
                helper.Replace("data_02", ((Data.PROPOSAL_KONSEPTOR == null) ? "" : Data.PROPOSAL_KONSEPTOR));
                helper.Replace("data_03", ((Data.PROPOSAL_INSTITUSI == null) ? "" : Data.PROPOSAL_INSTITUSI));
                helper.Replace("data_04", ((Data.PROPOSAL_TIM_NAMA == null) ? "" : Data.PROPOSAL_TIM_NAMA));
                helper.Replace("data_05", ((Data.PROPOSAL_INSTITUSI == null) ? "" : Data.PROPOSAL_INSTITUSI));
                helper.Replace("data_06", ((Data.PROPOSAL_TIM_ALAMAT == null) ? "" : Data.PROPOSAL_TIM_ALAMAT));
                helper.Replace("data_07", ((Data.PROPOSAL_TIM_PHONE == null) ? "" : Data.PROPOSAL_TIM_PHONE));
                helper.Replace("data_08", ((Data.PROPOSAL_TIM_EMAIL == null) ? "" : Data.PROPOSAL_TIM_EMAIL));
                helper.Replace("data_09", ((Data.PROPOSAL_TIM_FAX == null) ? "" : Data.PROPOSAL_TIM_FAX));
                helper.Replace("data_10", ((Data.PROPOSAL_JUDUL_PNPS == null) ? "" : Data.PROPOSAL_JUDUL_PNPS));
                helper.Replace("data_11", ((Data.PROPOSAL_RUANG_LINGKUP == null) ? "" : Data.PROPOSAL_RUANG_LINGKUP));
                //helper.Replace("data_12", ((Data.PROPOSAL_JALUR == 1) ? "✔  Ya" : "✘  Tidak"));
                var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == PROPOSAL_ID orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
                var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + PROPOSAL_ID + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
                string PROPOSAL_ADOPSI_NOMOR_JUDUL = "";
                if (AdopsiList.Count > 0)
                {
                    foreach (var ad in AdopsiList)
                    {
                        PROPOSAL_ADOPSI_NOMOR_JUDUL += ad.PROPOSAL_ADOPSI_NOMOR_JUDUL + ", ";
                    }
                }
                string PROPOSAL_REVISI_NOMOR_JUDUL = "";
                if (RevisiList.Count > 0)
                {
                    foreach (var ad in RevisiList)
                    {
                        PROPOSAL_REVISI_NOMOR_JUDUL += ad.TEXT + ", ";
                    }
                }

                if (Data.PROPOSAL_JENIS_PERUMUSAN == 1)
                {
                    helper.Replace("data_12", "✔ Ya");
                    helper.Replace("data_13", "");
                    helper.Replace("data_14", "");
                    helper.Replace("data_15", "");
                }
                else if (Data.PROPOSAL_JENIS_PERUMUSAN == 2)
                {
                    helper.Replace("data_12", "");
                    helper.Replace("data_13", "✔ Ya");
                    helper.Replace("data_14", "");
                    helper.Replace("data_15", "");
                }
                else if (Data.PROPOSAL_JENIS_PERUMUSAN == 3)
                {
                    helper.Replace("data_12", "");
                    helper.Replace("data_13", "");
                    helper.Replace("data_14", "✔ Ya");
                    helper.Replace("data_15", "");
                }
                else if (Data.PROPOSAL_JENIS_PERUMUSAN == 4)
                {
                    helper.Replace("data_12", "");
                    helper.Replace("data_13", "");
                    helper.Replace("data_14", "");
                    helper.Replace("data_15", "✔ Ya");
                }
                else
                {
                    helper.Replace("data_12", "");
                    helper.Replace("data_13", "");
                    helper.Replace("data_14", "");
                    helper.Replace("data_15", "");
                }
                if (Data.PROPOSAL_JALUR == 1)
                {
                    helper.Replace("data_16", "✔ Ya");
                    helper.Replace("data_17", "");
                    helper.Replace("data_23", "");
                    helper.Replace("data_21", "");
                    helper.Replace("data_24", "");
                }
                else if (Data.PROPOSAL_JALUR == 2)
                {
                    if (Data.PROPOSAL_JENIS_ADOPSI == 1)
                    {
                        helper.Replace("data_16", "");
                        helper.Replace("data_17", "✔ Ya");
                        helper.Replace("data_23", "");
                        helper.Replace("data_21", PROPOSAL_ADOPSI_NOMOR_JUDUL);
                        helper.Replace("data_24", "");
                    }
                    else if (Data.PROPOSAL_JENIS_ADOPSI == 2)
                    {
                        helper.Replace("data_16", "");
                        helper.Replace("data_17", "");
                        helper.Replace("data_23", "✔ Ya");
                        helper.Replace("data_21", "");
                        helper.Replace("data_24", PROPOSAL_ADOPSI_NOMOR_JUDUL);

                    }
                }
                else
                {
                    helper.Replace("data_16", "");
                    helper.Replace("data_17", "");
                    helper.Replace("data_23", "");
                }
                if (Data.PROPOSAL_JENIS_PERUMUSAN == 5)
                {
                    helper.Replace("data_22", Data.PROPOSAL_TERJEMAHAN_NOMOR + ", " + Data.PROPOSAL_TERJEMAHAN_JUDUL);
                }
                else
                {
                    helper.Replace("data_22", "");
                }

                if (Data.PROPOSAL_JALUR == 2)
                {
                    if (Data.PROPOSAL_METODE_ADOPSI == 1)
                    {
                        helper.Replace("data_18", "✔ Ya");
                        helper.Replace("data_19", "");
                        helper.Replace("data_20", "");
                    }
                    else if (Data.PROPOSAL_METODE_ADOPSI == 2)
                    {
                        helper.Replace("data_18", "");
                        helper.Replace("data_19", "✔ Ya");
                        helper.Replace("data_20", "");
                    }
                    else if (Data.PROPOSAL_METODE_ADOPSI == 3)
                    {
                        helper.Replace("data_18", "");
                        helper.Replace("data_19", "");
                        helper.Replace("data_20", "✔ Ya");
                    }
                }
                else
                {
                    helper.Replace("data_18", "");
                    helper.Replace("data_19", "");
                    helper.Replace("data_20", "");
                }


                helper.Replace("data_25", ((Data.PROPOSAL_IS_URGENT == 1) ? "✔  Ya" : "✘  Tidak"));
                if (PROPOSAL_REVISI_NOMOR_JUDUL != "")
                {
                    helper.Replace("data_26", PROPOSAL_REVISI_NOMOR_JUDUL);
                }
                else
                {
                    helper.Replace("data_26", "");
                }
                if (Data.PROPOSAL_AMD_SNI_ID != null)
                {
                    helper.Replace("data_26", Data.PROPOSAL_AMD_NOMOR + " " + Data.PROPOSAL_AMD_JUDUL);
                }
                else
                {
                    helper.Replace("data_26", "");
                }
                if (Data.PROPOSAL_RALAT_SNI_ID != null)
                {
                    if (Data.PROPOSAL_JENIS_PERUMUSAN != 5)
                    {
                        helper.Replace("data_26", Data.PROPOSAL_RALAT_NOMOR + " " + Data.PROPOSAL_RALAT_JUDUL);
                    }
                }
                else
                {
                    helper.Replace("data_26", "");
                }

                helper.Replace("data_27", ((Data.PROPOSAL_PASAL == null) ? "" : Data.PROPOSAL_PASAL));

                if (Data.PROPOSAL_IS_HAK_PATEN == 1)
                {
                    helper.Replace("data_28", "✔");
                    helper.Replace("data_29", "");
                }
                else if (Data.PROPOSAL_IS_HAK_PATEN == 0)
                {
                    helper.Replace("data_28", "");
                    helper.Replace("data_29", "✔");
                }
                else
                {
                    helper.Replace("data_28", "");
                    helper.Replace("data_29", "");
                }
                helper.Replace("data_30", ((Data.PROPOSAL_IS_HAK_PATEN_DESC == null) ? "" : Data.PROPOSAL_IS_HAK_PATEN_DESC));
                helper.Replace("data_31", ((Data.PROPOSAL_TUJUAN == null) ? "" : Data.PROPOSAL_TUJUAN));
                helper.Replace("data_32", ((Data.PROPOSAL_PROGRAM_PEMERINTAH == null) ? "" : Data.PROPOSAL_PROGRAM_PEMERINTAH));
                helper.Replace("data_33", ((Data.PROPOSAL_PIHAK_BERKEPENTINGAN == null) ? "" : Data.PROPOSAL_PIHAK_BERKEPENTINGAN.Replace("|", ", ")));
                helper.Replace("data_34", ((Data.PROPOSAL_INFORMASI == null) ? "" : Data.PROPOSAL_INFORMASI));
                helper.Replace("data_35", ((Data.PROPOSAL_IS_ORG_MENDUKUNG == 1) ? "Ya" : "Tidak"));
                helper.Replace("data_36", ((Data.PROPOSAL_RETEK_ID == null) ? "-" : Data.PROPOSAL_RETEK_ID));
                helper.Replace("data_37", ((Data.PROPOSAL_LPK_ID == null) ? "-" : Data.PROPOSAL_LPK_ID));
                helper.Replace("data_38", ((Data.PROPOSAL_IS_DUPLIKASI_DESC == null) ? "-" : Data.PROPOSAL_IS_DUPLIKASI_DESC));
                var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();

                helper.Replace("data_39", ((Outline == null) ? "" : "Terlampir"));
                helper.Replace("data_40", ((Outline == null) ? "" : PROPOSAL_CREATE_DATE));
                
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/PNPS/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/PNPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                string filePathdoc = path + "FORMULIR_PENGAJUAN_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".docx";
                string filePathpdf = path + "FORMULIR_PENGAJUAN_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                string filePathxml = path + "FORMULIR_PENGAJUAN_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                int LASTIDDOKUMEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_PNPS = MixHelper.GetLogCode();
                var FNAME_PNPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_PNPS = "'" + LASTIDDOKUMEN + "', " +
                            "'10', " +
                            "'31', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Formulir Pengajuan Usulan PNPS" + "', " +
                            "'Formulir Pengajuan Usulan PNPS " + Data.PROPOSAL_JUDUL_PNPS + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/PNPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "FORMULIR_PENGAJUAN_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                            "'DOCX', " +
                            "'0', " +
                            "'" + UserId + "', " +
                            datenow + "," +
                            "'1', " +
                            "'" + LOGCODE_PNPS + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_PNPS + ") VALUES (" + FVALUE_PNPS.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_PNPS.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_PNPS, objekTanggapan, 1);
                int LASTID_MONITORING = MixHelper.GetSequence("TRX_MONITORING");

                db.Database.ExecuteSqlCommand("INSERT INTO TRX_MONITORING (MONITORING_ID,MONITORING_PROPOSAL_ID,MONITORING_TGL_APP_PNPS,MONITORING_HASIL_APP_PNPS) VALUES (" + LASTID_MONITORING + "," + PROPOSAL_ID + "," + datenow + ",1)");
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_PNPS = (SELECT PROPOSAL_CREATE_DATE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID + ") WHERE MONITORING_ID = " + LASTID_MONITORING);
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_MTPS = " + TANGGAL_SKR + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);


                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data Berhasil Disimpan";
                //return Json(new { result = true, data = "INSERT INTO TRX_MONITORING (MONITORING_ID,MONITORING_PROPOSAL_ID,MONITORING_TGL_PNPS,MONITORING_TGL_APP_PNPS,MONITORING_HASIL_APP_PNPS) VALUES (" + LASTID_MONITORING + "," + PROPOSAL_ID + "," + TGL_PNPS + "," + datenow + ",1)" }, JsonRequestBehavior.AllowGet);

                //return RedirectToAction("Index");
            }
            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ApprovalUsulanReject(int PROPOSAL_ID = 0, string PROPOSAL_REASON = "")
        {
            var UserId = Session["USER_ID"];
            var logcode = db.Database.SqlQuery<String>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            var datenow = MixHelper.ConvertDateNow();
            var year_now = DateTime.Now.Year;
            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 0, PROPOSAL_STATUS_PROSES = 0 WHERE PROPOSAL_ID = " + PROPOSAL_ID);

            String objek = "PROPOSAL_STATUS = 0 ";
            MixHelper.InsertLog(logcode, objek.Replace("'", "-"), 2);

            int lastid = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION,APPROVAL_REASON) VALUES (" + lastid + "," + PROPOSAL_ID + ",0," + datenow + "," + UserId + ",1,0,1,'" + PROPOSAL_REASON + "')");

            int LASTID_MONITORING = MixHelper.GetSequence("TRX_MONITORING");

            db.Database.ExecuteSqlCommand("INSERT INTO TRX_MONITORING (MONITORING_ID,MONITORING_PROPOSAL_ID,MONITORING_TGL_APP_PNPS,MONITORING_HASIL_APP_PNPS) VALUES (" + LASTID_MONITORING + "," + PROPOSAL_ID + "," + datenow + ",0)");
            db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_PNPS = (SELECT PROPOSAL_CREATE_DATE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID + ") WHERE MONITORING_ID = " + LASTID_MONITORING);


            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
            //return RedirectToAction("Index");
        }
        public ActionResult Baru()
        {
            var IS_KOMTEK = Convert.ToInt32(Session["IS_KOMTEK"]);
            string ViewName = ((IS_KOMTEK == 1) ? "UsulanBaruKomtek" : "UsulanBaruPPS");
            return View(ViewName);
        }

        public ActionResult Approval(int id = 0)
        {
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            ViewData["Komtek"] = DataKomtek;
            ViewData["DataProposal"] = DataProposal;
            return View();
        }

        public ActionResult ValidasiUsulan(int id = 0)
        {
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            ViewData["Komtek"] = DataKomtek;
            ViewData["DataProposal"] = DataProposal;
            return View();
        }
        public ActionResult ValidasiKomtek(int id = 0)
        {
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            ViewData["Komtek"] = DataKomtek;
            ViewData["DataProposal"] = DataProposal;
            return View();
        }
        public ActionResult AssignKomtek(int id = 0)
        {
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            ViewData["Komtek"] = DataKomtek;
            ViewData["DataProposal"] = DataProposal;
            return View();
        }
        public ActionResult PengesahanUsulan(int id = 0)
        {
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            ViewData["Komtek"] = DataKomtek;
            ViewData["DataProposal"] = DataProposal;
            return View();
        }
        [HttpPost]
        public ActionResult Approval(TRX_PROPOSAL input)
        {
            return Json(new { query = "" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DataUsulanKomtek(DataTables param, int id = 0, int id2 = 0)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;
            var STATUS_TAHAPAN = id;
            var APPROVAL_TYPE = id2;
            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("PROPOSAL_TAHAPAN");
            order_field.Add("PROPOSAL_STATUS_NAME");
            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;
            string where_clause = "";

            if (id == 0)
            {
                where_clause += "PROPOSAL_STATUS = 0 AND PROPOSAL_STATUS_PROSES = 1 AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID;
            }
            else if (id == 1)
            {
                where_clause += "PROPOSAL_STATUS > 0 AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID;
            }
            else if (id == 2)
            {
                where_clause += "PROPOSAL_STATUS = 0 AND PROPOSAL_STATUS_PROSES = 2 AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID;
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
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString(list.KOMTEK_CODE+" "+list.KOMTEK_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                Convert.ToString("<center><a href='/Pengajuan/Usulan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS==0 && list.APPROVAL_TYPE == -1)?"<a href='javascript:void(0)' onclick='hapus_usulan("+list.PROPOSAL_ID+")' class='btn red btn-sm action tooltips btn_remove' data-container='body' data-placement='top' data-original-title='Hapus'><i class='action glyphicon glyphicon-remove'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")'  class='btn green btn-sm action tooltips btn_print' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DataUsulanPPSBaru(DataTables param, int id = 0, int id2 = 0)
        {
            var default_order = "PROPOSAL_CREATE_DATE";
            var BIDANG_ID = Convert.ToInt32(Session["BIDANG_ID"]);
            var limit = 10;
            var STATUS = id;
            var APPROVAL_TYPE = id2;
            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("PROPOSAL_TYPE_NAME");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
            order_field.Add("KOMTEK_CODE");
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
            string where_clause = "";
            
            if (STATUS == 0)
            {
                where_clause += "PROPOSAL_STATUS = 0 AND APPROVAL_TYPE = -1 "+((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");
            }
            else if (STATUS == 1)
            {
                where_clause += "PROPOSAL_STATUS > 0 " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");
            }
            else if (STATUS == 2)
            {
                where_clause += "PROPOSAL_STATUS = 0 AND APPROVAL_TYPE = 0 " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");
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
                search_clause += " OR PROPOSAL_CREATE_DATE_NAME LIKE '%" + search + "%' OR KOMTEK_NAME LIKE '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
                //inject_clause_select = "SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + " OFFSET " + Convert.ToString(start) + " ROWS FETCH NEXT " + Convert.ToString(limit) + " ROWS ONLY";
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString("<center>"+list.PROPOSAL_CREATE_DATE_NAME+"</center>"),
                Convert.ToString(list.PROPOSAL_TYPE_NAME),
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString((list.KOMTEK_CODE==null)?"-":list.KOMTEK_CODE+"."+list.KOMTEK_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString("<center>"+list.PROPOSAL_IS_URGENT_NAME+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                Convert.ToString("<center><a href='/Pengajuan/Usulan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 0 && list.APPROVAL_TYPE == -1)?"<a href='/Pengajuan/Usulan/Update/"+list.PROPOSAL_ID+"' class='btn btn-warning btn-sm action tooltips btn_update' data-container='body' data-placement='top' data-original-title='Ubah'><i class='action fa fa-edit'></i></a><a href='/Pengajuan/Usulan/ApprovalUsulan/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips btn_approve' data-container='body' data-placement='top' data-original-title='Persetujuan'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")'  class='btn green btn-sm action tooltips btn_print' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
            };

            return Json(new
            {
                //data = inject_clause_select,
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray(),
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DataUsulanPPSDitolak(DataTables param)
        {
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;
            var BIDANG_ID = Convert.ToInt32(Session["BIDANG_ID"]);

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PROPOSAL_TYPE_NAME");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("APPROVAL_REASON");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;
            string where_clause = "";


            where_clause += "PROPOSAL_STATUS = 0 AND APPROVAL_TYPE = 0 " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");

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
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString((list.KOMTEK_CODE==null)?"-":list.KOMTEK_CODE+"."+list.KOMTEK_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString(list.APPROVAL_REASON), 
                Convert.ToString("<center><a href='/Pengajuan/Usulan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a><a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")'  class='btn green btn-sm action tooltips btn_print' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
            };
            return Json(new
            {
                //data = inject_clause_select,
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DataUsulanKomtekDitolak(DataTables param)
        {
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_NO_SNI_PROPOSAL");
            order_field.Add("APPROVAL_REASON");


            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;
            string where_clause = "";


            where_clause += "PROPOSAL_STATUS = 0 AND APPROVAL_TYPE = 0 AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID;

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
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString((list.KOMTEK_CODE==null)?"-":list.KOMTEK_CODE+"."+list.KOMTEK_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString(list.APPROVAL_REASON), 
                //Convert.ToString("<center><a href='/Pengajuan/Usulan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a><a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")'  class='btn green btn-sm action tooltips btn_print' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                Convert.ToString("<center><a href='/Pengajuan/Usulan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a><a href='/Pengajuan/Usulan/Update/"+list.PROPOSAL_ID+"' class='btn btn-warning btn-sm action tooltips btn_update' data-container='body' data-placement='top' data-original-title='Ubah'><i class='action fa fa-edit'></i></a><a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")'  class='btn green btn-sm action tooltips btn_print' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
            };
            return Json(new
            {
                //data = inject_clause_select,
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult FindSNI(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI_SELECT WHERE LOWER(VIEW_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_SNI_SELECT.TEXT ASC").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI_SELECT WHERE LOWER(VIEW_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_SNI_SELECT.TEXT ASC) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_SNI_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.ID, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult FindNonSNI(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_ACUAN_NON_SNI_SELECT WHERE LOWER(VIEW_ACUAN_NON_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_ACUAN_NON_SNI_SELECT.ID ASC").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_ACUAN_NON_SNI_SELECT WHERE LOWER(VIEW_ACUAN_NON_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_ACUAN_NON_SNI_SELECT.ID ASC) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_SNI_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.TEXT, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult FindRegulator(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REGULASI_TEKNIS_SELECT WHERE LOWER(VIEW_REGULASI_TEKNIS_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_REGULASI_TEKNIS_SELECT.ID ASC").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REGULASI_TEKNIS_SELECT WHERE LOWER(VIEW_REGULASI_TEKNIS_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_REGULASI_TEKNIS_SELECT.ID ASC) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_REGULASI_TEKNIS_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.TEXT, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult FindLPK(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_LPK_SELECT WHERE LOWER(VIEW_LPK_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_LPK_SELECT.ID ASC").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_LPK_SELECT WHERE LOWER(VIEW_LPK_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_LPK_SELECT.ID ASC) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_LPK_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.TEXT, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult FindNonSNIAdopsi(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_ACUAN_NON_SNI_SELECT WHERE LOWER(VIEW_ACUAN_NON_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_ACUAN_NON_SNI_SELECT.ID ASC").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_ACUAN_NON_SNI_SELECT WHERE LOWER(VIEW_ACUAN_NON_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_ACUAN_NON_SNI_SELECT.ID ASC) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_SNI_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.TEXT, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }
        [HttpGet]
        public ActionResult FindBIBLIOGRAFI(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_BIBLIOGRAFI_SELECT WHERE LOWER(VIEW_BIBLIOGRAFI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_BIBLIOGRAFI_SELECT.TEXT ASC").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_BIBLIOGRAFI_SELECT WHERE LOWER(VIEW_BIBLIOGRAFI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%' ORDER BY VIEW_BIBLIOGRAFI_SELECT.TEXT ASC) T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_SNI_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.TEXT, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }
        public string ConvertTanggal(DateTime tanggal, string tipe = "")
        {
            var res = "";
            var AngkaBulan = tanggal.ToString("MM");
            var NamaHariEng = tanggal.ToString("dddd");
            var AngkaHari = tanggal.ToString("dd");
            var Tahun = tanggal.ToString("yyyy");
            var Bulan = "";
            var Hari = "";
            switch (NamaHariEng)
            {
                case "Sunday":
                    Hari = "Minggu";
                    break;
                case "Monday":
                    Hari = "Senin";
                    break;
                case "Tuesday":
                    Hari = "Selasa";
                    break;
                case "Wednesday":
                    Hari = "Rabu";
                    break;
                case "Thursday":
                    Hari = "Kamis";
                    break;
                case "Friday":
                    Hari = "Jumat";
                    break;
                case "Saturday":
                    Hari = "Sabtu";
                    break;
                default:
                    return "";
            }
            switch (AngkaBulan)
            {
                case "01":
                    Bulan = "Januari";
                    break;
                case "02":
                    Bulan = "Februari";
                    break;
                case "03":
                    Bulan = "Maret";
                    break;
                case "04":
                    Bulan = "April";
                    break;
                case "05":
                    Bulan = "Mei";
                    break;
                case "06":
                    Bulan = "Juni";
                    break;
                case "07":
                    Bulan = "Juli";
                    break;
                case "08":
                    Bulan = "Agustus";
                    break;
                case "09":
                    Bulan = "September";
                    break;
                case "10":
                    Bulan = "Oktober";
                    break;
                case "11":
                    Bulan = "November";
                    break;
                case "12":
                    Bulan = "Desember";
                    break;
                default:
                    return "";
            }
            if (tipe == "full")
            {
                res = AngkaHari + " " + Bulan + " " + Tahun;
            }
            else if (tipe == "namabulan")
            {
                res = Bulan;
            }
            else if (tipe == "angkabulan")
            {
                res = AngkaBulan;
            }
            else if (tipe == "tahun")
            {
                res = Tahun;
            }
            else if (tipe == "namahari")
            {
                res = Hari;
            }
            else if (tipe == "angkahari")
            {
                res = AngkaHari;
            }
            return res;
        }

        public Boolean UpdateProposal(TRX_PROPOSAL INPUT, int USER_ID, int[] PROPOSAL_REV_MERIVISI_ID, string[] PROPOSAL_ADOPSI_NOMOR_JUDUL, int[] PROPOSAL_REF_SNI_ID, string[] PROPOSAL_REF_NON_SNI, string[] BIBLIOGRAFI, string[] PROPOSAL_LPK_ID, string[] PROPOSAL_RETEK_ID)
        {
            //var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var LOGCODE = MixHelper.GetLogCode();
            int LASTID = MixHelper.GetSequence("TRX_PROPOSAL");
            var DATENOW = MixHelper.ConvertDateNow();
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == INPUT.PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_LPK_ID_CONVERT = "";
            var PROPOSAL_RETEK_ID_CONVERT = "";
            if (PROPOSAL_LPK_ID == null)
            {
                PROPOSAL_LPK_ID_CONVERT = null;
            }
            else
            {
                PROPOSAL_LPK_ID_CONVERT = string.Join(";", PROPOSAL_LPK_ID);
            }

            if (PROPOSAL_RETEK_ID == null)
            {
                PROPOSAL_RETEK_ID_CONVERT = null;
            }
            else
            {
                PROPOSAL_RETEK_ID_CONVERT = string.Join(";", PROPOSAL_RETEK_ID);
            }
            //var PROPOSAL_LPK_ID_CONVERT = string.Join(";", PROPOSAL_LPK_ID);
            //var PROPOSAL_RETEK_ID_CONVERT = string.Join(";", PROPOSAL_RETEK_ID);
            PROPOSAL_LPK_ID_CONVERT = ((PROPOSAL_LPK_ID_CONVERT == null) ? "" : PROPOSAL_LPK_ID_CONVERT);
            PROPOSAL_RETEK_ID_CONVERT = ((PROPOSAL_RETEK_ID_CONVERT == null) ? "" : PROPOSAL_RETEK_ID_CONVERT);
            var fupdate = "UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID = '" + INPUT.PROPOSAL_KOMTEK_ID + "'," +
                            "PROPOSAL_JUDUL_SNI_PROPOSAL = '" + INPUT.PROPOSAL_JUDUL_SNI_PROPOSAL + "'," +
                            "PROPOSAL_JUDUL_PNPS_ENG = '" + INPUT.PROPOSAL_JUDUL_PNPS_ENG + "'," +
                            "PROPOSAL_RUANG_LINGKUP = '" + INPUT.PROPOSAL_RUANG_LINGKUP + "'," +
                            "PROPOSAL_JENIS_PERUMUSAN = '" + INPUT.PROPOSAL_JENIS_PERUMUSAN + "'," +
                            "PROPOSAL_JALUR = '" + INPUT.PROPOSAL_JALUR + "'," +
                            "PROPOSAL_JENIS_ADOPSI = '" + INPUT.PROPOSAL_JENIS_ADOPSI + "'," +
                            "PROPOSAL_METODE_ADOPSI = '" + INPUT.PROPOSAL_METODE_ADOPSI + "'," +
                            "PROPOSAL_TERJEMAHAN_SNI_ID = '" + INPUT.PROPOSAL_TERJEMAHAN_SNI_ID + "'," +
                            "PROPOSAL_AMD_SNI_ID = '" + INPUT.PROPOSAL_AMD_SNI_ID + "'," +
                            "PROPOSAL_PASAL = '" + INPUT.PROPOSAL_PASAL + "'," +
                            "PROPOSAL_UPDATE_BY = '" + USER_ID + "'," +
                            "PROPOSAL_UPDATE_DATE = " + DATENOW + "," +
                            "PROPOSAL_STATUS_PROSES = " + ((DataProposal.APPROVAL_TYPE == 0) ? 1 : DataProposal.PROPOSAL_STATUS_PROSES) + "," +
                            "PROPOSAL_LOG_CODE = '" + LOGCODE + "' WHERE PROPOSAL_ID = " + INPUT.PROPOSAL_ID;

            db.Database.ExecuteSqlCommand(fupdate);

            var tester = fupdate;
            if (PROPOSAL_REV_MERIVISI_ID != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REV WHERE PROPOSAL_REV_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var PROPOSAL_REV_MERIVISI_ID_VAL in PROPOSAL_REV_MERIVISI_ID)
                {
                    var PROPOSAL_REV_ID = MixHelper.GetSequence("TRX_PROPOSAL_REV");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REV (PROPOSAL_REV_ID,PROPOSAL_REV_PROPOSAL_ID,PROPOSAL_REV_MERIVISI_ID) VALUES (" + PROPOSAL_REV_ID + "," + INPUT.PROPOSAL_ID + "," + PROPOSAL_REV_MERIVISI_ID_VAL + ")");
                }
            }
            if (PROPOSAL_ADOPSI_NOMOR_JUDUL != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_ADOPSI WHERE PROPOSAL_ADOPSI_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL in PROPOSAL_ADOPSI_NOMOR_JUDUL)
                {
                    var PROPOSAL_ADOPSI_ID = MixHelper.GetSequence("TRX_PROPOSAL_ADOPSI");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_ADOPSI (PROPOSAL_ADOPSI_ID,PROPOSAL_ADOPSI_PROPOSAL_ID,PROPOSAL_ADOPSI_NOMOR_JUDUL) VALUES (" + PROPOSAL_ADOPSI_ID + "," + INPUT.PROPOSAL_ID + ",'" + PROPOSAL_ADOPSI_NOMOR_JUDUL_VAL + "')");
                }
            }

            if (PROPOSAL_REF_SNI_ID != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 1 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var SNI_ID in PROPOSAL_REF_SNI_ID)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",1," + SNI_ID + ")");
                }
            }
            if (PROPOSAL_REF_NON_SNI != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 2 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var DATA_NON_SNI_VAL in PROPOSAL_REF_NON_SNI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    var CEK_PROPOSAL_REF_NON_SNI = db.Database.SqlQuery<MASTER_ACUAN_NON_SNI>("SELECT * FROM MASTER_ACUAN_NON_SNI WHERE ACUAN_NON_SNI_STATUS = 1 AND LOWER(ACUAN_NON_SNI_JUDUL) = '" + DATA_NON_SNI_VAL.ToLower() + "'").SingleOrDefault();
                    if (CEK_PROPOSAL_REF_NON_SNI != null)
                    {

                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",2,'" + CEK_PROPOSAL_REF_NON_SNI.ACUAN_NON_SNI_ID + "','" + DATA_NON_SNI_VAL + "')");
                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",2,'" + DATA_NON_SNI_VAL + "')");
                    }
                }
            }
            if (BIBLIOGRAFI != null)
            {
                db.Database.ExecuteSqlCommand("DELETE TRX_PROPOSAL_REFERENCE WHERE PROPOSAL_REF_TYPE = 3 AND PROPOSAL_REF_PROPOSAL_ID = " + INPUT.PROPOSAL_ID);
                foreach (var BIBLIOGRAFI_VAL in BIBLIOGRAFI)
                {
                    var PROPOSAL_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_REFERENCE");
                    //db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + BIBLIOGRAFI_VAL + "')");
                    var CEK_BIBLIOGRAFI = db.Database.SqlQuery<MASTER_BIBLIOGRAFI>("SELECT * FROM MASTER_BIBLIOGRAFI WHERE BIBLIOGRAFI_STATUS = 1 AND LOWER(BIBLIOGRAFI_JUDUL) = '" + BIBLIOGRAFI_VAL.ToLower() + "'").SingleOrDefault();
                    if (CEK_BIBLIOGRAFI != null)
                    {
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_SNI_ID,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + CEK_BIBLIOGRAFI.BIBLIOGRAFI_ID + "','" + BIBLIOGRAFI_VAL + "')");
                    }
                    else
                    {
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_REFERENCE (PROPOSAL_REF_ID,PROPOSAL_REF_PROPOSAL_ID,PROPOSAL_REF_TYPE,PROPOSAL_REF_EXT_JUDUL) VALUES (" + PROPOSAL_REF_ID + "," + INPUT.PROPOSAL_ID + ",3,'" + BIBLIOGRAFI_VAL + "')");
                    }
                }
            }


            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_CODE;
            var PROPOSAL_ID = INPUT.PROPOSAL_ID;
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
            if (INPUT.PROPOSAL_IS_ORG_MENDUKUNG == 1)
            {
                HttpPostedFileBase file = Request.Files["PROPOSAL_DUKUNGAN_FILE_PATH"];
                if (file.ContentLength > 0)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 29");
                    int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
                    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                    Stream stremdokumen = file.InputStream;
                    byte[] appData = new byte[file.ContentLength + 1];
                    stremdokumen.Read(appData, 0, file.ContentLength);
                    string Extension = Path.GetExtension(file.FileName);
                    if (Extension.ToLower() == ".pdf")
                    {
                        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                        string filePathpdf = path + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                        string filePathxml = path + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                        pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                        pdf.Save(@"" + filePathxml);
                        var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                        var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                        var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                    "'10', " +
                                    "'29', " +
                                    "'" + PROPOSAL_ID + "', " +
                                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Bukti Dukungan Usulan" + "', " +
                                    "'Bukti Dukungan Usulan dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                    "'" + "BUKTI_DUKUNGAN_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                    "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                    "'0', " +
                                    "'" + USER_ID + "', " +
                                    DATENOW + "," +
                                    "'1', " +
                                    "'" + LOGCODE_TANGGAPAN_MTPS + "'";
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                        String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                        MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
                    }
                }
            }
            //HttpPostedFileBase file2 = Request.Files["PROPOSAL_LAMPIRAN_FILE_PATH"];
            //if (file2.ContentLength > 0)
            //{
            //    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 30");
            //    int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
            //    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
            //    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
            //    Stream stremdokumen = file2.InputStream;
            //    byte[] appData = new byte[file2.ContentLength + 1];
            //    stremdokumen.Read(appData, 0, file2.ContentLength);
            //    string Extension = Path.GetExtension(file2.FileName);
            //    if (Extension.ToLower() == ".pdf")
            //    {
            //        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
            //        string filePathpdf = path + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
            //        string filePathxml = path + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
            //        pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
            //        pdf.Save(@"" + filePathxml);
            //        var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
            //        var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
            //        var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
            //                    "'10', " +
            //                    "'30', " +
            //                    "'" + PROPOSAL_ID + "', " +
            //                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Pendukung Usulan" + "', " +
            //                    "'Lampiran Pendukung Usulan dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
            //                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
            //                    "'" + "LAMPIRAN_PENDUKUNG_USULAN_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
            //                    "'" + Extension.ToUpper().Replace(".", "") + "', " +
            //                    "'0', " +
            //                    "'" + USER_ID + "', " +
            //                    DATENOW + "," +
            //                    "'1', " +
            //                    "'" + LOGCODE_TANGGAPAN_MTPS + "'";
            //        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
            //        String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
            //        MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
            //    }
            //}
            //HttpPostedFileBase file3 = Request.Files["PROPOSAL_SURAT_PENGAJUAN_PNPS"];
            //if (file3.ContentLength > 0)
            //{
            //    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 32");
            //    int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
            //    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
            //    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
            //    Stream stremdokumen = file3.InputStream;
            //    byte[] appData = new byte[file3.ContentLength + 1];
            //    stremdokumen.Read(appData, 0, file3.ContentLength);
            //    string Extension = Path.GetExtension(file3.FileName);
            //    if (Extension.ToLower() == ".pdf")
            //    {
            //        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
            //        string filePathpdf = path + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
            //        string filePathxml = path + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
            //        pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
            //        pdf.Save(@"" + filePathxml);
            //        var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
            //        var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
            //        var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
            //                    "'10', " +
            //                    "'32', " +
            //                    "'" + PROPOSAL_ID + "', " +
            //                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Surat Pengajuan PNPS" + "', " +
            //                    "'Lampiran Surat Pengajuan PNPS dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
            //                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
            //                    "'" + "SURAT_PENGAJUAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
            //                    "'" + Extension.ToUpper().Replace(".", "") + "', " +
            //                    "'0', " +
            //                    "'" + USER_ID + "', " +
            //                    DATENOW + "," +
            //                    "'1', " +
            //                    "'" + LOGCODE_TANGGAPAN_MTPS + "'";
            //        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
            //        String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
            //        MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
            //    }
            //}

            //HttpPostedFileBase file4 = Request.Files["PROPOSAL_OUTLINE_RSNI"];
            //if (file4.ContentLength > 0)
            //{
            //    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 36");
            //    int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");
            //    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER));
            //    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
            //    Stream stremdokumen = file4.InputStream;
            //    byte[] appData = new byte[file4.ContentLength + 1];
            //    stremdokumen.Read(appData, 0, file4.ContentLength);
            //    string Extension = Path.GetExtension(file4.FileName);
            //    if (Extension.ToLower() == ".pdf")
            //    {
            //        Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
            //        string filePathpdf = path + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
            //        string filePathxml = path + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
            //        pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
            //        pdf.Save(@"" + filePathxml);
            //        var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
            //        var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
            //        var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
            //                    "'10', " +
            //                    "'36', " +
            //                    "'" + PROPOSAL_ID + "', " +
            //                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Lampiran Outline RSNI" + "', " +
            //                    "'Lampiran Outline RSNI dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
            //                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
            //                    "'" + "LAMPIRAN_OUTLINE_RSNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
            //                    "'" + Extension.ToUpper().Replace(".", "") + "', " +
            //                    "'0', " +
            //                    "'" + USER_ID + "', " +
            //                    DATENOW + "," +
            //                    "'1', " +
            //                    "'" + LOGCODE_TANGGAPAN_MTPS + "'";
            //        db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
            //        String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
            //        MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);
            //    }
            //}

            if (DataProposal.APPROVAL_TYPE == 0)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND APPROVAL_STATUS_PROPOSAL = 0 AND APPROVAL_TYPE = 0");
            }

            //return Json(new { tester,INPUT, PROPOSAL_REV_MERIVISI_ID, PROPOSAL_ADOPSI_NOMOR_JUDUL, PROPOSAL_REF_SNI_ID, PROPOSAL_REF_NON_SNI, BIBLIOGRAFI }, JsonRequestBehavior.AllowGet);
            String objek = fupdate.Replace("'", "-");
            MixHelper.InsertLog(LOGCODE, objek, 2);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            //return RedirectToAction("Index");
            return true;
        }

    }
}
