using SISPK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SISPK.Controllers.Apps;
using System.DirectoryServices;

namespace SISPK.Controllers
{
    public class AuthController : Controller
    {
        //
        // GET: /Auth/
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            
            //return Json(new { wew= GenPassword("sispk")}, JsonRequestBehavior.AllowGet);
            return View();
        }
        [HttpPost]
        public ActionResult LoginLDAP(string username = "", string password = "")
        {
            var ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ip))
            {
                string[] ipRange = ip.Split(',');
                int le = ipRange.Length - 1;
                string trueIP = ipRange[le];
            }
            else
            {
                ip = Request.ServerVariables["REMOTE_ADDR"];
            }
            ViewData["loginType"] = "";
            
            var pDomainName = "ldap.bsn.go.id";
            var pUserID = username;
            var pPassword = password;

            DirectoryEntry dir = new DirectoryEntry(string.Format("LDAP://{0}", pDomainName), pUserID, pPassword);


            try
            {

                DirectorySearcher search = new DirectorySearcher(dir);

                search.Filter = "(SAMAccountName=" + pUserID + ")";

                search.PropertiesToLoad.Add("cn");

                search.FindOne();

                var convertUser = username;
                
                string passwordGen = GenPassword(password);
                var DATAUSER = db.Database.SqlQuery<VIEW_USERS>("SELECT * FROM VIEW_USERS WHERE USER_NAME = '" + username + "' AND USER_STATUS = 1 AND ROWNUM = 1").SingleOrDefault();

                if (DATAUSER != null)
                {
                    Session["USER_ID"] = DATAUSER.USER_ID;
                    Session["USER_NAME"] = DATAUSER.USER_NAME;
                    Session["USER_ACCESS_ID"] = DATAUSER.USER_ACCESS_ID;
                    //Session["USER_FIRST_NAME"] = DATAUSER.USER_FIRST_NAME;
                    //Session["USER_LAST_NAME"] = DATAUSER.USER_LAST_NAME;
                    Session["USER_FULL_NAME"] = DATAUSER.USER_FULL_NAME;
                    Session["ACCESS_NAME"] = DATAUSER.ACCESS_NAME;

                    //for session komtek
                    Session["IS_KOMTEK"] = ((DATAUSER.USER_TYPE_ID == 2) ? 1 : 0);
                    Session["KOMTEK_ID"] = DATAUSER.USER_KOMTEK_ID;
                    Session["KOMTEK_CODE"] = DATAUSER.USER_KOMTEK_KODE;
                    Session["KOMTEK_NAME"] = DATAUSER.USER_KOMTEK_NAMA;
                    Session["KOMTEK_SEKRE"] = DATAUSER.USER_KOMTEK_IS_SEKRE;

                    db.Database.ExecuteSqlCommand("UPDATE SYS_USER SET USER_IS_ONLINE = 1, USER_LAST_LOGIN = CURRENT_DATE WHERE USER_ID = '" + DATAUSER.USER_ID + "'");

                    //var DefaultMenu = DATAUSER.SYS_ACCESS.SYS_MENU.MENU_URL;

                    //return Redirect(DefaultMenu);
                    return RedirectToAction("index", new RouteValueDictionary(new { tipe = "Home", controller = "Dashboard", action = "index" }));
                }
                else
                {
                    TempData["IsError"] = 1;
                    return RedirectToRoute("Default", null);

                }

            }

            catch (Exception)
            {

                string passwordGen = GenPassword(password);
                var DATAUSER = db.Database.SqlQuery<VIEW_USERS>("SELECT * FROM VIEW_USERS WHERE USER_NAME = '" + username + "' AND USER_PASSWORD = '" + passwordGen + "' AND USER_STATUS = 1 AND ROWNUM = 1").SingleOrDefault();
                if (DATAUSER != null)    //User was found
                {
                    Session["USER_ID"] = DATAUSER.USER_ID;
                    Session["USER_NAME"] = DATAUSER.USER_NAME;
                    Session["USER_ACCESS_ID"] = DATAUSER.USER_ACCESS_ID;
                    //Session["USER_FIRST_NAME"] = DATAUSER.USER_FIRST_NAME;
                    //Session["USER_LAST_NAME"] = DATAUSER.USER_LAST_NAME;
                    Session["USER_FULL_NAME"] = DATAUSER.USER_FULL_NAME;
                    Session["ACCESS_NAME"] = DATAUSER.ACCESS_NAME;

                    //for session komtek
                    Session["IS_KOMTEK"] = ((DATAUSER.USER_TYPE_ID == 2) ? 1 : 0);
                    Session["KOMTEK_ID"] = DATAUSER.USER_KOMTEK_ID;
                    Session["KOMTEK_CODE"] = DATAUSER.USER_KOMTEK_KODE;
                    Session["KOMTEK_NAME"] = DATAUSER.USER_KOMTEK_NAMA;
                    Session["KOMTEK_SEKRE"] = DATAUSER.USER_KOMTEK_IS_SEKRE;

                    db.Database.ExecuteSqlCommand("UPDATE SYS_USER SET USER_IS_ONLINE = 1, USER_LAST_LOGIN = CURRENT_DATE WHERE USER_ID = '" + DATAUSER.USER_ID + "'");

                    //var DefaultMenu = DATAUSER.SYS_ACCESS.SYS_MENU.MENU_URL;

                    //return Redirect(DefaultMenu);
                    return RedirectToAction("index", new RouteValueDictionary(new { tipe = "Home", controller = "Dashboard", action = "index" }));
                }
                else    //User was not found
                {
                    TempData["IsError"] = 1;
                    return RedirectToRoute("Default", null);

                }

                //pServerMessage = ex.ToString();


            }

            

            //return Json(new { login_status, redirect_url, result, pServerMessage }, JsonRequestBehavior.AllowGet);
            //return result;
        }
        [HttpPost]
        public ActionResult Login(string username = "", string password = "")
        {
            string passwordGen = GenPassword(password);
            var DATAUSER = db.Database.SqlQuery<VIEW_USERS>("SELECT * FROM VIEW_USERS WHERE USER_NAME = '" + username + "' AND USER_PASSWORD = '" + passwordGen + "' AND USER_STATUS = 1 AND ROWNUM = 1").SingleOrDefault();
            //var DATAUSER = (from it in db.VIEW_USERS where it.USER_NAME == username && it.USER_PASSWORD == passwordGen && it.USER_STATUS == 1 select it).SingleOrDefault();
            if (DATAUSER != null)
            {
                Session["USER_ID"] = DATAUSER.USER_ID;
                Session["USER_NAME"] = DATAUSER.USER_NAME;
                Session["USER_ACCESS_ID"] = DATAUSER.USER_ACCESS_ID;
                //Session["USER_FIRST_NAME"] = DATAUSER.USER_FIRST_NAME;
                //Session["USER_LAST_NAME"] = DATAUSER.USER_LAST_NAME;
                Session["USER_FULL_NAME"] = DATAUSER.USER_FULL_NAME;
                Session["ACCESS_NAME"] = DATAUSER.ACCESS_NAME;
                Session["BIDANG_ID"] = DATAUSER.ACCESS_BIDANG_ID;
                //for session komtek
                Session["IS_KOMTEK"] = ((DATAUSER.USER_TYPE_ID == 2) ? 1 : 0);
                Session["KOMTEK_ID"] = DATAUSER.USER_KOMTEK_ID;
                Session["KOMTEK_CODE"] = DATAUSER.USER_KOMTEK_KODE;
                Session["KOMTEK_NAME"] = DATAUSER.USER_KOMTEK_NAMA;
                Session["KOMTEK_SEKRE"] = DATAUSER.USER_KOMTEK_IS_SEKRE;
                
                db.Database.ExecuteSqlCommand("UPDATE SYS_USER SET USER_IS_ONLINE = 1, USER_LAST_LOGIN = CURRENT_DATE WHERE USER_ID = '" + DATAUSER.USER_ID + "'");
                
                //var DefaultMenu = DATAUSER.SYS_ACCESS.SYS_MENU.MENU_URL;

                //return Redirect(DefaultMenu);
                return RedirectToAction("index", new RouteValueDictionary(new { tipe = "Home", controller = "Dashboard", action = "index" }));
            }
            else {
                TempData["IsError"] = 1;
                return RedirectToRoute("Default", null);
            }
        }
        public ActionResult Logout()
        {
            var USER_ID = Session["USER_ID"];
            db.Database.ExecuteSqlCommand("UPDATE SYS_USER SET USER_IS_ONLINE = 0 WHERE USER_ID = '" + USER_ID + "'");

            Session.Clear();
            Session.Abandon();
            return RedirectToRoute("Default", null);
            //return RedirectToAction("index", new RouteValueDictionary(new { tipe = "apps", controller = "auth", action = "index" }));
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
        public ActionResult AccessDenided()
        {
            return View();
        }
    }
}
