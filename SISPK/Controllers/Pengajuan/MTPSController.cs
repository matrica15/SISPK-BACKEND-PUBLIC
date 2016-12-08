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
using Aspose.Pdf;
using Aspose.Cells;
using Aspose.Words.Lists;
using System.Data;
using Oracle.DataAccess.Client;

namespace SISPK.Controllers.Pengajuan
{
    [Auth(RoleTipe = 1)]
    public class MTPSController : Controller
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
            var DataTanggapan = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT * FROM VIEW_POLLING_DETAIL WHERE POLLING_DETAIL_POLLING_ID = " + DataProposal.PROPOSAL_POLLING_ID + " ORDER BY POLLING_DETAIL_INPUT_TYPE ASC,POLLING_DETAIL_CREATE_DATE DESC").ToList();
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
            ViewData["DetailPolling"] = DataTanggapan;
            ViewData["DataTanggapan"] = DataTanggapan;
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
            var DataTanggapan = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT * FROM VIEW_POLLING_DETAIL WHERE POLLING_DETAIL_POLLING_ID = " + DataProposal.PROPOSAL_POLLING_ID + " ORDER BY POLLING_DETAIL_INPUT_TYPE ASC,POLLING_DETAIL_CREATE_DATE DESC").ToList();
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
            ViewData["DetailPolling"] = DataTanggapan;
            ViewData["DataTanggapan"] = DataTanggapan;
            return View();
        }
        [HttpPost]
        public ActionResult Approval(int PROPOSAL_ID = 0, string PROPOSAL_PNPS_CODEZZ = "", int PROPOSAL_KOMTEK_ID = 0, int[] PROPOSAL_ICS_REF_ICS_ID = null)
        {
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE = db.Database.SqlQuery<string>("SELECT CAST(TO_CHAR (SYSDATE, 'YYYY') || '.' || KOMTEK_CODE || '.' || ( SELECT CAST ( ( CASE WHEN LENGTH (COUNT(PROPOSAL_ID) + 1) = 1 THEN '0' || CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) ELSE CAST ( COUNT (PROPOSAL_ID) + 1 AS VARCHAR2 (255) ) END ) AS VARCHAR2 (255) ) PNPSCODE FROM TRX_PROPOSAL WHERE PROPOSAL_KOMTEK_ID = KOMTEK_ID ) AS VARCHAR2(255)) AS PNPSCODE FROM MASTER_KOMITE_TEKNIS WHERE KOMTEK_ID = " + PROPOSAL_KOMTEK_ID).SingleOrDefault();
            
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_CODE;
            var DATENOW = MixHelper.ConvertDateNow();

            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 3, PROPOSAL_STATUS_PROSES = 1,PROPOSAL_KOMTEK_ID = " + PROPOSAL_KOMTEK_ID + ", PROPOSAL_IS_POLLING = 0, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID);
            var PROPOSAL_LOG_CODE = db.Database.SqlQuery<string>("SELECT PROPOSAL_LOG_CODE FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            String objek1 = "UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 3,PROPOSAL_KOMTEK_ID = " + PROPOSAL_KOMTEK_ID + ", PROPOSAL_STATUS_PROSES = 1, PROPOSAL_IS_POLLING = 0, PROPOSAL_POLLING_ID = NULL, PROPOSAL_UPDATE_DATE = " + DATENOW + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PROPOSAL_ID;
            MixHelper.InsertLog(PROPOSAL_LOG_CODE, objek1.Replace("'", "-"), 2);

            int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID);
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PROPOSAL_ID + ",1," + DATENOW + "," + USER_ID + ",1,2,1)");

            db.Database.ExecuteSqlCommand("UPDATE TRX_MONITORING SET MONITORING_TGL_APP_MTPS = " + DATENOW + ", MONITORING_HASIL_APP_MTPS = 1 WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID);

            int LASTID = MixHelper.GetSequence("TRX_DOCUMENTS");
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
            var DataTanggapan = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT * FROM VIEW_POLLING_DETAIL WHERE POLLING_DETAIL_POLLING_ID = " + DataProposal.PROPOSAL_POLLING_ID + " ORDER BY POLLING_DETAIL_INPUT_TYPE ASC,POLLING_DETAIL_CREATE_DATE DESC").ToList();
            string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/");
            string dataFormat = Server.MapPath("~/Format/Laporan/");
            Stream stream = System.IO.File.OpenRead(dataFormat + "FORMAT_TANGGAPAN_PUBLIKASI_MTPS.docx");
            Aspose.Words.Document Tanggapan = new Aspose.Words.Document(stream);
            ReplaceHelper helper = new ReplaceHelper(Tanggapan);
            helper.Replace("JudulPNPS", DataProposal.PROPOSAL_JUDUL_PNPS);
            DateTime dt = Convert.ToDateTime(DataProposal.POLLING_START_DATE);
            helper.Replace("TanggalJP", dt.ToString("dd/MM/yyyy"));
            helper.Replace("RuangLingkupPNPS", DataProposal.PROPOSAL_RUANG_LINGKUP);


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
                               "<td width='5%' style='text-align:center'><b>No</b></td>" +
                               "<td width='15%' style='text-align:center'><b>Nama</b></td>" +
                               "<td width='15%' style='text-align:center'><b>Tipe tanggapan</b></td>" +
                               "<td width='65%' style='text-align:center'><b>Tanggapan</b></td>" +
                               "</tr>"; 

            foreach (var i in DataTanggapan)
            {
                number++;
                html += "<tr>" +
                    "<td>" + number + "</td>" +
                               "<td>" + i.USER_PUBLIC_NAMA+ "</td>" +
                               "<td>" + i.POLLING_DETAIL_INPUT_TYPE_NAME + "</td>" +
                               "<td>" + i.POLLING_DETAIL_REASON + "</td>" +
                               "</tr>"; 
            }
            html += "</table>";
            builder.InsertHtml(html);
            Tanggapan.Save(@"" + path + "DAFTAR_TANGGAPAN_PUBLIKASI_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".docx", Aspose.Words.SaveFormat.Docx);
            Tanggapan.Save(@"" + path + "DAFTAR_TANGGAPAN_PUBLIKASI_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf", Aspose.Words.SaveFormat.Pdf);
            Tanggapan.Save(@"" + path + "DAFTAR_TANGGAPAN_PUBLIKASI_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml");

            var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
            var FNAME_TANGGAPAN_MTPS = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
            var FVALUE_TANGGAPAN_MTPS = "'" + LASTID + "', " +
                        "'10', " +
                        "'1', " +
                        "'" + PROPOSAL_ID + "', " +
                        "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") Daftar Tanggapan Publikasi Usulan PNPS" + "', " +
                        "'Daftar Tanggapan Hasil Publikasi Usulan PNPS dengan Judul PNPS : " + DataProposal.PROPOSAL_JUDUL_PNPS + "', " +
                        "'" + "/Upload/Dokumen/RANCANGAN_SNI/MTPS/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                        "'" + "DAFTAR_TANGGAPAN_PUBLIKASI_USULAN_PNPS_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "" + "', " +
                        "'DOCX', " +
                        "'0', " +
                        "'" + USER_ID + "', " +
                        DATENOW + "," +
                        "'1', " +
                        "'" + LOGCODE_TANGGAPAN_MTPS + "'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_TANGGAPAN_MTPS + ") VALUES (" + FVALUE_TANGGAPAN_MTPS.Replace("''", "NULL") + ")");
            String objekTanggapan = FVALUE_TANGGAPAN_MTPS.Replace("'", "-");
            MixHelper.InsertLog(LOGCODE_TANGGAPAN_MTPS, objekTanggapan, 1);

            db.Database.ExecuteSqlCommand("UPDATE TRX_POLLING SET POLLING_IS_KUORUM = 1,POLLING_UPDATE_DATE = " + DATENOW + ", POLLING_UPDATE_BY = '" + USER_ID + "' WHERE POLLING_ID = " + DataProposal.PROPOSAL_POLLING_ID);
            
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }
        public ActionResult DataMTPS(DataTables param, int id = 0)
        {
            var STATUS_IS_DETERIMA = id;
            var default_order = "PROPOSAL_CREATE_DATE";
            var limit = 10;
            var BIDANG_ID = Convert.ToInt32(Session["BIDANG_ID"]);
            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_CREATE_DATE");
            order_field.Add("KOMTEK_CODE");
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


            string where_clause = "PROPOSAL_STATUS = 2 AND PROPOSAL_STATUS_PROSES = 1" + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");
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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString("<center>"+list.PROPOSAL_CREATE_DATE_NAME+"</center>"),
                Convert.ToString(list.KOMTEK_CODE+" "+list.KOMTEK_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
                Convert.ToString("<center>"+list.PROPOSAL_FULL_DATE_NAME+"</center>"),
                Convert.ToString(list.POLLING_MONITORING_NAME),
                Convert.ToString("<center>"+list.POLLING_JML_PARTISIPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                Convert.ToString((Convert.ToInt32(Session["IS_KOMTEK"]) == 0)?"<center><a href='/Pengajuan/MTPS/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS == 2)?"<a href='/Pengajuan/MTPS/Approval/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Persetujuan Publikasi Usulan PNPS'><i class='action fa fa-check'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>":"<center><a href='/Pengajuan/MTPS/Comment/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Buat Komentar'><i class='action fa fa-comments-o'></i></a></center>"),
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
            var DataTanggapan = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT * FROM VIEW_POLLING_DETAIL WHERE POLLING_DETAIL_POLLING_ID = " + DataProposal.PROPOSAL_POLLING_ID + " ORDER BY POLLING_DETAIL_INPUT_TYPE ASC,POLLING_DETAIL_CREATE_DATE DESC").ToList();
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
            ViewData["DetailPolling"] = DataTanggapan;
            ViewData["DataTanggapan"] = DataTanggapan;
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
                    var fname = "POLLING_DETAIL_ID,POLLING_DETAIL_POLLING_ID,POLLING_DETAIL_REASON,POLLING_DETAIL_CREATE_BY,POLLING_DETAIL_CREATE_DATE,POLLING_DETAIL_STATUS,POLLING_DETAIL_FILE_PATH,POLLING_DETAIL_INPUT_TYPE";
                  

                    cmd.Connection = con;
                    cmd.CommandType = System.Data.CommandType.Text;

                    cmd.CommandText = " INSERT INTO TRX_POLLING_DETAILS (" + fname + ") VALUES ('" + lastid + "','" + input.POLLING_DETAIL_POLLING_ID + "',:parameter," + UserId + "," + datenow + ",1,'" + pathnya + "',1) ";

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
        [HttpPost]
        public ActionResult FindICS(int KOMTEK_ID = 0)
        {
            var ics = db.Database.SqlQuery<VIEW_KOMTEK_ICS>("SELECT * FROM VIEW_KOMTEK_ICS WHERE KOMTEK_ICS_KOMTEK_ID = " + KOMTEK_ID).ToList();
            return Json(new { ics }, JsonRequestBehavior.AllowGet);

        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult FindKomtek(int KOMTEK_ID = 0)
        {
            var DATAKOMTEK = db.Database.SqlQuery<VIEW_KOMTEK_FULL>("SELECT * FROM VIEW_KOMTEK_FULL WHERE KOMTEK_ID = " + KOMTEK_ID).SingleOrDefault();
            var DATAICS = db.Database.SqlQuery<VIEW_KOMTEK_ICS>("SELECT * FROM VIEW_KOMTEK_ICS WHERE KOMTEK_ICS_KOMTEK_ID = " + KOMTEK_ID + " ORDER BY ICS_CODE ASC").ToList();
            var DATAANGGOTA = db.Database.SqlQuery<VIEW_ANGGOTA>("SELECT * FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + KOMTEK_ID + " ORDER BY KOMTEK_ANGGOTA_JABATAN ASC").ToList();
            return Json(new { DATAKOMTEK, DATAICS, DATAANGGOTA }, JsonRequestBehavior.AllowGet);
        }
    }
}
