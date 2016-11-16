using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Reflection;



namespace SISPK.Controllers.Laporan
{
    public class WordController : Controller
    {
        //
        // GET: /Word/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CetakUsulan() {
            string filename = Path.Combine(Server.MapPath(@"~/Download/"), "KIB_KENDARAAN.docx");

            return View();
        }

    }
}
