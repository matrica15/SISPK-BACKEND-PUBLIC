using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.IO;

namespace SISPK.Controllers.Penetapan
{
    public class PenetapanSNIController : Controller
    {
        //
        // GET: /PenetapanSNI/
        private SISPKEntities db = new SISPKEntities();

        [Auth(RoleTipe = 1)]
        public ActionResult Index()
        {
            return View();
        }

        [Auth(RoleTipe = 2)]
        public ActionResult Create(int id = 0) {
            var DataProposal = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + id).SingleOrDefault();
            ViewData["DataProposal"] = DataProposal;
            //ViewData["listSNI"] = (from t in db.VIEW_PROPOSAL where t.PROPOSAL_STATUS == 10 && t.PROPOSAL_STATUS_PROSES == 3 orderby t.PROPOSAL_CREATE_DATE ascending select t).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Create(TRX_SNI_SK tsk, FormCollection formCollection, int PROPOSAL_ID, string SNI_SK_DATE ="")
        {
            var USER_ID = Session["USER_ID"];
            var DATENOW = MixHelper.ConvertDateNow();
            var logcode = MixHelper.GetLogCode();
            var encript = new AuthController();

            //int lastid = MixHelper.GetSequence("TRX_SNI");
            int lastidsnisk = MixHelper.GetSequence("TRX_SNI_SK");
            int LASTID_SK_SNI = MixHelper.GetSequence("TRX_DOCUMENTS");
            var datenow = MixHelper.ConvertDateNow();
            String SNI_SK_DATE_CONVERT = "TO_DATE('" + SNI_SK_DATE + "', 'yyyy-mm-dd hh24:mi:ss')";

            HttpPostedFileBase FILE_SK_SNI = Request.Files["SK_SNI"];
            if (FILE_SK_SNI.ContentLength > 0)
            {
                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/SK_SNI"));
                string path = Server.MapPath("~/Upload/Dokumen/SK_SNI/");
                Stream STREAM_DOC_SK_SNI = FILE_SK_SNI.InputStream;

                string EXT_SK_SNI = Path.GetExtension(FILE_SK_SNI.FileName);
                if (EXT_SK_SNI.ToLower() == ".docx" || EXT_SK_SNI.ToLower() == ".doc")
                {
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_DOC_SK_SNI);
                    string filePathdoc = path + "SK_SNI_" + ((tsk.SNI_SK_NOMOR).Replace("/","_")).ToUpper() + ".docx";
                    string filePathpdf = path + "SK_SNI_" + ((tsk.SNI_SK_NOMOR).Replace("/", "_")).ToUpper() + ".pdf";
                    string filePathxml = path + "SK_SNI_" + ((tsk.SNI_SK_NOMOR).Replace("/", "_")).ToUpper() + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);
                    
                    var LOGCODE_SK_SNI = MixHelper.GetLogCode();
                    var FNAME_SK_SNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_SK_SNI = "'" + LASTID_SK_SNI + "', " +
                                "'9', " +
                                "'49', " +
                                "'SK SNI Dengan Nomor " + tsk.SNI_SK_NOMOR.ToUpper() +"', " +
                                "'Surat Keputusan SNI Dengan Nomor " + tsk.SNI_SK_NOMOR.ToUpper() + "', " +
                                "'" + "/Upload/Dokumen/SK_SNI/" + "', " +
                                "'" + "SK_SNI_" + ((tsk.SNI_SK_NOMOR).Replace("/", "_")).ToUpper() + "', " +
                                "'" + EXT_SK_SNI.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_SK_SNI + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_SK_SNI + ") VALUES (" + FVALUE_SK_SNI.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_SK_SNI.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_SK_SNI, objekTanggapan, 1);
                }
                else
                {
                    FILE_SK_SNI.SaveAs(path + "SK_SNI_" + ((tsk.SNI_SK_NOMOR).Replace("/", "_")).ToUpper() + EXT_SK_SNI.ToLower());
                    
                    var LOGCODE_SK_SNI = MixHelper.GetLogCode();
                    var FNAME_SK_SNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_SK_SNI = "'" + LASTID_SK_SNI + "', " +
                                "'9', " +
                                "'49', " +
                                "'SK SNI Dengan Nomor  " + tsk.SNI_SK_NOMOR.ToUpper() + "', " +
                                "'Surat Keputusan SNI Dengan Nomor " + tsk.SNI_SK_NOMOR.ToUpper() + "', " +
                                "'" + "/Upload/Dokumen/SK_SNI/" + "', " +
                                "'" + "SK_SNI_" + ((tsk.SNI_SK_NOMOR).Replace("/", "_")).ToUpper() + "', " +
                                "'" + EXT_SK_SNI.ToLower().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + USER_ID + "', " +
                                DATENOW + "," +
                                "'1', " +
                                "'" + LOGCODE_SK_SNI + "'";
                    
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_SK_SNI + ") VALUES (" + FVALUE_SK_SNI.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_SK_SNI.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_SK_SNI, objekTanggapan, 1);
                }
            }

            var NoSK = tsk.SNI_SK_NOMOR.ToUpper().Replace(" ", "_").Replace(".", "_").Replace("/", "_");     
            // Awal Tambahan Script 
            //int jml_sni = PROPOSAL_ID.Count();
            int jml_sni = 1;
            int lastid_sk = MixHelper.GetSequence("TRX_SNI_SK");
            var query2 = "INSERT INTO TRX_SNI_SK (SNI_SK_ID,SNI_SK_DOC_ID,SNI_SK_NOMOR,SNI_SK_DATE,SNI_SK_CREATE_DATE,SNI_SK_CREATE_BY,JML_SNI,IS_PUBLISH,SNI_SK_STATUS,SNI_SK_KET) VALUES (" + lastid_sk + "," + LASTID_SK_SNI + ",'" + tsk.SNI_SK_NOMOR + "'," + SNI_SK_DATE_CONVERT + "," + datenow + "," + USER_ID + "," + jml_sni + ",0,1,'" + tsk.SNI_SK_KET + "')";

            db.Database.ExecuteSqlCommand(query2);
            // Akhir Tambahan Script
            var query = "";
            if (PROPOSAL_ID != 0)
            {
                var PID = PROPOSAL_ID;
                
                int lastid_SNI = MixHelper.GetSequence("TRX_SNI");
                int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                var getProposal = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PID).SingleOrDefault();
                query = "INSERT INTO TRX_SNI (SNI_ID, SNI_SK_ID, SNI_PROPOSAL_ID, SNI_DOC_ID, SNI_CREATE_DATE, SNI_CREATE_BY, SNI_STATUS, SNI_NOMOR, SNI_JUDUL,SNI_IS_PUBLISH) VALUES (" + lastid_SNI + "," + lastid_sk + "," + PID + ",'" + LASTID_DATA_RSNI + "'," + datenow + "," + USER_ID + ",'1','" + getProposal.PROPOSAL_NO_SNI_PROPOSAL + "','" + getProposal.PROPOSAL_JUDUL_SNI_PROPOSAL + "',0)";
                db.Database.ExecuteSqlCommand(query);

                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 11,PROPOSAL_STATUS_PROSES = 1,PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + USER_ID + " WHERE PROPOSAL_ID = " + PID);
                String objek3 = "PROPOSAL_STATUS = 11,PROPOSAL_STATUS_PROSES = 1, PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + USER_ID;
                MixHelper.InsertLog(logcode, objek3.Replace("'", "-"), 2);

                int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PID);
                var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PID + " AND T1.APPROVAL_STATUS_PROPOSAL = 10").SingleOrDefault();
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PID + ",1,''," + datenow + "," + USER_ID + ",1,10," + APPROVAL_STATUS_SESSION + ")");

                var SNI = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + PID + " AND AA.DOC_RELATED_TYPE = 18 AND AA.DOC_FOLDER_ID = 18 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();

                var PROPOSAL_PNPS_CODE_FIXER = getProposal.PROPOSAL_PNPS_CODE;

                Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/SNI"));
                string path = Server.MapPath("~/Upload/Dokumen/SNI/");
                  
                string dataDir = Server.MapPath("~" + SNI.DOC_FILE_PATH + "" + SNI.DOC_FILE_NAME + "." + SNI.DOC_FILETYPE);
                    

                Stream stream = System.IO.File.OpenRead(dataDir);

                Aspose.Words.Document docs = new Aspose.Words.Document(stream);
                stream.Close();
                var SNI_NOMOR_CONVERT = getProposal.PROPOSAL_NO_SNI_PROPOSAL.Replace(" ", "_").Replace(".", "_").Replace("/", "_").Replace(":", "_").Replace(";", "_").Replace("@", "_");
                string filePathdoc = path + "SNI_" + SNI_NOMOR_CONVERT + ".docx";
                string filePathpdf = path + "SNI_" + SNI_NOMOR_CONVERT + ".pdf";
                string filePathxml = path + "SNI_" + SNI_NOMOR_CONVERT + ".xml";
                //return Json(new
                //{
                //    sEcho = @"" + filePathdoc
                //}, JsonRequestBehavior.AllowGet);
                docs.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                docs.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                docs.Save(@"" + filePathxml);
                int Total_Hal = docs.PageCount;
                var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_INFO,DOC_LOG_CODE";
                var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                            "'8', " +
                            "'100', " +
                            "'" + PID + "', " +
                            "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") " + getProposal.PROPOSAL_NO_SNI_PROPOSAL + "', " +
                            "'Data SNI dengan Nomor " + getProposal.PROPOSAL_NO_SNI_PROPOSAL + " Dengan Kode Proposal : " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                            "'" + "/Upload/Dokumen/SNI/" + "', " +
                            "'SNI_" + SNI_NOMOR_CONVERT + "" + "', " +
                            "'pdf', " +
                            "'0', " +
                            "'" + USER_ID + "', " +
                            datenow + "," +
                            "'1', " +
                            "'" + Total_Hal + "', " +
                            "'" + LOGCODE_DATA_RSNI + "'";
                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DATA_RSNI + ") VALUES (" + FVALUE_DATA_RSNI.Replace("''", "NULL") + ")");
                String objekTanggapan = FVALUE_DATA_RSNI.Replace("'", "-");
                MixHelper.InsertLog(LOGCODE_DATA_RSNI, objekTanggapan, 1);
            }

            String objek = query.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "SNI telah ditetapkan";
            return RedirectToAction("Index");
        }
               
        //public ActionResult listSNI(DataTables param) {

        //    var default_order = "SNI_NOMOR";
        //    var limit = 10;

        //    List<string> order_field = new List<string>();
        //    order_field.Add("SNI_NOMOR");
        //    order_field.Add("SNI_SK_DATE_NAME");
        //    order_field.Add("SNI_SK_NOMOR");

        //    string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
        //    string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
        //    string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
        //    string search = (param.sSearch == "") ? "" : param.sSearch;

        //    limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
        //    var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


        //    string where_clause = "";

        //    string search_clause = "";
        //    if (search != "")
        //    {
        //        if (where_clause != "")
        //        {
        //            search_clause += " AND ";
        //        }
        //        search_clause += "(";
        //        var i = 1;
        //        foreach (var fields in order_field)
        //        {
        //            if (fields != "")
        //            {
        //                search_clause += fields + "  LIKE '%" + search + "%'";
        //                if (i < order_field.Count())
        //                {
        //                    search_clause += " OR ";
        //                }
        //            }
        //            i++;
        //        }
        //        search_clause += " OR SNI_SK_NOMOR = '%" + search + "%')";
        //    }

        //    string inject_clause_count = "";
        //    string inject_clause_select = "";
        //    if (where_clause != "" || search_clause != "")
        //    {
        //        inject_clause_count = "WHERE " + where_clause + " " + search_clause;
        //        inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
        //    }
        //    var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI " + inject_clause_count);
        //    var SelectedData = db.Database.SqlQuery<VIEW_SNI>(inject_clause_select);

        //    var result = from list in SelectedData
        //                 select new string[] 
        //    { 
        //        Convert.ToString(list.SNI_NOMOR), 
        //        Convert.ToString("<center>"+list.SNI_SK_DATE_NAME+"</center>"),
        //        Convert.ToString("<center>"+list.SNI_SK_NOMOR+"</center>"),
        //        Convert.ToString(((list.SNI_IS_PUBLISH==0)?"<center><a href='/Publikasi/Publikasi' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Publikasi'><i class='action fa fa-file-text-o'></i>Belum Publikasi</a></center>":"<center><a href='javascript:void(0)' class='btn green btn-sm action' data-container='body'><i class='action fa fa-file-text-o'></i>Telah di Publikasi</a></center>")),
        //    };
        //    return Json(new
        //    {
        //        sEcho = param.sEcho,
        //        iTotalRecords = CountData,
        //        iTotalDisplayRecords = CountData,
        //        aaData = result.ToArray()
        //    }, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult listSNI(DataTables param)
        {
            var default_order = "SNI_SK_NOMOR";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("SNI_SK_NOMOR");
            order_field.Add("SNI_SK_DATE_NAME");
            order_field.Add("JML_SNI");
            order_field.Add("SNI_SK_KET");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "JML_SNI > 0";

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
                search_clause += " OR SNI_SK_NOMOR = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI_SK WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI_SK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SNI_SK>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.SNI_SK_NOMOR), 
                Convert.ToString(list.SNI_SK_DATE_NAME),
                Convert.ToString("<center>"+list.JML_SNI+"</center>"),
                Convert.ToString(list.SNI_SK_KET),
                Convert.ToString("<center><a href='PenetapanSNI/Detail/"+list.SNI_SK_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a><a href='PenetapanSNI/Edit/"+list.SNI_SK_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Edit'><i class='action fa fa-edit'></i></a></center>"),
                //Convert.ToString("<center><a href='PenetapanSNI/Detail/"+list.SNI_SK_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a></center>"),
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

        public ActionResult DetailRSNI(int id = 0)
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

        public ActionResult Detail(int id = 0) {
            var sni = db.Database.SqlQuery<VIEW_SNI_SK>("SELECT AA.SNI_SK_ID,AA.SNI_SK_SNI_ID,AA.SNI_SK_DOC_ID,AA.SNI_SK_NOMOR,AA.SNI_SK_DATE,AA.SNI_SK_DATE_NAME,AA.SNI_SK_CREATE_DATE,AA.SNI_SK_CREATE_BY,AA.SNI_SK_DATE_START,AA.SNI_SK_DATE_START_NAME,AA.SNI_SK_DATE_END,AA.SNI_SK_DATE_END_NAME,AA.JML_SNI,AA.IS_PUBLISH,AA.SNI_SK_KET,AA.DOC_ID,AA.DOC_CODE,AA.DOC_FOLDER_ID,AA.DOC_NUMBER,AA.DOC_NAME,AA.DOC_FILE_PATH,AA.DOC_FILE_NAME,AA.DOC_FILETYPE,AA.DOC_LINK FROM (SELECT AA.*, ROWNUM NOMOR FROM (SELECT * FROM VIEW_SNI_SK WHERE SNI_SK_ID = " + id + " ) AA WHERE ROWNUM <= 1 ) AA WHERE NOMOR > 0").SingleOrDefault();
            ViewData["sni"] = sni;
            var listsni = (from a in db.TRX_SNI where a.SNI_SK_ID == id select a).ToList();
            ViewData["listsni"] = listsni;
            return View();
        }

        [Auth(RoleTipe = 3)]
        public ActionResult Edit(int id = 0) {
            var sni = db.Database.SqlQuery<VIEW_SNI_SK>("SELECT AA.SNI_SK_ID,AA.SNI_SK_SNI_ID,AA.SNI_SK_DOC_ID,AA.SNI_SK_NOMOR,AA.SNI_SK_DATE,AA.SNI_SK_DATE_NAME,AA.SNI_SK_CREATE_DATE,AA.SNI_SK_CREATE_BY,AA.SNI_SK_DATE_START,AA.SNI_SK_DATE_START_NAME,AA.SNI_SK_DATE_END,AA.SNI_SK_DATE_END_NAME,AA.JML_SNI,AA.IS_PUBLISH,AA.SNI_SK_KET,AA.DOC_ID,AA.DOC_CODE,AA.DOC_FOLDER_ID,AA.DOC_NUMBER,AA.DOC_NAME,AA.DOC_FILE_PATH,AA.DOC_FILE_NAME,AA.DOC_FILETYPE,AA.DOC_LINK FROM (SELECT AA.*, ROWNUM NOMOR FROM (SELECT * FROM VIEW_SNI_SK WHERE SNI_SK_ID = " + id + " ) AA WHERE ROWNUM <= 1 ) AA WHERE NOMOR > 0").SingleOrDefault();
            ViewData["sni"] = sni;
            var listsni = (from a in db.TRX_SNI where a.SNI_SK_ID == id select a).ToList();
            ViewData["listsni"] = listsni;
            //ViewData["listSNI"] = (from t in db.VIEW_PROPOSAL where t.PROPOSAL_STATUS == 10 && t.PROPOSAL_STATUS_PROSES == 3 orderby t.PROPOSAL_CREATE_DATE ascending select t).ToList();
           return View();
        }

        [HttpPost]
        public ActionResult Edit(TRX_SNI_SK tsk, FormCollection formCollection, int[] PROPOSAL_ID)
        {

            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            //int lastid = MixHelper.GetSequence("TRX_SNI");
            int lastidsnisk = MixHelper.GetSequence("TRX_SNI_SK");
            int lastiddoc = MixHelper.GetSequence("TRX_DOCUMENTS");
            var datenow = MixHelper.ConvertDateNow();

            DateTime dates = Convert.ToDateTime(tsk.SNI_SK_DATE);
            var SNI_SK_DATE = "TO_DATE('" + dates.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";

            DateTime datess = Convert.ToDateTime(tsk.SNI_SK_DATE_END);
            var SNI_SK_DATE_END = "TO_DATE('" + dates.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";

            HttpPostedFileBase LEMBAR_KENDALI = Request.Files["DOC_NAME"];
            int LASTID_MEMO_KAPUS = lastiddoc;
            string file_SK = LEMBAR_KENDALI.FileName;
            if (file_SK != "")
            {
                Stream STREAM_LEMBAR_KENDALI = LEMBAR_KENDALI.InputStream;
                byte[] APPDATA_LEMBAR_KENDALI = new byte[LEMBAR_KENDALI.ContentLength + 1];
                STREAM_LEMBAR_KENDALI.Read(APPDATA_LEMBAR_KENDALI, 0, LEMBAR_KENDALI.ContentLength);
                string Extension_LEMBAR_KENDALI = Path.GetExtension(LEMBAR_KENDALI.FileName);
                var NoSK = tsk.SNI_SK_NOMOR.ToUpper().Replace(" ", "_").Replace(".", "_").Replace("/", "_");

                if (Extension_LEMBAR_KENDALI.ToLower() == ".docx" || Extension_LEMBAR_KENDALI.ToLower() == ".doc")
                {
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/SK_SNI"));
                    string path = Server.MapPath("~/Upload/Dokumen/SK_SNI/");
                    Aspose.Words.Document doc = new Aspose.Words.Document(STREAM_LEMBAR_KENDALI);
                    string filePathdoc = path + "" + tsk.SNI_SK_NOMOR.ToUpper() + ".docx";
                    string filePathpdf = path + "" + tsk.SNI_SK_NOMOR.ToUpper() + ".pdf";
                    string filePathxml = path + "" + tsk.SNI_SK_NOMOR.ToUpper() + ".xml";
                    doc.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    doc.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    doc.Save(@"" + filePathxml);

                    var LOGCODE_MEMO_KAPUS = MixHelper.GetLogCode();

                    var FNAME_MEMO_KAPUS = "DOC_ID,DOC_FOLDER_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_MEMO_KAPUS = "'" + LASTID_MEMO_KAPUS + "', " +
                                "'9', " +
                                "'" + "" + tsk.SNI_SK_NOMOR.ToUpper() + "', " +
                                "'SK Dengan Nomor " + tsk.SNI_SK_NOMOR.ToUpper() + "', " +
                                "'" + "/Upload/Dokumen/SK_SNI/', " +
                                "'" + "SK_" + NoSK + "" + "', " +
                                "'" + Extension_LEMBAR_KENDALI.ToUpper().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + UserId + "', " +
                                datenow + "," +
                                "'1', " +
                                "'" + LOGCODE_MEMO_KAPUS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_MEMO_KAPUS + ") VALUES (" + FVALUE_MEMO_KAPUS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_MEMO_KAPUS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_MEMO_KAPUS, objekTanggapan, 1);
                }
                else
                {
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/SK_SNI"));
                    string path = Server.MapPath("~/Upload/Dokumen/SK_SNI/");
                    LEMBAR_KENDALI.SaveAs(path + "SK_" + NoSK.Replace("/", "-") + Extension_LEMBAR_KENDALI.ToUpper());
                    
                    var LOGCODE_MEMO_KAPUS = MixHelper.GetLogCode();
                    var FNAME_MEMO_KAPUS = "DOC_ID,DOC_FOLDER_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_MEMO_KAPUS = "'" + LASTID_MEMO_KAPUS + "', " +
                                "'9', " +
                                "'" + "" + tsk.SNI_SK_NOMOR.ToUpper() + "', " +
                                "'SK Dengan Nomor " + tsk.SNI_SK_NOMOR.ToUpper() + "', " +
                                "'" + "/Upload/Dokumen/SK_SNI/', " +
                                "'" + "SK_" + NoSK.Replace("/", "-") + "" + "', " +
                                "'" + Extension_LEMBAR_KENDALI.ToUpper().Replace(".", "") + "', " +
                                "'0', " +
                                "'" + UserId + "', " +
                                datenow + "," +
                                "'1', " +
                                "'" + LOGCODE_MEMO_KAPUS + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_MEMO_KAPUS + ") VALUES (" + FVALUE_MEMO_KAPUS.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_MEMO_KAPUS.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_MEMO_KAPUS, objekTanggapan, 1);
                }

                // Awal Tambahan Script 
                int jml_sni = PROPOSAL_ID.Count();
                //int lastid_sk = MixHelper.GetSequence("TRX_SNI_SK");
                //var query2 = "INSERT INTO TRX_SNI_SK (SNI_SK_ID,SNI_SK_DOC_ID,SNI_SK_NOMOR,SNI_SK_DATE,SNI_SK_DATE_START,SNI_SK_CREATE_DATE,SNI_SK_CREATE_BY,JML_SNI,IS_PUBLISH,SNI_SK_STATUS) VALUES (" + lastid_sk + "," + lastiddoc + ",'" + tsk.SNI_SK_NOMOR + "'," + SNI_SK_DATE + "," + SNI_SK_DATE + "," + datenow + "," + UserId + "," + jml_sni + ",1,1)";
                //db.Database.ExecuteSqlCommand(query2);
                //var doc_exp = db.Database.SqlQuery<int>("SELECT AA.SNI_SK_DOC_ID from TRX_SNI_SK AA WHERE AA.SNI_SK_ID = 818");

                db.Database.ExecuteSqlCommand("DELETE FROM TRX_DOCUMENTS AA WHERE AA.DOC_ID ="+tsk.SNI_SK_DOC_ID);

                var update ="SNI_SK_DOC_ID = '" + LASTID_MEMO_KAPUS + "'," +
                            "SNI_SK_NOMOR = '" + tsk.SNI_SK_NOMOR + "'," +
                            "SNI_SK_DATE = " + SNI_SK_DATE + "," +
                            "SNI_SK_DATE_START = " + SNI_SK_DATE + "," +
                            "SNI_SK_UPDATE_DATE = " + datenow + "," +
                            "SNI_SK_UPDATE_BY = '" + UserId + "'," +
                            "JML_SNI = '" + jml_sni + "',"+
                            "SNI_SK_KET = '" + tsk.SNI_SK_KET + "'";

                var clause = "where SNI_SK_ID = " + tsk.SNI_SK_ID;
                db.Database.ExecuteSqlCommand("UPDATE TRX_SNI_SK SET " + update.Replace("''", "NULL") + " " + clause);

                // Akhir Tambahan Script
                var query = "";
                if (PROPOSAL_ID.Count() > 0)
                {
                    foreach (var PID in PROPOSAL_ID)
                    {
                        int lastid_SNI = MixHelper.GetSequence("TRX_SNI");
                        //var PROPOSAL_ID_EXT = (from a in db.TRX_SNI where a.SNI_PROPOSAL_ID == PID select a).SingleOrDefault();
                        int prop_is_ext = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM TRX_SNI AA WHERE AA.SNI_PROPOSAL_ID =" + PID).SingleOrDefault();

                        if (prop_is_ext == 0)
                        {
                            var getProposal = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PID).SingleOrDefault();
                            query = "INSERT INTO TRX_SNI (SNI_ID, SNI_SK_ID, SNI_PROPOSAL_ID, SNI_DOC_ID, SNI_CREATE_DATE, SNI_CREATE_BY, SNI_STATUS, SNI_NOMOR, SNI_JUDUL,SNI_IS_PUBLISH) VALUES (" + lastid_SNI + "," + tsk.SNI_SK_ID + "," + PID + ",'" + LASTID_MEMO_KAPUS + "'," + datenow + "," + UserId + ",'1','" + getProposal.PROPOSAL_NO_SNI_PROPOSAL + "','" + getProposal.PROPOSAL_JUDUL_SNI_PROPOSAL + "',0)";
                            db.Database.ExecuteSqlCommand(query);

                            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 11,PROPOSAL_STATUS_PROSES = 1,PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId + " WHERE PROPOSAL_ID = " + PID);
                            String objek3 = "PROPOSAL_STATUS = 11,PROPOSAL_STATUS_PROSES = 1, PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId;
                            MixHelper.InsertLog(logcode, objek3.Replace("'", "-"), 2);

                            int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PID);
                            var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PID + " AND T1.APPROVAL_STATUS_PROPOSAL = 10").SingleOrDefault();
                            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PID + ",1,''," + datenow + "," + UserId + ",1,10," + APPROVAL_STATUS_SESSION + ")");

                            var SNI = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + PID + " AND AA.DOC_RELATED_TYPE = 18 AND AA.DOC_FOLDER_ID = 18 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();

                            var PROPOSAL_PNPS_CODE_FIXER = getProposal.PROPOSAL_PNPS_CODE;

                            Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/SNI/" + PROPOSAL_PNPS_CODE_FIXER));
                            string path = Server.MapPath("~/Upload/Dokumen/SNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                            string dataDir = Server.MapPath("~" + SNI.DOC_FILE_PATH + "" + SNI.DOC_FILE_NAME + "." + SNI.DOC_FILETYPE);
                            Stream stream = System.IO.File.OpenRead(dataDir);

                            Aspose.Words.Document docs = new Aspose.Words.Document(stream);
                            stream.Close();
                            string filePathdoc = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + ".DOCX";
                            string filePathpdf = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + ".PDF";
                            string filePathxml = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + ".XML";
                            //return Json(new
                            //{
                            //    sEcho = @"" + filePathdoc
                            //}, JsonRequestBehavior.AllowGet);
                            docs.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                            docs.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                            docs.Save(@"" + filePathxml);
                            int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                            var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                            var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                            var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                                        "'8', " +
                                        "'100', " +
                                        "'" + PID + "', " +
                                        "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") " + getProposal.PROPOSAL_NO_SNI_PROPOSAL + "', " +
                                        "'Data SNI dengan Nomor " + getProposal.PROPOSAL_NO_SNI_PROPOSAL + " Dengan Kode Proposal : " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                        "'" + "/Upload/Dokumen/SNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                        "'SNI_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                                        "'PDF', " +
                                        "'0', " +
                                        "'" + UserId + "', " +
                                        datenow + "," +
                                        "'1', " +
                                        "'" + LOGCODE_DATA_RSNI + "'";
                            db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DATA_RSNI + ") VALUES (" + FVALUE_DATA_RSNI.Replace("''", "NULL") + ")");
                            String objekTanggapan = FVALUE_DATA_RSNI.Replace("'", "-");
                            MixHelper.InsertLog(LOGCODE_DATA_RSNI, objekTanggapan, 1);
                        }
                        else
                        {
                            var sni_sk_list = (from a in db.TRX_SNI where a.SNI_SK_ID == tsk.SNI_SK_ID select a).ToList();
                            foreach (var sn in sni_sk_list)
                            {
                                if (sn.SNI_PROPOSAL_ID != PID)
                                {
                                    db.Database.ExecuteSqlCommand("DELETE FROM TRX_SNI WHERE SNI_ID = " + sn.SNI_ID);
                                    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10,PROPOSAL_STATUS_PROSES = 3,PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId + " WHERE PROPOSAL_ID = " + sn.SNI_PROPOSAL_ID);
                                    String objek3 = "PROPOSAL_STATUS = 10,PROPOSAL_STATUS_PROSES = 3, PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId;
                                    MixHelper.InsertLog(logcode, objek3.Replace("'", "-"), 2);
                                    //int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                                    System.IO.File.Delete(@"C:\Users\Public\DeleteTest\test.txt");

                                    db.Database.ExecuteSqlCommand("DELETE FROM	TRX_PROPOSAL_APPROVAL WHERE	APPROVAL_ID = (SELECT MAX(AA.APPROVAL_ID) FROM	TRX_PROPOSAL_APPROVAL AA WHERE AA.APPROVAL_PROPOSAL_ID = " + sn.SNI_PROPOSAL_ID + ")");
                                    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 1 WHERE	APPROVAL_ID = (SELECT MAX (AA.APPROVAL_ID) FROM	TRX_PROPOSAL_APPROVAL AA WHERE AA.APPROVAL_PROPOSAL_ID = " + sn.SNI_PROPOSAL_ID + " AND AA.APPROVAL_STATUS_PROPOSAL = 10)");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + sn.SNI_PROPOSAL_ID + " AND AA.DOC_RELATED_TYPE = 100 AND AA.DOC_FOLDER_ID = 8");
                                }
                                else {
                                    var update_trx_sni =
                                                  " SNI_DOC_ID ='" + LASTID_MEMO_KAPUS + "'," +
                                                  " SNI_UPDATE_DATE =" + datenow + "," +
                                                  " SNI_UPDATE_BY ='" + UserId + "'";

                                    var clausess = "where SNI_PROPOSAL_ID = " + PID;
                                    db.Database.ExecuteSqlCommand("UPDATE TRX_SNI SET " + update_trx_sni.Replace("''", "NULL") + " " + clausess);
                                    //return Json(new { total_count = "UPDATE TRX_SNI SET " + update.Replace("''", "NULL") + " " + clauses }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            
                        }
                    }
                }
            }
            else
            {
                // Awal Tambahan Script 
                int jml_sni = PROPOSAL_ID.Count();
                //int lastid_sk = MixHelper.GetSequence("TRX_SNI_SK");
                //var query2 = "INSERT INTO TRX_SNI_SK (SNI_SK_ID,SNI_SK_DOC_ID,SNI_SK_NOMOR,SNI_SK_DATE,SNI_SK_DATE_START,SNI_SK_CREATE_DATE,SNI_SK_CREATE_BY,JML_SNI,IS_PUBLISH,SNI_SK_STATUS) VALUES (" + lastid_sk + "," + lastiddoc + ",'" + tsk.SNI_SK_NOMOR + "'," + SNI_SK_DATE + "," + SNI_SK_DATE + "," + datenow + "," + UserId + "," + jml_sni + ",1,1)";
                //db.Database.ExecuteSqlCommand(query2);

                var update =
                            "SNI_SK_DOC_ID = '" + tsk.SNI_SK_DOC_ID + "'," +
                            "SNI_SK_NOMOR = '" + tsk.SNI_SK_NOMOR + "'," +
                            "SNI_SK_DATE = " + SNI_SK_DATE + "," +
                            "SNI_SK_DATE_START = " + SNI_SK_DATE + "," +
                            "SNI_SK_UPDATE_DATE = " + datenow + "," +
                            "SNI_SK_UPDATE_BY = '" + UserId + "'," +
                            "JML_SNI = '" + jml_sni + "',"+
                            "SNI_SK_KET = '" + tsk.SNI_SK_KET + "'";

                var clause = "where SNI_SK_ID = " + tsk.SNI_SK_ID;
                db.Database.ExecuteSqlCommand("UPDATE TRX_SNI_SK SET " + update.Replace("''", "NULL") + " " + clause);
                //return Json(new { total_count = "UPDATE TRX_SNI_SK SET " + update.Replace("''", "NULL") + " " + clause }, JsonRequestBehavior.AllowGet);
                // Akhir Tambahan Script

                var query = "";
                if (PROPOSAL_ID.Count() > 0)
                {
                    foreach (var PID in PROPOSAL_ID)
                    {
                        int lastid_SNI = MixHelper.GetSequence("TRX_SNI");
                        //var PROPOSAL_ID_EXT = (from a in db.TRX_SNI where a.SNI_PROPOSAL_ID == PID select a).SingleOrDefault();
                        int prop_is_ext = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM TRX_SNI AA WHERE AA.SNI_PROPOSAL_ID =" + PID).SingleOrDefault();
                        //return Json(new { total_count = prop_is_ext }, JsonRequestBehavior.AllowGet);
                        if (prop_is_ext == 0)
                        {
                            var getProposal = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PID).SingleOrDefault();
                            query = "INSERT INTO TRX_SNI (SNI_ID, SNI_SK_ID, SNI_PROPOSAL_ID, SNI_DOC_ID, SNI_CREATE_DATE, SNI_CREATE_BY, SNI_STATUS, SNI_NOMOR, SNI_JUDUL,SNI_IS_PUBLISH) VALUES (" + lastid_SNI + "," + tsk.SNI_SK_ID + "," + PID + ",'" + tsk.SNI_SK_DOC_ID + "'," + datenow + "," + UserId + ",'1','" + getProposal.PROPOSAL_NO_SNI_PROPOSAL + "','" + getProposal.PROPOSAL_JUDUL_SNI_PROPOSAL + "',0)";
                            db.Database.ExecuteSqlCommand(query);

                            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 11,PROPOSAL_STATUS_PROSES = 1,PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId + " WHERE PROPOSAL_ID = " + PID);
                            String objek3 = "PROPOSAL_STATUS = 11,PROPOSAL_STATUS_PROSES = 1, PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId;
                            MixHelper.InsertLog(logcode, objek3.Replace("'", "-"), 2);

                            int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PID);
                            var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PID + " AND T1.APPROVAL_STATUS_PROPOSAL = 10").SingleOrDefault();
                            db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PID + ",1,''," + datenow + "," + UserId + ",1,10," + APPROVAL_STATUS_SESSION + ")");

                            var SNI = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + PID + " AND AA.DOC_RELATED_TYPE = 18 AND AA.DOC_FOLDER_ID = 18 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();

                            var PROPOSAL_PNPS_CODE_FIXER = getProposal.PROPOSAL_PNPS_CODE;

                            Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/SNI/" + PROPOSAL_PNPS_CODE_FIXER));
                            string path = Server.MapPath("~/Upload/Dokumen/SNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                            string dataDir = Server.MapPath("~" + SNI.DOC_FILE_PATH + "" + SNI.DOC_FILE_NAME + "." + SNI.DOC_FILETYPE);
                            Stream stream = System.IO.File.OpenRead(dataDir);

                            Aspose.Words.Document docs = new Aspose.Words.Document(stream);
                            stream.Close();
                            string filePathdoc = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + ".DOCX";
                            string filePathpdf = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + ".PDF";
                            string filePathxml = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + ".XML";
                            //return Json(new
                            //{
                            //    sEcho = @"" + filePathdoc
                            //}, JsonRequestBehavior.AllowGet);
                            docs.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                            docs.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                            docs.Save(@"" + filePathxml);
                            int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                            var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                            var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                            var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                                        "'8', " +
                                        "'100', " +
                                        "'" + PID + "', " +
                                        "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") " + getProposal.PROPOSAL_NO_SNI_PROPOSAL + "', " +
                                        "'Data SNI dengan Nomor " + getProposal.PROPOSAL_NO_SNI_PROPOSAL + " Dengan Kode Proposal : " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                        "'" + "/Upload/Dokumen/SNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                        "'SNI_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                                        "'PDF', " +
                                        "'0', " +
                                        "'" + UserId + "', " +
                                        datenow + "," +
                                        "'1', " +
                                        "'" + LOGCODE_DATA_RSNI + "'";
                            db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DATA_RSNI + ") VALUES (" + FVALUE_DATA_RSNI.Replace("''", "NULL") + ")");
                            String objekTanggapan = FVALUE_DATA_RSNI.Replace("'", "-");
                            MixHelper.InsertLog(LOGCODE_DATA_RSNI, objekTanggapan, 1);
                        }
                        else
                        {
                            var sni_sk_list = (from a in db.TRX_SNI where a.SNI_SK_ID == tsk.SNI_SK_ID select a).ToList();
                            foreach (var sn in sni_sk_list)
                            {
                                if (sn.SNI_PROPOSAL_ID != PID)
                                {
                                    db.Database.ExecuteSqlCommand("DELETE FROM TRX_SNI WHERE SNI_ID = " + sn.SNI_ID);
                                    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET PROPOSAL_STATUS = 10,PROPOSAL_STATUS_PROSES = 3,PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId + " WHERE PROPOSAL_ID = " + sn.SNI_PROPOSAL_ID);
                                    String objek3 = "PROPOSAL_STATUS = 10,PROPOSAL_STATUS_PROSES = 3, PROPOSAL_UPDATE_DATE = " + datenow + ", PROPOSAL_UPDATE_BY = " + UserId;
                                    MixHelper.InsertLog(logcode, objek3.Replace("'", "-"), 2);

                                    //int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                                    db.Database.ExecuteSqlCommand("DELETE FROM	TRX_PROPOSAL_APPROVAL WHERE	APPROVAL_ID = (SELECT MAX(AA.APPROVAL_ID) FROM	TRX_PROPOSAL_APPROVAL AA WHERE AA.APPROVAL_PROPOSAL_ID = " + sn.SNI_PROPOSAL_ID + ")");
                                    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 1 WHERE	APPROVAL_ID = (SELECT MAX (AA.APPROVAL_ID) FROM	TRX_PROPOSAL_APPROVAL AA WHERE AA.APPROVAL_PROPOSAL_ID = " + sn.SNI_PROPOSAL_ID + " AND AA.APPROVAL_STATUS_PROPOSAL = 10)");
                                    db.Database.ExecuteSqlCommand("DELETE FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + sn.SNI_PROPOSAL_ID + " AND AA.DOC_RELATED_TYPE = 100 AND AA.DOC_FOLDER_ID = 8");
                                }
                            }
                        }
                    }
                }
            }

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "SK telah diubah";
            return RedirectToAction("Index");
        }

        public ActionResult AddSNI(int id =0) {
            var sni = db.Database.SqlQuery<VIEW_SNI_SK>("SELECT AA.SNI_SK_ID,AA.SNI_SK_SNI_ID,AA.SNI_SK_DOC_ID,AA.SNI_SK_NOMOR,AA.SNI_SK_DATE,AA.SNI_SK_DATE_NAME,AA.SNI_SK_CREATE_DATE,AA.SNI_SK_CREATE_BY,AA.SNI_SK_DATE_START,AA.SNI_SK_DATE_START_NAME,AA.SNI_SK_DATE_END,AA.SNI_SK_DATE_END_NAME,AA.JML_SNI,AA.IS_PUBLISH,AA.DOC_ID,AA.DOC_CODE,AA.DOC_FOLDER_ID,AA.DOC_NUMBER,AA.DOC_NAME,AA.DOC_FILE_PATH,AA.DOC_FILE_NAME,AA.DOC_FILETYPE,AA.DOC_LINK FROM (SELECT AA.*, ROWNUM NOMOR FROM (SELECT * FROM VIEW_SNI_SK WHERE SNI_SK_ID = " + id + " ) AA WHERE ROWNUM <= 1 ) AA WHERE NOMOR > 0").SingleOrDefault();
            ViewData["sni"] = sni;
            var listsni = (from a in db.TRX_SNI where a.SNI_SK_ID == id select a).ToList();
            ViewData["listsni"] = listsni;
            //ViewData["listSNI"] = (from t in db.VIEW_PROPOSAL where t.PROPOSAL_STATUS == 10 && t.PROPOSAL_STATUS_PROSES == 3 orderby t.PROPOSAL_CREATE_DATE ascending select t).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult AddSNI(TRX_SNI_SK tsk, int[] PROPOSAL_ID) {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            //int lastid = MixHelper.GetSequence("TRX_SNI");
            int lastidsnisk = MixHelper.GetSequence("TRX_SNI_SK");
            int lastiddoc = MixHelper.GetSequence("TRX_DOCUMENTS");
            var datenow = MixHelper.ConvertDateNow();
            int lastid_sk = Convert.ToInt32(tsk.SNI_SK_ID);
            int LASTID_MEMO_KAPUS = Convert.ToInt32(tsk.SNI_SK_DOC_ID);

            var query = "";
            if (PROPOSAL_ID.Count() > 0)
            {
                foreach (var PID in PROPOSAL_ID)
                {
                    int lastid_SNI = MixHelper.GetSequence("TRX_SNI");

                    var getProposal = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PID).SingleOrDefault();
                    query = "INSERT INTO TRX_SNI (SNI_ID, SNI_SK_ID, SNI_PROPOSAL_ID, SNI_DOC_ID, SNI_CREATE_DATE, SNI_CREATE_BY, SNI_STATUS, SNI_NOMOR, SNI_JUDUL,SNI_IS_PUBLISH) VALUES (" + lastid_SNI + "," + lastid_sk + "," + PID + ",'" + LASTID_MEMO_KAPUS + "'," + datenow + "," + UserId + ",'1','" + getProposal.PROPOSAL_NO_SNI_PROPOSAL + "','" + getProposal.PROPOSAL_JUDUL_SNI_PROPOSAL + "',0)";
                    db.Database.ExecuteSqlCommand(query);

                    int APPROVAL_ID = MixHelper.GetSequence("TRX_PROPOSAL_APPROVAL");
                    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL_APPROVAL SET APPROVAL_STATUS = 0 WHERE APPROVAL_PROPOSAL_ID = " + PID);
                    var APPROVAL_STATUS_SESSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PID + " AND T1.APPROVAL_STATUS_PROPOSAL = 10").SingleOrDefault();
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_PROPOSAL_APPROVAL (APPROVAL_ID,APPROVAL_PROPOSAL_ID,APPROVAL_TYPE,APPROVAL_REASON,APPROVAL_DATE,APPROVAL_BY,APPROVAL_STATUS,APPROVAL_STATUS_PROPOSAL,APPROVAL_STATUS_SESSION) VALUES (" + APPROVAL_ID + "," + PID + ",1,''," + datenow + "," + UserId + ",1,10," + APPROVAL_STATUS_SESSION + ")");

                    var SNI = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT AA.* FROM TRX_DOCUMENTS AA WHERE AA.DOC_RELATED_ID = " + PID + " AND AA.DOC_RELATED_TYPE = 18 AND AA.DOC_FOLDER_ID = 18 AND AA.DOC_STATUS = 1 AND AA.DOC_ID = (SELECT MAX(DOC_ID) FROM TRX_DOCUMENTS BB WHERE BB.DOC_RELATED_ID = AA.DOC_RELATED_ID AND BB.DOC_RELATED_TYPE = AA.DOC_RELATED_TYPE AND BB.DOC_FOLDER_ID = AA.DOC_FOLDER_ID AND BB.DOC_STATUS = AA.DOC_STATUS)").SingleOrDefault();

                    var PROPOSAL_PNPS_CODE_FIXER = getProposal.PROPOSAL_PNPS_CODE;

                    Directory.CreateDirectory(Server.MapPath("~/Upload/Dokumen/SNI/" + PROPOSAL_PNPS_CODE_FIXER));
                    string path = Server.MapPath("~/Upload/Dokumen/SNI/" + PROPOSAL_PNPS_CODE_FIXER + "/");
                    string dataDir = Server.MapPath("~" + SNI.DOC_FILE_PATH + "" + SNI.DOC_FILE_NAME + "." + SNI.DOC_FILETYPE);
                    Stream stream = System.IO.File.OpenRead(dataDir);

                    Aspose.Words.Document docs = new Aspose.Words.Document(stream);
                    stream.Close();
                    string filePathdoc = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + ".DOCX";
                    string filePathpdf = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + ".PDF";
                    string filePathxml = path + "SNI_" + PROPOSAL_PNPS_CODE_FIXER + ".XML";
                    //return Json(new
                    //{
                    //    sEcho = @"" + filePathdoc
                    //}, JsonRequestBehavior.AllowGet);
                    docs.Save(@"" + filePathdoc, Aspose.Words.SaveFormat.Docx);
                    docs.Save(@"" + filePathpdf, Aspose.Words.SaveFormat.Pdf);
                    docs.Save(@"" + filePathxml);
                    int LASTID_DATA_RSNI = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var LOGCODE_DATA_RSNI = MixHelper.GetLogCode();
                    var FNAME_DATA_RSNI = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_TYPE,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_DATA_RSNI = "'" + LASTID_DATA_RSNI + "', " +
                                "'8', " +
                                "'100', " +
                                "'" + PID + "', " +
                                "'" + "(" + PROPOSAL_PNPS_CODE_FIXER + ") " + getProposal.PROPOSAL_NO_SNI_PROPOSAL + "', " +
                                "'Data SNI dengan Nomor " + getProposal.PROPOSAL_NO_SNI_PROPOSAL + " Dengan Kode Proposal : " + PROPOSAL_PNPS_CODE_FIXER + "', " +
                                "'" + "/Upload/Dokumen/SNI/" + PROPOSAL_PNPS_CODE_FIXER + "/" + "', " +
                                "'SNI_" + PROPOSAL_PNPS_CODE_FIXER + "" + "', " +
                                "'PDF', " +
                                "'0', " +
                                "'" + UserId + "', " +
                                datenow + "," +
                                "'1', " +
                                "'" + LOGCODE_DATA_RSNI + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DATA_RSNI + ") VALUES (" + FVALUE_DATA_RSNI.Replace("''", "NULL") + ")");
                    String objekTanggapan = FVALUE_DATA_RSNI.Replace("'", "-");
                    MixHelper.InsertLog(LOGCODE_DATA_RSNI, objekTanggapan, 1);
                }
            }

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "SNI telah ditambahkan pada SK";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult FindRasni(string q = "", int page = 1, int jp = 0)
        {           
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);
         
            if (jp == 0)
            {
                var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL_RASNI WHERE LOWER(VIEW_PROPOSAL_RASNI.TEXT) LIKE '%" + q.ToLower() + "%'").SingleOrDefault();
                string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL_RASNI WHERE LOWER(VIEW_PROPOSAL_RASNI.TEXT) LIKE '%" + q.ToLower() + "%') T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);

                var datarasni = db.Database.SqlQuery<VIEW_PROPOSAL_RASNI>(inject_clause_select);
                var rasni = from cust in datarasni select new { id = cust.ID, text = cust.TEXT };
                //return Content(inject_clause_select);
                return Json(new { rasni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);
            }
            else {
                var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL_RASNI WHERE JP = " + Convert.ToInt32(jp) + " AND LOWER(VIEW_PROPOSAL_RASNI.TEXT) LIKE '%" + q.ToLower() + "%'").SingleOrDefault();
                string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL_RASNI WHERE JP = " + Convert.ToInt32(jp) + " AND LOWER(VIEW_PROPOSAL_RASNI.TEXT) LIKE '%" + q.ToLower() + "%') T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);

                var datarasni = db.Database.SqlQuery<VIEW_PROPOSAL_RASNI>(inject_clause_select);
                var rasni = from cust in datarasni select new { id = cust.ID, text = cust.TEXT };
                //return Content(inject_clause_select);
                return Json(new { rasni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);
            }
            
           
            //return Json(new { query = inject_clause_select }, JsonRequestBehavior.AllowGet);
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


            string where_clause = "PROPOSAL_STATUS = 10 AND PROPOSAL_STATUS_PROSES = 3  " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN (" + BIDANG_ID + ",0)" : "");

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
                search_clause += " OR LOWER(PROPOSAL_CREATE_DATE_NAME) LIKE LOWER('%" + search + "%'))";
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
                Convert.ToString(list.PROPOSAL_PNPS_CODE),
                Convert.ToString(list.PROPOSAL_NO_SNI_PROPOSAL),
                Convert.ToString(list.MONITORING_NO_MEMO_DEPUTI),
                Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_SNI_PROPOSAL+"</span>"),
                Convert.ToString("<center>"+list.PROPOSAL_IS_URGENT_NAME+"</center>"),
                //Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
                Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
                Convert.ToString("<center><a href='/Penetapan/PenetapanSNI/DetailRSNI/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a><a href='/Penetapan/PenetapanSNI/create/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Pengesahan RASNI'><i class='action fa fa-check'></i></a><a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),

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
