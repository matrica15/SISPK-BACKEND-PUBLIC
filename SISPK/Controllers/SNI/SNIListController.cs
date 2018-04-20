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

namespace SISPK.Controllers.SNI
{
    [Auth(RoleTipe = 1)]
    public class SNIListController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            var ListAkses = db.Database.SqlQuery<SYS_DOC_ACCESS>("SELECT * FROM SYS_DOC_ACCESS WHERE DOC_ACCESS_STATUS = 1").ToList();
            ViewData["ListAkses"] = ListAkses;
            var USER_ACCESS_ID = Convert.ToInt32(Session["USER_ACCESS_ID"]);
            ViewData["UserAkses"] = USER_ACCESS_ID;
            return View();
        }
        public ActionResult GetSni(DataTables param)
        {
            var default_order = "SNI_ID";
            var limit = 10;
            var IS_KOMTEK = Convert.ToInt32(Session["IS_KOMTEK"]); 
            var KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var USER_ACCESS_ID = Convert.ToInt32(Session["USER_ACCESS_ID"]);

            List<string> order_field = new List<string>();
            order_field.Add("SNI_NOMOR");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
            order_field.Add("SNI_JUDUL");
            order_field.Add("SNI_SK_NOMOR");
            order_field.Add("KOMTEK_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "";
            if (USER_ACCESS_ID == 8 || USER_ACCESS_ID == 1)
            {
                where_clause += "SNI_STATUS = 1";
            } else
            {
                where_clause += "SNI_STATUS = 1";
            }

                if (IS_KOMTEK == 1) {
                where_clause += " AND KOMTEK_ID = '" + KOMTEK_ID + "' ";
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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<int>("SELECT CAST(COUNT(*) AS INT) AS Jml FROM  VIEW_SNI " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SNI>(inject_clause_select);

            if (USER_ACCESS_ID == 8 || USER_ACCESS_ID == 1)
            {
                var result = from list in SelectedData
                             select new string[]
                {
                Convert.ToString(list.SNI_NOMOR),
                Convert.ToString(list.KOMTEK_CODE+" "+list.KOMTEK_NAME),
                Convert.ToString("<center>"+list.PROPOSAL_JENIS_PERUMUSAN_NAME+"</center>"),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.SNI_JUDUL+"</span>"),
                Convert.ToString(list.SNI_SK_NOMOR),
                Convert.ToString("<center><a target='"+((IS_KOMTEK==1 || list.DSNI_DOC_FILETYPE.ToLower()!="docx" || list.DSNI_DOC_FILETYPE.ToLower()!="doc")?"_blank":"")+"' href='"+((IS_KOMTEK==1)?"javascript:void(0)":((list.DSNI_DOC_FILETYPE.ToLower()=="docx" || list.DSNI_DOC_FILETYPE.ToLower()=="doc")?"javascript:void("+list.PROPOSAL_ID+","+100+","+list.PROPOSAL_ID+");":"/Download/Files/SNIInternal?PROPOSAL_ID="+list.PROPOSAL_ID+"&ACCESS_ID=0&TYPE=pdf"))+"' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a><a href='SNIList/NonAktif/" + list.SNI_ID + "' class='btn yellow btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Non-Aktif'><i class='action fa fa-toggle-off'></i></a></center>"),


                };

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = CountData,
                    iTotalDisplayRecords = CountData,
                    aaData = result.ToArray()
                }, JsonRequestBehavior.AllowGet);
            } else
            {
                var result = from list in SelectedData
                             select new string[]
                {
                Convert.ToString(list.SNI_NOMOR),
                Convert.ToString(list.KOMTEK_CODE+" "+list.KOMTEK_NAME),
                Convert.ToString("<center>"+list.PROPOSAL_JENIS_PERUMUSAN_NAME+"</center>"),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.SNI_JUDUL+"</span>"),
                Convert.ToString(list.SNI_SK_NOMOR),
                Convert.ToString("<center><a target='"+((IS_KOMTEK==1 || list.DSNI_DOC_FILETYPE.ToLower()!="docx" || list.DSNI_DOC_FILETYPE.ToLower()!="doc")?"_blank":"")+"' href='"+((IS_KOMTEK==1)?"javascript:void(0)":((list.DSNI_DOC_FILETYPE.ToLower()=="docx" || list.DSNI_DOC_FILETYPE.ToLower()=="doc")?"javascript:void("+list.PROPOSAL_ID+","+100+","+list.PROPOSAL_ID+");":"/Download/Files/SNIInternal?PROPOSAL_ID="+list.PROPOSAL_ID+"&ACCESS_ID=0&TYPE=pdf"))+"' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a></center>"),


                };

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = CountData,
                    iTotalDisplayRecords = CountData,
                    aaData = result.ToArray()
                }, JsonRequestBehavior.AllowGet);
            }
            
            
        }

        public ActionResult GetSniNonAktif(DataTables param)
        {
            var default_order = "SNI_ID";
            var limit = 10;
            var IS_KOMTEK = Convert.ToInt32(Session["IS_KOMTEK"]);
            var KOMTEK_ID = Convert.ToInt32(Session["KOMTEK_ID"]);
            var USER_ACCESS_ID = Convert.ToInt32(Session["USER_ACCESS_ID"]);

            List<string> order_field = new List<string>();
            order_field.Add("SNI_NOMOR");
            order_field.Add("KOMTEK_CODE");
            order_field.Add("PROPOSAL_JENIS_PERUMUSAN_NAME");
            order_field.Add("SNI_JUDUL");
            order_field.Add("SNI_SK_NOMOR");
            order_field.Add("KOMTEK_NAME");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;


            string where_clause = "SNI_STATUS = 0";

            if (IS_KOMTEK == 1)
            {
                where_clause += " AND KOMTEK_ID = '" + KOMTEK_ID + "' ";
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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_SNI WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<int>("SELECT CAST(COUNT(*) AS INT) AS Jml FROM  VIEW_SNI " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_SNI>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[]
            {
                Convert.ToString(list.SNI_NOMOR),
                Convert.ToString(list.KOMTEK_CODE+" "+list.KOMTEK_NAME),
                Convert.ToString("<center>"+list.PROPOSAL_JENIS_PERUMUSAN_NAME+"</center>"),
                Convert.ToString("<span class='judul_"+list.PROPOSAL_ID+"'>"+list.SNI_JUDUL+"</span>"),
                Convert.ToString(list.SNI_SK_NOMOR),
                Convert.ToString("<center><a target='"+((IS_KOMTEK==1 || list.DSNI_DOC_FILETYPE.ToLower()!="docx" || list.DSNI_DOC_FILETYPE.ToLower()!="doc")?"_blank":"")+"' href='"+((IS_KOMTEK==1)?"javascript:void(0)":((list.DSNI_DOC_FILETYPE.ToLower()=="docx" || list.DSNI_DOC_FILETYPE.ToLower()=="doc")?"javascript:void("+list.PROPOSAL_ID+","+100+","+list.PROPOSAL_ID+");":"/Download/Files/SNIInternal?PROPOSAL_ID="+list.PROPOSAL_ID+"&ACCESS_ID=0&TYPE=pdf"))+"' class='btn green btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Cetak'><i class='action fa fa-print'></i></a><a href='SNIList/Aktif/" + list.SNI_ID + "' class='btn blue btn-sm action tooltips' data-container='body' data-placement='top' data-original-title='Aktif'><i class='action fa fa-toggle-on'></i></a></center>"),


            };

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);


        }

        public ActionResult NonAktif(int id)
        {
            var UserId = Session["USER_ID"];
            var datenow = MixHelper.ConvertDateNow();
            var idsk = id;
            db.Database.ExecuteSqlCommand("UPDATE TRX_SNI SET SNI_STATUS = 0, SNI_UPDATE_BY = " + UserId + ", SNI_UPDATE_DATE = " + datenow + " WHERE SNI_ID = " + idsk);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Dinonaktifkan";

            return RedirectToAction("Index");
        }

        public ActionResult Aktif(int id)
        {
            var UserId = Session["USER_ID"];
            var datenow = MixHelper.ConvertDateNow();
            var idsk = id;
            db.Database.ExecuteSqlCommand("UPDATE TRX_SNI SET SNI_STATUS = 1, SNI_UPDATE_BY = " + UserId + ", SNI_UPDATE_DATE = " + datenow + " WHERE SNI_ID = " + idsk);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Diaktifkan";

            return RedirectToAction("Index");
        }
    }
}
