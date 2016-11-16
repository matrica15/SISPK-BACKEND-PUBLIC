using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.Security.Cryptography;

namespace SISPK.Controllers.Portal
{
    public class ProfileController : Controller
    {
        //
        // GET: /Profile/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            ViewData["profil"] = (from t in db.PORTAL_PROFILE where t.PROFILE_STATUS == 1 select t).SingleOrDefault();

            //return Json(new
            //{

            //    aaData = ViewBag.aa.PROFILE_TENTANG_KAMI
            //}, JsonRequestBehavior.AllowGet);
            return View();

        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Index(PORTAL_PROFILE pp) {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();            
            var datenow = MixHelper.ConvertDateNow();
            var idk = db.Database.SqlQuery<int>("SELECT MAX(MK.PROFILE_ID) FROM PORTAL_PROFILE MK").SingleOrDefault();

            var update =                        
                        "PROFILE_TENTANG_KAMI = '"+pp.PROFILE_TENTANG_KAMI+"',"+
                        "PROFILE_KONTAK_KAMI = '"+pp.PROFILE_KONTAK_KAMI+"',"+
                        "PROFILE_UPDATE_DATE = "+datenow+","+
                        "PROFILE_UPDATE_BY = '"+UserId+"'";

            var clause = "where PROFILE_ID = " + idk;
            //return Json(new { query = "UPDATE PORTAL_PROFILE SET " + update.Replace("''", "NULL") + " " + clause }, JsonRequestBehavior.AllowGet);
            db.Database.ExecuteSqlCommand("UPDATE PORTAL_PROFILE SET " + update.Replace("''", "NULL") + " " + clause);

            //var logId = AuditTrails.GetLogId();            
            String objek = update.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

    }
}
