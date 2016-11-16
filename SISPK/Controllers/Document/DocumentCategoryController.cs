using SISPK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SISPK.Helpers;

namespace SISPK.Controllers.Document
{
    public class DocumentCategoryController : Controller
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
            var default_order = "REF_CODE";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("REF_CODE");
            order_field.Add("REF_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "REF_TYPE = 1 AND REF_STATUS = '" + status + "' ";

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM MASTER_REFERENCES WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  MASTER_REFERENCES " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<MASTER_REFERENCES>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            {
                Convert.ToString(list.REF_CODE), 
                Convert.ToString(list.REF_NAME),

                Convert.ToString("<center>") +
                Convert.ToString((status == 1) ? "<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Document/DocumentCategory/Edit/"+list.REF_ID+"'><i class='action fa fa-edit'></i></a><a data-original-title='Non Aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Document/DocumentCategory/Delete/"+list.REF_ID+"'><i class='action glyphicon glyphicon-remove'></i></a></center>":"") +
                Convert.ToString((status == 0) ? "<a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn green btn-sm action tooltips' href='/Document/DocumentCategory/Activate/"+list.REF_ID+"'><i class='action glyphicon glyphicon-check'></i></a></center>":"") +
                Convert.ToString("<center>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(MASTER_REFERENCES references)
        {

            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("MASTER_REFERENCES");
            var datenow = MixHelper.ConvertDateNow();

            var fname = "REF_ID, REF_TYPE, REF_CODE, REF_NAME, REF_CREATE_BY, REF_CREATE_DATE, REF_STATUS, REF_LOG_CODE";
            var fvalue = "'" + lastid + "', " +
                        "'1', " +
                        "'" + references.REF_CODE + "', " +
                        "'" + references.REF_NAME + "'," +
                        "'" + UserId + "', " +
                        datenow + "," +
                        "'1'," +
                        "'" + logcode + "'";
            db.Database.ExecuteSqlCommand("INSERT INTO MASTER_REFERENCES (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

            String objek = fvalue.Replace("'", "-");
            MixHelper.InsertLog(logcode, objek, 1);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id = 0)
        {
            MASTER_REFERENCES referenceData = db.MASTER_REFERENCES.Find(id);
            if (referenceData == null)
            {
                return HttpNotFound();
            }
            return View(referenceData);
        }

        [HttpPost]
        public ActionResult Edit(FormCollection formCollection)
        {
            TempData["Notifikasi"] = 0;
            TempData["NotifikasiText"] = "Data Gagal Disimpan";
            if (ModelState.IsValid)
            {
                string RefId = formCollection["DOCCATE_ID"];
                string RefName = formCollection["DOCCATE_NAME"];

                string query_reset = "UPDATE MASTER_REFERENCES SET REF_NAME = '" + RefName + "' WHERE REF_TYPE = 1 AND REF_ID = " + RefId;
                db.Database.ExecuteSqlCommand(query_reset);
                TempData["Notifikasi"] = 1;
                TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            }
            return RedirectToAction("Index");

        }

        public ActionResult Delete(int id = 0)
        {
            string query_update_group = "UPDATE MASTER_REFERENCES SET REF_STATUS = 0 WHERE REF_ID = '" + id + "' AND REF_TYPE = 1";
            db.Database.ExecuteSqlCommand(query_update_group);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di Nonaktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult Activate(int id = 0)
        {
            string query_update_group = "UPDATE MASTER_REFERENCES SET REF_STATUS = 1 WHERE REF_ID = '" + id + "' AND REF_TYPE = 1";
            db.Database.ExecuteSqlCommand(query_update_group);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di Aktifkan";
            return RedirectToAction("Index");
        }


    }
}
