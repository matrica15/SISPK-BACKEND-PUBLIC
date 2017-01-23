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

namespace SISPK.Controllers.Master
{
    public class SubkomtekController : Controller
    {
        //
        // GET: /Subkomtek/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            ViewData["listICS"] = (from t in db.MASTER_ICS where t.ICS_STATUS == 1 orderby t.ICS_CODE ascending select t).ToList();
            ViewData["listbidang"] = (from t in db.MASTER_BIDANG where t.BIDANG_STATUS == 1 orderby t.BIDANG_CODE ascending select t).ToList();
            ViewData["listinstansi"] = (from t in db.MASTER_INSTANSI where t.INSTANSI_STATUS == 1 orderby t.INSTANSI_CODE ascending select t).ToList();
            ViewData["listparent"] = (from t in db.VIEW_KOMTEK where t.KOMTEK_STATUS == 1 orderby t.KOMTEK_CODE ascending select t).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Create(MASTER_KOMITE_TEKNIS m_komtek, FormCollection formCollection, int[] KOMTEK_ICS_ID)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_KOMITE_TEKNIS");
            var datenow = MixHelper.ConvertDateNow();
            DateTime dates = Convert.ToDateTime(m_komtek.KOMTEK_TANGGAL_PEMBENTUKAN);
            var date = "TO_DATE('" + dates.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";
            //var parent = "";

            //KOMTEK_BIDANG_ID,KOMTEK_INSTANSI_ID,

            int lastid_doc = MixHelper.GetSequence("TRX_DOCUMENTS");

            //string pathnya = Server.MapPath("~/Upload/Dokumen/KOMTEK_CV/");
            //HttpPostedFileBase file_cv = Request.Files["KOMTEK_CV"];
            //var file_name_cv = "";
            //var filePath_cv = "";
            //var fileExtension_cv = "";
            //if (file_cv != null)
            //{
            //    //Check whether Directory (Folder) exists.
            //    if (!Directory.Exists(pathnya))
            //    {
            //        //If Directory (Folder) does not exists. Create it.
            //        Directory.CreateDirectory(pathnya);
            //    }
            //    string lampiranregulasipath = file_cv.FileName;
            //    if (lampiranregulasipath.Trim() != "")
            //    {
            //        lampiranregulasipath = Path.GetFileNameWithoutExtension(file_cv.FileName);
            //        fileExtension_cv = Path.GetExtension(file_cv.FileName);
            //        file_name_cv = "Komtek_CV_" + m_komtek.KOMTEK_CODE.Replace('/', '-') + "_" + lastid + fileExtension_cv;
            //        filePath_cv = pathnya + file_name_cv;
            //        file_cv.SaveAs(filePath_cv);
            //    }
            //}

            //var fname = "KOMTEK_ID,KOMTEK_PARENT_CODE,KOMTEK_CODE,KOMTEK_BIDANG_ID,KOMTEK_INSTANSI_ID,KOMTEK_NAME,KOMTEK_SEKRETARIAT,KOMTEK_ADDRESS,KOMTEK_PHONE,KOMTEK_FAX,KOMTEK_EMAIL,KOMTEK_SK_PENETAPAN,KOMTEK_CV,KOMTEK_TANGGAL_PEMBENTUKAN,KOMTEK_DESCRIPTION,KOMTEK_CREATE_BY,KOMTEK_CREATE_DATE,KOMTEK_LOG_CODE,KOMTEK_CONTACT_PERSON,KOMTEK_KETERANGAN,KOMTEK_STATUS";
            var fname = "KOMTEK_ID,KOMTEK_PARENT_CODE,KOMTEK_CODE,KOMTEK_BIDANG_ID,KOMTEK_INSTANSI_ID,KOMTEK_NAME,KOMTEK_SEKRETARIAT,KOMTEK_ADDRESS,KOMTEK_PHONE,KOMTEK_FAX,KOMTEK_EMAIL,KOMTEK_SK_PENETAPAN,KOMTEK_TANGGAL_PEMBENTUKAN,KOMTEK_DESCRIPTION,KOMTEK_CREATE_BY,KOMTEK_CREATE_DATE,KOMTEK_LOG_CODE,KOMTEK_CONTACT_PERSON,KOMTEK_KETERANGAN,KOMTEK_LINGKUP,KOMTEK_STATUS";
            var fvalue = "'" + lastid + "', " +
                        "'" + m_komtek.KOMTEK_PARENT_CODE + "', " +
                        "'" + m_komtek.KOMTEK_CODE + "', " +
                        "'" + m_komtek.KOMTEK_BIDANG_ID + "', " +
                        "'" + m_komtek.KOMTEK_INSTANSI_ID + "'," +
                        "'" + m_komtek.KOMTEK_NAME + "'," +
                        "'" + m_komtek.KOMTEK_SEKRETARIAT + "'," +
                        "'" + m_komtek.KOMTEK_ADDRESS + "'," +
                        "'" + m_komtek.KOMTEK_PHONE + "'," +
                        "'" + m_komtek.KOMTEK_FAX + "'," +
                        "'" + m_komtek.KOMTEK_EMAIL + "'," +
                        "'" + m_komtek.KOMTEK_SK_PENETAPAN + "'," +
                        //"'/Upload/Dokumen/KOMTEK_CV/" + file_name_cv + "'," +
                        date + "," +
                        "'" + m_komtek.KOMTEK_DESCRIPTION + "'," +
                        "'" + UserId + "'," +
                         datenow + "," +
                        "'" + logcode + "'," +
                        "'" + m_komtek.KOMTEK_CONTACT_PERSON + "'," +
                        "'" + m_komtek.KOMTEK_KETERANGAN + "'," +
                        "'" + m_komtek.KOMTEK_LINGKUP + "'," +
                        "1";
            //return Json(new { query = "INSERT INTO MASTER_KOMITE_TEKNIS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("INSERT INTO MASTER_KOMITE_TEKNIS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            var idk = db.Database.SqlQuery<int>("SELECT MAX(MK.KOMTEK_ID) FROM MASTER_KOMITE_TEKNIS MK").SingleOrDefault();
            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);

            if (KOMTEK_ICS_ID != null)
            {
                foreach (var ICS_ID in KOMTEK_ICS_ID)
                {
                    var logcodeS = MixHelper.GetLogCode();
                    int lastid_mki = MixHelper.GetSequence("MASTER_KOMTEK_ICS");
                    string query_create = "INSERT INTO MASTER_KOMTEK_ICS (KOMTEK_ICS_ID, KOMTEK_ICS_KOMTEK_ID, KOMTEK_ICS_ICS_ID, KOMTEK_ICS_CREATE_BY, KOMTEK_ICS_CREATE_DATE, KOMTEK_ICS_STATUS, KOMTEK_ICS_LOG_CODE) VALUES (" + lastid_mki + "," + lastid + "," + ICS_ID + "," + UserId + "," + datenow + ",1,'" + logcodeS + "')";
                    db.Database.ExecuteSqlCommand(query_create);

                    String objek1 = query_create.Replace("'", "-");
                    MixHelper.InsertLog(logcodeS, objek1, 1);
                }
            }

            string path = Server.MapPath("~/Upload/Dokumen/KOMTEK_SK/");
            HttpPostedFileBase file_att = Request.Files["FILE_KOMTEK_SK_PENETAPAN"];
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
                    file_name_att = "Komtek_" + m_komtek.KOMTEK_CODE.Replace('/', '-') + "_" + lastid + fileExtension;
                    filePath = path + file_name_att;
                    file_att.SaveAs(filePath);
                }

                var logcodeDOC = MixHelper.GetLogCode();
                var FNAME_DOC = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DOC = lastid_doc + ", " +
                            "'22', " +
                            "'" + lastid + "', " +
                            "'SK_" + m_komtek.KOMTEK_CODE.ToUpper() + "', " +
                            "'SK Komtek dengan Nomor " + m_komtek.KOMTEK_CODE.ToUpper() + "', " +
                            "'" + "/Upload/Dokumen/KOMTEK_SK/', " +
                            "'" + "Komtek_" + m_komtek.KOMTEK_CODE.Replace('/', '-') + "_" + lastid + "', " +
                            "'" + fileExtension.Replace(".", "").ToUpper() + "', " +
                            "'0', " +
                            "'" + UserId + "', " +
                            datenow + "," +
                            "'1', " +
                            "'" + logcodeDOC + "'";

                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DOC + ") VALUES (" + FVALUE_DOC.Replace("''", "NULL") + ")");
                String objekDOC = FVALUE_DOC.Replace("'", "-");
                MixHelper.InsertLog(logcodeDOC, objekDOC, 1);
            }

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("CreateAnggotaSub/" + lastid);
        }

       
        public ActionResult Edit(int id = 0)
        {
            ViewData["listICS"] = (from t in db.MASTER_ICS where t.ICS_STATUS == 1 orderby t.ICS_CODE ascending select t).ToList();
            ViewData["listbidang"] = (from t in db.MASTER_BIDANG where t.BIDANG_STATUS == 1 orderby t.BIDANG_CODE ascending select t).ToList();
            ViewData["listinstansi"] = (from t in db.MASTER_INSTANSI where t.INSTANSI_STATUS == 1 orderby t.INSTANSI_CODE ascending select t).ToList();
            ViewData["listparent"] = (from t in db.VIEW_KOMTEK where t.KOMTEK_STATUS == 1 orderby t.KOMTEK_CODE ascending select t).ToList();
            ViewData["komtek_item"] = (from t in db.VIEW_SUBKOMTEK where t.KOMTEK_ID == id select t).SingleOrDefault();
            ViewData["komtek_ics"] = (from t in db.VIEW_KOMTEK_ICS where t.KOMTEK_ICS_KOMTEK_ID == id select t).ToList();
            ViewData["sk_file"] = (from t in db.TRX_DOCUMENTS where t.DOC_FOLDER_ID == 22 && t.DOC_RELATED_ID == id && t.DOC_STATUS == 1 select t).SingleOrDefault();

            return View();
        }

        [HttpPost]
        public ActionResult Edit(MASTER_KOMITE_TEKNIS m_komtek, FormCollection formCollection, int[] KOMTEK_ICS_ID, TRX_DOCUMENTS doc)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_KOMITE_TEKNIS");
            var datenow = MixHelper.ConvertDateNow();
            DateTime dates = Convert.ToDateTime(m_komtek.KOMTEK_TANGGAL_PEMBENTUKAN);
            var date = "TO_DATE('" + dates.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";
            var status = "1";

            var update = "";

            string path_cv = Server.MapPath("~/Upload/Dokumen/KOMTEK_CV/");
            HttpPostedFileBase file_cv = Request.Files["KOMTEK_CV"];
            var file_name_cv = "";
            var filePath_cv = "";
            var fileExtension_cv = "";
            if (file_cv != null)
            {
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(path_cv))
                {
                    //If Directory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(path_cv);
                }
                string lampiranregulasipath = file_cv.FileName;
                if (lampiranregulasipath.Trim() != "")
                {
                    lampiranregulasipath = Path.GetFileNameWithoutExtension(file_cv.FileName);
                    fileExtension_cv = Path.GetExtension(file_cv.FileName);
                    file_name_cv = "Komtek_" + m_komtek.KOMTEK_CODE.Replace('/', '-') + "_" + DateTime.Today.ToString("dd-MM-yyyy") + fileExtension_cv;
                    filePath_cv = path_cv + file_name_cv;
                    file_cv.SaveAs(filePath_cv);
                }
                update =
                           "KOMTEK_BIDANG_ID = '" + m_komtek.KOMTEK_BIDANG_ID + "'," +
                           "KOMTEK_INSTANSI_ID = '" + m_komtek.KOMTEK_INSTANSI_ID + "'," +
                           "KOMTEK_NAME = '" + m_komtek.KOMTEK_NAME + "'," +
                           "KOMTEK_SEKRETARIAT = '" + m_komtek.KOMTEK_SEKRETARIAT + "'," +
                           "KOMTEK_ADDRESS = '" + m_komtek.KOMTEK_ADDRESS + "'," +
                           "KOMTEK_PHONE = '" + m_komtek.KOMTEK_PHONE + "'," +
                           "KOMTEK_FAX = '" + m_komtek.KOMTEK_FAX + "'," +
                           "KOMTEK_EMAIL = '" + m_komtek.KOMTEK_EMAIL + "'," +
                           "KOMTEK_SK_PENETAPAN = '" + m_komtek.KOMTEK_SK_PENETAPAN + "'," +
                           //"KOMTEK_CV = 'Upload/Dokumen/KOMTEK_CV/" + file_name_cv + "'," +
                           "KOMTEK_TANGGAL_PEMBENTUKAN = " + date + "," +
                           "KOMTEK_DESCRIPTION = '" + m_komtek.KOMTEK_DESCRIPTION + "'," +
                           "KOMTEK_CONTACT_PERSON = '" + m_komtek.KOMTEK_CONTACT_PERSON + "'," +
                           "KOMTEK_KETERANGAN = '" + m_komtek.KOMTEK_KETERANGAN + "'," +
                           "KOMTEK_CREATE_BY = '" + UserId + "'," +
                           "KOMTEK_CREATE_DATE = " + datenow + "," +
                           "KOMTEK_STATUS = '" + status + "'," +
                           "KOMTEK_LINGKUP = '" + m_komtek.KOMTEK_LINGKUP + "'," +
                           "KOMTEK_LOG_CODE = '" + m_komtek.KOMTEK_LOG_CODE + "'";
            }
            else
            {

                update =
                            "KOMTEK_BIDANG_ID = '" + m_komtek.KOMTEK_BIDANG_ID + "'," +
                            "KOMTEK_INSTANSI_ID = '" + m_komtek.KOMTEK_INSTANSI_ID + "'," +
                            "KOMTEK_NAME = '" + m_komtek.KOMTEK_NAME + "'," +
                            "KOMTEK_SEKRETARIAT = '" + m_komtek.KOMTEK_SEKRETARIAT + "'," +
                            "KOMTEK_ADDRESS = '" + m_komtek.KOMTEK_ADDRESS + "'," +
                            "KOMTEK_PHONE = '" + m_komtek.KOMTEK_PHONE + "'," +
                            "KOMTEK_FAX = '" + m_komtek.KOMTEK_FAX + "'," +
                            "KOMTEK_EMAIL = '" + m_komtek.KOMTEK_EMAIL + "'," +
                            "KOMTEK_SK_PENETAPAN = '" + m_komtek.KOMTEK_SK_PENETAPAN + "'," +
                            "KOMTEK_TANGGAL_PEMBENTUKAN = " + date + "," +
                            "KOMTEK_DESCRIPTION = '" + m_komtek.KOMTEK_DESCRIPTION + "'," +
                            "KOMTEK_CONTACT_PERSON = '" + m_komtek.KOMTEK_CONTACT_PERSON + "'," +
                            "KOMTEK_KETERANGAN = '" + m_komtek.KOMTEK_KETERANGAN + "'," +
                            "KOMTEK_CREATE_BY = '" + UserId + "'," +
                            "KOMTEK_CREATE_DATE = " + datenow + "," +
                            "KOMTEK_STATUS = '" + status + "'," +
                            "KOMTEK_LOG_CODE = '" + m_komtek.KOMTEK_LOG_CODE + "'";
            }


            var clause = "where KOMTEK_ID = " + m_komtek.KOMTEK_ID;
            //return Json(new { query = "UPDATE MASTER_KOMITE_TEKNIS SET " + update.Replace("''", "NULL") + " " + clause }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE MASTER_KOMITE_TEKNIS SET " + update.Replace("''", "NULL") + " " + clause);

            //var logId = AuditTrails.GetLogId();
            String objek = update.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);

            if (KOMTEK_ICS_ID != null)
            {
                string query_update = "UPDATE MASTER_KOMTEK_ICS  SET KOMTEK_ICS_STATUS = 0, KOMTEK_ICS_UPDATE_BY =" + UserId + ", KOMTEK_ICS_UPDATE_DATE=" + datenow + " WHERE KOMTEK_ICS_KOMTEK_ID = " + m_komtek.KOMTEK_ID;
                db.Database.ExecuteSqlCommand(query_update);

                foreach (var ICS_ID in KOMTEK_ICS_ID)
                {
                    int cek = db.Database.SqlQuery<int>("SELECT COUNT(1) AS JML FROM MASTER_KOMTEK_ICS WHERE KOMTEK_ICS_KOMTEK_ID = '" + m_komtek.KOMTEK_ID + "' AND KOMTEK_ICS_ICS_ID = '" + ICS_ID + "'").SingleOrDefault();
                    if (cek == 0)
                    {
                        int lastid_mki = MixHelper.GetSequence("MASTER_KOMTEK_ICS");
                        string query_insert = "INSERT INTO MASTER_KOMTEK_ICS (KOMTEK_ICS_ID, KOMTEK_ICS_KOMTEK_ID, KOMTEK_ICS_ICS_ID, KOMTEK_ICS_UPDATE_BY, KOMTEK_ICS_UPDATE_DATE, KOMTEK_ICS_STATUS, KOMTEK_ICS_LOG_CODE) VALUES (" + lastid_mki + "," + m_komtek.KOMTEK_ID + "," + ICS_ID + "," + UserId + "," + datenow + ",1,'" + m_komtek.KOMTEK_LOG_CODE + "')";
                        db.Database.ExecuteSqlCommand(query_insert);
                    }
                    else
                    {
                        string query_updatea = "UPDATE MASTER_KOMTEK_ICS  SET KOMTEK_ICS_STATUS = 1, KOMTEK_ICS_UPDATE_BY =" + UserId + ", KOMTEK_ICS_UPDATE_DATE=" + datenow + " WHERE KOMTEK_ICS_KOMTEK_ID = " + m_komtek.KOMTEK_ID + " AND KOMTEK_ICS_ICS_ID = '" + ICS_ID + "'";
                        
                        db.Database.ExecuteSqlCommand(query_updatea);
                    }

                }
            }
            
            var updat = "DOC_STATUS = '0'";
            var claus = "where DOC_ID = " + doc.DOC_ID;
            
            db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET " + updat.Replace("''", "NULL") + " " + claus);


            string path = Server.MapPath("~/Upload/Dokumen/KOMTEK_SK/");
            HttpPostedFileBase file_att = Request.Files["FILE_KOMTEK_SK_PENETAPAN"];
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
                    file_name_att = "Komtek_" + m_komtek.KOMTEK_CODE.Replace('/', '-') + "_" + lastid + fileExtension;
                    filePath = path + file_name_att;
                    file_att.SaveAs(filePath);
                }

                int lastid_doc = MixHelper.GetSequence("TRX_DOCUMENTS");
                var logcodeDOC = MixHelper.GetLogCode();
                var FNAME_DOC = "DOC_ID,DOC_FOLDER_ID,DOC_RELATED_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
                var FVALUE_DOC = lastid_doc + ", " +
                            "'22', " +
                            "'" + lastid + "', " +
                            "'SK_" + m_komtek.KOMTEK_CODE.ToUpper() + "', " +
                            "'SK Komtek dengan Nomor " + m_komtek.KOMTEK_CODE.ToUpper() + "', " +
                            "'" + "/Upload/Dokumen/KOMTEK_SK/', " +
                            "'" + "Komtek_" + m_komtek.KOMTEK_CODE.Replace('/', '-') + "_" + lastid + "', " +
                            "'" + fileExtension.Replace(".", "").ToUpper() + "', " +
                            "'0', " +
                            "'" + UserId + "', " +
                            datenow + "," +
                            "'1', " +
                            "'" + logcodeDOC + "'";

                db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DOC + ") VALUES (" + FVALUE_DOC.Replace("''", "NULL") + ")");
                String objekDOC = FVALUE_DOC.Replace("'", "-");
                MixHelper.InsertLog(logcodeDOC, objekDOC, 1);
            }

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Read(int id = 0)
        {
            ViewData["listICS"] = (from t in db.MASTER_ICS where t.ICS_STATUS == 1 orderby t.ICS_CODE ascending select t).ToList();
            ViewData["listbidang"] = (from t in db.MASTER_BIDANG where t.BIDANG_STATUS == 1 orderby t.BIDANG_CODE ascending select t).ToList();
            ViewData["listinstansi"] = (from t in db.MASTER_INSTANSI where t.INSTANSI_STATUS == 1 orderby t.INSTANSI_CODE ascending select t).ToList();
            ViewData["listparent"] = (from t in db.VIEW_KOMTEK where t.KOMTEK_STATUS == 1 orderby t.KOMTEK_CODE ascending select t).ToList();
            ViewData["komtek_item"] = (from t in db.VIEW_SUBKOMTEK where t.KOMTEK_ID == id select t).SingleOrDefault();
            ViewData["komtek_ics"] = (from t in db.VIEW_KOMTEK_ICS where t.KOMTEK_ICS_KOMTEK_ID == id && t.KOMTEK_ICS_STATUS == 1 select t).ToList();
            
            return View();
        }

        public ActionResult Nonaktif(int id = 0)
        {

            db.Database.ExecuteSqlCommand("UPDATE MASTER_KOMITE_TEKNIS SET KOMTEK_STATUS = 0 WHERE KOMTEK_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");

        }

        public ActionResult Aktif(int id = 0)
        {
            VIEW_SUBKOMTEK vsk = db.VIEW_SUBKOMTEK.SingleOrDefault(t => t.KOMTEK_ID == id);
            int cek_komtek = db.Database.SqlQuery<int>("SELECT KOMTEK_STATUS FROM MASTER_KOMITE_TEKNIS WHERE KOMTEK_CODE = '" + vsk.KOMTEK_PARENT_CODE + "'").SingleOrDefault();
            var status = 0;
            
            if(cek_komtek == 0)
            {
                status = 2;
            }         
            else {
                db.Database.ExecuteSqlCommand("UPDATE MASTER_KOMITE_TEKNIS SET KOMTEK_STATUS = 1 WHERE KOMTEK_ID = " + id);
                status = 1;
            }
            
            TempData["Notifikasi"] = status;
            TempData["NotifikasiText"] = (status == 1)?"Data Berhasil Di Aktifkan":"Data Tidak Bisa diaktifkan Karena Data Komtek masih Nonaktif";
            return RedirectToAction("Index");
        }

        public ActionResult ListDataSubKomtek(DataTables param, int status= 0)
        {
            var default_order = "KOMTEK_CODE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("KOMTEK_CODE");
            order_field.Add("KOMTEK");
            order_field.Add("SUB_KOMTEK");
            order_field.Add("KOMTEK_SEKRETARIAT");
            order_field.Add("KOMTEK_ADDRESS");
            order_field.Add("KOMTEK_SK_PENETAPAN");
            //order_field.Add("KOMTEK_SEKRETARIAT");
            //order_field.Add("KOMTEK_EMAIL");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "KOMTEK_STATUS ="+status;

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
                search_clause += ")";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SUBKOMTEK WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SUBKOMTEK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SUBKOMTEK>(inject_clause_select);
            //int no = 1;
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.SUB_KOMTEK_CODE), 
                Convert.ToString(list.KOMTEK), 
                Convert.ToString(list.SUB_KOMTEK),
                Convert.ToString(list.KOMTEK_SEKRETARIAT),
                Convert.ToString(list.KOMTEK_ADDRESS),
                //Convert.ToString(list.KOMTEK_SEKRETARIAT),
                //Convert.ToString(list.KOMTEK_EMAIL),
                Convert.ToString(list.KOMTEK_SK_PENETAPAN),
                //Convert.ToString((list.KOMTEK_STATUS == 0)?"<span class='red'>Tidak Aktif</span>":"<span class='red'>Aktif</span>"),
                //<a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Subkomtek/Read/"+list.KOMTEK_ID+"'><i class='action fa fa-file-text-o'></i></a>
                //<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Subkomtek/Read/"+list.KOMTEK_ID+"'><i class='action fa fa-file-text-o'></i></a>
                Convert.ToString((list.KOMTEK_STATUS == 1)?"<center><a data-original-title='Add anggota' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Subkomtek/CreateAnggotaSub/"+list.KOMTEK_ID+"'><i class='action fa fa-users'></i></a><a data-original-title='Detail' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Subkomtek/DetailSubKomtek/"+list.KOMTEK_ID+"'><i class='action fa fa-file-text-o'></i></a>"+"<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Master/Subkomtek/Edit/"+list.KOMTEK_ID+"'><i class='action fa fa-edit'></i></a>"+"<a data-original-title='Non-Aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Master/Subkomtek/Nonaktif/"+list.KOMTEK_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>":"<center><a data-original-title='Add anggota' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Komtek/CreateAnggotaSub/"+list.KOMTEK_ID+"'><i class='action fa fa-users'></i></a><a data-original-title='Detail' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Subkomtek/DetailSubKomtek/"+list.KOMTEK_ID+"'><i class='action fa fa-file-text-o'></i></a>"+"<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Master/Subkomtek/Edit/"+list.KOMTEK_ID+"'><i class='action fa fa-edit'></i></a>"+"<a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Master/Subkomtek/Aktif/"+list.KOMTEK_ID+"'><i class='action glyphicon glyphicon-ok'></i></a></center>"),
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        [Auth(RoleTipe = 1)]
        public ActionResult DetailSubKomtek(int id = 0)
        {
            ViewData["list_anggota"] = (from t in db.VIEW_ANGGOTA where t.KOMTEK_ANGGOTA_KOMTEK_ID == id && t.KOMTEK_ANGGOTA_STATUS == 1 && t.JABATAN != "Sekretariat"  orderby t.KOMTEK_ANGGOTA_JABATAN ascending select t).ToList();
            ViewData["komtek_item"] = (from t in db.VIEW_SUBKOMTEK where t.KOMTEK_ID == id select t).SingleOrDefault();
            ViewData["komtek_ics"] = (from t in db.VIEW_KOMTEK_ICS where t.KOMTEK_ICS_KOMTEK_ID == id && t.KOMTEK_ICS_STATUS == 1 select t).ToList();
            ViewData["sk_file"] = (from t in db.TRX_DOCUMENTS where t.DOC_RELATED_ID == id && t.DOC_FOLDER_ID == 22 && t.DOC_STATUS == 1 select t).SingleOrDefault();
            //var DataAnggota = (from ics in db.VIEW_ANGGOTA where ics.KOMTEK_ANGGOTA_KOMTEK_ID == id select ics).ToList();
            //ViewData["DataAnggota"] = DataAnggota;

            //return Json(new { komtek_item = ViewData["komtek_item"], komtek_ics = ViewData["komtek_ics"], skfile = ViewData["sk_file"], list_anggota = ViewData["list_anggota"] }, JsonRequestBehavior.AllowGet);

            var komposis_anggota = db.Database.SqlQuery<SISPK.Models.DataTables.KomposisiKomtek>("SELECT CAST(VA.KOMTEK_ANGGOTA_STAKEHOLDER AS DECIMAL) AS KOMTEK_ANGGOTA_STAKEHOLDER,VA.STAKEHOLDER,CAST(VA.KOMTEK_ANGGOTA_KOMTEK_ID AS DECIMAL) AS KOMTEK_ANGGOTA_KOMTEK_ID, CAST(COUNT (VA.KOMTEK_ANGGOTA_STAKEHOLDER) AS DECIMAL) AS JML, CAST(ROUND((COUNT (VA.KOMTEK_ANGGOTA_STAKEHOLDER)/(SELECT COUNT(VAS.KOMTEK_ANGGOTA_STAKEHOLDER) FROM VIEW_ANGGOTA VAS WHERE VAS.KOMTEK_ANGGOTA_KOMTEK_ID = " + id + " AND VAS.JABATAN != 'Sekretariat')* 100),2) AS DECIMAL) AS PERSENTASE FROM VIEW_ANGGOTA VA WHERE VA.KOMTEK_ANGGOTA_KOMTEK_ID = " + id + " AND VA.JABATAN != 'Sekretariat' AND VA.KOMTEK_ANGGOTA_STAKEHOLDER IS NOT NULL GROUP BY VA.KOMTEK_ANGGOTA_STAKEHOLDER, VA.STAKEHOLDER, VA.KOMTEK_ANGGOTA_KOMTEK_ID").ToList();
            ViewData["Komp_Ang"] = komposis_anggota;
            return View();
        }

        public ActionResult CreateAnggotaSub(int id = 0)
        {
            ViewData["listJabatan"] = (from t in db.MASTER_REFERENCES where t.REF_TYPE == 4 && t.REF_STATUS == 1 select t).ToList();
            ViewData["listEducation"] = (from t in db.MASTER_REFERENCES where t.REF_TYPE == 3 && t.REF_STATUS == 1 select t).ToList();
            ViewData["listJabatanAktif"] = (from t in db.MASTER_KOMTEK_ANGGOTA where t.KOMTEK_ANGGOTA_KOMTEK_ID == id && t.KOMTEK_ANGGOTA_STATUS == 1 select t).ToList();
            ViewData["liststakeholder"] = (from t in db.MASTER_REFERENCES where t.REF_TYPE == 5 && t.REF_STATUS == 1 select t).ToList();
            ViewData["listinstansi"] = (from t in db.MASTER_INSTANSI where t.INSTANSI_STATUS == 1 orderby t.INSTANSI_CODE ascending select t).ToList();
            ViewData["komtek_id"] = id;
            ViewData["user_akses_id"] = Session["USER_ACCESS_ID"];
            ViewData["komtek_kode"] = db.VIEW_SUBKOMTEK.SingleOrDefault(t => t.KOMTEK_ID == id);
            //ViewData["file_anggota"] = db.VIEW_SUBKOMTEK.SingleOrDefault(t => t.KOMTEK_ID == id);
            return View();
        }

        [HttpPost]
        public ActionResult CreateAnggotaSub(MASTER_KOMTEK_ANGGOTA mka)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_KOMTEK_ANGGOTA");
            var datenow = MixHelper.ConvertDateNow();
            var kode = db.Database.SqlQuery<int>("SELECT MAX(MKA.KOMTEK_ANGGOTA_KODE) + 1 FROM MASTER_KOMTEK_ANGGOTA MKA").SingleOrDefault();
            //int kodes = Convert.ToInt32(kode);
            //DateTime dates = Convert.ToDateTime(m_komtek.KOMTEK_TANGGAL_PEMBENTUKAN);
            //var date = "TO_DATE('" + dates.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";
            //var parent = "0";

            string pathnya = Server.MapPath("~/Upload/Dokumen/KOMTEK_CV/");
            HttpPostedFileBase file_cv = Request.Files["KOMTEK_CV"];
            var file_name_cv = "dddd";
            var filePath_cv = "";
            var fileExtension_cv = "";
            if (file_cv != null)
            {
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(pathnya))
                {
                    //If Directory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(pathnya);
                }
                string lampiranregulasipath = file_cv.FileName;
                if (lampiranregulasipath.Trim() != "")
                {
                    lampiranregulasipath = Path.GetFileNameWithoutExtension(file_cv.FileName);
                    fileExtension_cv = Path.GetExtension(file_cv.FileName);
                    file_name_cv = "Sub_Komtek_CV_" + mka.KOMTEK_ANGGOTA_KOMTEK_ID + "_" + mka.KOMTEK_ANGGOTA_NAMA + fileExtension_cv;
                    filePath_cv = pathnya + file_name_cv.Replace(" ", "_");
                    file_cv.SaveAs(filePath_cv);
                }
            }

            var fname = "KOMTEK_ANGGOTA_ID,KOMTEK_ANGGOTA_KOMTEK_ID,KOMTEK_ANGGOTA_KODE,KOMTEK_ANGGOTA_NAMA,KOMTEK_ANGGOTA_JABATAN,KOMTEK_ANGGOTA_INSTANSI,KOMTEK_ANGGOTA_ADDRESS,KOMTEK_ANGGOTA_TELP,KOMTEK_ANGGOTA_FAX,KOMTEK_ANGGOTA_EMAIL,KOMTEK_ANGGOTA_STAKEHOLDER,KOMTEK_ANGGOTA_EDUCATION,KOMTEK_ANGGOTA_EXPERTISE,KOMTEK_ANGGOTA_CREATE_BY,KOMTEK_ANGGOTA_DATE,KOMTEK_ANGGOTA_LOG_CODE,KOMTEK_ANGGOTA_STATUS,KOMTEK_ANGGOTA_CV";
            var fvalue = "'" + lastid + "'," +
                        "'" + mka.KOMTEK_ANGGOTA_KOMTEK_ID + "'," +
                        "" + kode + "," +
                        "'" + mka.KOMTEK_ANGGOTA_NAMA + "'," +
                        "'" + mka.KOMTEK_ANGGOTA_JABATAN + "'," +
                        "'" + mka.KOMTEK_ANGGOTA_INSTANSI + "'," +
                        "'" + mka.KOMTEK_ANGGOTA_ADDRESS + "'," +
                        "'" + mka.KOMTEK_ANGGOTA_TELP + "'," +
                        "'" + mka.KOMTEK_ANGGOTA_FAX + "'," +
                        "'" + mka.KOMTEK_ANGGOTA_EMAIL + "'," +
                        "'" + mka.KOMTEK_ANGGOTA_STAKEHOLDER + "'," +
                        "'" + mka.KOMTEK_ANGGOTA_EDUCATION + "'," +
                        "'" + mka.KOMTEK_ANGGOTA_EXPERTISE + "'," +
                        "'" + UserId + "'," +
                        datenow + "," +
                        "'" + logcode + "'," +
                        "1," +
                        "'/Upload/Dokumen/KOMTEK_CV/" + file_name_cv.Replace(" ", "_") + "'";

            //return Json(new { query = "INSERT INTO MASTER_KOMTEK_ANGGOTA (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("INSERT INTO MASTER_KOMTEK_ANGGOTA (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");
            var chars = "0123456789";
            var random = new Random();
            var result = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
            int iduser = MixHelper.GetSequence("SYS_USER");
            var komtek_name = (from t in db.MASTER_KOMITE_TEKNIS where t.KOMTEK_ID == mka.KOMTEK_ANGGOTA_KOMTEK_ID select t).SingleOrDefault();
            //var password = "kom" + kode + result.ToString();
            var password = "sispk";
            var fnameu = "USER_ID,USER_NAME,USER_PASSWORD,USER_ACCESS_ID,USER_TYPE_ID,USER_REF_ID,USER_CREATE_BY,USER_CREATE_DATE,USER_LOG_CODE,USER_STATUS";
            var fvalueu = "'" + iduser + "', " +
                        "'" + komtek_name.KOMTEK_CODE + "_" + mka.KOMTEK_ANGGOTA_EMAIL + "', " +
                        "'" + GenPassword(password) + "', " +
                        "2," +
                        "2," +
                        "'" + kode + "', " +
                        "'" + UserId + "', " +
                        datenow + "," +
                        "'" + logcode + "'," +
                        "1";
            //return Json(new { query = "INSERT INTO SYS_USER (" + fnameu + ") VALUES (" + fvalueu.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("INSERT INTO SYS_USER (" + fnameu + ") VALUES (" + fvalueu.Replace("''", "NULL") + ")");



            //Send Account Activation to Email

            var email = (from t in db.SYS_EMAIL where t.EMAIL_IS_USE == 1 select t).SingleOrDefault();
            SendMailHelper.MailUsername = email.EMAIL_NAME;      //"aleh.mail@gmail.com";
            SendMailHelper.MailPassword = email.EMAIL_PASSWORD;  //"r4h45143uy";

            SendMailHelper mailer = new SendMailHelper();
            mailer.ToEmail = mka.KOMTEK_ANGGOTA_EMAIL;
            mailer.Subject = "Authentifikasi Anggota Komtek - SISPK";
            var isiEmail = "Selamat anda sekarang terdaftar sebagai anggota subkomtek " + komtek_name.KOMTEK_NAME + ". Berikut Data Detail anda : <br />";
            isiEmail += "Username : " + komtek_name.KOMTEK_CODE + "_" + mka.KOMTEK_ANGGOTA_EMAIL + "<br />";
            isiEmail += "Password : " + password + "<br />";
            isiEmail += "Status   : Aktif <br />";
            isiEmail += "Silahkan klik tautan <a href=" + @Request.Url.GetLeftPart(UriPartial.Authority) + "/Auth target='_blank'>berikut</a><br />";
            isiEmail += "Demikian Informasi yang kami sampaikan, atas kerjasamanya kami ucapkan terimakasih. <br />";
            isiEmail += "<span style='text-align:right;font-weight:bold;margin-top:20px;'>Web Administrator</span>";

            mailer.Body = isiEmail;
            mailer.IsHtml = true;
            mailer.Send();

            TempData["MailMember"] = mka.KOMTEK_ANGGOTA_EMAIL;

            String objek = fvalue.Replace("'", "-");
            String objek1 = fvalueu.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            MixHelper.InsertLog(logcode, objek1, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            TempData["Password"] = password;
            return RedirectToAction("DetailSubKomtek/" + mka.KOMTEK_ANGGOTA_KOMTEK_ID);

            //return Content(file_name_cv);
        }

        public ActionResult ReadAnggota(int id = 0)
        {
            ViewData["anggota_item"] = (from t in db.VIEW_ANGGOTA where t.KOMTEK_ANGGOTA_ID == id select t).SingleOrDefault();
            //var komtek_item = (from t in db.VIEW_ANGGOTA where t.KOMTEK_ANGGOTA_ID == id select t).SingleOrDefault();
            //VIEW_ANGGOTA anggota_item = db.VIEW_ANGGOTA.SingleOrDefault(t => t.KOMTEK_ANGGOTA_ID == id);
            ////return Json(new { data = komtek_item }, JsonRequestBehavior.AllowGet);
            //if (anggota_item == null)
            //{
            //    return HttpNotFound();
            //}
            //return Json(new { data = komtek_item }, JsonRequestBehavior.AllowGet);
            return View();
        }

        public ActionResult EditAnggota(int id = 0)
        {
            ViewData["listJabatan"] = (from t in db.MASTER_REFERENCES where t.REF_TYPE == 4 && t.REF_STATUS == 1 select t).ToList();
            ViewData["listEducation"] = (from t in db.MASTER_REFERENCES where t.REF_TYPE == 3 && t.REF_STATUS == 1 select t).ToList();
            ViewData["liststakeholder"] = (from t in db.MASTER_REFERENCES where t.REF_TYPE == 5 && t.REF_STATUS == 1 select t).ToList();
            ViewData["listinstansi"] = (from t in db.MASTER_INSTANSI where t.INSTANSI_STATUS == 1 orderby t.INSTANSI_CODE ascending select t).ToList();
            ViewData["anggota_id"] = id;
            ViewData["user_akses_id"] = Session["USER_ACCESS_ID"];
            ViewData["anggota_item"] = (from t in db.VIEW_ANGGOTA where t.KOMTEK_ANGGOTA_ID == id select t).SingleOrDefault();
            VIEW_ANGGOTA anggota_item = db.VIEW_ANGGOTA.SingleOrDefault(t => t.KOMTEK_ANGGOTA_ID == id);
            //return Json(new { data = komtek_item }, JsonRequestBehavior.AllowGet);
            if (anggota_item == null)
            {
                return HttpNotFound();
            }
            ViewData["listJabatanAktif"] = (from t in db.MASTER_KOMTEK_ANGGOTA where t.KOMTEK_ANGGOTA_KOMTEK_ID == anggota_item.KOMTEK_ANGGOTA_KOMTEK_ID && t.KOMTEK_ANGGOTA_STATUS == 1 select t).ToList();
            return View(anggota_item);
        }

        [HttpPost]
        public ActionResult EditAnggota(MASTER_KOMTEK_ANGGOTA mka)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_KOMITE_TEKNIS");
            var datenow = MixHelper.ConvertDateNow();

            string pathnya = Server.MapPath("~/Upload/Dokumen/KOMTEK_CV/");
            HttpPostedFileBase file_cv = Request.Files["KOMTEK_CV"];
            var upload = Request.Files["KOMTEK_CV"];
            var file_name_cv = "";
            var filePath_cv = "";
            var fileExtension_cv = "";
            if (upload.ContentLength > 0)
            {
                //Check whether Directory (Folder) exists.
                if (!Directory.Exists(pathnya))
                {
                    //If Directory (Folder) does not exists. Create it.
                    Directory.CreateDirectory(pathnya);
                }
                string lampiranregulasipath = file_cv.FileName;
                if (lampiranregulasipath.Trim() != "")
                {
                    lampiranregulasipath = Path.GetFileNameWithoutExtension(file_cv.FileName);
                    fileExtension_cv = Path.GetExtension(file_cv.FileName);
                    file_name_cv = "Sub_Komtek_CV_" + mka.KOMTEK_ANGGOTA_KOMTEK_ID + "_" + mka.KOMTEK_ANGGOTA_NAMA + fileExtension_cv;
                    filePath_cv = pathnya + file_name_cv.Replace(" ", "_");
                    file_cv.SaveAs(filePath_cv);
                }

                var update =
                        "KOMTEK_ANGGOTA_NAMA = '" + mka.KOMTEK_ANGGOTA_NAMA + "'," +
                        "KOMTEK_ANGGOTA_JABATAN = '" + mka.KOMTEK_ANGGOTA_JABATAN + "'," +
                        "KOMTEK_ANGGOTA_INSTANSI = '" + mka.KOMTEK_ANGGOTA_INSTANSI + "'," +
                        "KOMTEK_ANGGOTA_ADDRESS = '" + mka.KOMTEK_ANGGOTA_ADDRESS + "'," +
                        "KOMTEK_ANGGOTA_TELP = '" + mka.KOMTEK_ANGGOTA_TELP + "'," +
                        "KOMTEK_ANGGOTA_FAX = '" + mka.KOMTEK_ANGGOTA_FAX + "'," +
                        "KOMTEK_ANGGOTA_EMAIL = '" + mka.KOMTEK_ANGGOTA_EMAIL + "'," +
                        "KOMTEK_ANGGOTA_STAKEHOLDER = '" + mka.KOMTEK_ANGGOTA_STAKEHOLDER + "'," +
                        "KOMTEK_ANGGOTA_EDUCATION = '" + mka.KOMTEK_ANGGOTA_EDUCATION + "'," +
                        "KOMTEK_ANGGOTA_EXPERTISE = '" + mka.KOMTEK_ANGGOTA_EXPERTISE + "'," +
                        "KOMTEK_ANGGOTA_UPDATE_BY = '" + UserId + "'," +
                        "KOMTEK_ANGGOTA_UPDATE_DATE = " + datenow + "," +
                        "KOMTEK_ANGGOTA_LOG_CODE = '" + mka.KOMTEK_ANGGOTA_LOG_CODE + "'," +
                        "KOMTEK_ANGGOTA_CV = '/Upload/Dokumen/KOMTEK_CV/" + file_name_cv.Replace(" ", "_") + "'";

                var clause = "where KOMTEK_ANGGOTA_ID = " + mka.KOMTEK_ANGGOTA_ID;
                db.Database.ExecuteSqlCommand("UPDATE MASTER_KOMTEK_ANGGOTA SET " + update.Replace("''", "NULL") + " " + clause);

                var updateu = "USER_NAME = '" + mka.KOMTEK_ANGGOTA_EMAIL + "'," +
                          "USER_UPDATE_BY = '" + UserId + "'," +
                          "USER_UPDATE_DATE = " + datenow + "," +
                          "USER_LOG_CODE = '" + mka.KOMTEK_ANGGOTA_LOG_CODE + "'";

                var clauseu = "where USER_REF_ID = " + mka.KOMTEK_ANGGOTA_ID + " AND USER_ACCESS_ID = 2 ";
                db.Database.ExecuteSqlCommand("UPDATE SYS_USER SET " + updateu.Replace("''", "NULL") + " " + clauseu);


                String objek = update.Replace("'", "-");
                String objek1 = updateu.Replace("'", "-");
                MixHelper.InsertLog(logcode, objek, 1);
                MixHelper.InsertLog(logcode, objek1, 1);
                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            }
            else
            {
                var update =
                        "KOMTEK_ANGGOTA_NAMA = '" + mka.KOMTEK_ANGGOTA_NAMA + "'," +
                        "KOMTEK_ANGGOTA_JABATAN = '" + mka.KOMTEK_ANGGOTA_JABATAN + "'," +
                        "KOMTEK_ANGGOTA_INSTANSI = '" + mka.KOMTEK_ANGGOTA_INSTANSI + "'," +
                        "KOMTEK_ANGGOTA_ADDRESS = '" + mka.KOMTEK_ANGGOTA_ADDRESS + "'," +
                        "KOMTEK_ANGGOTA_TELP = '" + mka.KOMTEK_ANGGOTA_TELP + "'," +
                        "KOMTEK_ANGGOTA_FAX = '" + mka.KOMTEK_ANGGOTA_FAX + "'," +
                        "KOMTEK_ANGGOTA_EMAIL = '" + mka.KOMTEK_ANGGOTA_EMAIL + "'," +
                        "KOMTEK_ANGGOTA_STAKEHOLDER = '" + mka.KOMTEK_ANGGOTA_STAKEHOLDER + "'," +
                        "KOMTEK_ANGGOTA_EDUCATION = '" + mka.KOMTEK_ANGGOTA_EDUCATION + "'," +
                        "KOMTEK_ANGGOTA_EXPERTISE = '" + mka.KOMTEK_ANGGOTA_EXPERTISE + "'," +
                        "KOMTEK_ANGGOTA_UPDATE_BY = '" + UserId + "'," +
                        "KOMTEK_ANGGOTA_UPDATE_DATE = " + datenow + "," +
                        "KOMTEK_ANGGOTA_LOG_CODE = '" + mka.KOMTEK_ANGGOTA_LOG_CODE + "'";

                var clause = "where KOMTEK_ANGGOTA_ID = " + mka.KOMTEK_ANGGOTA_ID;
                db.Database.ExecuteSqlCommand("UPDATE MASTER_KOMTEK_ANGGOTA SET " + update.Replace("''", "NULL") + " " + clause);
                var komtek_name = (from t in db.MASTER_KOMITE_TEKNIS where t.KOMTEK_ID == mka.KOMTEK_ANGGOTA_KOMTEK_ID select t).SingleOrDefault();

                var updateu = "USER_NAME = '" + komtek_name.KOMTEK_CODE + "_" + mka.KOMTEK_ANGGOTA_EMAIL + "', " +
                          "USER_UPDATE_BY = '" + UserId + "'," +
                          "USER_UPDATE_DATE = " + datenow + "," +
                          "USER_LOG_CODE = '" + mka.KOMTEK_ANGGOTA_LOG_CODE + "'";

                var clauseu = "where USER_REF_ID = " + mka.KOMTEK_ANGGOTA_ID + " AND USER_ACCESS_ID = 2 ";
                db.Database.ExecuteSqlCommand("UPDATE SYS_USER SET " + updateu.Replace("''", "NULL") + " " + clauseu);


                String objek = update.Replace("'", "-");
                String objek1 = updateu.Replace("'", "-");
                MixHelper.InsertLog(logcode, objek, 1);
                MixHelper.InsertLog(logcode, objek1, 1);
                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            }
            return RedirectToAction("DetailSubKomtek/" + mka.KOMTEK_ANGGOTA_KOMTEK_ID);
        }

        public ActionResult NonaktifAnggota(int id = 0)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            var datenow = MixHelper.ConvertDateNow();
            var status = 0;
            VIEW_ANGGOTA mka = db.VIEW_ANGGOTA.SingleOrDefault(t => t.KOMTEK_ANGGOTA_ID == id);
            var update = "KOMTEK_ANGGOTA_STATUS = '" + status + "'," +
                            "KOMTEK_ANGGOTA_UPDATE_BY = '" + UserId + "'," +
                            "KOMTEK_ANGGOTA_UPDATE_DATE = " + datenow;

            var clause = "where KOMTEK_ANGGOTA_ID = " + mka.KOMTEK_ANGGOTA_ID + " AND USER_ACCESS_ID = 2 ";
            //return Json(new { query = "UPDATE SYS_USER SET " + update.Replace("''", "NULL") + " " + clauseu }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE MASTER_KOMTEK_ANGGOTA SET " + update.Replace("''", "NULL") + " " + clause);

            var updateu = "USER_STATUS = '" + status + "'," +
                            "USER_UPDATE_BY = '" + UserId + "'," +
                            "USER_UPDATE_DATE = " + datenow + "";

            var clauseu = "where USER_REF_ID = " + mka.KOMTEK_ANGGOTA_KODE;
            //return Json(new { query = "UPDATE SYS_USER SET " + updateu.Replace("''", "NULL") + " " + clauseu }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE SYS_USER SET " + updateu.Replace("''", "NULL") + " " + clauseu);
            //db.Database.ExecuteSqlCommand("UPDATE MASTER_KOMTEK_ANGGOTA SET KOMTEK_ANGGOTA_STATUS = 0 WHERE KOMTEK_ANGGOTA_ID = " + id);
            String objek = update.Replace("'", "-");
            String objek1 = updateu.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            MixHelper.InsertLog(logcode, objek1, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Anggota dengan nama "+mka.KOMTEK_ANGGOTA_NAMA+" Berhasil Di Non Aktifkan";
            return RedirectToAction("DetailSubKomtek/"+mka.KOMTEK_ANGGOTA_KOMTEK_ID);
        }

        public ActionResult AktifAnggota(int id = 0)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            var datenow = MixHelper.ConvertDateNow();
            var status = 1;
            VIEW_ANGGOTA mka = db.VIEW_ANGGOTA.SingleOrDefault(t => t.KOMTEK_ANGGOTA_ID == id);
            var update = "KOMTEK_ANGGOTA_STATUS = '" + status + "'," +
                            "KOMTEK_ANGGOTA_UPDATE_BY = '" + UserId + "'," +
                            "KOMTEK_ANGGOTA_UPDATE_DATE = " + datenow;

            var clause = "where KOMTEK_ANGGOTA_ID = " + mka.KOMTEK_ANGGOTA_ID + " AND USER_ACCESS_ID = 2 ";
            //return Json(new { query = "UPDATE SYS_USER SET " + update.Replace("''", "NULL") + " " + clauseu }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE MASTER_KOMTEK_ANGGOTA SET " + update.Replace("''", "NULL") + " " + clause);

            var updateu = "USER_STATUS = '" + status + "'," +
                            "USER_UPDATE_BY = '" + UserId + "'," +
                            "USER_UPDATE_DATE = " + datenow;

            var clauseu = "where USER_REF_ID = " + mka.KOMTEK_ANGGOTA_KODE;
            //return Json(new { query = "UPDATE SYS_USER SET " + updateu.Replace("''", "NULL") + " " + clauseu }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE SYS_USER SET " + updateu.Replace("''", "NULL") + " " + clauseu);
            //db.Database.ExecuteSqlCommand("UPDATE MASTER_KOMTEK_ANGGOTA SET KOMTEK_ANGGOTA_STATUS = 0 WHERE KOMTEK_ANGGOTA_ID = " + id);
            String objek = update.Replace("'", "-");
            String objek1 = updateu.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            MixHelper.InsertLog(logcode, objek1, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("DetailSubKomtek/"+mka.KOMTEK_ANGGOTA_KOMTEK_ID);
        }

        public ActionResult ResetPassword(int id = 0)
        {
            int status = 1;
            var chars = "0123456789";
            var random = new Random();
            var result = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());

            //var newpass = "SISPK" + result.ToString();
            var newpass = "sispk";

            string query_update_group = "UPDATE SYS_USER SET USER_PASSWORD = '" + GenPassword(newpass) + "' WHERE USER_REF_ID = '" + id + "' AND USER_ACCESS_ID = 2";
            db.Database.ExecuteSqlCommand(query_update_group);

            var user = (from a in db.SYS_USER where a.USER_ID == id select a).SingleOrDefault();
            var anggota = (from b in db.VIEW_ANGGOTA where b.KOMTEK_ANGGOTA_KODE == id select b).SingleOrDefault();

            //return Json(new { a = id }, JsonRequestBehavior.AllowGet);
            //Send Account Activation to Email

            var email = (from t in db.SYS_EMAIL where t.EMAIL_IS_USE == 1 select t).SingleOrDefault();
            SendMailHelper.MailUsername = email.EMAIL_NAME;      //"aleh.mail@gmail.com";
            SendMailHelper.MailPassword = email.EMAIL_PASSWORD;  //"r4h45143uy";

            SendMailHelper mailer = new SendMailHelper();
            mailer.ToEmail = anggota.KOMTEK_ANGGOTA_EMAIL;
            mailer.Subject = "Authentifikasi Perubahan Password Aplikasi SISPK";
            var isiEmail = "Berikut User dan password anda yang baru <br />";
            isiEmail += "Username : " + anggota.USER_NAME + "<br />";
            isiEmail += "Password : " + newpass + "<br />";
            isiEmail += "Status   : Aktif <br />";
            isiEmail += "Silahkan klik tautan <a href=" + @Request.Url.GetLeftPart(UriPartial.Authority) + "/Auth target='_blank'>berikut</a> untuk login aplikasi<br />";
            isiEmail += "Demikian Informasi yang kami sampaikan, atas kerjasamanya kami ucapkan terimakasih. <br />";
            isiEmail += "<span style='text-align:right;font-weight:bold;margin-top:20px;'>Web Administrator</span>";

            mailer.Body = isiEmail;
            mailer.IsHtml = true;
            mailer.Send();

            return Json(new { status = status, value = newpass }, JsonRequestBehavior.AllowGet);
        }

        public string GenPassword(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();
        }

        public ActionResult getics(string q = "")
        {
            var SelectedData = db.Database.SqlQuery<MASTER_ICS>("SELECT * FROM MASTER_ICS WHERE UPPER(ICS_NAME) LIKE UPPER('%" + q + "%')");
            var results = new List<object>();
            foreach (var listField in SelectedData)
            {
                results.Add(new
                {
                    id_ics = listField.ICS_ID.ToString(),
                    code_ics = listField.ICS_CODE.ToString(),
                    name_ics = listField.ICS_NAME.ToString(),
                    name_ics_ind = ((listField.ICS_NAME_IND == null) ? "" : listField.ICS_NAME_IND.ToString())
                });
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult FindICS(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_ICS_SELECT WHERE LOWER(VIEW_ICS_SELECT.TEXT) LIKE '%" + q.ToLower() + "%'").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_ICS_SELECT WHERE LOWER(VIEW_ICS_SELECT.TEXT) LIKE '%" + q.ToLower() + "%') T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var dataics = db.Database.SqlQuery<VIEW_ICS_SELECT>(inject_clause_select);
            var ics = from cust in dataics select new { id = cust.ID, text = cust.TEXT };

            return Json(new { ics, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }

    }
}
