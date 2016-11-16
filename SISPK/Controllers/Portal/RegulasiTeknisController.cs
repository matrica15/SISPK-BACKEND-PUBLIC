using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.Security.Cryptography;
using System.IO;

namespace SISPK.Controllers.Portal
{
    public class RegulasiTeknisController : Controller
    {
        //
        // GET: /RegulasiTeknis/
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List_regulasi_teknis(DataTables param, int status=0)
        {
            var default_order = "RETEK_NO_SK";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("RETEK_NO_SK");
            order_field.Add("REGULATOR");
            order_field.Add("RETEK_KETERANGAN");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = " RETEK_STATUS = "+status;

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
                search_clause += " OR RETEK_NO_SK = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_REGTEK WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_REGTEK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_REGTEK>(inject_clause_select);

            //var result = from list in SelectedData
            //             select new string[] 
            //{ 
            //    Convert.ToString("<center>"+list.PROPOSAL_CREATE_DATE_NAME+"</center>"),
            //    Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
            //    Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.PROPOSAL_JUDUL_PNPS+"</span>"),
            //    Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME), 
            //    Convert.ToString("<center>"+list.PROPOSAL_TAHAPAN+"</center>"),
            //    Convert.ToString("<center>"+list.PROPOSAL_STATUS_NAME+"</center>"),
            //    Convert.ToString("<center><a href='/Pengajuan/Usulan/Detail/"+list.PROPOSAL_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a>"+((list.PROPOSAL_STATUS==0 && list.PROPOSAL_APPROVAL_STATUS == 1)?"<a href='/Pengajuan/Usulan/Update/"+list.PROPOSAL_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Ubah'><i class='action fa fa-edit'></i></a><a href='javascript:void(0)' onclick='hapus_usulan("+list.PROPOSAL_ID+")' class='btn red btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Hapus'><i class='action glyphicon glyphicon-remove'></i></a>":"")+"<a href='javascript:void(0)' onclick='cetak_usulan("+list.PROPOSAL_ID+")'  class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),

            //};
            //<a data-original-title='Tambah SNI' data-placement='top' data-container='body' class='btn yellow btn-sm action tooltips' href='/Portal/RegulasiTeknis/TambahSNI/" + reg.RETEK_ID + "'><i class='action fa fa-plus'></i></a>
            var regtek = from reg in SelectedData
                         select new
                         {
                             RETEK_NO_SK = "<a href='RegulasiTeknis/Read/" + reg.RETEK_ID + "'>" + reg.RETEK_NO_SK + "</a>",
                             RETEK_TENTANG = reg.RETEK_TENTANG,
                             REGULATOR = reg.REGULATOR,
                             RETEK_KETERANGAN = reg.RETEK_KETERANGAN,
                             RETEK_ACTION = (reg.RETEK_STATUS == 1) ? "<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Portal/RegulasiTeknis/Read/" + reg.RETEK_ID + "'><i class='action fa fa-file-text-o'></i></a><a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Portal/RegulasiTeknis/Edit/" + reg.RETEK_ID + "'><i class='action fa fa-edit'></i></a><a data-original-title='Nonaktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Portal/RegulasiTeknis/nonaktifkan/" + reg.RETEK_ID + "'><i class='action glyphicon glyphicon-remove'></i></a></center>" : "<center><a data-original-title='Tambah SNI' data-placement='top' data-container='body' class='btn yellow btn-sm action tooltips' href='/Portal/RegulasiTeknis/TambahSNI/" + reg.RETEK_ID + "'><i class='action fa fa-plus'></i></a><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Portal/RegulasiTeknis/Read/" + reg.RETEK_ID + "'><i class='action fa fa-file-text-o'></i></a><a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Portal/RegulasiTeknis/Edit/" + reg.RETEK_ID + "'><i class='action fa fa-edit'></i></a><a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Portal/RegulasiTeknis/aktifkan/" + reg.RETEK_ID + "'><i class='action glyphicon glyphicon-ok'></i></a></center>",
                             RETEK_FILE = (reg.RETEK_FILE != null)?"<a href='../" + reg.DOC_FILE_PATH + "/"+reg.DOC_FILE_NAME+"."+reg.DOC_FILETYPE+"' download><img border='0' width='15' height='15' alt='download' src='http://sisni.bsn.go.id//static/images/pdf.ico'></a>" : "-",
                             RETEK_SNI_TERKAIT = "-"
                         };


            return Json(new
            {
                draw = param.sEcho,
                recordsTotal = CountData,
                recordsFiltered = CountData,
                data = regtek
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create() {
            ViewData["listSNI"] = (from t in db.VIEW_SNI where t.PROPOSAL_STATUS == 11 orderby t.PROPOSAL_CREATE_DATE ascending select t).ToList();
            ViewData["listregulasi"] = (from t in db.VIEW_INSTANSI where t.INSTANSI_STATUS == 1 select t).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Create(TRX_REGULASI_TEKNIS trt, FormCollection formCollection)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("TRX_REGULASI_TEKNIS");
            int lastid_detail = MixHelper.GetSequence("TRX_REGULASI_TEKNIS_DETAIL");
            int lastid_doc = MixHelper.GetSequence("TRX_DOCUMENTS");
            var datenow = MixHelper.ConvertDateNow();

            string path = Server.MapPath("~/Upload/Dokumen/SK_REGULASI/");
            HttpPostedFileBase file_att = Request.Files["file_regtek"];
            var file_name_att = "";
            var filePath = "";
            var fileExtension = "";
            if (file_att != null)
            {
                string lampiranregulasipath = file_att.FileName;
                if (lampiranregulasipath.Trim() != "")
                {
                    lampiranregulasipath = Path.GetFileNameWithoutExtension(file_att.FileName);
                    fileExtension = Path.GetExtension(file_att.FileName);
                    file_name_att = "RegulasiTeknis_"+trt.RETEK_NO_SK.Replace('/','-')+"_" + lastid + fileExtension;
                    filePath = path + file_name_att;
                    file_att.SaveAs(filePath);
                }
            }

            var logcodeDOC = MixHelper.GetLogCode();            
            var FNAME_DOC  = "DOC_ID,DOC_FOLDER_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
            var FVALUE_DOC = lastid_doc + ", " +
                        "'4', " +
                        "'"+ trt.RETEK_NO_SK.ToUpper() + "', " +
                        "'Regulasi Teknis dengan Nomor " + trt.RETEK_NO_SK.ToUpper() + "', " +
                        "'" + "/Upload/Dokumen/SK_REGULASI/', " +
                        "'" + "RegulasiTeknis_" + trt.RETEK_NO_SK.Replace('/', '-') + "_" + lastid + "', " +
                        "'" + fileExtension.Replace(".", "").ToUpper() + "', " +
                        "'0', " +
                        "'" + UserId + "', " +
                        datenow + "," +
                        "'1', " +
                        "'" + logcodeDOC + "'";
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DOC + ") VALUES (" + FVALUE_DOC.Replace("''", "NULL") + ")");
            String objekDOC = FVALUE_DOC.Replace("'", "-");
            MixHelper.InsertLog(logcodeDOC, objekDOC, 1);

            var fname = "RETEK_ID,RETEK_NO_SK,RETEK_TENTANG,RETEK_REGULATOR,RETEK_KETERANGAN,RETEK_FILE,RETEK_CREATE_BY,RETEK_CREATE_DATE,RETEK_STATUS";
            var fvalue = "'" + lastid + "'," +
                         "'" + trt.RETEK_NO_SK + "'," +
                         "'" + trt.RETEK_TENTANG + "'," +
                         "'" + trt.RETEK_REGULATOR + "'," +
                         "'" + trt.RETEK_KETERANGAN + "'," +
                         "" + lastid_doc + "," +
                         "" + UserId + "," +
                         datenow + "," +
                         "1";

            db.Database.ExecuteSqlCommand("INSERT INTO TRX_REGULASI_TEKNIS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);

            var idk = db.Database.SqlQuery<int>("SELECT MAX(MK.RETEK_ID) FROM TRX_REGULASI_TEKNIS MK").SingleOrDefault();

            var sni_id = formCollection["RETEK_DETAIL_SNI_ID"];
            if (sni_id != null)
            {   //int n = 0;

                string[] vals = sni_id.Split(',');
                for (int n = 0;n < vals.Length; n++)
                {
                    int lastid_mki = MixHelper.GetSequence("TRX_REGULASI_TEKNIS_DETAIL");
                    //string query_update = "INSERT INTO MASTER_KOMTEK_ICS (KOMTEK_ICS_ID, KOMTEK_ICS_KOMTEK_ID, KOMTEK_ICS_ICS_ID, KOMTEK_ICS_CREATE_BY, KOMTEK_ICS_CREATE_DATE, KOMTEK_ICS_STATUS, KOMTEK_ICS_LOG_CODE) VALUES (" + lastid_mki + "," + lastid + "," + vals[n] + "," + UserId + "," + datenow + ",1,'" + logcode + "')";
                    //db.Database.ExecuteSqlCommand(query_update);
                    //return Json(new { query = query_update, id = komtek_ics_id });
                    var fname1 = "RETEK_DETAIL_ID,RETEK_DETAIL_RETEK_ID,RETEK_DETAIL_SNI_ID,RETEK_DETAIL_CREATE_BY,RETEK_DETAIL_CREATE_DATE,RETEK_DETAIL_STATUS";
                    var fvalue1 = "'" + lastid_mki + "'," +
                                 "'" + idk + "'," +
                                 "'" + vals[n] + "'," +
                                 "" + UserId + "," +
                                 datenow + "," +
                                 "1";

                    //return Json(new { query = "INSERT INTO TRX_REGULASI_TEKNIS (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_REGULASI_TEKNIS_DETAIL (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")");

                    String objek1 = fvalue1.Replace("'", "-");
                    MixHelper.InsertLog(logcode, objek1, 1);
                }
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Edit( int id = 0)
        {
            ViewData["id"] = id;
            ViewData["listSNI"] = (from t in db.VIEW_REGTEK_DETAIL where t.RETEK_DETAIL_RETEK_ID == id && t.RETEK_DETAIL_STATUS != 0 orderby t.RETEK_DETAIL_ID ascending select t).ToList();
            ViewData["listregulasi"] = (from t in db.VIEW_INSTANSI where t.INSTANSI_STATUS == 1 select t).ToList();
            ViewData["regulasi"] = (from t in db.VIEW_REGTEK where t.RETEK_ID == id select t).SingleOrDefault();
            return View();
        }

        [HttpPost]
        public ActionResult Edit(TRX_REGULASI_TEKNIS trt, FormCollection formCollection)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("TRX_REGULASI_TEKNIS");
            var datenow = MixHelper.ConvertDateNow();
            int lastid_doc = MixHelper.GetSequence("TRX_DOCUMENTS");
            var status = "1";
            var update = "";

            string path = Server.MapPath("~/Upload/Dokumen/SK_REGULASI/");
            HttpPostedFileBase file_att = Request.Files["file_regtek"];
            var file_name_att = "";
            var filePath = "";
            var fileExtension = "";

                string lampiranregulasipath = file_att.FileName;
                if (lampiranregulasipath.Trim() != "")
                {
                    lampiranregulasipath = Path.GetFileNameWithoutExtension(file_att.FileName);
                    fileExtension = Path.GetExtension(file_att.FileName);
                    file_name_att = "RegulasiTeknis_" + trt.RETEK_NO_SK.Replace('/', '-') + "_" + lastid + fileExtension;
                    filePath = path + file_name_att;
                    file_att.SaveAs(filePath);


                    var logcodeDOC1 = MixHelper.GetLogCode();
                    var id = trt.RETEK_ID;
                    var qupdate = "UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_ID = " + id;
                    db.Database.ExecuteSqlCommand(qupdate);
                    String objekDOC1 = qupdate.Replace("'", "-");
                    MixHelper.InsertLog(logcodeDOC1, objekDOC1, 1);

                    var logcodeDOC = MixHelper.GetLogCode();
                    var FNAME_DOC = "DOC_ID,DOC_FOLDER_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                    var FVALUE_DOC = lastid_doc + ", " +
                                "'4', " +
                                "'" + trt.RETEK_NO_SK.ToUpper() + "', " +
                                "'Regulasi Teknis dengan Nomor " + trt.RETEK_NO_SK.ToUpper() + "', " +
                                "'" + "/Upload/Dokumen/SK_REGULASI/', " +
                                "'" + "RegulasiTeknis_" + trt.RETEK_NO_SK.Replace('/', '-') + "_" + lastid + "', " +
                                "'" + fileExtension.Replace(".", "").ToUpper() + "', " +
                                "'0', " +
                                "'" + UserId + "', " +
                                datenow + "," +
                                "'1', " +
                                "'" + logcodeDOC + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DOC + ") VALUES (" + FVALUE_DOC.Replace("''", "NULL") + ")");
                    String objekDOC = FVALUE_DOC.Replace("'", "-");
                    MixHelper.InsertLog(logcodeDOC, objekDOC, 1);

                    update =
                            "RETEK_NO_SK = '" + trt.RETEK_NO_SK + "'," +
                            "RETEK_TENTANG = '" + trt.RETEK_TENTANG + "'," +
                            "RETEK_REGULATOR = '" + trt.RETEK_REGULATOR + "'," +
                            "RETEK_KETERANGAN = '" + trt.RETEK_KETERANGAN + "'," +
                            "RETEK_FILE = '" + trt.RETEK_FILE + "'," +
                            "RETEK_UPDATE_BY = " + UserId + "," +
                            "RETEK_UPDATE_DATE = " + datenow + "," +
                            "RETEK_STATUS = '" + status + "'";
                }
                else {
                    update =
                       "RETEK_NO_SK = '" + trt.RETEK_NO_SK + "'," +
                       "RETEK_TENTANG = '" + trt.RETEK_TENTANG + "'," +
                       "RETEK_REGULATOR = '" + trt.RETEK_REGULATOR + "'," +
                       "RETEK_KETERANGAN = '" + trt.RETEK_KETERANGAN + "'," +
                       "RETEK_UPDATE_BY = " + UserId + "," +
                       "RETEK_UPDATE_DATE = " + datenow + "," +
                       "RETEK_STATUS = '" + status + "'";
                }



            var clause = "where RETEK_ID = " + trt.RETEK_ID;
            //return Json(new { query = "UPDATE TRX_REGULASI_TEKNIS SET " + update.Replace("''", "NULL") + " " + clause }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE TRX_REGULASI_TEKNIS SET " + update.Replace("''", "NULL") + " " + clause);

            var idk = db.Database.SqlQuery<int>("SELECT MAX(MK.RETEK_ID) FROM TRX_REGULASI_TEKNIS MK").SingleOrDefault();

            var sni_id = formCollection["RETEK_DETAIL_SNI_ID"];
            if (sni_id != null)
            {   //int n = 0;
                var sni_list = (from t in db.VIEW_REGTEK_DETAIL where t.RETEK_DETAIL_ID == trt.RETEK_ID select t).ToList();

                string[] vals = sni_id.Split(',');
                string query_update = "UPDATE TRX_REGULASI_TEKNIS_DETAIL  SET RETEK_DETAIL_STATUS = 0, RETEK_DETAIL_UPDATE_BY =" + UserId + ", RETEK_DETAIL_UPDATE_DATE=" + datenow + " WHERE RETEK_DETAIL_RETEK_ID = " + trt.RETEK_ID;
                db.Database.ExecuteSqlCommand(query_update);

                for (int n = 0; n < vals.Length; n++)
                {
                    //int lastid_mki = MixHelper.GetSequence("TRX_REGULASI_TEKNIS_DETAIL");

                    int cek = db.Database.SqlQuery<int>("SELECT COUNT(1) AS JML FROM TRX_REGULASI_TEKNIS_DETAIL WHERE RETEK_DETAIL_RETEK_ID = '" + trt.RETEK_ID + "' AND RETEK_DETAIL_SNI_ID = '" + vals[n] + "'").SingleOrDefault();
                    if (cek == 0)
                    {
                        int lastid_mki = MixHelper.GetSequence("TRX_REGULASI_TEKNIS_DETAIL");
                        string query_insert = "INSERT INTO TRX_REGULASI_TEKNIS_DETAIL (RETEK_DETAIL_ID,RETEK_DETAIL_RETEK_ID,RETEK_DETAIL_SNI_ID,RETEK_DETAIL_UPDATE_BY,RETEK_DETAIL_UPDATE_DATE,RETEK_DETAIL_STATUS) VALUES (" + lastid_mki + "," + trt.RETEK_ID + "," + vals[n] + "," + UserId + "," + datenow + ",1)";
                        db.Database.ExecuteSqlCommand(query_insert);
                    }
                    else
                    {
                        string query_updatea = "UPDATE TRX_REGULASI_TEKNIS_DETAIL  SET RETEK_DETAIL_STATUS = 1, RETEK_DETAIL_UPDATE_BY =" + UserId + ", RETEK_DETAIL_UPDATE_DATE=" + datenow + " WHERE RETEK_DETAIL_RETEK_ID = " + trt.RETEK_ID + " AND RETEK_DETAIL_SNI_ID = '" + vals[n] + "'";
                        //return Json(new { data = query_updatea }, JsonRequestBehavior.AllowGet);
                        db.Database.ExecuteSqlCommand(query_updatea);
                    }
                }
            }
            else {
                string query_updates = "UPDATE TRX_REGULASI_TEKNIS_DETAIL  SET RETEK_DETAIL_STATUS = 0, RETEK_DETAIL_UPDATE_BY =" + UserId + ", RETEK_DETAIL_UPDATE_DATE=" + datenow + " WHERE RETEK_DETAIL_RETEK_ID = " + trt.RETEK_ID;
                db.Database.ExecuteSqlCommand(query_updates);
            }
            //var logId = AuditTrails.GetLogId();
            String objek = update.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Read(int id = 0) {
            ViewData["regtek"] = (from a in db.VIEW_REGTEK where a.RETEK_ID == id select a).SingleOrDefault();
            ViewData["regtekdetail"] = (from b in db.VIEW_REGTEK_DETAIL where b.RETEK_DETAIL_RETEK_ID == id select b).ToList();
            return View();
        }

        public ActionResult TambahSNI(int id = 0) {
            ViewData["regtek"] = (from t in db.VIEW_REGTEK where t.RETEK_ID == id select t).SingleOrDefault();
            ViewData["listSNI"] = (from t in db.VIEW_PROPOSAL where t.PROPOSAL_STATUS == 11 && t.PROPOSAL_STATUS_PROSES == 3 orderby t.PROPOSAL_CREATE_DATE ascending select t).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult TambahSNI(TRX_REGULASI_TEKNIS trt, FormCollection formCollection)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("TRX_REGULASI_TEKNIS_DETAIL");
            var datenow = MixHelper.ConvertDateNow();

            var sni_id = formCollection["RETEK_DETAIL_SNI_ID"];
            if (sni_id != null)
            {   //int n = 0;

                string[] vals = sni_id.Split(',');
                for (int n = 0; n < vals.Length; n++)
                {
                    int lastid_mki = MixHelper.GetSequence("TRX_REGULASI_TEKNIS_DETAIL");
                    var jml = db.Database.SqlQuery<int>("SELECT COUNT(*) FROM TRX_REGULASI_TEKNIS_DETAIL MK WHERE RETEK_DETAIL_RETEK_ID = " + trt.RETEK_ID + "AND RETEK_DETAIL_SNI_ID = " + vals[n]);

                    if (Convert.ToInt32(jml) == 0) {
                    //string query_update = "INSERT INTO MASTER_KOMTEK_ICS (KOMTEK_ICS_ID, KOMTEK_ICS_KOMTEK_ID, KOMTEK_ICS_ICS_ID, KOMTEK_ICS_CREATE_BY, KOMTEK_ICS_CREATE_DATE, KOMTEK_ICS_STATUS, KOMTEK_ICS_LOG_CODE) VALUES (" + lastid_mki + "," + lastid + "," + vals[n] + "," + UserId + "," + datenow + ",1,'" + logcode + "')";
                    //db.Database.ExecuteSqlCommand(query_update);
                    //return Json(new { query = query_update, id = komtek_ics_id });
                    var fname1 = "RETEK_DETAIL_ID,RETEK_DETAIL_RETEK_ID,RETEK_DETAIL_SNI_ID,RETEK_DETAIL_CREATE_BY,RETEK_DETAIL_CREATE_DATE,RETEK_DETAIL_STATUS";
                    var fvalue1 = "'" + lastid_mki + "'," +
                                 "'" + trt.RETEK_ID + "'," +
                                 "'" + vals[n] + "'," +
                                 "" + UserId + "," +
                                 datenow + "," +
                                 "1";

                    //return Json(new { query = "INSERT INTO TRX_REGULASI_TEKNIS (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_REGULASI_TEKNIS_DETAIL (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")");

                    String objek1 = fvalue1.Replace("'", "-");
                    MixHelper.InsertLog(logcode, objek1, 1);
                    }
                }
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult aktifkan(int id = 0){
            db.Database.ExecuteSqlCommand("UPDATE TRX_REGULASI_TEKNIS SET RETEK_STATUS = 1 WHERE RETEK_ID = " + id);
            db.Database.ExecuteSqlCommand("UPDATE TRX_REGULASI_TEKNIS_DETAIL SET RETEK_DETAIL_STATUS = 1 WHERE RETEK_DETAIL_RETEK_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Aktifkan";
            return RedirectToAction("Index");
        }
        public ActionResult nonaktifkan(int id = 0) {
            db.Database.ExecuteSqlCommand("UPDATE TRX_REGULASI_TEKNIS SET RETEK_STATUS = 0 WHERE RETEK_ID = " + id);
            db.Database.ExecuteSqlCommand("UPDATE TRX_REGULASI_TEKNIS_DETAIL SET RETEK_DETAIL_STATUS = 0 WHERE RETEK_DETAIL_RETEK_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult getsni(string q = "")
        {
            var SelectedData = db.Database.SqlQuery<VIEW_SNI>("SELECT * FROM VIEW_SNI WHERE UPPER(SNI_JUDUL) LIKE UPPER('%" + q + "%')");
            var results = new List<object>();
            foreach (var listField in SelectedData)
            {
                results.Add(new
                {
                    id_sni = listField.SNI_ID.ToString(),
                    code_sni = listField.SNI_NOMOR.ToString(),
                    name_sni = listField.SNI_JUDUL.ToString(),
                    //name_sni_eng = ((listField.ICS_NAME_IND == null) ? listField.ICS_NAME.ToString() : listField.ICS_NAME_IND.ToString())
                });
            }
            return Json(results, JsonRequestBehavior.AllowGet);
        }
    }
}
