using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Models;
using SISPK.Filters;
using SISPK.Helpers;
using System.Security.Cryptography;

namespace SISPK.Controllers.Master
{
    public class AnggotaController : Controller
    {
        //
        // GET: /Anggota/
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Read(int id = 0) {
            //VIEW_ANGGOTA anggota_item = db.VIEW_ANGGOTA.SingleOrDefault(t => t.KOMTEK_ANGGOTA_ID == id);
            ////return Json(new { data = komtek_item }, JsonRequestBehavior.AllowGet);
            //if (anggota_item == null)
            //{
            //    return HttpNotFound();
            //}
            ViewData["anggota_item"] = (from t in db.VIEW_ANGGOTA where t.KOMTEK_ANGGOTA_ID == id select t).SingleOrDefault();
            return View();
        }

        public ActionResult ListDataAnggota(DataTables param, int status=0)
        {
            var default_order = "KOMTEK_ANGGOTA_ID";
            var limit = 10;

            List<string> order_field = new List<string>();
            order_field.Add("KOMTEK_ANGGOTA_NAMA");
            order_field.Add("JABATAN");
            order_field.Add("KOMTEK_ANGGOTA_EMAIL");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("KOMTEK_NAME");
            order_field.Add("KOMTEK_ANGGOTA_STATUS");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "asc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "KOMTEK_ANGGOTA_STATUS ="+status;

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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_ANGGOTA WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_ANGGOTA " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_ANGGOTA>(inject_clause_select);
            //int no = 1;
            var result = from list in SelectedData
                         select new string[] 
            { 
                Convert.ToString(list.KOMTEK_ANGGOTA_NAMA), 
                Convert.ToString(list.JABATAN), 
                Convert.ToString(list.KOMTEK_ANGGOTA_EMAIL),
                Convert.ToString(list.KOMTEK_CODE),
                Convert.ToString(list.KOMTEK_NAME),
                //Convert.ToString(list.KOMTEK_NAME),
                //Convert.ToString(list.KOMTEK_SEKRETARIAT),
                //Convert.ToString(list.KOMTEK_EMAIL),
                //Convert.ToString((list.KOMTEK_ANGGOTA_STATUS == 1)?"<center>Aktif</center>":"<center>Tidak Aktif</center>"),
                //Convert.ToString((list.KOMTEK_STATUS == 0)?"<span class='red'>Tidak Aktif</span>":"<span class='red'>Aktif</span>"),
                "<center><a data-original-title='Lihat' data-placement='top' data-container='body' class='btn blue btn-sm action tooltips' href='/Master/Anggota/Read/"+list.KOMTEK_ANGGOTA_ID+"'><i class='action fa fa-file-text-o'></i></a></center>",
            };
            return Json(new
            {
                inject_clause_select,
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }

    }
}
