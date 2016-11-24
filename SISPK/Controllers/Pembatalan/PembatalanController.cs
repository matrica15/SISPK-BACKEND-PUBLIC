using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.Security.Cryptography;
using System.IO;

namespace SISPK.Controllers.Pembatalan
{
    public class PembatalanController : Controller
    {
        //
        // GET: /Pembatalan/
        private SISPKEntities db = new SISPKEntities();

        [Auth(RoleTipe = 1)]
        public ActionResult Index()
        {
            ViewData["User_Akses"] = Session["USER_ACCESS_ID"];
            return View();
        }

        public ActionResult listPerumusanDibatalkan(DataTables param)
        {
            var default_order = "PROPOSAL_PNPS_CODE";
            var limit = 10;
            var user_akses = Session["USER_ACCESS_ID"];
            var KomtekID = Session["KOMTEK_ID"];
            List<string> order_field = new List<string>();
            order_field.Add("PROPOSAL_PNPS_CODE");
            order_field.Add("PROPOSAL_JUDUL_PNPS");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
            order_field.Add("PROPOSAL_TAHAPAN");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;

            string where_clause = "PROPOSAL_IS_BATAL = 1";

            if (Convert.ToInt32(user_akses) == 2)
            {
                where_clause = "PROPOSAL_IS_BATAL = 1 AND PROPOSAL_KOMTEK_ID = " + KomtekID;
            }


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
                search_clause += " OR PROPOSAL_PNPS_CODE = '%" + search + "%')";
            }

            string inject_clause_count = "";
            string inject_clause_select = "";
            if (where_clause != "" || search_clause != "")
            {
                inject_clause_count = "WHERE " + where_clause + " " + search_clause;
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);

            var aksi = "";
            if (Convert.ToInt32(user_akses) == 2)
            {
                aksi = "display:none";
            }
            else
            {
                aksi = "";
            }

            var result = from list in SelectedData
                            select new string[]
            {
            Convert.ToString("<center>"+list.PROPOSAL_PNPS_CODE+"</center>"),
            Convert.ToString(list.PROPOSAL_JUDUL_PNPS),
            Convert.ToString(list.PROPOSAL_JENIS_PERUMUSAN_NAME),
            Convert.ToString(list.PROPOSAL_TAHAPAN),
            Convert.ToString(list.PROPOSAL_KET_BATAL),
            Convert.ToString("<center><a href='Pembatalan/Detail/" + list.PROPOSAL_ID + "' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Lihat'><i class='action fa fa-file-text-o'></i></a><a href='Pembatalan/Backstatus/" + list.PROPOSAL_ID + "' class='btn purple btn-sm action tooltips' data-container='body' style='"+ aksi +"' data-placement='top' data-original-title='Tidak Jadi dibatalkan'><i class='action fa fa-check'></i></a></center>"),
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

        [Auth(RoleTipe = 2)]
        public ActionResult Create() {
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult Create(TRX_PROPOSAL tp, int[] PROPOSAL_ID) {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            var datenow = MixHelper.ConvertDateNow();
            var date_now = DateTime.Now.ToString("dd_MM_yyyy_hh_mm");

            //return Json(new { total_count = date }, JsonRequestBehavior.AllowGet);

            if (PROPOSAL_ID.Count() > 0)
            {
                string path = Server.MapPath("~/Upload/Dokumen/PEMBATALAN/");
                HttpPostedFileBase file_att = Request.Files["PROPOSAL_BATAL_ATTACHMENT"];
                var file_name_att = "";
                var filePath = "";
                if (file_att != null)
                {
                    string attach_batal = file_att.FileName;
                    if (attach_batal.Trim() != "")
                    {
                        attach_batal = Path.GetFileNameWithoutExtension(file_att.FileName);
                        string fileExtension = Path.GetExtension(file_att.FileName);
                        file_name_att = "Attachment_Batal_" + date_now + "" + fileExtension;
                        filePath = path + file_name_att;
                        file_att.SaveAs(filePath);
                    }
                }

                foreach (var PID in PROPOSAL_ID)
                {
                    var update = " PROPOSAL_IS_BATAL = 1," +
                                 " PROPOSAL_KET_BATAL = '" + tp.PROPOSAL_KET_BATAL + "'," +
                                 " PROPOSAL_NO_SURAT_PEMBATALAN = '" + tp.PROPOSAL_NO_SURAT_PEMBATALAN + "'," +
                                 " PROPOSAL_BATAL_ATTACHMENT = '" + file_name_att + "'," +
                                 " PROPOSAL_UPDATE_DATE =" + datenow + "," +
                                 " PROPOSAL_UPDATE_BY ='" + UserId + "'";

                    var clausess = "where PROPOSAL_ID = " + PID;
                    db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET " + update.Replace("''", "NULL") + " " + clausess);
                    //return Json(new { total_count = "UPDATE TRX_PROPOSAL SET " + update.Replace("''", "NULL") + " " + clausess }, JsonRequestBehavior.AllowGet);
                }
            }
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Proposal telah dibatalkan";
            return RedirectToAction("Index");
        }

        public ActionResult Detail(int id = 0) {

            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == id select proposal).SingleOrDefault();
            var AcuanNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 1 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            //return Content("Test - " + id);
            var AcuanNonNormatif = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 2 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var Bibliografi = (from an in db.VIEW_PROPOSAL_REF where an.PROPOSAL_REF_TYPE == 3 && an.PROPOSAL_REF_PROPOSAL_ID == id orderby an.PROPOSAL_REF_ID ascending select an).ToList();
            var ICS = (from an in db.VIEW_PROPOSAL_ICS where an.PROPOSAL_ICS_REF_PROPOSAL_ID == id orderby an.ICS_CODE ascending select an).ToList();
            var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == id orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
            var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + id + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
            var Lampiran = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_NAME LIKE '%Lampiran Pendukung Usulan'").FirstOrDefault();
            var Bukti = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + id + " AND DOC_NAME LIKE '%Bukti Dukungan Usulan%'").FirstOrDefault();
            ViewData["DataProposal"] = DataProposal;
            ViewData["AcuanNormatif"] = AcuanNormatif;
            ViewData["AcuanNonNormatif"] = AcuanNonNormatif;
            ViewData["Bibliografi"] = Bibliografi;
            ViewData["ICS"] = ICS;
            ViewData["AdopsiList"] = AdopsiList;
            ViewData["RevisiList"] = RevisiList;
            ViewData["Lampiran"] = Lampiran;
            ViewData["Bukti"] = Bukti;
            return View();
        }

        public ActionResult Backstatus(int id = 0) {

            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            var datenow = MixHelper.ConvertDateNow();

            var update = " PROPOSAL_IS_BATAL = 0," +
                                 " PROPOSAL_KET_BATAL = 'Diaktifkan Kembali'," +
                                 " PROPOSAL_UPDATE_DATE =" + datenow + "," +
                                 " PROPOSAL_UPDATE_BY ='" + UserId + "'";

            var clausess = "where PROPOSAL_ID = " + id;

            db.Database.ExecuteSqlCommand("UPDATE TRX_PROPOSAL SET " + update.Replace("''", "NULL") + " " + clausess);

            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Usulan/Perumusan sudah aktif kembali";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult FindProposal(string q = "", int page = 1, int status = 0)
        {
            var limit = 10;
            var start = (page == 1) ? 10 : (page * limit);
            var end = (page == 1) ? 0 : ((page - 1) * limit);
            var CountDataS = "";
            string inject_clause_selects = "";
            if (status == 0)
            {
                CountDataS = "SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL WHERE PROPOSAL_STATUS IN (0,1) AND (PROPOSAL_IS_BATAL IS NULL OR PROPOSAL_IS_BATAL != 1) AND LOWER(VIEW_PROPOSAL.PROPOSAL_JUDUL_PNPS) LIKE '%" + q.ToLower() + "%'";
                inject_clause_selects = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_STATUS IN (0,1) AND (PROPOSAL_IS_BATAL IS NULL OR PROPOSAL_IS_BATAL != 1) AND LOWER(VIEW_PROPOSAL.PROPOSAL_JUDUL_PNPS) LIKE '%" + q.ToLower() + "%') T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            }
            else
            {
                CountDataS = "SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_PROPOSAL WHERE PROPOSAL_STATUS = " + status + " AND (PROPOSAL_IS_BATAL IS NULL OR PROPOSAL_IS_BATAL != 1) AND LOWER(VIEW_PROPOSAL.PROPOSAL_JUDUL_PNPS) LIKE '%" + q.ToLower() + "%'";
                inject_clause_selects = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_STATUS = " + status + " AND (PROPOSAL_IS_BATAL IS NULL OR PROPOSAL_IS_BATAL != 1) AND LOWER(VIEW_PROPOSAL.PROPOSAL_JUDUL_PNPS) LIKE '%" + q.ToLower() + "%') T1 WHERE ROWNUM <= " + Convert.ToString(start) + ") WHERE ROWNUMBER > " + Convert.ToString(end);
            }
            var CountData = db.Database.SqlQuery<decimal>(CountDataS).SingleOrDefault();
            string inject_clause_select = inject_clause_selects;

            var datarasni = db.Database.SqlQuery<VIEW_PROPOSAL>(inject_clause_select);
            var rasni = from cust in datarasni select new { id = cust.PROPOSAL_ID, text = cust.PROPOSAL_PNPS_CODE + " " + cust.PROPOSAL_JUDUL_PNPS };

            return Json(new { rasni, total_count = CountData, inject_clause_select }, JsonRequestBehavior.AllowGet);
        }

    }
}
