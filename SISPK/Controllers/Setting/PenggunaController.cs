using SISPK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SISPK.Helpers;
using SISPK.Filters;

namespace SISPK.Controllers.Setting
{
    [Auth(RoleTipe = 1)]
    public class PenggunaController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        //
        // GET: /Pengguna/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ListData(DataTables param, int status)
        {
            var default_order = "USER_FULL_NAME";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("USER_FULL_NAME");
            order_field.Add("USER_NAME");
            order_field.Add("ACCESS_NAME");
            order_field.Add("USER_LAST_LOGIN");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "USER_STATUS = '" + status + "' AND USER_TYPE_ID = 1 ";

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_USERS WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_USERS " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_USERS>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.USER_FULL_NAME), 
                Convert.ToString(list.USER_NAME), 
                Convert.ToString(list.ACCESS_NAME),                
                Convert.ToString(list.USER_LAST_LOGIN), 
                "<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Setting/Pengguna/Read/"+list.USER_ID+"'><i class='action fa fa-file-text-o'></i></a>"+
                "<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Setting/Pengguna/Edit/"+list.USER_ID+"'><i class='action fa fa-edit'></i></a>"+
                Convert.ToString((status == 1) ? "<a data-original-title='Non Aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Setting/Pengguna/Delete/"+list.USER_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>":"") +
                Convert.ToString((status == 0) ? "<a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Setting/Pengguna/Activated/"+list.USER_ID+"'><i class='action glyphicon glyphicon-check'></i></a></center>":"")
            };
            return Json(new
            {                
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
                //query = inject_clause_select
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Read(int id = 0)
        {
            SYS_USER users = db.SYS_USER.Find(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            ViewData["sysuser"] = users;
            ViewData["userinternal"] = (from t in db.SYS_USER_INTERN where t.USER_INTERN_ID == users.USER_REF_ID select t).SingleOrDefault();
            ViewData["listAccess"] = (from t in db.SYS_ACCESS where t.ACCESS_STATUS == 1 && t.ACCESS_ID != 3 && t.ACCESS_ID != 4 select t).ToList();
            return View();
        }

        public ActionResult Create()
        {
            ViewData["listAccess"] = (from t in db.SYS_ACCESS where t.ACCESS_STATUS == 1 && t.ACCESS_ID != 2 && t.ACCESS_ID != 4 select t).ToList();
            return View();
        }


        [HttpPost]
        public ActionResult Create(SYS_USER sys_users, SYS_USER_INTERN sys_users_intern)
        {

            var checkemail = db.SYS_USER.SqlQuery("SELECT * FROM SYS_USER WHERE USER_NAME = '" + sys_users.USER_NAME + "'  and USER_STATUS = 1").Count();
            if (checkemail > 0)
            {
                TempData["Notifikasi"] = 2;
                TempData["NotifikasiText"] = "Terjadi duplikasi data dengan Uername : " + sys_users.USER_NAME;
                return RedirectToAction("Create");
            }
            else
            {
                var UserId = Session["USER_ID"];
                var logcode = MixHelper.GetLogCode();
                int idintern = MixHelper.GetSequence("SYS_USER_INTERN");
                int lastid = MixHelper.GetSequence("SYS_USER");
                var datenow = MixHelper.ConvertDateNow();
                var fname1 = "USER_INTERN_ID,USER_INTERN_FULLNAME,USER_INTERN_ADDRESS,USER_INTERN_EMAIL,USER_INTERN_PHONE,USER_INTERN_CREATE_BY,USER_INTERN_CREATE_DATE,USER_INTERN_STATUS,USER_INTERN_LOG_CODE ";
                var fvalue1 = "'" + idintern + "', " +
                           "'" + sys_users_intern.USER_INTERN_FULLNAME + "', " +
                           "'" + sys_users_intern.USER_INTERN_ADDRESS + "'," +
                           "'" + sys_users_intern.USER_INTERN_EMAIL + "'," +
                           "'" + sys_users_intern.USER_INTERN_PHONE + "'," +
                           "'" + UserId + "', " +
                           datenow + "," +
                           "1,"+
                           "'" + logcode + "'";
                
                db.Database.ExecuteSqlCommand("INSERT INTO SYS_USER_INTERN (" + fname1 + ") VALUES (" + fvalue1.Replace("''", "NULL") + ")");

                var fname = "USER_ID,USER_NAME,USER_PASSWORD,USER_ACCESS_ID,USER_TYPE_ID,USER_REF_ID,USER_CREATE_BY,USER_CREATE_DATE,USER_LOG_CODE,USER_STATUS";
                var fvalue = "'" + lastid +"', " + 
                            "'" + sys_users.USER_NAME + "', " +
                            "'" + GenPassword(sys_users.USER_PASSWORD) + "', " +
                            "'" + sys_users.USER_ACCESS_ID + "'," +
                            "1," +
                            "'" + idintern + "', " +
                            "'" + UserId + "', " +
                            datenow + "," +
                            "'" + logcode + "'," +                            
                            "1";
                //return Json(new { query = "INSERT INTO SYS_USER (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")" }, JsonRequestBehavior.AllowGet);
                db.Database.ExecuteSqlCommand("INSERT INTO SYS_USER (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

                String objek = fvalue.Replace("'", "-");
                MixHelper.InsertLog(logcode,objek,1);
                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data Berhasil Disimpan";
                return RedirectToAction("Index");
            }

        }

        public ActionResult Edit(int id = 0)
        {
            SYS_USER users = db.SYS_USER.Find(id);
            if (users == null)
            {
                return HttpNotFound();
            }
            ViewData["sysuser"] = users;
            ViewData["userinternal"] = (from t in db.SYS_USER_INTERN where t.USER_INTERN_ID == users.USER_REF_ID select t).SingleOrDefault();
            ViewData["listAccess"] = (from t in db.SYS_ACCESS where t.ACCESS_STATUS == 1 && t.ACCESS_ID != 2 && t.ACCESS_ID != 4 select t).ToList();
            return View();
        }

        
        [HttpPost]
        public ActionResult Edit(SYS_USER sys_users, SYS_USER_INTERN sys_users_intern)
        {

            var checkemail = db.SYS_USER.SqlQuery("SELECT * FROM SYS_USER WHERE USER_NAME = '" + sys_users.USER_NAME + "'  AND USER_STATUS = 1 AND USER_ID != "+sys_users.USER_ID).Count();
            if (checkemail > 0)
            {
                TempData["Notifikasi"] = 2;
                TempData["NotifikasiText"] = "Terjadi duplikasi data dengan Uername : " + sys_users.USER_NAME;
                return RedirectToAction("Edit/" + sys_users.USER_ID);
            }
            else
            {
                var UserId = Session["USER_ID"];
                var datenow = MixHelper.ConvertDateNow();
                var fupdate1 = "USER_ACCESS_ID = '" + sys_users.USER_ACCESS_ID + "'," +
                                "USER_NAME = '" + sys_users.USER_NAME + "'," +
                                //"USER_PASSWORD = '" + sys_users.USER_PASSWORD + "'," +
                                "USER_UPDATE_BY = '" + UserId + "'," +
                                "USER_UPDATE_DATE = " + datenow;
                var fupdate2 = "USER_INTERN_FULLNAME = '" + sys_users_intern.USER_INTERN_FULLNAME + "'," +
                                "USER_INTERN_ADDRESS = '" + sys_users_intern.USER_INTERN_ADDRESS + "'," +
                                "USER_INTERN_EMAIL = '" + sys_users_intern.USER_INTERN_EMAIL + "'," +
                                "USER_INTERN_PHONE = '" + sys_users_intern.USER_INTERN_PHONE + "'," +
                                "USER_INTERN_UPDATE_BY = '" + UserId + "'," +
                                "USER_INTERN_UPDATE_DATE = " + datenow;
                //return Json(new { query1 = "UPDATE SYS_USER SET " + fupdate1 + " WHERE USER_ID = " + sys_users.USER_ID, query2 = "UPDATE SYS_USER_INTERN SET " + fupdate2 + " WHERE USER_INTERN_ID = " + sys_users_intern.USER_INTERN_ID}, JsonRequestBehavior.AllowGet);
                db.Database.ExecuteSqlCommand("UPDATE SYS_USER SET " + fupdate1 + " WHERE USER_ID = " + sys_users.USER_ID);
                db.Database.ExecuteSqlCommand("UPDATE SYS_USER_INTERN SET " + fupdate2 + " WHERE USER_INTERN_ID = " + sys_users_intern.USER_INTERN_ID);

                String objek = fupdate2.Replace("'", "-");
                //MixHelper.InsertLog(logcode, objek, 1);
                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data Berhasil Disimpan";
                return RedirectToAction("Index");
            }

        }

        public ActionResult Delete(int id = 0)
        {
            string query_update_group = "UPDATE SYS_USER SET USER_STATUS = 0 WHERE USER_ID = '" + id + "'";
            db.Database.ExecuteSqlCommand(query_update_group);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di Nonaktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult Activated(int id = 0)
        {
            string query_update_group = "UPDATE SYS_USER SET USER_STATUS = 1 WHERE USER_ID = '" + id + "'";
            db.Database.ExecuteSqlCommand(query_update_group);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di Aktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult ResetPassword(int id = 0)
        {
            int status = 1;
            var chars = "0123456789";
            var random = new Random();
            var result = new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
            
            var newpass = "SISPK" + result.ToString();

            string query_update_group = "UPDATE SYS_USER SET USER_PASSWORD = '" + GenPassword(newpass) + "' WHERE USER_ID = '" + id + "'";
            db.Database.ExecuteSqlCommand(query_update_group);
            
            return Json(new { status = status, value = newpass},JsonRequestBehavior.AllowGet);
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


    }
}
