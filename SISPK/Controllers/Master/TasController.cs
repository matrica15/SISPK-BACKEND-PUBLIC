using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.IO;

namespace SISPK.Controllers.Master
{
    public class TasController : Controller
    {
        //
        // GET: /Tas/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListDataTas(DataTables param, int status = 0)
        {
            var default_order = "TAS_ID";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("TAS_ID");
            order_field.Add("TAS_NAME");
            order_field.Add("TAS_EMAIL");
            order_field.Add("INSTANSI_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "TAS_STATUS = "+status;

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
                search_clause += ")";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_MASTER_TAS WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_MASTER_TAS " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_MASTER_TAS>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.TAS_NAME), 
                Convert.ToString(list.TAS_EMAIL),
                Convert.ToString(list.INSTANSI_NAME),
                Convert.ToString((list.TAS_STATUS == 0)?"<center><a href='Tas/Detail/"+list.TAS_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a><a href='Tas/Edit/"+list.TAS_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Edit'><i class='action fa fa-edit'></i></a><a href='Tas/Aktifkan/"+list.TAS_ID+"' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Aktifkan'><i class='action fa fa-check'></i></a></center>":"<center><a href='Tas/Detail/"+list.TAS_ID+"' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a><a href='Tas/Edit/"+list.TAS_ID+"' class='btn purple btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Edit'><i class='action fa fa-edit'></i></a><a href='Tas/Nonaktifkan/"+list.TAS_ID+"' class='btn red btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Nonaktifkan'><i class='action glyphicon glyphicon-remove'></i></a></center>"),
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

        public ActionResult Create() {
            ViewData["listinstansi"] = (from t in db.MASTER_INSTANSI where t.INSTANSI_STATUS == 1 orderby t.INSTANSI_CODE ascending select t).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Create(MASTER_TAS mt, int[] ICS_ID, string[] TAS_EDUCATION_INSTITUSI, string[] TAS_EDUCATION_YEAR, string[] TAS_EDUCATION_PRODI, string[] TAS_RIWAYAT_INSTITUSI_NAME, string[] TAS_RIWAYAT_TAHUN, string[] TAS_RIWAYAT_BIDANG_JABATAN, string[] TAS_RIWAYAT_JENIS_PELATIHAN, string[] TAS_RIWAYAT_TAHUN_PEL, string[] TAS_RIWAYAT_PENYELENGGARA, string[] TAS_RIWAYAT_PENGALAMAN)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            //int lastid = MixHelper.GetSequence("TRX_SNI");
            int last_id = MixHelper.GetSequence("MASTER_TAS");
            var datenow = MixHelper.ConvertDateNow();

            var fname = "TAS_ID,TAS_NAME,TAS_INSTANSI_ID,TAS_ADDRESS,TAS_PHONE,TAS_FAX,TAS_OFFICE_ADDRESS,TAS_OFFICE_PHONE,TAS_HANDPHONE,TAS_EMAIL,TAS_CREATE_BY,TAS_CREATE_DATE,TAS_STATUS,TAS_LOG_CODE";
            var value = "'" + last_id + "', " +
                        "'" + mt.TAS_NAME + "'," +
                        "'" + mt.TAS_INSTANSI_ID + "'," +
                        "'" + mt.TAS_ADDRESS + "'," +
                        "'" + mt.TAS_PHONE + "'," +
                        "'" + mt.TAS_FAX + "'," +
                        "'" + mt.TAS_OFFICE_ADDRESS + "'," +
                        "'" + mt.TAS_OFFICE_PHONE + "'," +
                        "'" + mt.TAS_HANDPHONE + "'," +
                        "'" + mt.TAS_EMAIL + "'," +
                        "'" + UserId + "'," +
                        "" + datenow + "," +
                        "1," +
                        "" + logcode + "";               

            db.Database.ExecuteSqlCommand("INSERT INTO MASTER_TAS (" + fname + ") VALUES (" + value.Replace("''", "NULL") + ")");

            if (ICS_ID != null)
            {
                foreach (var TAS_ICS_ID in ICS_ID)
                {
                    var logcodeS = MixHelper.GetLogCode();
                    int lastid_mTDI = MixHelper.GetSequence("MASTER_TAS_DETAIL_ICS");
                    string query_create = "INSERT INTO MASTER_TAS_DETAIL_ICS (TAS_DETAIL_ICS_ID,TAS_DETAIL_ICS_TAS_ID,TAS_DETAIL_ICS_ICS_ID,TAS_DETAIL_ICS_STATUS) VALUES (" + lastid_mTDI + "," + last_id + "," + TAS_ICS_ID + ",1)";
                    db.Database.ExecuteSqlCommand(query_create);
                }
            }

            if (TAS_EDUCATION_INSTITUSI.Count() > 0)
            {
                var fnameSS = "TAS_EDUCATION_ID," +
                                "TAS_EDUCATION_TAS_ID," +
                                "TAS_EDUCATION_INSTITUSI," +
                                "TAS_EDUCATION_YEAR," +
                                "TAS_EDUCATION_PRODI,"+
                                "TAS_EDUCATION_STATUS";

                for (var i = 0; i < TAS_EDUCATION_INSTITUSI.Count(); i++)
                {
                    int lastidxx = MixHelper.GetSequence("MASTER_TAS_EDUCATION");
                    //var logcodeSS = MixHelper.GetLogCode();
                    //decimal amount = Convert.ToDecimal(Price[i].Replace(",", "")) * Convert.ToDecimal(QtyOpname[i].Replace(",", ""));
                    var fvalueSS = "'" + lastidxx + "'," +
                            "'" + last_id + "'," +
                            "'" + TAS_EDUCATION_INSTITUSI[i] + "'," +
                            "'" + TAS_EDUCATION_YEAR[i] + "'," +
                            "'" + TAS_EDUCATION_PRODI[i] + "',"+
                            "1";

                    db.Database.ExecuteSqlCommand("INSERT INTO MASTER_TAS_EDUCATION (" + fnameSS + ") VALUES (" + fvalueSS.Replace("''", "NULL") + ")");
                }
            }

            if (TAS_RIWAYAT_INSTITUSI_NAME.Count() > 0)
            {
                var fnameSSS = "TAS_RIWAYAT_ID," +
                                "TAS_RIWAYAT_TAS_ID," +
                                "TAS_RIWAYAT_TYPE," +
                                "TAS_RIWAYAT_INSTITUSI_NAME," +
                                "TAS_RIWAYAT_TAHUN," +
                                "TAS_RIWAYAT_BIDANG_JABATAN,"+
                                "TAS_RIWAYAT_STATUS";

                for (var i = 0; i < TAS_RIWAYAT_INSTITUSI_NAME.Count(); i++)
                {
                    int lastidxxx = MixHelper.GetSequence("MASTER_TAS_RIWAYAT");
                    //var logcodeSS = MixHelper.GetLogCode();
                    //decimal amount = Convert.ToDecimal(Price[i].Replace(",", "")) * Convert.ToDecimal(QtyOpname[i].Replace(",", ""));
                    var fvalueSSS = "'" + lastidxxx + "'," +
                            "'" + last_id + "'," +
                            "1," +
                            "'" + TAS_RIWAYAT_INSTITUSI_NAME[i] + "'," +
                            "'" + TAS_RIWAYAT_TAHUN[i] + "'," +
                            "'" + TAS_RIWAYAT_BIDANG_JABATAN[i] + "',"+
                            "1";
                    db.Database.ExecuteSqlCommand("INSERT INTO MASTER_TAS_RIWAYAT (" + fnameSSS + ") VALUES (" + fvalueSSS.Replace("''", "NULL") + ")");
                }
            }

            if (TAS_RIWAYAT_JENIS_PELATIHAN.Count() > 0)
            {
                var fnameSSS = "TAS_RIWAYAT_ID," +
                                "TAS_RIWAYAT_TAS_ID," +
                                "TAS_RIWAYAT_TYPE," +
                                "TAS_RIWAYAT_TAHUN," +
                                "TAS_RIWAYAT_JENIS_PELATIHAN," +
                                "TAS_RIWAYAT_PENYELENGGARA," +
                                "TAS_RIWAYAT_STATUS";

                for (var i = 0; i < TAS_RIWAYAT_JENIS_PELATIHAN.Count(); i++)
                {
                    int lastidxxxx = MixHelper.GetSequence("MASTER_TAS_RIWAYAT");
                    //var logcodeSS = MixHelper.GetLogCode();
                    //decimal amount = Convert.ToDecimal(Price[i].Replace(",", "")) * Convert.ToDecimal(QtyOpname[i].Replace(",", ""));
                    var fvalueSSS = "'" + lastidxxxx + "'," +
                            "'" + last_id + "'," +
                            "2," +
                            "'" + TAS_RIWAYAT_TAHUN_PEL[i] + "'," +
                            "'" + TAS_RIWAYAT_JENIS_PELATIHAN[i] + "'," +
                            "'" + TAS_RIWAYAT_PENYELENGGARA[i] + "'," +
                            "1";
                    db.Database.ExecuteSqlCommand("INSERT INTO MASTER_TAS_RIWAYAT (" + fnameSSS + ") VALUES (" + fvalueSSS.Replace("''", "NULL") + ")");
                }
            }

            if (TAS_RIWAYAT_PENGALAMAN.Count() > 0)
            {
                var fnameSSS = "TAS_RIWAYAT_ID," +
                                "TAS_RIWAYAT_TAS_ID," +
                                "TAS_RIWAYAT_TYPE," +
                                "TAS_RIWAYAT_PENGALAMAN," +
                                "TAS_RIWAYAT_STATUS";

                for (var i = 0; i < TAS_RIWAYAT_PENGALAMAN.Count(); i++)
                {
                    int lastidxxxxx = MixHelper.GetSequence("MASTER_TAS_RIWAYAT");
                    //var logcodeSS = MixHelper.GetLogCode();
                    //decimal amount = Convert.ToDecimal(Price[i].Replace(",", "")) * Convert.ToDecimal(QtyOpname[i].Replace(",", ""));
                    var fvalueSSS = "'" + lastidxxxxx + "'," +
                            "'" + last_id + "'," +
                            "3," +
                            "'" + TAS_RIWAYAT_PENGALAMAN[i] + "'," +
                            "1";
                    db.Database.ExecuteSqlCommand("INSERT INTO MASTER_TAS_RIWAYAT (" + fnameSSS + ") VALUES (" + fvalueSSS.Replace("''", "NULL") + ")");
                }
            }

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id =0)
        {
            ViewData["tas"] = (from t in db.VIEW_MASTER_TAS where t.TAS_ID == id select t).SingleOrDefault();
            ViewData["ics"] = (from a in db.VIEW_TAS_DETAIL_ICS where a.TAS_DETAIL_ICS_TAS_ID == id && a.TAS_DETAIL_ICS_STATUS == 1 select a).ToList();
            ViewData["education"] = (from a in db.MASTER_TAS_EDUCATION where a.TAS_EDUCATION_TAS_ID == id select a).ToList();
            ViewData["pengker"] = (from b in db.MASTER_TAS_RIWAYAT where b.TAS_RIWAYAT_TAS_ID == id && b.TAS_RIWAYAT_TYPE == 1 select b).ToList();
            ViewData["pelatihan"] = (from b in db.MASTER_TAS_RIWAYAT where b.TAS_RIWAYAT_TAS_ID == id && b.TAS_RIWAYAT_TYPE == 2 select b).ToList();
            ViewData["pengalaman"] = (from b in db.MASTER_TAS_RIWAYAT where b.TAS_RIWAYAT_TAS_ID == id && b.TAS_RIWAYAT_TYPE == 3 select b).ToList();
            ViewData["listinstansi"] = (from t in db.MASTER_INSTANSI where t.INSTANSI_STATUS == 1 orderby t.INSTANSI_CODE ascending select t).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult Edit(MASTER_TAS mt, int[] ICS_ID, string[] TAS_EDUCATION_INSTITUSI, string[] TAS_EDUCATION_YEAR, string[] TAS_EDUCATION_PRODI, string[] TAS_RIWAYAT_INSTITUSI_NAME, string[] TAS_RIWAYAT_TAHUN, string[] TAS_RIWAYAT_BIDANG_JABATAN, string[] TAS_RIWAYAT_JENIS_PELATIHAN, string[] TAS_RIWAYAT_TAHUN_PEL, string[] TAS_RIWAYAT_PENYELENGGARA, string[] TAS_RIWAYAT_PENGALAMAN)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            //int lastid = MixHelper.GetSequence("TRX_SNI");
            int last_id = MixHelper.GetSequence("MASTER_TAS");
            var datenow = MixHelper.ConvertDateNow();

            //var fname = "TAS_ID,TAS_NAME,TAS_INSTANSI_ID,TAS_ADDRESS,TAS_PHONE,TAS_FAX,TAS_OFFICE_ADDRESS,TAS_OFFICE_PHONE,TAS_HANDPHONE,TAS_EMAIL,TAS_CREATE_BY,TAS_CREATE_DATE,TAS_STATUS,TAS_LOG_CODE";
            var value = "TAS_NAME = '" + mt.TAS_NAME + "'," +
                        "TAS_INSTANSI_ID = '" + mt.TAS_INSTANSI_ID + "'," +
                        "TAS_ADDRESS = '" + mt.TAS_ADDRESS + "'," +
                        "TAS_PHONE = '" + mt.TAS_PHONE + "'," +
                        "TAS_FAX = '" + mt.TAS_FAX + "'," +
                        "TAS_OFFICE_ADDRESS ='" + mt.TAS_OFFICE_ADDRESS + "'," +
                        "TAS_OFFICE_PHONE ='" + mt.TAS_OFFICE_PHONE + "'," +
                        "TAS_HANDPHONE ='" + mt.TAS_HANDPHONE + "'," +
                        "TAS_EMAIL ='" + mt.TAS_EMAIL + "'," +
                        "TAS_UPDATE_BY ='" + UserId + "'," +
                        "TAS_UPDATE_DATE =" + datenow + "," +
                        "TAS_LOG_CODE =" + logcode + "";
            var clause = " WHERE TAS_ID =" + mt.TAS_ID;
            //db.Database.ExecuteSqlCommand("INSERT INTO MASTER_TAS (" + fname + ") VALUES (" + value.Replace("''", "NULL") + ")");
            db.Database.ExecuteSqlCommand("UPDATE MASTER_TAS SET " + value.Replace("''", "NULL") + " " + clause);

            //return Json(new
            //{

            //    aaData = "UPDATE MASTER_TAS SET " + value.Replace("''", "NULL") + " " + clause
            //}, JsonRequestBehavior.AllowGet);

            if (ICS_ID != null)
            {
                var updates = "TAS_DETAIL_ICS_STATUS=0";
                var clauses = "WHERE TAS_DETAIL_ICS_TAS_ID = " + mt.TAS_ID;
                db.Database.ExecuteSqlCommand("UPDATE MASTER_TAS_DETAIL_ICS SET " + updates.Replace("''", "NULL") + " " + clauses);
                foreach (var TAS_ICS_ID in ICS_ID)
                {
                    var jml = db.Database.ExecuteSqlCommand("SELECT COUNT(*) FROM MASTER_TAS_DETAIL_ICS WHERE TAS_DETAIL_ICS_ICS_ID =" + TAS_ICS_ID + " AND TAS_DETAIL_ICS_TAS_ID =" + mt.TAS_ID);
                    if (jml == 0)
                    {
                        int lastid_mTDI = MixHelper.GetSequence("MASTER_TAS_DETAIL_ICS");
                        string query_create = "INSERT INTO MASTER_TAS_DETAIL_ICS (TAS_DETAIL_ICS_ID,TAS_DETAIL_ICS_TAS_ID,TAS_DETAIL_ICS_ICS_ID,TAS_DETAIL_ICS_STATUS) VALUES (" + lastid_mTDI + "," + mt.TAS_ID + "," + TAS_ICS_ID + ",1)";
                        db.Database.ExecuteSqlCommand(query_create);
                    }
                    else
                    {
                        var query_update = "TAS_DETAIL_ICS_STATUS=1";
                        var query_clause = "WHERE TAS_DETAIL_ICS_TAS_ID = '" + mt.TAS_ID + "' AND TAS_DETAIL_ICS_ICS_ID=" + TAS_ICS_ID;
                        db.Database.ExecuteSqlCommand("UPDATE MASTER_TAS_DETAIL_ICS SET " + query_update.Replace("''", "NULL") + " " + query_clause);
                    }
                }
            }

            if (TAS_EDUCATION_INSTITUSI.Count() > 0)
            {
                db.Database.ExecuteSqlCommand("DELETE FROM MASTER_TAS_EDUCATION WHERE TAS_EDUCATION_TAS_ID = " + mt.TAS_ID);

                var fnameSS = "TAS_EDUCATION_ID," +
                                "TAS_EDUCATION_TAS_ID," +
                                "TAS_EDUCATION_INSTITUSI," +
                                "TAS_EDUCATION_YEAR," +
                                "TAS_EDUCATION_PRODI," +
                                "TAS_EDUCATION_STATUS";

                for (var i = 0; i < TAS_EDUCATION_INSTITUSI.Count(); i++)
                {
                    int lastidxx = MixHelper.GetSequence("MASTER_TAS_EDUCATION");
                    //var jml = db.Database.ExecuteSqlCommand("SELECT COUNT(*) FROM MASTER_TAS_EDUCATION WHERE TAS_EDUCATION_TAS_ID =" + mt.TAS_ID);
                    //var logcodeSS = MixHelper.GetLogCode();
                    //decimal amount = Convert.ToDecimal(Price[i].Replace(",", "")) * Convert.ToDecimal(QtyOpname[i].Replace(",", ""));
                    var fvalueSS = "'" + lastidxx + "'," +
                            "'" + mt.TAS_ID + "'," +
                            "'" + TAS_EDUCATION_INSTITUSI[i] + "'," +
                            "'" + TAS_EDUCATION_YEAR[i] + "'," +
                            "'" + TAS_EDUCATION_PRODI[i] + "'," +
                            "1";

                    db.Database.ExecuteSqlCommand("INSERT INTO MASTER_TAS_EDUCATION (" + fnameSS + ") VALUES (" + fvalueSS.Replace("''", "NULL") + ")");
                }
            }

            if (TAS_RIWAYAT_INSTITUSI_NAME.Count() > 0)
            {
                db.Database.ExecuteSqlCommand("DELETE FROM MASTER_TAS_RIWAYAT WHERE TAS_RIWAYAT_TAS_ID = " + mt.TAS_ID + " AND TAS_RIWAYAT_TYPE = 1");

                var fnameSSS = "TAS_RIWAYAT_ID," +
                                "TAS_RIWAYAT_TAS_ID," +
                                "TAS_RIWAYAT_TYPE," +
                                "TAS_RIWAYAT_INSTITUSI_NAME," +
                                "TAS_RIWAYAT_TAHUN," +
                                "TAS_RIWAYAT_BIDANG_JABATAN," +
                                "TAS_RIWAYAT_STATUS";

                for (var i = 0; i < TAS_RIWAYAT_INSTITUSI_NAME.Count(); i++)
                {
                    int lastidxxx = MixHelper.GetSequence("MASTER_TAS_RIWAYAT");
                    //var logcodeSS = MixHelper.GetLogCode();
                    //decimal amount = Convert.ToDecimal(Price[i].Replace(",", "")) * Convert.ToDecimal(QtyOpname[i].Replace(",", ""));
                    var fvalueSSS = "'" + lastidxxx + "'," +
                            "'" + mt.TAS_ID + "'," +
                            "1," +
                            "'" + TAS_RIWAYAT_INSTITUSI_NAME[i] + "'," +
                            "'" + TAS_RIWAYAT_TAHUN[i] + "'," +
                            "'" + TAS_RIWAYAT_BIDANG_JABATAN[i] + "'," +
                            "1";
                    db.Database.ExecuteSqlCommand("INSERT INTO MASTER_TAS_RIWAYAT (" + fnameSSS + ") VALUES (" + fvalueSSS.Replace("''", "NULL") + ")");
                }
            }

            if (TAS_RIWAYAT_JENIS_PELATIHAN.Count() > 0)
            {
                db.Database.ExecuteSqlCommand("DELETE FROM MASTER_TAS_RIWAYAT WHERE TAS_RIWAYAT_TAS_ID = " + mt.TAS_ID + " AND TAS_RIWAYAT_TYPE = 2");
                var fnameSSS = "TAS_RIWAYAT_ID," +
                                "TAS_RIWAYAT_TAS_ID," +
                                "TAS_RIWAYAT_TYPE," +
                                "TAS_RIWAYAT_TAHUN," +
                                "TAS_RIWAYAT_JENIS_PELATIHAN," +
                                "TAS_RIWAYAT_PENYELENGGARA," +
                                "TAS_RIWAYAT_STATUS";

                for (var i = 0; i < TAS_RIWAYAT_JENIS_PELATIHAN.Count(); i++)
                {
                    int lastidxxxx = MixHelper.GetSequence("MASTER_TAS_RIWAYAT");
                    //var logcodeSS = MixHelper.GetLogCode();
                    //decimal amount = Convert.ToDecimal(Price[i].Replace(",", "")) * Convert.ToDecimal(QtyOpname[i].Replace(",", ""));
                    var fvalueSSS = "'" + lastidxxxx + "'," +
                            "'" + mt.TAS_ID + "'," +
                            "2," +
                            "'" + TAS_RIWAYAT_TAHUN_PEL[i] + "'," +
                            "'" + TAS_RIWAYAT_JENIS_PELATIHAN[i] + "'," +
                            "'" + TAS_RIWAYAT_PENYELENGGARA[i] + "'," +
                            "1";
                    db.Database.ExecuteSqlCommand("INSERT INTO MASTER_TAS_RIWAYAT (" + fnameSSS + ") VALUES (" + fvalueSSS.Replace("''", "NULL") + ")");
                }
            }

            if (TAS_RIWAYAT_PENGALAMAN.Count() > 0)
            {
                db.Database.ExecuteSqlCommand("DELETE FROM MASTER_TAS_RIWAYAT WHERE TAS_RIWAYAT_TAS_ID = " + mt.TAS_ID + " AND TAS_RIWAYAT_TYPE = 3");
                var fnameSSS = "TAS_RIWAYAT_ID," +
                                "TAS_RIWAYAT_TAS_ID," +
                                "TAS_RIWAYAT_TYPE," +
                                "TAS_RIWAYAT_PENGALAMAN," +
                                "TAS_RIWAYAT_STATUS";

                for (var i = 0; i < TAS_RIWAYAT_PENGALAMAN.Count(); i++)
                {
                    int lastidxxxxx = MixHelper.GetSequence("MASTER_TAS_RIWAYAT");
                    //var logcodeSS = MixHelper.GetLogCode();
                    //decimal amount = Convert.ToDecimal(Price[i].Replace(",", "")) * Convert.ToDecimal(QtyOpname[i].Replace(",", ""));
                    var fvalueSSS = "'" + lastidxxxxx + "'," +
                            "'" + mt.TAS_ID + "'," +
                            "3," +
                            "'" + TAS_RIWAYAT_PENGALAMAN[i] + "'," +
                            "1";
                    db.Database.ExecuteSqlCommand("INSERT INTO MASTER_TAS_RIWAYAT (" + fnameSSS + ") VALUES (" + fvalueSSS.Replace("''", "NULL") + ")");
                }
            }

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Detail(int id = 0) {
            ViewData["tas"] = (from t in db.VIEW_MASTER_TAS where t.TAS_ID == id select t).SingleOrDefault();
            ViewData["ics"] = (from a in db.VIEW_TAS_DETAIL_ICS where a.TAS_DETAIL_ICS_TAS_ID == id && a.TAS_DETAIL_ICS_STATUS == 1 select a).ToList();
            ViewData["education"] = (from a in db.MASTER_TAS_EDUCATION where a.TAS_EDUCATION_TAS_ID == id select a).ToList();
            ViewData["pengker"] = (from b in db.MASTER_TAS_RIWAYAT where b.TAS_RIWAYAT_TAS_ID == id && b.TAS_RIWAYAT_TYPE == 1 select b).ToList();
            ViewData["pelatihan"] = (from b in db.MASTER_TAS_RIWAYAT where b.TAS_RIWAYAT_TAS_ID == id && b.TAS_RIWAYAT_TYPE == 2 select b).ToList();
            ViewData["pengalaman"] = (from b in db.MASTER_TAS_RIWAYAT where b.TAS_RIWAYAT_TAS_ID == id && b.TAS_RIWAYAT_TYPE == 3 select b).ToList();
            return View();
        }

        public ActionResult Aktifkan(int id = 0) {

            var updates = "TAS_STATUS=1";
            var clauses = "WHERE TAS_ID = " + id;
            db.Database.ExecuteSqlCommand("UPDATE MASTER_TAS SET " + updates.Replace("''", "NULL") + " " + clauses);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Diaktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult Nonaktifkan(int id = 0)
        {
            var updates = "TAS_STATUS=0";
            var clauses = "WHERE TAS_ID = " + id;
            db.Database.ExecuteSqlCommand("UPDATE MASTER_TAS SET " + updates.Replace("''", "NULL") + " " + clauses);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Dinonaktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult find_ics(string q = "", int page = 1)
        {

            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);

            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_ICS_SELECT WHERE LOWER(VIEW_ICS_SELECT.TEXT) LIKE '%" + q.ToLower() + "%'").SingleOrDefault();
            string inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_ICS_SELECT WHERE LOWER(VIEW_ICS_SELECT.TEXT) LIKE '%" + q.ToLower() + "%') T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            var ICS = db.Database.SqlQuery<VIEW_ICS_SELECT>(inject_clause_select);
            var ics_select = from cust in ICS select new { id = cust.ID, text = cust.TEXT };
            return Json(new { ics_select, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);
            //return Json(new { query = inject_clause_select }, JsonRequestBehavior.AllowGet);
        }
    }
}
