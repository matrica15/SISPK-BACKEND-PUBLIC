using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.Security.Cryptography;

namespace SISPK.Controllers.Home
{
    public class AccountController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index(int id = 0)
        {
            ViewData["tipe"] = id;
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);
            var DataUser = db.Database.SqlQuery<VIEW_USERS>("SELECT * FROM VIEW_USERS WHERE USER_ID = " + USER_ID).FirstOrDefault();
            ViewData["DataUser"] = DataUser;
            var type_id = DataUser.USER_TYPE_ID;
            ViewData["DataType_id"] = type_id;
            ViewData["USER"] = "";
            if (type_id == 1)
            {
                ViewData["USER"] = (from t in db.SYS_USER_INTERN where t.USER_INTERN_ID == DataUser.USER_REF_ID select t).SingleOrDefault();
            }
            else
            {
                ViewData["USER"] = (from t in db.MASTER_KOMTEK_ANGGOTA where t.KOMTEK_ANGGOTA_KODE == DataUser.USER_REF_ID select t).SingleOrDefault();
            }
            
            return View();
        }
        [HttpPost]
        public ActionResult Update(string USER_INTERN_FULLNAME = "", string USER_INTERN_PHONE = "", string USER_INTERN_EMAIL = "", string USER_INTERN_ADDRESS = "", string USER_NAME = "", int USER_ID = 0, int USER_REF_ID = 0, int USER_TYPE_ID = 0)
        {
            var checkemail = db.SYS_USER.SqlQuery("SELECT * FROM SYS_USER WHERE USER_NAME = '" + USER_NAME + "'  AND USER_STATUS = 1 AND USER_ID != " + USER_ID).Count();
            if (checkemail > 0)
            {
                TempData["Notifikasi"] = 2;
                TempData["NotifikasiText"] = "Terjadi duplikasi data dengan Uername : " + USER_NAME;
                return RedirectToAction("Index", new { id = "1" });
            }
            else
            {
                var UserId = USER_ID;
                var datenow = MixHelper.ConvertDateNow();
                var fupdate1 = "USER_NAME = '" + USER_NAME + "'," +
                                "USER_UPDATE_BY = '" + UserId + "'," +
                                "USER_UPDATE_DATE = " + datenow;
                db.Database.ExecuteSqlCommand("UPDATE SYS_USER SET " + fupdate1 + " WHERE USER_ID = " + USER_ID);
                if (USER_TYPE_ID == 1)
                {
                    var fupdate2 = "USER_INTERN_FULLNAME = '" + USER_INTERN_FULLNAME + "'," +
                               "USER_INTERN_ADDRESS = '" + USER_INTERN_ADDRESS + "'," +
                               "USER_INTERN_EMAIL = '" + USER_INTERN_EMAIL + "'," +
                               "USER_INTERN_PHONE = '" + USER_INTERN_PHONE + "'," +
                               "USER_INTERN_UPDATE_BY = '" + UserId + "'," +
                               "USER_INTERN_UPDATE_DATE = " + datenow;
                    db.Database.ExecuteSqlCommand("UPDATE SYS_USER_INTERN SET " + fupdate2 + " WHERE USER_INTERN_ID = " + USER_REF_ID);

                    String objek = fupdate2.Replace("'", "-");
                }
                else {
                    var fupdate2 = "KOMTEK_ANGGOTA_NAMA = '" + USER_INTERN_FULLNAME + "'," +
                               "KOMTEK_ANGGOTA_ADDRESS = '" + USER_INTERN_ADDRESS + "'," +
                               "KOMTEK_ANGGOTA_EMAIL = '" + USER_INTERN_EMAIL + "'," +
                               "KOMTEK_ANGGOTA_TELP = '" + USER_INTERN_PHONE + "'," +
                               "KOMTEK_ANGGOTA_UPDATE_BY = '" + UserId + "'," +
                               "KOMTEK_ANGGOTA_UPDATE_DATE = " + datenow;
                    db.Database.ExecuteSqlCommand("UPDATE MASTER_KOMTEK_ANGGOTA SET " + fupdate2 + " WHERE KOMTEK_ANGGOTA_KODE = " + USER_REF_ID);

                    String objek = fupdate2.Replace("'", "-");
                }
               
                
                
                //MixHelper.InsertLog(logcode, objek, 1);
                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data Berhasil Disimpan";
                return RedirectToAction("Index", new { id = "1" });
            }
        }
        [HttpPost]
        public ActionResult ChangePassword(int USER_ID = 0, int USER_REF_ID = 0, string oldpassword = "", string USER_PASSWORD = "", string RE_USER_PASSWORD = "", string USER_EMAIL = "")
        {
            if (USER_PASSWORD == RE_USER_PASSWORD)
            {
                var DataUser = db.Database.SqlQuery<VIEW_USERS>("SELECT * FROM VIEW_USERS WHERE USER_ID = " + USER_ID).FirstOrDefault();
                if (GenPassword(oldpassword) == DataUser.USER_PASSWORD)
                {
                    var UserId = USER_ID;
                    var datenow = MixHelper.ConvertDateNow();
                    var fupdate1 = "USER_PASSWORD = '" + GenPassword(USER_PASSWORD) + "'," +
                                    "USER_UPDATE_BY = '" + UserId + "'," +
                                    "USER_UPDATE_DATE = " + datenow;
                    db.Database.ExecuteSqlCommand("UPDATE SYS_USER SET " + fupdate1 + " WHERE USER_ID = " + USER_ID);

                    //String objek = fupdate1.Replace("'", "-");
                    //MixHelper.InsertLog(logcode, objek, 1);
                    //Send Account Activation to Email
                    var email = (from t in db.SYS_EMAIL where t.EMAIL_IS_USE == 1 select t).SingleOrDefault();
                    SendMailHelper.MailUsername = email.EMAIL_NAME;      //"aleh.mail@gmail.com";
                    SendMailHelper.MailPassword = email.EMAIL_PASSWORD;  //"r4h45143uy";

                    SendMailHelper mailer = new SendMailHelper();
                    mailer.ToEmail = USER_EMAIL;
                    mailer.Subject = "Authentifikasi Perubahan Password Anggota Komtek - SISPK";
                    var isiEmail = "Selamat password anda sekarang diubah menjadi seperti di bawah ini : <br />";
                    isiEmail += "Username : " + DataUser.USER_NAME + "<br />";
                    isiEmail += "Password : " + USER_PASSWORD + "<br />";
                    isiEmail += "Demikian Informasi yang kami sampaikan, atas kerjasamanya kami ucapkan terimakasih. <br />";
                    isiEmail += "<span style='text-align:right;font-weight:bold;margin-top:20px;'>Web Administrator</span>";

                    mailer.Body = isiEmail;
                    mailer.IsHtml = true;
                    mailer.Send();

                    TempData["Notifikasi"] = 1;
                    TempData["NotifikasiText"] = "Data Berhasil Disimpan";
                }
                else
                {
                    TempData["Notifikasi"] = 1;
                    TempData["NotifikasiText"] = "Data Gagal Disimpan Password Lama Salah";
                }
            }
            else {
                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data Gagal Disimpan Password baru tidak sama";
            }
            return RedirectToAction("Index", new { id = "2" });
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
