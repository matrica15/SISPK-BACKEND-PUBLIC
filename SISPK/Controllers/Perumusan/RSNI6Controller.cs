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
    public class RSNI6Controller : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            var IS_KOMTEK = Convert.ToInt32(Session["IS_KOMTEK"]);
            string ViewName = ((IS_KOMTEK == 1) ? "DaftarPenyusunanRSNI6" : "DaftarPengesahanRSNI6");
            return View(ViewName);
        }
        [Auth(RoleTipe = 2)]
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

            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 23 AND DOC_FOLDER_ID = 17 AND DOC_STATUS = 1").SingleOrDefault();
            if (DefaultDokumen == null)
            {
                var DefaultDokumenRSNI1 = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 17 AND DOC_FOLDER_ID = 15 AND DOC_STATUS = 1").SingleOrDefault();
                ViewData["DefaultDokumen"] = DefaultDokumenRSNI1;
            }
            else
            {
                ViewData["DefaultDokumen"] = DefaultDokumen;
            }
            var DataPolling = db.Database.SqlQuery<VIEW_POLLING_SINGLE>("SELECT AA.* FROM VIEW_POLLING_SINGLE AA WHERE AA.POLLING_PROPOSAL_ID = " + id + " AND AA.POLLING_VERSION = 2 AND AA.POLLING_TYPE = 7").SingleOrDefault();
            ViewData["DataPolling"] = DataPolling;
            var DetailPolling = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT AA.* FROM VIEW_POLLING_DETAIL AA WHERE AA.POLLING_PROPOSAL_ID = " + id + " AND AA.POLLING_VERSION = 2 ORDER BY AA.POLLING_DETAIL_PASAL,AA.POLLING_DETAIL_OPTION ASC").ToList();
            ViewData["DetailPolling"] = DetailPolling;
            var DokJp = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS BB WHERE BB.DOC_ID = (SELECT MAX(AA.DOC_ID) FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 16 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_NAME LIKE '%V2')").SingleOrDefault();
            ViewData["IsKetua"] = ((IsKetua == "Ketua" || IsKetua == "Sekretariat") ? 1 : 0);
            ViewData["Dokumen"] = Dokumen;
            ViewData["DokJp"] = DokJp;
            ViewData["RefLain"] = RefLain;

            return View();
        }
        public ActionResult Detail(int id = 0)
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

            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 23 AND DOC_FOLDER_ID = 17 AND DOC_STATUS = 1").SingleOrDefault();
            if (DefaultDokumen == null)
            {
                var DefaultDokumenRSNI1 = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 17 AND DOC_FOLDER_ID = 15 AND DOC_STATUS = 1").SingleOrDefault();
                ViewData["DefaultDokumen"] = DefaultDokumenRSNI1;
            }
            else
            {
                ViewData["DefaultDokumen"] = DefaultDokumen;
            }
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER) AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 9").SingleOrDefault();
            var DetailRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 9 AND PROPOSAL_RAPAT_VERSION = " + VERSION_RATEK).SingleOrDefault();
            var DataPolling = db.Database.SqlQuery<VIEW_POLLING_SINGLE>("SELECT AA.* FROM VIEW_POLLING_SINGLE AA WHERE AA.POLLING_PROPOSAL_ID = " + id + " AND AA.POLLING_VERSION = 2 AND AA.POLLING_TYPE = 7").SingleOrDefault();
            var DetailPolling = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT AA.* FROM VIEW_POLLING_DETAIL AA WHERE AA.POLLING_PROPOSAL_ID = " + id + " AND AA.POLLING_VERSION = 2 ORDER BY AA.POLLING_DETAIL_PASAL,AA.POLLING_DETAIL_OPTION ASC").ToList();
            var DokJp = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS BB WHERE BB.DOC_ID = (SELECT MAX(AA.DOC_ID) FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 16 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_NAME LIKE '%V2')").SingleOrDefault();
            var BA = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 24 AND AA.DOC_FOLDER_ID = 17 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DH = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 25 AND AA.DOC_FOLDER_ID = 17 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var NT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 26 AND AA.DOC_FOLDER_ID = 17 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            ViewData["DetailRatek"] = DetailRatek;
            ViewData["DataPolling"] = DataPolling;
            ViewData["DetailPolling"] = DetailPolling;
            ViewData["DaftarHadir"] = DH;
            ViewData["Berita"] = BA;
            ViewData["Notulen"] = NT;
            ViewData["IsKetua"] = ((IsKetua == "Ketua" || IsKetua == "Sekretariat") ? 1 : 0);
            ViewData["Dokumen"] = Dokumen;
            ViewData["DokJp"] = DokJp;
            ViewData["RefLain"] = RefLain;

            return View();
        }
        [Auth(RoleTipe = 5)]
        public ActionResult Pengesahan(int id = 0)
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

            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 23 AND AA.DOC_FOLDER_ID = 17 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER) AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 9").SingleOrDefault();
            var DetailRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 9 AND PROPOSAL_RAPAT_VERSION = " + VERSION_RATEK).SingleOrDefault();
            var DataPolling = db.Database.SqlQuery<VIEW_POLLING_SINGLE>("SELECT AA.* FROM VIEW_POLLING_SINGLE AA WHERE AA.POLLING_PROPOSAL_ID = " + id + " AND AA.POLLING_VERSION = 2 AND AA.POLLING_TYPE = 7").SingleOrDefault();
            var DetailPolling = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT AA.* FROM VIEW_POLLING_DETAIL AA WHERE AA.POLLING_PROPOSAL_ID = " + id + " AND AA.POLLING_VERSION = 2 ORDER BY AA.POLLING_DETAIL_PASAL,AA.POLLING_DETAIL_OPTION ASC").ToList();
            var DokJp = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS BB WHERE BB.DOC_ID = (SELECT MAX(AA.DOC_ID) FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 16 AND AA.DOC_FOLDER_ID = 15 AND AA.DOC_STATUS = 1 AND AA.DOC_NAME LIKE '%V2')").SingleOrDefault();
            var BA = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 24 AND AA.DOC_FOLDER_ID = 17 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DH = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 25 AND AA.DOC_FOLDER_ID = 17 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var NT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 26 AND AA.DOC_FOLDER_ID = 17 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            ViewData["DefaultDokumen"] = DefaultDokumen;
            ViewData["DetailRatek"] = DetailRatek;
            ViewData["DataPolling"] = DataPolling;
            ViewData["DetailPolling"] = DetailPolling;
            ViewData["DaftarHadir"] = DH;
            ViewData["Berita"] = BA;
            ViewData["Notulen"] = NT;
            ViewData["Dokumen"] = Dokumen;
            ViewData["DokJp"] = DokJp;
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
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 9").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,9," + APPROVAL_STATUS_SESSION + ")");
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 7, PROPOSAL_STATUS_PROSES = 2, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 7, PROPOSAL_STATUS_PROSES = 2, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 9").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0,'" + APPROVAL_REASON + "'," + DATENOW + "," + USER_ID + ",1,9," + APPROVAL_STATUS_SESSION + ")");
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
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT"));
                    string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                    Stream stremdokumen = DATA_RSNI.InputStream;
                    byte[] appData = new byte[DATA_RSNI.ContentLength + 1];
                    stremdokumen.Read(appData, 0, DATA_RSNI.ContentLength);
                    string Extension = Path.GetExtension(DATA_RSNI.FileName);
                    if (Extension.ToLower() == ".docx" || Extension.ToLower() == ".doc")
                    {

                        Aspose.Words.Document doc = new Aspose.Words.Document(stremdokumen);
                        var SNI_DOC_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0)+1 AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.SNI_DOC_TYPE = 5").SingleOrDefault();
                        string filePathdoc = path + "DRAFT_RSNI6_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX";
                        string filePathpdf = path + "DRAFT_RSNI6_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".PDF";
                        string filePathxml = path + "DRAFT_RSNI6_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".XML";
                        doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                        doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                        doc.Save(@"" + filePathxml);
                        doc.Save(@"" + path + "DRAFT_RSNI6_" + PROPOSAL_PNPS_CODE_FIXER + ".DOCX", Aspose.Words.SaveFormat.Docx);

                        var DATENOW = MixHelper.ConvertDateNow();
                        var LOGCODE_SNI_DOC = MixHelper.GetLogCode();
                        int LASTID_SNI_DOC = MixHelper.GetSequence("TRX_SNI_DOC");

                        var fname = "SNI_DOC_ID,SNI_DOC_PROPOSAL_ID,SNI_DOC_TYPE,SNI_DOC_FILE_PATH,SNI_DOC_VERSION,SNI_DOC_CREATE_BY,SNI_DOC_CREATE_DATE,SNI_DOC_LOG_CODE,SNI_DOC_STATUS";
                        var fvalue = "'" + LASTID_SNI_DOC + "', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'6', " +
                                        "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI6/DRAFT/DRAFT_RSNI6_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX" + "', " +
                                        "'" + SNI_DOC_VERSION + "', " +
                                        "'" + USER_ID + "', " +
                                        DATENOW + "," +
                                        "'" + LOGCODE_SNI_DOC + "', " +
                                        "'1'";
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_SNI_DOC (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");


                        var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 23 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 17 AND DOC_STATUS = 1").SingleOrDefault();
                        if (CEKDOKUMEN == null)
                        {
                            int LASTID = MixHelper.GetSequence("TRX_DOCUMENTS");
                            var LOGCODE_RSNI6 = MixHelper.GetLogCode();
                            var FNAME_RSNI6 = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                            var FVALUE_RSNI6 = "'" + LASTID + "', " +
                                        "'17', " +
                                        "'23', " +
                                        "'" + PROPOSAL_ID + "', " +
                                        "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Draft RSNI 6" + "', " +
                                        "'Draft Rancangan SNI 5 " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                        "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/" + "', " +
                                        "'" + "DRAFT_RSNI6_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                                        "'" + Extension.ToUpper().Replace(".", "") + "', " +
                                        "'0', " +
                                        "'" + USER_ID + "', " +
                                        DATENOW + "," +
                                        "'1', " +
                                        "'" + LOGCODE_RSNI6 + "'";
                            db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_RSNI6 + ") VALUES (" + FVALUE_RSNI6.Replace("''", "NULL") + ")");
                            String objekTanggapan = FVALUE_RSNI6.Replace("'", "-");
                            MixHelper.InsertLog(LOGCODE_RSNI6, objekTanggapan, 1);
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

            Stream STREAM_NOTULEN = NOTULEN.InputStream;
            byte[] APPDATA_NOTULEN = new byte[NOTULEN.ContentLength + 1];
            STREAM_NOTULEN.Read(APPDATA_NOTULEN, 0, NOTULEN.ContentLength);
            string Extension_NOTULEN = Path.GetExtension(NOTULEN.FileName);

            int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 9").SingleOrDefault();
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
                                        "'9'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_RAPAT (" + FNAME_PROPOSAL_RAPAT + ") VALUES (" + FVALUE_PROPOSAL_RAPAT.Replace("''", "NULL") + ")");

            if (Extension_BA_RATEK.ToLower() == ".docx" || Extension_BA_RATEK.ToLower() == ".doc")
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/");
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
                            "'17', " +
                            "'24', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Ver " + VERSION_RATEK + "', " +
                            "'Berita Acara Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                BA_RATEK.SaveAs(path + "BERITA_ACARA_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + Extension_BA_RATEK.ToUpper());

                int LASTID_BA_RATEK = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_BA_RATEK = MixHelper.GetLogCode();
                var FNAME_BA_RATEK = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_BA_RATEK = "'" + LASTID_BA_RATEK + "', " +
                            "'17', " +
                            "'24', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Berita Acara Ver " + VERSION_RATEK + "', " +
                            "'Berita Acara Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/");
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
                            "'17', " +
                            "'25', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Ver " + VERSION_RATEK + "', " +
                            "'Daftar Hadir Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                DAFTAR_HADIR.SaveAs(path + "DAFTAR_HADIR_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + Extension_DAFTAR_HADIR.ToUpper());

                int LASTID_DAFTAR_HADIR = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DAFTAR_HADIR = MixHelper.GetLogCode();
                var FNAME_DAFTAR_HADIR = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DAFTAR_HADIR = "'" + LASTID_DAFTAR_HADIR + "', " +
                            "'17', " +
                            "'25', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Hadir Ver " + VERSION_RATEK + "', " +
                            "'Daftar Hadir Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_NOTULEN);
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
                            "'17', " +
                            "'26', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Ver " + VERSION_RATEK + "', " +
                            "'Notulen Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                NOTULEN.SaveAs(path + "BERITA_ACARA_RAPAT_TEKNIS_" + VERSION_RATEK + "_RSNI6_" + PROPOSAL_PNPS_CODE_FIXER + Extension_NOTULEN.ToUpper());

                int LASTID_NOTULEN = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_NOTULEN = MixHelper.GetLogCode();
                var FNAME_NOTULEN = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_NOTULEN = "'" + LASTID_NOTULEN + "', " +
                            "'17', " +
                            "'26', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Notulen Ver " + VERSION_RATEK + "', " +
                            "'Notulen Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
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
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/");

                Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DATA_RSNI);
                string filePathdoc = path + "RSNI6_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                string filePathpdf = path + "RSNI6_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                string filePathxml = path + "RSNI6_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxml);

                string pathSNIDOC = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                var SNI_DOC_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0)+1 AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.SNI_DOC_TYPE = 5").SingleOrDefault();
                string filePathdocSNI_DOC = pathSNIDOC + "DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX";
                string filePathpdfSNI_DOC = pathSNIDOC + "DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".PDF";
                string filePathxmlSNI_DOC = pathSNIDOC + "DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".XML";
                doc.Save(@"" + filePathdocSNI_DOC, Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + filePathpdfSNI_DOC, Aspose.Words.SaveFormat.Pdf);
                doc.Save(@"" + filePathxmlSNI_DOC);
                doc.Save(@"" + pathSNIDOC + "RSNI6_" + PROPOSAL_PNPS_CODE_FIXER + ".DOCX", Aspose.Words.SaveFormat.Docx);
                doc.Save(@"" + path + "RSNI6_" + PROPOSAL_PNPS_CODE_FIXER + ".DOCX", Aspose.Words.SaveFormat.Docx);

                var LOGCODE_SNI_DOC = MixHelper.GetLogCode();
                int LASTID_SNI_DOC = MixHelper.GetSequence("TRX_SNI_DOC");

                var fname = "SNI_DOC_ID,SNI_DOC_PROPOSAL_ID,SNI_DOC_TYPE,SNI_DOC_FILE_PATH,SNI_DOC_VERSION,SNI_DOC_CREATE_BY,SNI_DOC_CREATE_DATE,SNI_DOC_LOG_CODE,SNI_DOC_STATUS";
                var fvalue = "'" + LASTID_SNI_DOC + "', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'6', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI6/DRAFT/DRAFT_" + PROPOSAL_PNPS_CODE_FIXER + "_Ver" + SNI_DOC_VERSION + ".DOCX" + "', " +
                                "'" + SNI_DOC_VERSION + "', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'" + LOGCODE_SNI_DOC + "', " +
                                "'1'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_SNI_DOC (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

                var CEKDOKUMEN = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_TYPE = 23 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_FOLDER_ID = 17 AND DOC_STATUS = 1").SingleOrDefault();
                if (CEKDOKUMEN != null)
                {
                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET DOC_STATUS = '0' WHERE DOC_ID = " + CEKDOKUMEN.DOC_ID);
                }
                int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                            "'17', " +
                            "'23', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") RSNI 6 Ver " + VERSION_RATEK + "" + "', " +
                            "'RSNI 6 Ver " + VERSION_RATEK + " " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/RANCANGAN_SNI/RSNI6/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                            "'" + "RSNI6_Ver" + VERSION_RATEK + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
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
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 9, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 9, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);


                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 9").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,9," + APPROVAL_STATUS_SESSION + ")");
            }
            else
            {
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 9, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 9, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);


                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER)+1 AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 9").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0," + DATENOW + "," + USER_ID + ",1,9," + APPROVAL_STATUS_SESSION + ")");
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        public ActionResult DataRSNI6Komtek(DataTables param)
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

            string where_clause = "PROPOSAL_STATUS = 9 AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID;

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
                Convert.ToString("<center><a href='/Perumusan/RSNI6/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 9 && list.PROPOSAL_STATUS_PROSES == 0)?"<a href='/Perumusan/RSNI6/Create/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Susun RSNI 6'><i class='action fa fa-check'></i></a>":"")+"</center>"),
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DataRSNI6PPS(DataTables param)
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


            string where_clause = "PROPOSAL_STATUS = 9 AND PROPOSAL_STATUS_PROSES = 1  " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");

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
                Convert.ToString("<center><a href='/Perumusan/RSNI6/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 9 && list.PROPOSAL_STATUS_PROSES == 1)?"<a href='/Perumusan/RSNI6/Pengesahan/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Pengesahan RSNI 6'><i class='action fa fa-check'></i></a>":"")+"</center>"),
                
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
