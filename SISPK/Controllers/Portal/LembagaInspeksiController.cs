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
    public class LembagaInspeksiController : Controller
    {
        //
        // GET: /LembagaInspeksi/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListLembagaInspeksi(DataTables param, int status = 0) {
            var default_order = "LPK_NAMA";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("LPK_NAMA");
            order_field.Add("LPK_NOMOR");
            order_field.Add("LPK_LINGKUP");
            order_field.Add("LPK_ALAMAT");
            order_field.Add("LPK_TELEPON");
            order_field.Add("LPK_EMAIL");
            order_field.Add("LPK_CONTACT_PERSON");
            order_field.Add("LPK_PERIODE_AWAL");
            order_field.Add("LPK_PERIODE_AKHIR");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = " LPK_STATUS = "+status+" AND LPK_KATEGORI != 1";

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
                search_clause += " OR LPK_NAMA = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_LPK WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_LPK " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_LPK>(inject_clause_select);

           
            var result = from lpk in SelectedData
                         select new
                         {
                             LPK_NAMA = lpk.LPK_NAMA,
                             LPK_NOMOR = lpk.LPK_NOMOR,
                             LPK_LINGKUP = lpk.LPK_LINGKUP,
                             LPK_LINGKUP_DETAIL = lpk.LPK_LINGKUP_DETAIL,
                             LPK_ALAMAT = lpk.LPK_ALAMAT,
                             LPK_TELEPON = lpk.LPK_TELEPON,
                             LPK_EMAIL = lpk.LPK_EMAIL,
                             LPK_CONTACT_PERSON = lpk.LPK_CONTACT_PERSON.Replace("|",", "),
                             PERIODE = lpk.LPK_PERIODE_AWAL + " - " + lpk.LPK_PERIODE_AKHIR,
                             TGL_AKHIR = Convert.ToDateTime(lpk.LPK_PERIODE_AKHIR).ToString("dd MMMM yyyy"),
                             JML_SNI = lpk.JML_SNI,
                             AKSI = (Convert.ToInt32(lpk.LPK_STATUS) == 1)?"<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Portal/LembagaInspeksi/Read/" + lpk.LPK_ID + "'><i class='action fa fa-file-text-o'></i></a><a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Portal/LembagaInspeksi/Edit/" + lpk.LPK_ID + "'><i class='action fa fa-edit'></i></a><a data-original-title='Nonaktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Portal/LembagaInspeksi/nonaktifkan/" + lpk.LPK_ID + "'><i class='action glyphicon glyphicon-remove'></i></a></center>" : "<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Portal/LembagaInspeksi/Read/" + lpk.LPK_ID + "'><i class='action fa fa-file-text-o'></i></a><a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Portal/LembagaInspeksi/Edit/" + lpk.LPK_ID + "'><i class='action fa fa-edit'></i></a><a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Portal/LembagaInspeksi/aktifkan/" + lpk.LPK_ID + "'><i class='action glyphicon glyphicon-ok'></i></a></center>" 
                         };

            return Json(new
            {
                draw = param.sEcho,
                recordsTotal = CountData,
                recordsFiltered = CountData,
                data = result
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create() {           
            var DataProvinsi = (from provinsi in db.VIEW_WILAYAH_PROVINSI where provinsi.WILAYAH_PARENT_ID == 0 && provinsi.WILAYAH_STATUS == 1 orderby provinsi.WILAYAH_ID ascending select provinsi).ToList();
            ViewData["Provinsi"] = DataProvinsi;
            var DataKabupaten = (from kab in db.VIEW_WILAYAH_PROVINSI where kab.WILAYAH_PARENT_ID != 0 && kab.WILAYAH_STATUS == 1 orderby kab.WILAYAH_ID ascending select kab).ToList();
            ViewData["Kabupaten"] = DataKabupaten;           
            return View();
        }

        [HttpPost]
        public ActionResult Create(TRX_LPK tl, FormCollection formCollection, int[] LPK_DETAIL_SNI_SNI_ID, int[] LPK_SCOPE_SCOPE_ID)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_KOMITE_TEKNIS");
            var datenow = MixHelper.ConvertDateNow();
            int lastid_doc = MixHelper.GetSequence("TRX_DOCUMENTS");

            string path = Server.MapPath("~/Upload/Dokumen/LAINNYA/LPK/");
            HttpPostedFileBase file_att = Request.Files["LPK_FILE"];
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
                    file_name_att = "LPK_" + tl.LPK_NOMOR.Replace(' ', '-') + "_" + lastid + fileExtension;
                    filePath = path + file_name_att;
                    file_att.SaveAs(filePath);
                }
            }

            //var logcodeDOC = MixHelper.GetLogCode();
            //var FNAME_DOC = "DOC_ID,DOC_FOLDER_ID,DOC_NAME,DOC_DESCRIPTION,DOC_FILE_PATH,DOC_FILE_NAME,DOC_FILETYPE,DOC_EDITABLE,DOC_CREATE_BY,DOC_CREATE_DATE,DOC_STATUS,DOC_LOG_CODE";
            //var FVALUE_DOC = "'" + lastid_doc + "', " +
            //            "'4', " +
            //            "'" + "" + trt..RETEK_NO_SK.ToUpper() + "', " +
            //            "'Regulasi Teknis dengan Nomor " + trt.RETEK_NO_SK.ToUpper() + "', " +
            //            "'" + "/Upload/Dokumen/SK_REGULASI/', " +
            //            "'" + "RegulasiTeknis_" + trt.RETEK_NO_SK.Replace('/', '-') + "_" + lastid + "', " +
            //            "'" + fileExtension.Remove('.').ToUpper() + "', " +
            //            "'0', " +
            //            "'" + UserId + "', " +
            //            datenow + "," +
            //            "'1', " +
            //            "'" + logcodeDOC + "'";
            //db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + FNAME_DOC + ") VALUES (" + FVALUE_DOC.Replace("''", "NULL") + ")");
            //String objekDOC = FVALUE_DOC.Replace("'", "-");
            //MixHelper.InsertLog(logcodeDOC, objekDOC, 1);

            var pathss = "/Upload/Dokumen/LAINNYA/LPK/";

            DateTime date2 = Convert.ToDateTime(tl.LPK_PERIODE_AKHIR);
            var dateakhir = "TO_DATE('" + date2.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";

            var fname = "LPK_ID,LPK_JENIS,LPK_NAMA,LPK_NOMOR,LPK_ALAMAT,LPK_LAMPIRAN,LPK_TELEPON,LPK_FAX,LPK_KODE_POS,LPK_EMAIL,LPK_WEBSITE,LPK_CONTACT_PERSON,LPK_PERIODE_AKHIR,LPK_KETERANGAN,LPK_KATEGORI,LPK_CREATE_BY,LPK_CREATE_DATE,LPK_PROVINSI,LPK_KABUPATENKOTA,LPK_LOGCODE,LPK_STATUS";
            var fvalue =
                        "'" + lastid + "'," +
                        "'" + tl.LPK_JENIS + "'," +
                        "'" + tl.LPK_NAMA + "'," +
                        "'" + tl.LPK_NOMOR + "'," +
                        "'" + tl.LPK_ALAMAT + "'," +
                        "'" + pathss + "" + file_name_att + "'," +
                        "'" + tl.LPK_TELEPON + "'," +
                        "'" + tl.LPK_FAX + "'," +
                        "'" + tl.LPK_KODE_POS + "'," +
                        "'" + tl.LPK_EMAIL + "'," +
                        "'" + tl.LPK_WEBSITE + "'," +
                        "'" + tl.LPK_CONTACT_PERSON + "'," +
                        "" + dateakhir + "," +
                        "'" + tl.LPK_KETERANGAN + "'," +
                        "2," +
                        "'" + UserId + "'," +
                        datenow + "," +
                        "'" + tl.LPK_PROVINSI + "'," +
                        "'" + tl.LPK_KABUPATENKOTA + "'," +
                        "'" + logcode + "'," +
                        "1";

            //return Json(new { query = "INSERT INTO TRX_LPK (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("INSERT INTO TRX_LPK (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");
            
            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            var idk = db.Database.SqlQuery<int>("SELECT MAX(TL.LPK_ID) FROM TRX_LPK TL").SingleOrDefault();

            if (LPK_DETAIL_SNI_SNI_ID != null)
            {
                foreach (var SNI_ID in LPK_DETAIL_SNI_SNI_ID)
                {
                    var logcodeS = MixHelper.GetLogCode();
                    int lastid_mki = MixHelper.GetSequence("TRX_LPK_DETAIL_SNI");
                    //string query_update = "INSERT INTO MASTER_KOMTEK_ICS (KOMTEK_ICS_ID, KOMTEK_ICS_KOMTEK_ID, KOMTEK_ICS_ICS_ID, KOMTEK_ICS_CREATE_BY, KOMTEK_ICS_CREATE_DATE, KOMTEK_ICS_STATUS, KOMTEK_ICS_LOG_CODE) VALUES (" + lastid_mki + "," + lastid + "," + vals[n] + "," + UserId + "," + datenow + ",1,'" + logcode + "')";
                    //db.Database.ExecuteSqlCommand(query_update);
                    //return Json(new { query = query_update, id = komtek_ics_id });
                    var fname1 = "LPK_DETAIL_SNI_ID,LPK_DETAIL_SNI_LPK_ID,LPK_DETAIL_SNI_SNI_ID,LPK_DETAIL_SNI_CREATE_BY,LPK_DETAIL_SNI_CREATE_DATE,LPK_DETAIL_SNI_STATUS,LPK_DETAIL_SNI_LOGCODE";
                    var fvalue1 = "'" + lastid_mki + "'," +
                                 "'" + idk + "'," +
                                 "'" + SNI_ID + "'," +
                                 "" + UserId + "," +
                                 datenow + "," +
                                 "1," +
                                 logcodeS;

                    //return Json(new { query = "INSERT INTO TRX_REGULASI_TEKNIS (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_LPK_DETAIL_SNI (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")");

                    String objek1 = fvalue1.Replace("'", "-");
                    MixHelper.InsertLog(logcodeS, objek1, 1);
                }
            }

            if (LPK_SCOPE_SCOPE_ID != null)
            {
                foreach (var SCOPE_ID in LPK_SCOPE_SCOPE_ID)
                {
                    var logcodeS = MixHelper.GetLogCode();
                    int lastid_mki = MixHelper.GetSequence("TRX_LPK_SCOPE");
                    //string query_update = "INSERT INTO MASTER_KOMTEK_ICS (KOMTEK_ICS_ID, KOMTEK_ICS_KOMTEK_ID, KOMTEK_ICS_ICS_ID, KOMTEK_ICS_CREATE_BY, KOMTEK_ICS_CREATE_DATE, KOMTEK_ICS_STATUS, KOMTEK_ICS_LOG_CODE) VALUES (" + lastid_mki + "," + lastid + "," + vals[n] + "," + UserId + "," + datenow + ",1,'" + logcode + "')";
                    //db.Database.ExecuteSqlCommand(query_update);
                    //return Json(new { query = query_update, id = komtek_ics_id });
                    var fname1 = "LPK_SCOPE_ID,LPK_SCOPE_LPK_ID,LPK_SCOPE_SCOPE_ID,LPK_SCOPE_CREATE_BY,LPK_SCOPE_CREATE_DATE,LPK_SCOPE_STATUS,LPK_SCOPE_LOGCODE";
                    var fvalue1 = "'" + lastid_mki + "'," +
                                 "'" + idk + "'," +
                                 "'" + SCOPE_ID + "'," +
                                 "" + UserId + "," +
                                 datenow + "," +
                                 "1," +
                                 logcodeS;

                    //return Json(new { query = "INSERT INTO TRX_REGULASI_TEKNIS (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_LPK_SCOPE (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")");

                    String objek1 = fvalue1.Replace("'", "-");
                    MixHelper.InsertLog(logcodeS, objek1, 1);
                }
            }

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id = 0) {
            var lemins = (from t in db.VIEW_LPK where t.LPK_ID == id select t).SingleOrDefault();
            ViewData["lemins"] = lemins;
            //ViewData["listICS"] = (from t in db.VIEW_LPK_DETAIL where t.LPK_LINGKUP_LPK_ID == lemins.LPK_ID && t.LPK_LINGKUP_STATUS == 1 orderby t.ICS_CODE ascending select t).ToList();
            ViewData["sni"] = (from sni in db.VIEW_LPK_SNI where sni.LPK_DETAIL_SNI_LPK_ID == id && sni.LPK_DETAIL_SNI_STATUS == 1 select sni).ToList();
            ViewData["scope"] = (from scope in db.VIEW_LPK_SCOPE where scope.LPK_SCOPE_LPK_ID == id && scope.LPK_SCOPE_STATUS == 1 select scope).ToList();
            var DataProvinsi = (from provinsi in db.VIEW_WILAYAH_PROVINSI where provinsi.WILAYAH_PARENT_ID == 0 && provinsi.WILAYAH_STATUS == 1 orderby provinsi.WILAYAH_ID ascending select provinsi).ToList();
            ViewData["Provinsi"] = DataProvinsi;
            var DataKabupaten = (from kab in db.VIEW_WILAYAH_KABUPATEN where kab.WILAYAH_PARENT_ID != 0 orderby kab.WILAYAH_ID ascending select kab).ToList();
            ViewData["Kabupaten"] = DataKabupaten;            
            return View();
        }

        [HttpPost]
        public ActionResult Edit(TRX_LPK tl, FormCollection formCollection, int[] LPK_DETAIL_SNI_SNI_ID, int[] LPK_SCOPE_SCOPE_ID)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_BIDANG");
            var datenow = MixHelper.ConvertDateNow();
            var update = "";
            var pathss = "/Upload/Dokumen/LAINNYA/LPK/";
            DateTime date2 = Convert.ToDateTime(tl.LPK_PERIODE_AKHIR);
            var dateakhir = "TO_DATE('" + date2.ToString("yyyy-MM-dd HH:mm:ss") + "', 'yyyy-mm-dd hh24:mi:ss')";

            string path = Server.MapPath("~/Upload/Dokumen/LAINNYA/LPK/");
            HttpPostedFileBase file_att = Request.Files["LPK_FILE"];
            var file_name_att = "";
            var filePath = "";
            var fileExtension = "";
           
                string lampiranregulasipath = file_att.FileName;
                if (lampiranregulasipath.Trim() != "")
                {
                    lampiranregulasipath = Path.GetFileNameWithoutExtension(file_att.FileName);
                    fileExtension = Path.GetExtension(file_att.FileName);
                    file_name_att = "LPK_" + tl.LPK_NOMOR.Replace(' ', '-') + "_" + lastid + fileExtension;
                    filePath = path + file_name_att;
                    file_att.SaveAs(filePath);

                    update =
                        "LPK_NAMA = '" + tl.LPK_NAMA + "'," +
                        "LPK_NOMOR = '" + tl.LPK_NOMOR + "'," +
                        "LPK_ALAMAT = '" + tl.LPK_ALAMAT + "'," +
                        "LPK_LAMPIRAN = '" + pathss + "" + file_name_att + "'," +
                        "LPK_TELEPON = '" + tl.LPK_TELEPON + "'," +
                        "LPK_EMAIL = '" + tl.LPK_EMAIL + "'," +
                        "LPK_WEBSITE = '" + tl.LPK_WEBSITE + "'," +
                        "LPK_CONTACT_PERSON = '" + tl.LPK_CONTACT_PERSON + "'," +
                        "LPK_PERIODE_AKHIR = " + dateakhir + "," +
                        "LPK_KETERANGAN = '" + tl.LPK_KETERANGAN + "'," +
                        "LPK_UPDATE_BY = '" + UserId + "'," +
                        "LPK_UPDATE_DATE = " + datenow + "";
                }
                else {
                    update =
                        "LPK_NAMA = '" + tl.LPK_NAMA + "'," +
                        "LPK_NOMOR = '" + tl.LPK_NOMOR + "'," +
                        "LPK_ALAMAT = '" + tl.LPK_ALAMAT + "'," +
                        "LPK_TELEPON = '" + tl.LPK_TELEPON + "'," +
                        "LPK_EMAIL = '" + tl.LPK_EMAIL + "'," +
                        "LPK_WEBSITE = '" + tl.LPK_WEBSITE + "'," +
                        "LPK_CONTACT_PERSON = '" + tl.LPK_CONTACT_PERSON + "'," +
                        "LPK_PERIODE_AKHIR = " + dateakhir + "," +
                        "LPK_KETERANGAN = '" + tl.LPK_KETERANGAN + "'," +
                        "LPK_UPDATE_BY = '" + UserId + "'," +
                        "LPK_UPDATE_DATE = " + datenow + "";
                }
                      

            var clause = "where LPK_ID = " + tl.LPK_ID;
            //return Json(new { query = "UPDATE TRX_LPK SET " + update.Replace("''", "NULL") + " " + clause }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE TRX_LPK SET " + update.Replace("''", "NULL") + " " + clause);

            var idk = db.Database.SqlQuery<int>("SELECT MAX(TL.LPK_ID) FROM TRX_LPK TL").SingleOrDefault();

            if (LPK_DETAIL_SNI_SNI_ID != null)
            {
                string query_update = "UPDATE TRX_LPK_DETAIL_SNI  SET LPK_DETAIL_SNI_STATUS = 0, LPK_DETAIL_SNI_UPDATE_BY =" + UserId + ", LPK_DETAIL_SNI_UPDATE_DATE=" + datenow + " WHERE LPK_DETAIL_SNI_LPK_ID = " + tl.LPK_ID;
                db.Database.ExecuteSqlCommand(query_update);
                foreach (var SNI_ID in LPK_DETAIL_SNI_SNI_ID)
                {
                    int cek = db.Database.SqlQuery<int>("SELECT COUNT(1) AS JML FROM TRX_LPK_DETAIL_SNI WHERE LPK_DETAIL_SNI_LPK_ID = '" + tl.LPK_ID + "' AND LPK_DETAIL_SNI_SNI_ID = '" + SNI_ID + "'").SingleOrDefault();
                    if (cek == 0)
                    {
                        var logcodeS = MixHelper.GetLogCode();
                        int lastid_mki = MixHelper.GetSequence("TRX_LPK_DETAIL_SNI");

                        var fname1 = "LPK_DETAIL_SNI_ID,LPK_DETAIL_SNI_LPK_ID,LPK_DETAIL_SNI_SNI_ID,LPK_DETAIL_SNI_CREATE_BY,LPK_DETAIL_SNI_CREATE_DATE,LPK_DETAIL_SNI_STATUS,LPK_DETAIL_SNI_LOGCODE";
                        var fvalue1 = "'" + lastid_mki + "'," +
                                     "'" + tl.LPK_ID + "'," +
                                     "'" + SNI_ID + "'," +
                                     "" + UserId + "," +
                                     datenow + "," +
                                     "1," +
                                     logcodeS;

                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_LPK_DETAIL_SNI (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")");

                        String objek1 = fvalue1.Replace("'", "-");
                        MixHelper.InsertLog(logcodeS, objek1, 1);
                    }
                    else
                    {
                        string query_updatea = "UPDATE TRX_LPK_DETAIL_SNI  SET LPK_DETAIL_SNI_STATUS = 1, LPK_DETAIL_SNI_UPDATE_BY =" + UserId + ", LPK_DETAIL_SNI_UPDATE_DATE=" + datenow + " WHERE LPK_DETAIL_SNI_LPK_ID = " + tl.LPK_ID + " AND LPK_DETAIL_SNI_SNI_ID = '" + SNI_ID + "'";
                        //return Json(new { data = query_updatea }, JsonRequestBehavior.AllowGet);
                        db.Database.ExecuteSqlCommand(query_updatea);
                    }

                }
            }

            if (LPK_SCOPE_SCOPE_ID != null)
            {
                string query_update = "UPDATE TRX_LPK_SCOPE  SET LPK_SCOPE_STATUS = 0, LPK_SCOPE_UPDATE_BY =" + UserId + ", LPK_SCOPE_UPDATE_DATE=" + datenow + " WHERE LPK_SCOPE_LPK_ID = " + tl.LPK_ID;
                db.Database.ExecuteSqlCommand(query_update);
                foreach (var SCOPE_ID in LPK_SCOPE_SCOPE_ID)
                {
                    int cek = db.Database.SqlQuery<int>("SELECT COUNT(1) AS JML FROM TRX_LPK_SCOPE WHERE LPK_SCOPE_LPK_ID = '" + tl.LPK_ID + "' AND LPK_SCOPE_SCOPE_ID = '" + SCOPE_ID + "'").SingleOrDefault();
                    if (cek == 0)
                    {
                        var logcodeS = MixHelper.GetLogCode();
                        int lastid_mki = MixHelper.GetSequence("TRX_LPK_SCOPE");
                        var fname1 = "LPK_SCOPE_ID,LPK_SCOPE_LPK_ID,LPK_SCOPE_SCOPE_ID,LPK_SCOPE_CREATE_BY,LPK_SCOPE_CREATE_DATE,LPK_SCOPE_STATUS,LPK_SCOPE_LOGCODE";
                        var fvalue1 = "'" + lastid_mki + "'," +
                                     "'" + tl.LPK_ID + "'," +
                                     "'" + SCOPE_ID + "'," +
                                     "" + UserId + "," +
                                     datenow + "," +
                                     "1," +
                                     logcodeS;

                        //return Json(new { query = "INSERT INTO TRX_REGULASI_TEKNIS (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
                        db.Database.ExecuteSqlCommand("INSERT INTO TRX_LPK_SCOPE (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")");

                        String objek1 = fvalue1.Replace("'", "-");
                        MixHelper.InsertLog(logcodeS, objek1, 1);
                    }
                    else
                    {
                        string query_updatea = "UPDATE TRX_LPK_SCOPE  SET LPK_SCOPE_STATUS = 1, LPK_SCOPE_UPDATE_BY =" + UserId + ", LPK_SCOPE_UPDATE_DATE=" + datenow + " WHERE LPK_SCOPE_LPK_ID = " + tl.LPK_ID + " AND LPK_SCOPE_SCOPE_ID = '" + SCOPE_ID + "'";
                        //return Json(new { data = query_updatea }, JsonRequestBehavior.AllowGet);
                        db.Database.ExecuteSqlCommand(query_updatea);
                    }

                }
            }

            //var logId = AuditTrails.GetLogId();
            String objek = update.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Read(int id = 0) {
           
            var lemins = (from t in db.VIEW_LPK where t.LPK_ID == id select t).SingleOrDefault();
            ViewData["lemins"] = lemins;
            ViewData["sni"] = (from a in db.VIEW_LPK_SNI where a.LPK_DETAIL_SNI_LPK_ID == id && a.LPK_DETAIL_SNI_STATUS == 1 select a).ToList();
            ViewData["scope"] = (from s in db.VIEW_LPK_SCOPE where s.LPK_SCOPE_LPK_ID == id && s.LPK_SCOPE_STATUS == 1 select s).ToList();
            var DataProvinsi = (from provinsi in db.VIEW_WILAYAH_PROVINSI where provinsi.WILAYAH_ID == lemins.LPK_PROVINSI && provinsi.WILAYAH_STATUS == 1 select provinsi).SingleOrDefault();
            ViewData["Provinsi"] = DataProvinsi;
            var DataKabupaten = (from kab in db.VIEW_WILAYAH_KABUPATEN where kab.WILAYAH_ID == lemins.LPK_KABUPATENKOTA select kab).SingleOrDefault();
            ViewData["kabupaten"] = DataKabupaten;
            return View();
        }

        public ActionResult aktifkan(int id = 0)
        {
            db.Database.ExecuteSqlCommand("UPDATE TRX_LPK SET LPK_STATUS = 1 WHERE LPK_ID = " + id);
            //db.Database.ExecuteSqlCommand("UPDATE TRX_LPK_DETAIL SET LPK_LINGKUP_STATUS = 1 WHERE LPK_LINGKUP_LPK_ID = " + id);
            db.Database.ExecuteSqlCommand("UPDATE TRX_LPK_DETAIL_SNI SET LPK_DETAIL_SNI_STATUS = 1 WHERE LPK_DETAIL_SNI_LPK_ID = " + id);
            db.Database.ExecuteSqlCommand("UPDATE TRX_LPK_SCOPE SET LPK_SCOPE_STATUS = 1 WHERE LPK_SCOPE_LPK_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Aktifkan";
            return RedirectToAction("Index");
        }
        public ActionResult nonaktifkan(int id = 0)
        {
            db.Database.ExecuteSqlCommand("UPDATE TRX_LPK SET LPK_STATUS = 0 WHERE LPK_ID = " + id);
            //db.Database.ExecuteSqlCommand("UPDATE TRX_LPK_DETAIL SET LPK_LINGKUP_STATUS = 0 WHERE LPK_LINGKUP_LPK_ID = " + id);
            db.Database.ExecuteSqlCommand("UPDATE TRX_LPK_DETAIL_SNI SET LPK_DETAIL_SNI_STATUS = 0 WHERE LPK_DETAIL_SNI_LPK_ID = " + id);
            db.Database.ExecuteSqlCommand("UPDATE TRX_LPK_SCOPE SET LPK_SCOPE_STATUS = 0 WHERE LPK_SCOPE_LPK_ID = " + id);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Non Aktifkan";
            return RedirectToAction("Index");
        }

        public JsonResult GetKotaKab(int id = 0)
        {
            var list = (from kotakab in db.VIEW_WILAYAH_KABUPATEN where kotakab.WILAYAH_PARENT_ID == id select kotakab).ToList();
            var result = from lists in list
                         select new string[] 
           { 
               Convert.ToString("<option value='"+lists.WILAYAH_ID+"'>"+lists.WILAYAH_NAMA+"</option>")
           };
            return Json(new
            {
                message = 1,
                value = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult getics(string q = "")
        {
            var SelectedData = db.Database.SqlQuery<MASTER_ICS>("SELECT * FROM MASTER_ICS WHERE UPPER(ICS_NAME_IND) LIKE UPPER('%" + q + "%')");
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
        public ActionResult FindSNI(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SNI_SELECT WHERE LOWER(VIEW_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%'").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI_SELECT WHERE LOWER(VIEW_SNI_SELECT.TEXT) LIKE '%" + q.ToLower() + "%') T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_SNI_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.ID, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }

        [HttpGet]
        public ActionResult FindSCOPE(string q = "", int page = 1)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_SCOPE_SELECT WHERE LOWER(VIEW_SCOPE_SELECT.TEXT) LIKE '%" + q.ToLower() + "%'").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SCOPE_SELECT WHERE LOWER(VIEW_SCOPE_SELECT.TEXT) LIKE '%" + q.ToLower() + "%') T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var datasni = db.Database.SqlQuery<VIEW_SCOPE_SELECT>(inject_clause_select);
            var sni = from cust in datasni select new { id = cust.ID, text = cust.TEXT };

            return Json(new { sni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);

        }
    }
}
