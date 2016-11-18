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
using Aspose.Words.Lists;


namespace SISPK.Controllers.Pengajuan
{
    [Auth(RoleTipe = 1)]
    public class PengesahanController : Controller
    {
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }
        [Auth(RoleTipe = 5)]
        public ActionResult Approval(int id = 0)
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
            var ListKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 orderby komtek.KOMTEK_CODE ascending select komtek).ToList();
            ViewData["ListKomtek"] = ListKomtek;
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

            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_RELATED_ID = " + id).ToList();
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

        public ActionResult Detail(int id = 0)
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
        public ActionResult Approval(int PROPOSAL_ID = 0, int APPROVAL_TYPE = 0, string PROPOSAL_ST_KOM_NO = "", string PROPOSAL_ST_KOM_LAMPIRAN = "-", string PROPOSAL_ST_KOM_DATE = "", string PROPOSAL_ST_KOM_TIME = "", string APPROVAL_REASON = "", int[] PROPOSAL_ICS_REF_ICS_ID = null, int PROPOSAL_KOMTEK_ID = 0, int PROPOSAL_KOMTEK_ID_OLD = 0)
        {
            var LOGCODE = MixHelper.GetLogCode();
            //int LASTID = MixHelper.GetSequence("TRX_SURAT_TUGAS");
            var DATENOW = MixHelper.ConvertDateNow();
            var join_tgl_surat = (PROPOSAL_ST_KOM_DATE + " " + PROPOSAL_ST_KOM_TIME);
            String ST_DATE = "TO_DATE('" + PROPOSAL_ST_KOM_DATE + "', 'yyyy-mm-dd hh24:mi:ss')";
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
            
            var PROPOSAL_PNPS_CODE = db.Database.SqlQuery<string>("SELECT CAST(TO_CHAR (SYSDATE, 'YYYY') || '.' || KOMTEK_CODE || '.' || ( SELECT CAST ( ( CASE WHEN LENGTH (COUNT(PROPOSAL_ID) + 1) = 1 THEN '0' || CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) ELSE CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) END ) AS VARCHAR2 (255) ) PNPSCODE FROM TRX_PROPOSAL WHERE PROPOSAL_KOMTEK_ID = KOMTEK_ID ) AS VARCHAR2(255)) AS PNPSCODE FROM MASTER_KOMITE_TEKNIS WHERE KOMTEK_ID = " + PROPOSAL_KOMTEK_ID).SingleOrDefault();
            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID=" + PROPOSAL_KOMTEK_ID + ",PROPOSAL_PNPS_CODE = '" + PROPOSAL_PNPS_CODE + "', PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
            if (APPROVAL_TYPE == 1)
            {                
                var Data = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/PNPS/" + Data.PROPOSAL_PNPS_CODE));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/PNPS/" + Data.PROPOSAL_PNPS_CODE + "/");

                string dataDir = Server.MapPath("~/Format/Laporan/");
                Stream stream = System.IO.File.OpenRead(dataDir + "FORMULIR_PENUGASAN_KOMITE_TEKNIS.docx");

                Aspose.Words.Document doc = new Aspose.Words.Document(stream);
                stream.Close();
                if (Data != null)
                {
                    var DataKomtek = db.Database.SqlQuery<VIEW_ANGGOTA>("SELECT * FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + Data.KOMTEK_ID + " AND JABATAN = 'Ketua' AND KOMTEK_ANGGOTA_STATUS = 1").SingleOrDefault();

                    var dt = Convert.ToDateTime(Data.ST_DATE);
                    string time = dt.ToString("hh:mm tt");
                    doc.Range.Replace("FullTanggalPenugasan", ConvertTanggal(Convert.ToDateTime(DateTime.Now), "full"), false, true);
                    doc.Range.Replace("HariPenugasan", ConvertTanggal(Convert.ToDateTime(Data.ST_DATE), "namahari"), false, true);
                    doc.Range.Replace("TanggalPenugasan", ConvertTanggal(Convert.ToDateTime(Data.ST_DATE), "full"), false, true);
                    doc.Range.Replace("NomorPenugasan", "", false, true);
                    doc.Range.Replace("LampiranPenugasan", "", false, true);
                    doc.Range.Replace("NamaKepalaPPS", "Sumartini Maksum", false, true);
                    doc.Range.Replace("NIPKepalaPPS", "Nip : 19561014 198107 2 001", false, true);
                    doc.Range.Replace("NamaKetuaKomiteTeknis", DataKomtek.KOMTEK_ANGGOTA_NAMA, false, true);
                    doc.Range.Replace("KodeKomtek", Data.KOMTEK_CODE, false, true);
                    doc.Range.Replace("NamaKomtek", Data.KOMTEK_NAME, false, true);
                    doc.Range.Replace("WaktuPenugasan", time, false, true);
                    doc.Range.Replace("TahunPenugasan", ConvertTanggal(Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE), "tahun"), false, true);
                }
                doc.Save(@"" + path + "SURAT_PERSETUJUAN_PNPS_" + Data.PROPOSAL_PNPS_CODE + "_" + TGL_SEKARANG + ".docx", Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + path + "SURAT_PERSETUJUAN_PNPS_" + Data.PROPOSAL_PNPS_CODE + "_" + TGL_SEKARANG + ".pdf", Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + path + "SURAT_PERSETUJUAN_PNPS_" + Data.PROPOSAL_PNPS_CODE + "_" + TGL_SEKARANG + ".xml");
                var LOGCODE_ST = MixHelper.GetLogCode();
                int LASTID_ST = MixHelper.GetSequence("TRX_DOCUMENTS");
                var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_ST + "', " +
                            "'11', " +
                            "'2', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + Data.PROPOSAL_PNPS_CODE + ") Surat Persetujuan PNPS" + "', " +
                            "'Surat Persetujuan PNPS dengan kode PNPS : " + Data.PROPOSAL_PNPS_CODE + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/PNPS/" + Data.PROPOSAL_PNPS_CODE + "/" + "', " +
                            "'" + "SURAT_PERSETUJUAN_PNPS_" + Data.PROPOSAL_PNPS_CODE + "_" + TGL_SEKARANG + "" + "', " +
                            "'DOCX', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            DATENOW + "," +
                            "'1', " +
                            "'" + LOGCODE_ST + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_ST, objekTanggapan, 1);

                if (Data.PROPOSAL_JENIS_PERUMUSAN == 2)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET  PROPOSAL_KOMTEK_ID=" + PROPOSAL_KOMTEK_ID + ",PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                    var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                    String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                    MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);
                }
                else if (Data.PROPOSAL_JENIS_PERUMUSAN == 3)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET  PROPOSAL_KOMTEK_ID=" + PROPOSAL_KOMTEK_ID + ",PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                    var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                    String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                    MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);
                }
                else {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID=" + PROPOSAL_KOMTEK_ID + ",PROPOSAL_STATUS = 4, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                    var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                    String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 4, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                    MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);
                }

                

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,3,1)");
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_PNPS = " + DATENOW + ", MONITORING_HASIL_APP_PNPS = 1, MONITORING_NO_SRT_APP_PUB_PNPS = '" + PROPOSAL_ST_KOM_NO + "', MONITORING_TGL_APP_PUB_PNPS = " + ST_DATE + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID=" + PROPOSAL_KOMTEK_ID + ", PROPOSAL_STATUS_PROSES = 2, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_KOMTEK_ID=" + PROPOSAL_KOMTEK_ID + ", PROPOSAL_STATUS_PROSES = 2, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_REASON,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",'" + APPROVAL_REASON + "',0," + DATENOW + "," + USER_ID + ",1,3,1)");
                db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_PNPS = " + DATENOW + ", MONITORING_HASIL_APP_PNPS = 0, MONITORING_NO_SRT_APP_PUB_PNPS = '" + PROPOSAL_ST_KOM_NO + "', MONITORING_TGL_APP_PUB_PNPS = " + ST_DATE + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            }
            
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
            HttpPostedFileBase file2 = Request.Files["RISALAH_RAPAT"];

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
                    string filePathpdf = path + "RISALAH_RAPAT_PUBLIKASI_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                    string filePathxml = path + "RISALAH_RAPAT_PUBLIKASI_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                    pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    pdf.Save(@"" + filePathxml);
                    var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                    var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOC + "', " +
                                "'10', " +
                                "'33', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Risalah Rapat Publikasi Usulan PNPS" + "', " +
                                "'Risalah Rapat Publikasi Usulan PNPS dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "RISALAH_RAPAT_PUBLIKASI_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
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
            if (PROPOSAL_ICS_REF_ICS_ID != null)
            {
                foreach (var i in PROPOSAL_ICS_REF_ICS_ID)
                {
                    int PROPOSAL_ICS_REF_ID = MixHelper.GetSequence("TRX_PROPOSAL_ICS_REF");
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_ICS_REF (PROPOSAL_ICS_REF_ID,PROPOSAL_ICS_REF_PROPOSAL_ID,PROPOSAL_ICS_REF_ICS_ID)VALUES(" + PROPOSAL_ICS_REF_ID + "," + PROPOSAL_ID + "," + i + ")");
                }
            }

            string pathnya = Server.MapPath("~/Upload/Dokumen/SK_PNPS/");
            HttpPostedFileBase file_sk = Request.Files["SK_PNPS"];
            var upload = Request.Files["SK_PNPS"];
            var file_name_sk = "";
            var filePath_sk = "";
            var fileExtension_sk = "";
            if (upload.ContentLength > 0)
            {
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(pathnya))
                {
                    //If Directory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(pathnya);
                }
                string lampiranregulasipath = file_sk.FileName;
                if (lampiranregulasipath.Trim() != "")
                {
                    var Data = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                    lampiranregulasipath = Path.GetFileNameWithoutExtension(file_sk.FileName);
                    fileExtension_sk = Path.GetExtension(file_sk.FileName);
                    file_name_sk = "SK_PNPS_" + Data.PROPOSAL_PNPS_CODE + "_" + PROPOSAL_ID  + "_" + TGL_SEKARANG + fileExtension_sk;
                    filePath_sk = pathnya + file_name_sk.Replace(" ", "_");
                    file_sk.SaveAs(filePath_sk);

                    var LOGCODE_ST = MixHelper.GetLogCode();
                    int LASTID_ST = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_ST + "', " +
                                "'11', " +
                                "'99', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + Data.PROPOSAL_PNPS_CODE + ") SK PNPS" + "', " +
                                "'SK PNPS dengan kode PNPS : " + Data.PROPOSAL_PNPS_CODE + "', " +
                                "'" + "/Upload/Dokumen/SK_PNPS/" + "', " +
                                "'" + "SK_PNPS_" + Data.PROPOSAL_PNPS_CODE + "_" + PROPOSAL_ID + "_" + TGL_SEKARANG + "', " +
                                "'pdf', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_ST + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_ST, objekTanggapan, 1);
                }
            }

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";

            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult ApprovalSave(int PROPOSAL_ID = 0)
        {
            var UserId = Session["USER_ID"];
            var logcode = db.Database.SqlQuery<String>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            var datenow = MixHelper.ConvertDateNow();
            var year_now = DateTime.Now.Year;

            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 2, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);

            String objek = "PROPOSAL_STATUS = 2, PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId;
            MixHelper.InsertLog(logcode, objek.Replace("'", "-"), 2);

            int lastid = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + lastid + "," + PROPOSAL_ID + ",1," + datenow + "," + UserId + ",1,1,1)");

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return Json(new { result = true }, JsonRequestBehavior.AllowGet);
            //return RedirectToAction("Index");
        }
        public ActionResult DataPengesahan(DataTables param, int id = 0)
        {
            var IsDiterima = id;
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;
            var BIDANG_ID = Convert.ToInt32(Session["BIDANG_ID"]);

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("PROPOSAL_PNPS_CODE");
            order_field.Add("PROPOSAL_TYPE_NAME");
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


            string where_clause = "";
            if (IsDiterima == 0)
            {
                where_clause += "PROPOSAL_STATUS_PROSES = 1 AND PROPOSAL_STATUS = 3 " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");
            }
            else
            {
                where_clause += "PROPOSAL_STATUS > 3 " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");
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
                Convert.ToString(list.KOMTEK_CODE+" "+list.KOMTEK_NAME),
                Convert.ToString(list.PROPOSAL_TYPE_NAME),
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString("<center>"+list.PROPOSAL_IS_URGENT_NAME+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                Convert.ToString("<center><a href='/Pengajuan/Pengesahan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 3 && list.PROPOSAL_STATUS_PROSES == 1)?"<a href='/Pengajuan/Pengesahan/Approval/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Pengesahan Usulan'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
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
    }
}
