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
using Aspose.Words.Tables;
using System.Drawing;
using Oracle.DataAccess.Client;

namespace SISPK.Controllers.Perumusan
{
    [Auth(RoleTipe = 1)]
    public class JajakPendapatController : Controller
    {
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            var IS_KOMTEK = Convert.ToInt32(Session["IS_KOMTEK"]);
            string ViewName = ((IS_KOMTEK == 1) ? "DaftarPenyusunanJP" : "DaftarPengesahanJP");
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

            var DetailPolling = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT AA.* FROM VIEW_POLLING_DETAIL AA WHERE AA.POLLING_DETAIL_POLLING_ID = " + DataProposal.PROPOSAL_POLLING_ID + " ORDER BY AA.POLLING_DETAIL_INPUT_TYPE ASC,AA.POLLING_DETAIL_PASAL,AA.POLLING_DETAIL_OPTION ASC,POLLING_DETAIL_CREATE_DATE DESC").ToList();
            //ViewData["DetailPolling"] = (from t in db.VIEW_POLLING_DETAIL where t.POLLING_DETAIL_POLLING_ID == DataProposal.PROPOSAL_POLLING_ID orderby t.POLLING_DETAIL_INPUT_TYPE ascending, t.POLLING_DETAIL_PASAL ascending, t.POLLING_DETAIL_OPTION ascending, t.POLLING_DETAIL_CREATE_DATE descending select t).ToList();
            ViewData["DetailPolling"] = DetailPolling;
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
                var DefaultDokumenRSNI1 = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 11 AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1 AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
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
        public ActionResult Approval(int id = 0)
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
            var DetailPolling = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT AA.* FROM VIEW_POLLING_DETAIL AA WHERE AA.POLLING_DETAIL_POLLING_ID = " + DataProposal.PROPOSAL_POLLING_ID + " ORDER BY AA.POLLING_DETAIL_INPUT_TYPE ASC,AA.POLLING_DETAIL_PASAL,AA.POLLING_DETAIL_OPTION ASC,POLLING_DETAIL_CREATE_DATE DESC").ToList();

            ViewData["DetailPolling"] = DetailPolling;
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
            var DefaultDokumen = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 38 AND DOC_FOLDER_ID = 25 AND DOC_STATUS = 1  AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
            if (DefaultDokumen == null)
            {
                var DefaultDokumenRSNI3 = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 11 AND DOC_FOLDER_ID = 14 AND DOC_STATUS = 1  AND ROWNUM = 1 ORDER BY DOC_ID DESC").SingleOrDefault();
                ViewData["DefaultDokumen"] = DefaultDokumenRSNI3;
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
        public ActionResult Approval(HttpPostedFileBase DATA_RSNI, int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, string PROPOSAL_PNPS_CODE = "", int APPROVAL_TYPE = 0, int APPROVAL_STATUS = 1, string APPROVAL_REASON = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();

            var Data = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var DataTanggapan = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT * FROM VIEW_POLLING_DETAIL WHERE POLLING_DETAIL_POLLING_ID = " + Data.POLLING_ID + " ORDER BY POLLING_DETAIL_INPUT_TYPE ASC,POLLING_DETAIL_PASAL ASC,POLLING_DETAIL_CREATE_DATE DESC").ToList();
            var PROPOSAL_PNPS_CODE_FIXER = Data.PROPOSAL_PNPS_CODE;
            var LAST_POLLING_VERSION = db.Database.SqlQuery<int>("SELECT NVL(CAST(MAX(POLLING_VERSION) AS INT),1) FROM TRX_POLLING WHERE POLLING_TYPE = 7 AND POLLING_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/JAJAK_PENDAPAT/" + PROPOSAL_PNPS_CODE_FIXER));
            string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/JAJAK_PENDAPAT/" + PROPOSAL_PNPS_CODE_FIXER + "/");

            string dataFormat = Server.MapPath("~/Format/Laporan/");
            Stream stream = System.IO.File.OpenRead(dataFormat + "FORMAT_TANGGAPAN_JAJAK_PENDAPAT.docx");
            Aspose.Words.Document Tanggapan = new Aspose.Words.Document(stream);
            ReplaceHelper helper = new ReplaceHelper(Tanggapan);
            helper.Replace("JudulPNPS", Data.PROPOSAL_JUDUL_PNPS);
            DateTime dt = Convert.ToDateTime(Data.POLLING_START_DATE);
            helper.Replace("TanggalJP", dt.ToString("dd/MM/yyyy"));
            helper.Replace("RuangLingkupPNPS", Data.PROPOSAL_RUANG_LINGKUP);


            Paragraph paragraph = new Paragraph(Tanggapan);

            DocumentBuilder builder = new DocumentBuilder(Tanggapan);
            Aspose.Words.Font font = builder.Font;
            font.Bold = false;
            font.Color = System.Drawing.Color.Black;
            font.Italic = false;
            font.Name = "Calibri";
            font.Size = 11;
            builder.MoveToDocumentEnd();

            var number = 0;
            string html = "<table width='100%' border='1' bordercolor='#111111' cellpadding='2'  style='border-collapse: collapse' cellpadding='0' cellspacing='0'>" +
                            "<tr>" +
                               "<td width='5%' style='text-align:center'><b>Tipe Pemberi Komentar</b></td>" +
                               "<td width='5%' style='text-align:center'><b>No Pasal/No Subpasal</b></td>" +
                               "<td width='15%' style='text-align:center'><b>Nama</b></td>" +
                               "<td width='10%' style='text-align:center'><b>Tipe Tanggapan</b></td>" +
                               "<td width='65%' style='text-align:center'><b>Tanggapan</b></td>" +
                               "</tr>";
            if (DataTanggapan != null)
            {
                foreach (var i in DataTanggapan)
                {
                    number++;
                    html += "<tr>" +
                        "<td style='text-align:center'>" + i.POLLING_DETAIL_INPUT_TYPE_NAME + "</td>" +
                        "<td style='text-align:center'>" + i.POLLING_DETAIL_PASAL + "</td>" +

                                   "<td>" + i.USER_PUBLIC_NAMA + "</td>" +
                                   "<td style='text-align:center'>" + i.PILIHAN + "</td>" +
                                   "<td>" + i.POLLING_DETAIL_REASON + "</td>" +
                                   "</tr>";
                }
            }
            else
            {
                html += "<tr>" +
                       "<td style='text-align:center'>-</td>" +
                       "<td style='text-align:center'>-</td>" +
                       "<td>-</td>" +
                       "<td style='text-align:center'>-</td>" +
                       "<td>-</td>" +
                       "</tr>";
            }
            html += "</table>";

            builder.InsertHtml(html);
            Tanggapan.Save(@"" + path + "DAFTAR_TANGGAPAN_JAJAK_PENDAPAT_KE_" + LAST_POLLING_VERSION + "_" + PROPOSAL_PNPS_CODE_FIXER + ".docx", Aspose.Words.SaveFormat.Docx);
            Tanggapan.Save(@"" + path + "DAFTAR_TANGGAPAN_JAJAK_PENDAPAT_KE_" + LAST_POLLING_VERSION + "_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf", Aspose.Words.SaveFormat.Pdf);
            Tanggapan.Save(@"" + path + "DAFTAR_TANGGAPAN_JAJAK_PENDAPAT_KE_" + LAST_POLLING_VERSION + "_" + PROPOSAL_PNPS_CODE_FIXER + ".xml");

            int LASTID_DOKUMEN = MixHelper.GetSequence("TRX_DOCUMENTS");
            var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
            var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
            var FVALUE_TANGGAPAN_MTPS = "'" + LASTID_DOKUMEN + "', " +
                        "'25', " +
                        "'41', " +
                        "'" + PROPOSAL_ID + "', " +
                        "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Tanggapan Jajak Pendapat Ke " + LAST_POLLING_VERSION + "', " +
                        "'Daftar Tanggapan Jajak Pendapat Ke " + LAST_POLLING_VERSION + " dengan kode PNPS : " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                        "'" + "/Upload/Dokumen/RANCANGAN_SNI/JAJAK_PENDAPAT/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                        "'" + "DAFTAR_TANGGAPAN_JAJAK_PENDAPAT_KE_" + LAST_POLLING_VERSION + "_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                        "'DOCX', " +
                        "'0', " +
                        "'" + USER_ID + "', " +
                        DATENOW + "," +
                        "'1', " +
                        "'" + LOGCODE_TANGGAPAN_MTPS + "'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
            String objeks = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
            MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objeks, 1);

            
            if (APPROVAL_TYPE == 0)
            {
                //Perlu Rapat Koordinasi = Tidak Masuk Ke RASNI
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 7").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1,'" + APPROVAL_REASON + "'," + DATENOW + "," + USER_ID + ",1,7," + APPROVAL_STATUS_SESSION + ")");
            }
            else if (APPROVAL_TYPE == 1) 
            {
                //Perlu Rapat Koordinasi = Ya Masuk Ke RSNI4
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 12, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
                var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 12, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
                MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 7").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1,'" + APPROVAL_REASON + "'," + DATENOW + "," + USER_ID + ",1,7," + APPROVAL_STATUS_SESSION + ")");
            }
            //if (APPROVAL_TYPE == 1)
            //{
            //    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 8, PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
            //    var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            //    String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 8, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
            //    MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

            //    int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            //    var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 7").SingleOrDefault();
            //    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,7," + APPROVAL_STATUS_SESSION + ")");
            //    db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_JP_" + Convert.ToString((LAST_POLLING_VERSION + 1)) + " = " + DATENOW + ", MONITORING_HASIL_APP_JP_" + Convert.ToString((LAST_POLLING_VERSION + 1)) + " = 1 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            //}
            //else if (APPROVAL_TYPE == 0)
            //{
            //    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 7, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
            //    var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            //    String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 7, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
            //    MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

            //    int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            //    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
            //    var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 7").SingleOrDefault();
            //    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0,'" + APPROVAL_REASON + "'," + DATENOW + "," + USER_ID + ",1,7," + APPROVAL_STATUS_SESSION + ")");
            //    db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_JP_" + Convert.ToString((LAST_POLLING_VERSION + 1)) + " = " + DATENOW + ", MONITORING_HASIL_APP_JP_" + Convert.ToString((LAST_POLLING_VERSION + 1)) + " = 0 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);
            //}
            //else if (APPROVAL_TYPE == 2)
            //{
            //    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 9, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
            //    var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            //    String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 9, PROPOSAL_STATUS_PROSES = 0, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
            //    MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

            //    int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            //    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
            //    var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 7").SingleOrDefault();
            //    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1,'" + APPROVAL_REASON + "'," + DATENOW + "," + USER_ID + ",1,7," + APPROVAL_STATUS_SESSION + ")");
            //}
            //else if (APPROVAL_TYPE == 3)
            //{
            //    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 7, PROPOSAL_STATUS_PROSES = 2, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
            //    var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            //    String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 7, PROPOSAL_STATUS_PROSES = 2, PROPOSAL_IS_POLLING = NULL, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
            //    MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

            //    int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            //    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
            //    var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 7").SingleOrDefault();
            //    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",0,'" + APPROVAL_REASON + "'," + DATENOW + "," + USER_ID + ",1,7," + APPROVAL_STATUS_SESSION + ")");
            //}

            return RedirectToAction("Index");
        }
        [Auth(RoleTipe = 2)]
        public ActionResult Setting(int id = 0)
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            
            var SuratRapat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 14 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 35").FirstOrDefault();
            var Lampiran = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 30").FirstOrDefault();

            var Bukti = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 29").FirstOrDefault();
            var Surat = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 14 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();
            var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();
            var DataKomtek = (from komtek in db.MASTER_KOMITE_TEKNIS where komtek.KOMTEK_STATUS == 1 && komtek.KOMTEK_ID == DataProposal.KOMTEK_ID select komtek).SingleOrDefault();
            var IsKetua = db.Database.SqlQuery<string>("SELECT JABATAN FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + DataProposal.KOMTEK_ID + " AND USER_ID = " + USER_ID).SingleOrDefault();
            var LastNumber = db.Database.SqlQuery<string>("WITH AA AS(SELECT CAST(SUBSTR(regexp_replace(SNI_NOMOR, '[^0-9]', ''), 1, LENGTH(regexp_replace(SNI_NOMOR, '[^0-9]', '')) - 4) AS NUMBER) LAST_NOMOR FROM VIEW_SNI ) SELECT CAST(MAX(LAST_NOMOR)+1 AS VARCHAR(255)) LAST_NOMOR FROM AA").SingleOrDefault();
            ViewData["LastNumber"] = LastNumber;

            var SURAT_UNDANGAN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 35 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var SURAT_PENYERAHAN_RSNI = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 36 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var BERITA_ACARA_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 8 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var DAFTAR_HADIR_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 9 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var NOTULEN_RAPAT = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 10 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var LAPORAN_TAS = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 37 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            ViewData["SURAT_UNDANGAN_RAPAT"] = SURAT_UNDANGAN_RAPAT;
            ViewData["SURAT_PENYERAHAN_RSNI"] = SURAT_PENYERAHAN_RSNI;
            ViewData["BERITA_ACARA_RAPAT"] = BERITA_ACARA_RAPAT;
            ViewData["DAFTAR_HADIR_RAPAT"] = DAFTAR_HADIR_RAPAT;
            ViewData["NOTULEN_RAPAT"] = NOTULEN_RAPAT;
            ViewData["LAPORAN_TAS"] = NOTULEN_RAPAT;

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
        public ActionResult Setting(string PROPOSAL_ABSTRAK = "", int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0, string PROPOSAL_NO_SNI_PROPOSAL = "", string PROPOSAL_JUDUL_SNI_PROPOSAL = "", string POLLING_START_DATE = "", string POLLING_END_DATE = "")
        {
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DATENOW = MixHelper.ConvertDateNow();
            var LOGCODE_POLLING = MixHelper.GetLogCode();
            int LASTID_POLLING = MixHelper.GetSequence("TRX_POLLING");
            String POLLING_START_DATE_CONVERT = "TO_DATE('" + POLLING_START_DATE + "', 'yyyy-mm-dd hh24:mi:ss')";
            String POLLING_END_DATE_CONVERT = "TO_DATE('" + POLLING_END_DATE + "', 'yyyy-mm-dd hh24:mi:ss')";

            var LAST_POLLING_VERSION = db.Database.SqlQuery<int>("SELECT NVL(CAST(MAX(POLLING_VERSION) AS INT),0) FROM TRX_POLLING WHERE POLLING_TYPE = 7 AND POLLING_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            var POLLING_IS_EXIST = db.Database.SqlQuery<TRX_POLLING>("SELECT * FROM TRX_POLLING WHERE POLLING_VERSION = " + (LAST_POLLING_VERSION + 1) + " AND POLLING_TYPE = 7 AND POLLING_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            if (POLLING_IS_EXIST == null)
            {
                var fname = "POLLING_ID,POLLING_PROPOSAL_ID,POLLING_TYPE,POLLING_START_DATE,POLLING_END_DATE,POLLING_VERSION,POLLING_CREATE_BY,POLLING_CREATE_DATE,POLLING_STATUS,POLLING_LOGCODE";
                var fvalue = "'" + LASTID_POLLING + "', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + 7 + "', " +
                                POLLING_START_DATE_CONVERT + ", " +
                                POLLING_END_DATE_CONVERT + ", " +
                                "'" + (LAST_POLLING_VERSION + 1) + "', " +
                                "'" + USER_ID + "'," +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_POLLING + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_POLLING (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");
                String objek2 = fvalue.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_POLLING, objek2, 1);
            }
            db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_AWAL_JP_" + Convert.ToString((LAST_POLLING_VERSION + 1)) + " = " + POLLING_START_DATE_CONVERT + ", MONITORING_TGL_AKHIR_JP_" + Convert.ToString((LAST_POLLING_VERSION + 1)) + " = " + POLLING_START_DATE_CONVERT + " WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);

            var NEW_LASTID_POLLING = db.Database.SqlQuery<int>("SELECT POLLING_ID FROM TRX_POLLING WHERE POLLING_PROPOSAL_ID =" + PROPOSAL_ID + " AND POLLING_VERSION = " + (LAST_POLLING_VERSION + 1) + " AND POLLING_TYPE = 7").SingleOrDefault();
            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS_PROSES = 1,PROPOSAL_ABSTRAK = '" + PROPOSAL_ABSTRAK + "', PROPOSAL_IS_POLLING = 1, PROPOSAL_NO_SNI_PROPOSAL = '" + PROPOSAL_NO_SNI_PROPOSAL + "', PROPOSAL_JUDUL_SNI_PROPOSAL = '" + PROPOSAL_JUDUL_SNI_PROPOSAL + "',PROPOSAL_JUDUL_PNPS = '" + PROPOSAL_JUDUL_SNI_PROPOSAL + "', PROPOSAL_POLLING_ID = " + NEW_LASTID_POLLING + ", PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
            var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = 1, PROPOSAL_NO_SNI_PROPOSAL = '" + PROPOSAL_NO_SNI_PROPOSAL + "', PROPOSAL_JUDUL_SNI_PROPOSAL = '" + PROPOSAL_JUDUL_SNI_PROPOSAL + "', PROPOSAL_POLLING_ID = " + NEW_LASTID_POLLING + ", PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
            MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

            var Data = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = Data.PROPOSAL_PNPS_CODE;

            HttpPostedFileBase FILE_DATA_RSNI = Request.Files["DATA_RSNI"];
            if (FILE_DATA_RSNI.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/JAJAK_PENDAPAT/" + PROPOSAL_PNPS_CODE_FIXER));
                string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/JAJAK_PENDAPAT/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                string PATH_SNI_DOC = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/JAJAK_PENDAPAT/" + PROPOSAL_PNPS_CODE_FIXER + "/DRAFT/");
                Stream STREAM_DOC_DATA_RSNI = FILE_DATA_RSNI.InputStream;

                string EXT_DATA_RSNI = Path.GetExtension(FILE_DATA_RSNI.FileName);
                if (EXT_DATA_RSNI.ToLower() == ".docx" || EXT_DATA_RSNI.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_DATA_RSNI);
                    string filePathdoc = path + "RSNI3_DOK_JAJAK_PENDAPAT_" + PROPOSAL_PNPS_CODE_FIXER + ".docx";
                    string filePathpdf = path + "RSNI3_DOK_JAJAK_PENDAPAT_" + PROPOSAL_PNPS_CODE_FIXER + ".pdf";
                    string filePathxml = path + "RSNI3_DOK_JAJAK_PENDAPAT_" + PROPOSAL_PNPS_CODE_FIXER + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);

                    int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                    var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                                "'25', " +
                                "'38', " +
                                "'" + PROPOSAL_ID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") RSNI 3 DOK JAJAK PENDAPAT', " +
                                "'RSNI 3 DOK JAJAK PENDAPAT " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/RANCANGAN_SNI/JAJAK_PENDAPAT/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'" + "RSNI3_DOK_JAJAK_PENDAPAT_" + PROPOSAL_PNPS_CODE_FIXER + "', " +
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

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult SetPolling(string POLLING_START_DATE = "", string POLLING_END_DATE = "", int PROPOSAL_ID = 0, int PROPOSAL_KOMTEK_ID = 0)
        {
            var UserId = Convert.ToInt32(Session["USER_ID"]);
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("TRX_POLLING");
            var datenow = MixHelper.ConvertDateNow();

            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_IS_POLLING = 1 WHERE PROPOSAL_ID = " + PROPOSAL_ID);
            var POLLING_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(POLLING_VERSION),0) AS NUMBER) AS SNI_DOC_VERSION FROM TRX_POLLING WHERE POLLING_TYPE = 3 AND POLLING_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            String POLLING_START_DATE_FIX = "TO_DATE('" + POLLING_START_DATE + "', 'yyyy-mm-dd hh24:mi:ss')";
            String POLLING_END_DATE_FIX = "TO_DATE('" + POLLING_END_DATE + "', 'yyyy-mm-dd hh24:mi:ss')";
            var fname = "POLLING_ID,POLLING_PROPOSAL_ID,POLLING_TYPE,POLLING_START_DATE,POLLING_END_DATE,POLLING_VERSION,POLLING_IS_KUORUM,POLLING_JML_PARTISIPAN,POLLING_CREATE_BY,POLLING_CREATE_DATE,POLLING_STATUS,POLLING_LOGCODE";
            var fvalue = "'" + lastid + "', " +
                            "'" + PROPOSAL_ID + "', " +
                            "'" + 4 + "', " +
                            POLLING_START_DATE_FIX + "," +
                            POLLING_END_DATE_FIX + "," +
                            "'" + ((POLLING_VERSION == 0) ? 1 : POLLING_VERSION) + "', " +
                            "'" + 0 + "', " +
                            "'" + 0 + "', " +
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
        public ActionResult DataJajakPendapatPPS(DataTables param)
        {
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;
            var BIDANG_ID = Convert.ToInt32(Session["BIDANG_ID"]);

            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("PROPOSAL_PNPS_CODE");
            order_field.Add("PROPOSAL_FULL_DATE_NAME");
            order_field.Add("POLLING_MONITORING_NAME");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("POLLING_JML_PARTISIPAN");
            order_field.Add("PROPOSAL_TAHAPAN");
            order_field.Add("PROPOSAL_STATUS_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "PROPOSAL_STATUS = 7 " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");

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
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString("<center>"+list.PROPOSAL_FULL_DATE_NAME+"</center>"),
                Convert.ToString(list.POLLING_MONITORING_NAME),
                Convert.ToString("<center>"+list.POLLING_JML_PARTISIPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                Convert.ToString("<center>"+((list.PROPOSAL_IS_POLLING == 0 || list.PROPOSAL_IS_POLLING == null && list.PROPOSAL_STATUS_PROSES != 2)?"<a href='/Perumusan/JajakPendapat/Setting/"+list.PROPOSAL_ID+"' class='btn yellow btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Setting Jajak Pendapat'><i class='action fa fa-bar-chart-o'></i></a>":"<a href='/Perumusan/JajakPendapat/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>")+((list.PROPOSAL_IS_POLLING == 1)?"<a href='/Perumusan/JajakPendapat/Approval/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Persetujuan Jajak Pendapat'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DataJajakPendapatKomtek(DataTables param)
        {
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;
            var USER_KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("PROPOSAL_PNPS_CODE");
            order_field.Add("PROPOSAL_FULL_DATE_NAME");
            order_field.Add("POLLING_MONITORING_NAME");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("POLLING_JML_PARTISIPAN");
            order_field.Add("PROPOSAL_TAHAPAN");
            order_field.Add("PROPOSAL_STATUS_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "PROPOSAL_STATUS = 7 AND PROPOSAL_IS_POLLING = 1 AND PROPOSAL_KOMTEK_ID = " + USER_KOMTEK_ID;

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
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString("<center>"+list.PROPOSAL_FULL_DATE_NAME+"</center>"),
                Convert.ToString(list.POLLING_MONITORING_NAME),
                Convert.ToString("<center>"+list.POLLING_JML_PARTISIPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                Convert.ToString((list.POLLING_MONITORING_TYPE == "Sedang Berlangsung")?"<center><a href='/Perumusan/JajakPendapat/Comment/"+list.PROPOSAL_ID+"' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Buat Komentar'><i class='action fa fa-comments-o'></i></a><a href='/Perumusan/JajakPendapat/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>":"<center><a href='/Perumusan/JajakPendapat/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Comment(int id = 0)
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
            var LTAS = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + id + " AND AA.DOC_RELATED_TYPE = 37 AND AA.DOC_FOLDER_ID = 14 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();
            var VERSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.PROPOSAL_RAPAT_VERSION),0) AS NUMBER)+1 AS PROPOSAL_RAPAT_VERSION FROM TRX_PROPOSAL_RAPAT T1 WHERE T1.PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND T1.PROPOSAL_RAPAT_PROPOSAL_STATUS = 6").SingleOrDefault();
            var DetailRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS = 6 AND PROPOSAL_RAPAT_VERSION = " + (VERSION_RATEK - 1)).FirstOrDefault();
            var DetailHistoryRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + id + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS IN (5,6) ").ToList();
            var DetailPolling = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT AA.* FROM VIEW_POLLING_DETAIL AA WHERE AA.POLLING_DETAIL_POLLING_ID = " + DataProposal.PROPOSAL_POLLING_ID + " ORDER BY AA.POLLING_DETAIL_INPUT_TYPE ASC,AA.POLLING_DETAIL_PASAL,AA.POLLING_DETAIL_OPTION ASC,POLLING_DETAIL_CREATE_DATE DESC").ToList();

            ViewData["DetailPolling"] = DetailPolling;
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
            ViewData["LTAS"] = LTAS;

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

        [HttpPost, ValidateInput(false)]
        public ActionResult Comment(TRX_POLLING_DETAILS input, VIEW_POLLING VP, FormCollection form)
        {
            var GetIP = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 12").FirstOrDefault();
            var GetUser = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 13").FirstOrDefault();
            var GetPassword = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 14").FirstOrDefault();
            var GetPath = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 15").FirstOrDefault();
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");

            string path = "";
            string filePathpdf = "";

            HttpPostedFileBase file4 = Request.Files["POLLING_FILE"];
            if (file4.ContentLength > 0)
            {
                Directory.CreateDirectory(GetPath.CONFIG_VALUE + "/Upload/DokPolling");
                path = GetPath.CONFIG_VALUE + "/Upload/DokPolling/";
                Stream stremdokumen = file4.InputStream;
                byte[] appData = new byte[file4.ContentLength + 1];
                stremdokumen.Read(appData, 0, file4.ContentLength);
                string Extension = Path.GetExtension(file4.FileName);
                if (Extension.ToLower() == ".pdf")
                {
                    Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                    //Aspose.Words.Document docx = new Aspose.Words.Document(stremdokumen);
                    filePathpdf = path + "POLLING_" + VP.PROPOSAL_ID + "_" + TGL_SEKARANG + ".pdf";
                    pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                }
            }

            using (OracleConnection con = new OracleConnection("Data Source=" + GetIP.CONFIG_VALUE + ";User ID=" + GetUser.CONFIG_VALUE + ";PASSWORD=" + GetPassword.CONFIG_VALUE + ";"))
            {
                con.Open();

                using (OracleCommand cmd = new OracleCommand())
                {
                    var pathnya = "/Upload/DokPolling/POLLING_" + VP.PROPOSAL_ID + "_" + TGL_SEKARANG + ".pdf";

                    var UserId = Session["USER_ID"];
                    var logcode = MixHelper.GetLogCode();
                    int lastid = MixHelper.GetSequence("TRX_POLLING_DETAILS");
                    var datenow = MixHelper.ConvertDateNow();
                    var year_now = DateTime.Now.Year;
                    var fname = "POLLING_DETAIL_ID,POLLING_DETAIL_POLLING_ID,POLLING_DETAIL_OPTION,POLLING_DETAIL_REASON,POLLING_DETAIL_PASAL,POLLING_DETAIL_CREATE_BY,POLLING_DETAIL_CREATE_DATE,POLLING_DETAIL_STATUS,POLLING_DETAIL_FILE_PATH,POLLING_DETAIL_INPUT_TYPE";


                    cmd.Connection = con;
                    cmd.CommandType = System.Data.CommandType.Text;

                    cmd.CommandText = " INSERT INTO TRX_POLLING_DETAILS (" + fname + ") VALUES ('" + lastid + "','" + input.POLLING_DETAIL_POLLING_ID + "','" + input.POLLING_DETAIL_OPTION + "',:parameter,'" + input.POLLING_DETAIL_PASAL + "'," + UserId + "," + datenow + ",1,'" + pathnya + "',1) ";

                    OracleParameter oracleParameterClob = new OracleParameter();
                    oracleParameterClob.OracleDbType = OracleDbType.Clob;
                    //1 million long string
                    oracleParameterClob.Value = input.POLLING_DETAIL_REASON;


                    cmd.Parameters.Add(oracleParameterClob);

                    cmd.ExecuteNonQuery();
                    db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING TP SET TP.POLLING_JML_PARTISIPAN = (TP.POLLING_JML_PARTISIPAN + 1) WHERE TP.POLLING_ID =" + input.POLLING_DETAIL_POLLING_ID);

                    TempData["Notifikasi"] = 1;
                    TempData["NotifikasiText"] = "Terima kasih, pendapat anda berhasil di simpan.";

                }

                con.Close();

                var proposal_id = Convert.ToInt32(form["PROPOSAL_ID"]);
                return RedirectToAction("Comment/" + proposal_id);
            }
        }

        public ActionResult cekdata(TRX_POLLING_DETAILS form)
        {
            int status = 0;
            var UserId = Session["USER_ID"];
            status = db.Database.SqlQuery<int>("SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = " + form.POLLING_DETAIL_POLLING_ID + " AND POLLING_DETAIL_PASAL = " + form.POLLING_DETAIL_PASAL + " AND POLLING_DETAIL_OPTION = " + form.POLLING_DETAIL_OPTION + " AND POLLING_DETAIL_CREATE_BY =" + UserId).SingleOrDefault();
            return Json(new { status = status, query = "SELECT COUNT(POLLING_DETAIL_ID) AS JML_POLL FROM TRX_POLLING_DETAILS WHERE POLLING_DETAIL_POLLING_ID = " + form.POLLING_DETAIL_POLLING_ID + " AND POLLING_DETAIL_PASAL = " + form.POLLING_DETAIL_PASAL + " AND POLLING_DETAIL_OPTION = " + form.POLLING_DETAIL_OPTION + " AND POLLING_DETAIL_CREATE_BY =" + UserId }, JsonRequestBehavior.AllowGet);
        }
    }
}
