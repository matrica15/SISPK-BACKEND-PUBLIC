using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.IO;
using Aspose.Words;

namespace SISPK.Controllers.Master
{
    public class TimeRevController : Controller
    {
        //
        // GET: /TimeRev/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Cek_timeRev()
        {

            var kmt = db.Database.SqlQuery<TRX_AKTIF_SNI_REV>("SELECT * FROM TRX_AKTIF_SNI_REV WHERE ID = '1'  ").SingleOrDefault();
            var hasil = kmt.MASA_AKTIF_SNI_REV;
            //return Content(hasil);

            return Json(new { hasil = hasil }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update_ms_aktif(TRX_AKTIF_SNI_REV dt)
        {
            var ms_aktif = dt.MASA_AKTIF_SNI_REV;
            db.Database.ExecuteSqlCommand("UPDATE TRX_AKTIF_SNI_REV SET MASA_AKTIF_SNI_REV = '"+ ms_aktif + "' WHERE ID = 1");

            return Json(new { hasil = ms_aktif }, JsonRequestBehavior.AllowGet);
        }

    }
}
