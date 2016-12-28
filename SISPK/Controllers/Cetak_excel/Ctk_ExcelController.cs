
using System.Web.Mvc;
using SISPK.Models;

namespace SISPK.Controllers.Cetak_excel
{
    public class Ctk_ExcelController : Controller
    {
        //
        // GET: /Ctk_Excel/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult ctk_usulan_baru()
        {
            return View();
        }

    }
}
