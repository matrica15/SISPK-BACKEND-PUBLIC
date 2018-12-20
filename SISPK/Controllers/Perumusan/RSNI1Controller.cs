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
    public class RSNI1Controller : Controller
    {
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            var IS_KOMTEK = Convert.ToInt32(Session["IS_KOMTEK"]);
            string ViewName = ((IS_KOMTEK == 1) ? "DaftarPenyusunanRSNI1" : "DaftarPengesahanRSNI1");
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
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = " + 3 + " AND DOC_FOLDER_ID = 12 AND DOC_STATUS = 1").SingleOrDefault();
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
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = " + 3 + " AND DOC_FOLDER_ID = 12 AND DOC_STATUS = 1").SingleOrDefault();
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
        public ActionResult GetContent(int DOC_ID = 0)
        {
            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_ID = " + DOC_ID).SingleOrDefault();

            string path = Server.MapPath("~" + Dokumen.DOC_FILE_PATH + ".txt");
            string text = System.IO.File.ReadAllText(@"" + path);
            return Json(new
            {
                data = text
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult GetContentSearch(int PROPOSAL_ID = 0, string SEARCH_NAME = "", int SEARCH_TIPE = 1)
        {
            string QuerySearch = "";
            string SearchName = SEARCH_NAME;
            string[] Name = SearchName.Split(' ');
            QuerySearch = "SELECT * FROM VIEW_DOCUMENTS WHERE DOC_STATUS = 1 AND " + ((SEARCH_TIPE == 1) ? "DOC_RELATED_ID = " + PROPOSAL_ID : " (DOC_RELATED_ID <> " + PROPOSAL_ID + " OR DOC_RELATED_ID IS NULL)") + " AND ( ";
            string lastItem = Name.Last();

            foreach (string Res in Name)
            {
                if (!object.ReferenceEquals(Res, lastItem))
                {
                    QuerySearch += " LOWER(DOC_NAME) LIKE '%" + Res.ToLower() + "%' OR ";
                }
                else
                {
                    QuerySearch += " LOWER(DOC_NAME) LIKE '%" + Res.ToLower() + "%' )";
                }
            }

            var Dokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>(QuerySearch).ToList();

            return Json(new
            {
                Dokumen,
                qse = QuerySearch
            }, JsonRequestBehavior.AllowGet);
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
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 3 AND AA.DOC_FOLDER_ID = 12 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
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
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 4").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,4," + APPROVAL_STATUS_SESSION + ")");
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 4, PROPOSAL_STATUS_PROSES = " + APPROVAL_STATUS + ", PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 4, PROPOSAL_STATUS_PROSES = " + APPROVAL_STATUS + ", PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 4").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0,'" + APPROVAL_REASON + "'," + DATENOW + "," + USER_ID + ",1,4," + APPROVAL_STATUS_SESSION + ")");
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Pengesahan1(HttpPostedFileBase BA_RATEK, HttpPostedFileBase DAFTAR_HADIR, HttpPostedFileBase NOTULEN, int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, string PROPOSAL_PNPS_CODE = "", int APPROVAL_TYPE = 0, int APPROVAL_STATUS = 1, string APPROVAL_REASON = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            var PROPOSAL_PNPS_CODE_FIXER = PROPOSAL_PNPS_CODE;
            Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER));
            string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER + "/");

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


            if (Extension_BA_RATEK.ToLower() == ".docx" || Extension_BA_RATEK.ToLower() == ".doc")
            {
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_BA_RATEK);
                string filePathdoc = path + "BERITA_ACARA_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "BERITA_ACARA_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "BERITA_ACARA_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                
                doc.Save(@"" + filePathxml);

                var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 4 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 12 AND DOC_STATUS = 1").SingleOrDefault();
                if (CEKDOKUMEN != null)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                }
                int LASTID = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_BA_RATEK = MixHelper.GetLogCode();
                var FNAME_BA_RATEK = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_BA_RATEK = "'" + LASTID + "', " +
                            "'12', " +
                            "'4', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Rapat Teknis RSNI 1" + "', " +
                            "'Berita Acara Rapat Teknis RSNI 1" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "BERITA_ACARA_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
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
                BA_RATEK.SaveAs(path + "BERITA_ACARA_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + Extension_BA_RATEK.ToUpper());
                var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 4 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 12 AND DOC_STATUS = 1").SingleOrDefault();
                if (CEKDOKUMEN != null)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                }
                int LASTID = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_BA_RATEK = MixHelper.GetLogCode();
                var FNAME_BA_RATEK = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_BA_RATEK = "'" + LASTID + "', " +
                            "'12', " +
                            "'4', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Rapat Teknis RSNI 1" + "', " +
                            "'Berita Acara Rapat Teknis RSNI 1" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "BERITA_ACARA_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
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
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DAFTAR_HADIR);
                string filePathdoc = path + "DAFTAR_HADIR_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "DAFTAR_HADIR_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "DAFTAR_HADIR_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 5 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 12 AND DOC_STATUS = 1").SingleOrDefault();
                if (CEKDOKUMEN != null)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                }
                int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                            "'12', " +
                            "'5', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Rapat Teknis RSNI 1" + "', " +
                            "'Daftar Hadir Rapat Teknis RSNI 1" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "DAFTAR_HADIR_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                            "'" + Extension_BA_RATEK.ToUpper().Replace(".", "") + "', " +
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
                DAFTAR_HADIR.SaveAs(path + "DAFTAR_HADIR_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + Extension_DAFTAR_HADIR.ToUpper());
                var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 5 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 12 AND DOC_STATUS = 1").SingleOrDefault();
                if (CEKDOKUMEN != null)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                }
                int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                            "'12', " +
                            "'5', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Rapat Teknis RSNI 1" + "', " +
                            "'Daftar Hadir Rapat Teknis RSNI 1" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "DAFTAR_HADIR_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
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
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_NOTULEN);
                string filePathdoc = path + "HASIL_NOTULEN_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "HASIL_NOTULEN_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "HASIL_NOTULEN_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 6 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 12 AND DOC_STATUS = 1").SingleOrDefault();
                if (CEKDOKUMEN != null)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                }
                int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                            "'12', " +
                            "'6', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Hasil Notulen Rapat Teknis RSNI 1" + "', " +
                            "'Hasil Notulen Rapat Teknis RSNI 1" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "HASIL_NOTULEN_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
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
                NOTULEN.SaveAs(path + "HASIL_NOTULEN_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + Extension_NOTULEN.ToUpper());
                var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 6 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 12 AND DOC_STATUS = 1").SingleOrDefault();
                if (CEKDOKUMEN != null)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                }
                int LASTID = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_NOTULEN = "'" + LASTID + "', " +
                            "'12', " +
                            "'6', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Hasil Notulen Rapat Teknis RSNI 1" + "', " +
                            "'Hasil Notulen Rapat Teknis RSNI 1" + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "HASIL_NOTULEN_RAPAT_TEKNIS_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
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
            if (APPROVAL_TYPE == 1)
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 5, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 4").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,4," + APPROVAL_STATUS_SESSION + ")");
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 4, PROPOSAL_STATUS_PROSES = " + APPROVAL_STATUS + ", PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 4, PROPOSAL_STATUS_PROSES = " + APPROVAL_STATUS + ", PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 4").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0,'" + APPROVAL_REASON + "'," + DATENOW + "," + USER_ID + ",1,4," + APPROVAL_STATUS_SESSION + ")");
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Create(int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, int SUBMIT_TIPE = 0, string rsni1contents = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
            HttpPostedFileBase DATA_RSNI = Request.Files["DATA_RSNI"];
            var DATENOW = MixHelper.ConvertDateNow();
            if (SUBMIT_TIPE == 1)
            {
                if (DATA_RSNI.ContentLength > 0)
                {
                    var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
                    var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER));
                    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                    Stream stremdokumen = DATA_RSNI.InputStream;
                    byte[] appData = new byte[DATA_RSNI.ContentLength + 1];
                    stremdokumen.Read(appData, 0, DATA_RSNI.ContentLength); 
                    string Extension = Path.GetExtension(DATA_RSNI.FileName);
                    if (Extension.ToLower() == ".docx" || Extension.ToLower() == ".doc")
                    {

                        Aspose.Words.Document doc = new Aspose.Words.Document(stremdokumen);
                        doc.RemoveMacros();
                        string filePathdoc = path + "RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".docx";
                        string filePathpdf = path + "RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf";
                        string filePathxml = path + "RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml";
                        doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                        doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                        doc.Save(@"" + filePathxml);

                        var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 3 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 12 AND DOC_STATUS = 1").SingleOrDefault();
                        if (CEKDOKUMEN != null)
                        {
                            db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                        }
                        int LASTID = MixHelper.GetSequence("TRX_DOCUMENTS");
                        var LOGCODE_RSNI1 = MixHelper.GetLogCode();
                        var FNAME_RSNI1 = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                        var FVALUE_RSNI1 = "'" + LASTID + "', " +
                                    "'12', " +
                                    "'3', " +
                                    "'" + PROPOSAL_ID + "', " +
                                    "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") RSNI 1" + "', " +
                                    "'Hasil Rancangan SNI 1 dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                                    "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                    "'" + "RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                                    "'docx', " +
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
                } else
                {
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
            else
            {
                if (DATA_RSNI.ContentLength > 0)
                {
                    var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
                    var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT"));
                    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                    Stream stremdokumen = DATA_RSNI.InputStream;
                    byte[] appData = new byte[DATA_RSNI.ContentLength + 1];
                    stremdokumen.Read(appData, 0, DATA_RSNI.ContentLength);
                    string Extension = Path.GetExtension(DATA_RSNI.FileName);
                    if (Extension.ToLower() == ".docx" || Extension.ToLower() == ".doc")
                    {

                        Aspose.Words.Document doc = new Aspose.Words.Document(stremdokumen);
                        doc.RemoveMacros();
                        var SNI_DOC_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0)+1 AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.SNI_DOC_TYPE = 1").SingleOrDefault();
                        string filePathdoc = path + "DRAFT_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".docx";
                        string filePathpdf = path + "DRAFT_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".pdf";
                        string filePathxml = path + "DRAFT_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".xml";
                        doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                        doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                        doc.Save(@"" + filePathxml);
                        doc.Save(@"" + path + "DRAFT_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + ".docx", Aspose.Words.SaveFormat.Docx);
                        
                        var LOGCODE_SNI_DOC = MixHelper.GetLogCode();
                        int LASTID_SNI_DOC = MixHelper.GetSequence("TRX_SNI_DOC");

                        var fname = "SNI_DOC_ID,SNI_DOC_PROPOSAL_ID,SNI_DOC_TYPE,SNI_DOC_FILE_PATH,SNI_DOC_VERSION,SNI_DOC_CREATE_BY,SNI_DOC_CREATE_DATE,SNI_DOC_LOG_CODE,SNI_DOC_STATUS";
                        var fvalue = "'" + LASTID_SNI_DOC + "', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'" + 1 + "', " +
                                        "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI1/DRAFT/DRAFT_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".docx" + "', " +
                                        "'" + SNI_DOC_VERSION + "', " +
                                        "'" + USER_ID + "', " +
                                        DATENOW + "," +
                                        "'" + LOGCODE_SNI_DOC + "', " +
                                        "'1'";
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_SNI_DOC (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");


                        var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 3 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 12 AND DOC_STATUS = 1").SingleOrDefault();
                        if (CEKDOKUMEN == null)
                        {
                            int LASTID = MixHelper.GetSequence("TRX_DOCUMENTS");
                            var LOGCODE_RSNI1 = MixHelper.GetLogCode();
                            var FNAME_RSNI1 = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                            var FVALUE_RSNI1 = "'" + LASTID + "', " +
                                        "'12', " +
                                        "'3', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Draft RSNI 1" + "', " +
                                        "'Draft Rancangan SNI 1 " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                        "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI1/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/" + "', " +
                                        "'" + "DRAFT_RSNI1_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                                        "'docx', " +
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
                } else
                {
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
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult DataRSNI1Komtek(DataTables param)
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

            string where_clause = "PROPOSAL_STATUS = 4 AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID;

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
                Convert.ToString("<center><a href='/Perumusan/RSNI1/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 4 && list.PROPOSAL_STATUS_PROSES == 0)?"<a href='/Perumusan/RSNI1/Create/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Susun RSNI 1'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),

            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DataRSNI1PPS(DataTables param)
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


            string where_clause = "PROPOSAL_STATUS = 4 AND PROPOSAL_STATUS_PROSES = 1  " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");

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
                search_clause += " OR LOWER(PROPOSAL_CREATE_DATE_NAME) LIKE LOWER('%" + search + "%') OR LOWER(KOMTEK_NAME) LIKE LOWER('%" + search + "%'))";
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

            //Response.Write(var SelectedData);
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
                Convert.ToString("<center><a href='/Perumusan/RSNI1/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 4 && list.PROPOSAL_STATUS_PROSES == 1)?"<a href='/Perumusan/RSNI1/Pengesahan/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Pengesahan RSNI 1'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),

            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Test()
        {

            Aspose.Words.Document doc = new Aspose.Words.Document(@"C:\Users\irghi\Desktop\BSN\test.docx");
            doc.Save(@"C:\Users\irghi\Desktop\TestExport\test.html", SaveFormat.Html);
            doc.Save(@"C:\Users\irghi\Desktop\TestExport\test.pdf", SaveFormat.Pdf);
            doc.Save(@"C:\Users\irghi\Desktop\TestExport\test.xml", SaveFormat.WordML);
            doc.Save(@"C:\Users\irghi\Desktop\TestExport\test1.xml");
            doc.Save(@"C:\Users\irghi\Desktop\TestExport\test.docm", SaveFormat.Docm);
            doc.Save(@"C:\Users\irghi\Desktop\TestExport\test.epub");

            return Json(new { query = Server.MapPath("/Upload") }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Wew()
        {
            int lastid = MixHelper.GetSequence("TRX_SNI_DOC");
            Aspose.Words.Document doc = new Aspose.Words.Document(@"C:\Users\irghi\Desktop\BSN\test.docx");
            // Save the document to a MemoryStream object.
            MemoryStream stream = new MemoryStream();
            doc.Save(stream, SaveFormat.Doc);

            // Get the filename from the document.
            string fileName = Path.GetFileName(doc.OriginalFileName);

            // Create the SQL command.
            string StringOutput = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            string commandString = "UPDATE TRX_PROPOSAL SET PROPOSAL_RUANG_LINGKUP = '" + StringOutput + "' WHERE PROPOSAL_ID = 1";

            //db.Database.ExecuteSqlCommand(commandString);
            return Json(new { query = StringOutput }, JsonRequestBehavior.AllowGet);

        }
    }
}
