using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.Security.Cryptography;
using Aspose.Words;
using Aspose.Words.Tables;
using System.Diagnostics;
using Aspose.Words.Saving;
using System.Text;

namespace SISPK.Controllers.Laporan
{
    public class SniController : Controller
    {
        //
        // GET: /Sni/
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            ViewData["listICS"] = (from t in db.MASTER_ICS where t.ICS_STATUS == 1 orderby t.ICS_CODE ascending select t).ToList();
            ViewData["listKomtek"] = (from t in db.VIEW_KOMTEK where t.KOMTEK_STATUS == 1 orderby t.KOMTEK_CODE ascending select t).ToList();
            return View();
        }

        public ActionResult Review() {
            return View();
        }

    }
}
