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
    public class PenggunapublicController : Controller
    {
        //
        // GET: /Penggunapublic/
        private SISPKEntities db = new SISPKEntities();

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
                Convert.ToString((status == 1) ? "<center><a data-original-title='Non Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Setting/Penggunapublic/Delete/"+list.USER_ID+"'><i class='action fa fa-check'></i></a></center>":"") +
                Convert.ToString((status == 0) ? "<center><a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Setting/Penggunapublic/Activated/"+list.USER_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>":"")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()                
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListDataPenggunaPublic(DataTables param, int status)
        {
            var default_order = "USER_PUBLIC_KTPSIM";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("USER_PUBLIC_KTPSIM");
            order_field.Add("USER_PUBLIC_NAMA_LENGKAP");
            order_field.Add("USER_PUBLIC_EMAIL");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "USER_STATUS = '" + status + "' AND ACCESS_ID = 4 ";

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_USERS_PUBLIC WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_USERS_PUBLIC " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_USERS_PUBLIC>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.USER_PUBLIC_KTPSIM), 
                Convert.ToString(list.USER_PUBLIC_NAMA_LENGKAP), 
                Convert.ToString(list.USER_PUBLIC_EMAIL),
                Convert.ToString((status == 0) ? "<center><a data-original-title='Approve' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Setting/Penggunapublic/Approve/"+list.USER_ID+"'><i class='action fa fa-check'></i></a><a data-original-title='Reject' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Setting/Penggunapublic/Reject/"+list.USER_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>":"")
                
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        //public ActionResult PenggunaPublikDetail(int id = 0)
        //{
        //    ViewData["pepude"] = (from t in db.VIEW_USERS_PUBLIC where t.USER_ID == id select t).SingleOrDefault();
        //    return View();
        //}

        public ActionResult Approve(int id = 0) {
            ViewData["pepude"] = (from t in db.VIEW_USERS_PUBLIC where t.USER_ID == id select t).SingleOrDefault();
            return View();
        }

        [HttpPost]
        public ActionResult Approve(VIEW_USERS_PUBLIC up) {
            int id = Convert.ToInt32(up.USER_PUBLIC_ID);

            var query = "UPDATE SYS_USER_PUBLIC SET USER_PUBLIC_STATUS = 1 WHERE USER_PUBLIC_ID = " + id;
            db.Database.ExecuteSqlCommand(query);

            VIEW_USERS user_item = db.VIEW_USERS.SingleOrDefault(t => t.USER_REF_ID == id && t.USER_ACCESS_ID == 4);

            var query1 = "UPDATE SYS_USER SET USER_STATUS = 1 WHERE USER_ID = " + user_item.USER_ID;
            db.Database.ExecuteSqlCommand(query1);

            var sysuser_public = (from s in db.SYS_USER_PUBLIC where s.USER_PUBLIC_ID == id select s).SingleOrDefault();
            //var sysuser = (from t in db.SYS_USER where t.USER_REF_ID == sysuser_public.USER_PUBLIC_ID select t).SingleOrDefault();

            //Send Account Activation to Email
            var email = (from t in db.SYS_EMAIL where t.EMAIL_IS_USE == 1 select t).SingleOrDefault();
            var link = (from s in db.SYS_LINK where s.LINK_IS_USE == 1 select s).SingleOrDefault();

            SendMailHelper.MailUsername = email.EMAIL_NAME;     //"aleh.mail@gmail.com";
            SendMailHelper.MailPassword = email.EMAIL_PASSWORD; //"r4h45143uy";

            SendMailHelper mailer = new SendMailHelper();
            mailer.ToEmail = sysuser_public.USER_PUBLIC_EMAIL;
            mailer.Subject = "Konfirmasi Member Baru - Sistem Informasi SNI";
            var isiEmail = "Terimakasih telah Melakukan Registrasi pada sistem kami. Berikut Data Detail anda : <br />";
            isiEmail += "Username : " + user_item.USER_NAME + "<br />";
            isiEmail += "Status   : Aktif <br />";
            isiEmail += "Silahkan klik tautan <a href='" + link.LINK_NAME + "/auth/index' target='_blank'>berikut</a> untuk login<br />";
            isiEmail += "Demikian Informasi yang kami sampaikan, atas kerjasamanya kami ucapkan terimakasih. <br />";
            isiEmail += "<span style='text-align:right;font-weight:bold;margin-top:20px;'>Web Administrator</span>";

            mailer.Body = isiEmail;
            mailer.IsHtml = true;
            mailer.Send();

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Setujui";
            return RedirectToAction("Index");
        }

        public ActionResult Reject(int id = 0) {
            VIEW_USERS_PUBLIC public_id = db.VIEW_USERS_PUBLIC.SingleOrDefault(t => t.USER_ID == id && t.USER_ACCESS_ID == 4);
            var query = "UPDATE SYS_USER_PUBLIC SET USER_PUBLIC_STATUS = 2 WHERE USER_ID = " + public_id.USER_PUBLIC_ID;
            db.Database.ExecuteSqlCommand(query);

            var query1 = "UPDATE SYS_USER SET USER_STATUS = 2 WHERE USER_ID = " + id;
            db.Database.ExecuteSqlCommand(query1);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Di Tolak";
            return RedirectToAction("Index");
        }
    }
}
