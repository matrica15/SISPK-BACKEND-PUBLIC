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

namespace SISPK.Controllers.Publikasi
{
    public class PublikasiController : Controller
    {
        //
        // GET: /Publikasi/
        private SISPKEntities db = new SISPKEntities();
        [Auth(RoleTipe = 1)]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult listSNI(DataTables param, int status = 0)
        {
            var default_order = "SNI_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_NOMOR");
            order_field.Add("SNI_JUDUL");
            order_field.Add("SNI_SK_NOMOR");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = " SNI_IS_PUBLISH = " + status;

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
                search_clause += " OR SNI_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SK_PER_SNI WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SK_PER_SNI " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SK_PER_SNI>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.SNI_NOMOR), 
                Convert.ToString(list.SNI_JUDUL),
                Convert.ToString(list.SNI_SK_NOMOR),
                Convert.ToString((list.SNI_IS_PUBLISH == 0)?"<center><a href='Publikasi/Detil/"+list.SNI_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-upload'></i>Publish</a></center>":"<center><a href='Publikasi/Detail/"+list.SNI_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a><a href='Publikasi/Republish/"+list.SNI_ID+"' class='btn yellow btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Jadikan Tidak Publish'><i class='action fa fa-history'></i></a></center>"),
            };
            return Json(new
            {
                SelectedData,
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detil(int id = 0)
        {
            var sni = (from a in db.VIEW_SNI where a.SNI_ID == id select a).SingleOrDefault();
            ViewData["sni"] = sni;
            var doc = (from b in db.TRX_DOCUMENTS where b.DOC_ID == sni.SNI_DOC_ID && b.DOC_FOLDER_ID == 8 && b.DOC_RELATED_TYPE == 100 select b).SingleOrDefault();
            ViewData["doc"] = doc;
            var listsni = (from a in db.TRX_SNI where a.SNI_SK_ID == id select a).ToList();
            ViewData["listsni"] = listsni;

            return View();
        }
        [HttpPost]
        public ActionResult Detil(TRX_SNI ts, TRX_PROPOSAL tp, int PROPOSAL_ID = 0)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("TRX_DOCUMENTS");
            var datenow = MixHelper.ConvertDateNow();

            var update_sni = "UPDATE TRX_SNI SET SNI_IS_PUBLISH = 1, SNI_NOMOR = '" + ts.SNI_NOMOR + "', SNI_JUDUL = '" + ts.SNI_JUDUL + "', SNI_JUDUL_ENG = '" + ts.SNI_JUDUL_ENG + "' WHERE SNI_ID = " + ts.SNI_ID;
            db.Database.ExecuteSqlCommand(update_sni);

            var update_PROPOSAL = "UPDATE TRX_PROPOSAL SET PROPOSAL_ABSTRAK = '" + tp.PROPOSAL_ABSTRAK + "', PROPOSAL_ABSTRAK_ENG = '" + tp.PROPOSAL_ABSTRAK_ENG + "', PROPOSAL_RUANG_LINGKUP = '" + tp.PROPOSAL_RUANG_LINGKUP + "', PROPOSAL_RUANG_LINGKUP_ENG = '" + tp.PROPOSAL_RUANG_LINGKUP_ENG + "' WHERE PROPOSAL_ID = " + ts.SNI_PROPOSAL_ID;
            db.Database.ExecuteSqlCommand(update_PROPOSAL);

            var updates = "UPDATE TRX_SNI_SK SET IS_PUBLISH = 1 WHERE SNI_SK_ID = " + ts.SNI_SK_ID;
            db.Database.ExecuteSqlCommand(updates);

            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == ts.SNI_PROPOSAL_ID select proposal).SingleOrDefault();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;


            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");


            HttpPostedFileBase file4 = Request.Files["file_sni"];
            if (file4.ContentLength > 0)
            {
                var doc = (from b in db.TRX_DOCUMENTS where b.DOC_ID == ts.SNI_DOC_ID && b.DOC_FOLDER_ID == 8 && b.DOC_RELATED_TYPE == 100 select b).SingleOrDefault();

                int LASTID_DOC = MixHelper.GetSequence("TRX_DOCUMENTS");

                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/SNI"));
                string path = Server.MapPath("~/Upload/Dokumen/SNI/");

                Stream stremdokumen = file4.InputStream;
                byte[] appData = new byte[file4.ContentLength + 1];
                stremdokumen.Read(appData, 0, file4.ContentLength);
                string Extension = Path.GetExtension(file4.FileName);
                if (Extension.ToLower() == ".doc" || Extension.ToLower() == ".docx")
                {
                    //Aspose.Pdf.Document pdf = new Aspose.Pdf.Document(stremdokumen);
                    Aspose.Words.Document docx = new Aspose.Words.Document(stremdokumen);
                    string filePathdoc = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "(publish).docx";
                    string filePathpdf = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "(publish).pdf";
                    string filePathxml = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "(publish).xml";
                    docx.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    docx.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    docx.Save(@"" + filePathxml);
                    int Total_Hal = docx.PageCount;
                    ////string filePathpdf = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "(publish).pdf";
                    //string filePathxml = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "(publish).xml";
                    //pdf.Save(@"" + filePathpdf, Aspose.Pdf.SaveFormat.Pdf);
                    //pdf.Save(@"" + filePathxml);
                    var LOGCODE_TANGGAPAN_MTPS = MixHelper.GetLogCode();
                    var fupdate = "UPDATE TRX_DOCUMENTS SET DOC_FILE_NAME = 'SNI_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + "(publish)'," +
                            "DOC_UPDATE_BY = " + UserId + "," +
                            "DOC_INFO = " + Total_Hal + "," +
                            "DOC_UPDATE_DATE = " + datenow + "" +
                            "WHERE DOC_RELATED_ID = " + ts.SNI_PROPOSAL_ID + " AND DOC_FOLDER_ID = 8 AND DOC_RELATED_TYPE = 100";

                    db.Database.ExecuteSqlCommand(fupdate);

                }
            }


            String objek = update_sni.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);

            String objeks = update_PROPOSAL.Replace("'", "-");
            MixHelper.InsertLog(logcode, objeks, 1);

            String objekss = updates.Replace("'", "-");
            MixHelper.InsertLog(logcode, objekss, 1);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data SNI Berhasil di Publikasikan";
            return RedirectToAction("Index");
        }

        public ActionResult publish(int id = 0)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("TRX_DOCUMENTS");
            var datenow = MixHelper.ConvertDateNow();

            var updates = "UPDATE TRX_SNI_SK SET IS_PUBLISH = 1 WHERE SNI_SK_ID = " + id;
            db.Database.ExecuteSqlCommand(updates);

            var update = "UPDATE TRX_SNI SET SNI_IS_PUBLISH = 1 WHERE SNI_SK_ID = " + id;
            db.Database.ExecuteSqlCommand(update);

            String objek = update.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data SNI Berhasil di Publikasikan";
            return RedirectToAction("Index");
        }

        [Auth(RoleTipe = 1)]
        public ActionResult Detail(int id = 0)
        {
            var sninya = db.Database.SqlQuery<VIEW_SNI>("SELECT AA.* FROM VIEW_SNI AA WHERE AA.SNI_ID = " + id).SingleOrDefault();
            ViewData["sninya"] = sninya;

            var doc = (from b in db.TRX_DOCUMENTS where b.DOC_ID == sninya.SNI_DOC_ID && b.DOC_FOLDER_ID == 8 && b.DOC_RELATED_TYPE == 100 select b).SingleOrDefault();
            ViewData["doc"] = doc;

            var sni = db.Database.SqlQuery<VIEW_SNI_SK>("SELECT AA.SNI_SK_ID,AA.SNI_SK_SNI_ID,AA.SNI_SK_DOC_ID,AA.SNI_SK_NOMOR,AA.SNI_SK_DATE,AA.SNI_SK_DATE_NAME,AA.SNI_SK_CREATE_DATE,AA.SNI_SK_CREATE_BY,AA.SNI_SK_DATE_START,AA.SNI_SK_DATE_START_NAME,AA.SNI_SK_DATE_END,AA.SNI_SK_DATE_END_NAME,AA.JML_SNI,AA.IS_PUBLISH,AA.SNI_SK_KET,AA.DOC_ID,AA.DOC_CODE,AA.DOC_FOLDER_ID,AA.DOC_NUMBER,AA.DOC_NAME,AA.DOC_FILE_PATH,AA.DOC_FILE_NAME,AA.DOC_FILETYPE,AA.DOC_LINK FROM (SELECT AA.*, ROWNUM NOMOR FROM (SELECT * FROM VIEW_SNI_SK WHERE SNI_SK_ID = " + id + " ) AA WHERE ROWNUM <= 1 ) AA WHERE NOMOR > 0").SingleOrDefault();
            ViewData["sni"] = sni;
            var listsni = (from a in db.TRX_SNI where a.SNI_SK_ID == id select a).ToList();
            ViewData["listsni"] = listsni;
            return View();
        }

        public ActionResult Republish(int id) {

            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("TRX_DOCUMENTS");
            var datenow = MixHelper.ConvertDateNow();

            var updates = "UPDATE TRX_SNI SET SNI_IS_PUBLISH = 0, SNI_UPDATE_BY = "+UserId+", SNI_UPDATE_DATE = "+datenow+" WHERE SNI_ID = " + id;
            db.Database.ExecuteSqlCommand(updates);

            String objek = updates.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Berhasil, Data SNI menjadi tidak dipublikasi ";
            return RedirectToAction("Index");
        }
    }
}
