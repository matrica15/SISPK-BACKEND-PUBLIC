using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SISPK.Models;
using System.Text;
using System.Security.Cryptography;

namespace SISPK.Helpers
{
    public class AuthHelper
    {
        private SISPKEntities db = new SISPKEntities();
        public static Decimal punya_sub(Decimal id)
        {
            using (var db = new SISPKEntities())
            {
                var USER_ACCESS_ID = Convert.ToInt32(System.Web.HttpContext.Current.Session["USER_ACCESS_ID"]);
                var jml = db.Database.SqlQuery<Decimal>("SELECT CAST(COUNT(*) AS NUMBER) FROM SYS_MENU WHERE MENU_PARENT_ID=" + id + " AND MENU_ID IN(SELECT SYS_MENU.MENU_ID FROM SYS_ACCESS_DETAIL INNER JOIN SYS_ACCESS ON SYS_ACCESS_DETAIL.ACCESS_DETAIL_ACCESS_ID = SYS_ACCESS.ACCESS_ID AND SYS_ACCESS.ACCESS_STATUS = 1 INNER JOIN SYS_MENU ON SYS_ACCESS_DETAIL.ACCESS_DETAIL_MENU_ID = SYS_MENU.MENU_ID AND SYS_MENU.MENU_STATUS = 1 WHERE SYS_ACCESS.ACCESS_ID = " + USER_ACCESS_ID + " AND SYS_ACCESS_DETAIL.ACCESS_DETAIL_STATUS = 1 AND SYS_ACCESS_DETAIL.ACCESS_DETAIL_TYPE = 1 GROUP BY SYS_MENU.MENU_ID) ORDER BY MENU_SORT ASC").SingleOrDefault();
                return jml;
            }
        }
        
        public static string buat_menu(Decimal parent = 0)
        {
            using (var db = new SISPKEntities())
            {
                var menu = ""; // inisialisasi awal
                var USER_ACCESS_ID = Convert.ToInt32(System.Web.HttpContext.Current.Session["USER_ACCESS_ID"]);
                var hasil = db.Database.SqlQuery<SYS_MENU>("SELECT * FROM SYS_MENU WHERE MENU_PARENT_ID=" + parent + " AND MENU_ID IN(SELECT SYS_MENU.MENU_ID FROM SYS_ACCESS_DETAIL INNER JOIN SYS_ACCESS ON SYS_ACCESS_DETAIL.ACCESS_DETAIL_ACCESS_ID = SYS_ACCESS.ACCESS_ID AND SYS_ACCESS.ACCESS_STATUS = 1 INNER JOIN SYS_MENU ON SYS_ACCESS_DETAIL.ACCESS_DETAIL_MENU_ID = SYS_MENU.MENU_ID AND SYS_MENU.MENU_STATUS = 1 WHERE SYS_ACCESS.ACCESS_ID = " + USER_ACCESS_ID + " AND SYS_ACCESS_DETAIL.ACCESS_DETAIL_STATUS = 1 GROUP BY SYS_MENU.MENU_ID) ORDER BY MENU_SORT ASC").ToList();

                foreach (var res in hasil)
                {
                    if (res.MENU_PARENT_ID == parent)
                    {
                        var host = System.Web.HttpContext.Current.Request.Url.AbsolutePath;
                        var NewHost = res.MENU_URL;
                        if (host == NewHost)
                        {
                            if (punya_sub(res.MENU_ID) == 0)
                            {
                                menu += "<li class='start active open'><a href='" + res.MENU_URL + "'><i class='" + res.MENU_ICON + "'></i><span class='title'> " + res.MENU_NAME + "</span><span class='selected'></span>";
                            }
                            else
                            {
                                menu += "<li class='start active open'><a href='" + res.MENU_URL + "'><i class='" + res.MENU_ICON + "'></i><span class='title'> " + res.MENU_NAME + "</span><span class='selected'></span><span class='arrow open'></span>";
                            }
                        }
                        else
                        {
                            menu += "<li class=''><a href='" + res.MENU_URL + "'><i class='" + res.MENU_ICON + "'></i> <span class='title'> " + res.MENU_NAME + "</span>";
                        }
                        if (punya_sub(res.MENU_ID) > 0)
                        {
                            menu += "<span class='arrow'></span>";
                        }
                        menu += "</a>";

                        if (punya_sub(res.MENU_ID) > 0)
                        {
                            menu += "<ul class='sub-menu'>";
                            menu += buat_menu(res.MENU_ID);
                            menu += "</ul>";
                        }
                        menu += "</li>";
                    }
                }
                menu += "";
                return menu;
            }

        }
        public static string buat_breadcrumb(int parent = 0)
        {
            using (var db = new SISPKEntities())
            {
                var menu = ""; // inisialisasi awal
                //var host = System.Web.HttpContext.Current.Request.Url.AbsolutePath;
                var tipe = HttpContext.Current.Request.RequestContext.RouteData.Values["tipe"];
                var controller = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"];
                var action = Convert.ToString(HttpContext.Current.Request.RequestContext.RouteData.Values["action"]).ToLower();
                var host = "/" + tipe + "/" + controller;
                var hasil = db.Database.SqlQuery<SYS_MENU>("select * from ( select * from SYS_MENU WHERE MENU_URL LIKE '" + host + "%' order by MENU_PARENT_ID ASC ) where ROWNUM = 1").SingleOrDefault();
                if (hasil != null)
                {
                    var hasil2 = db.Database.SqlQuery<SYS_MENU>("select * from ( select * from SYS_MENU WHERE MENU_ID = " + hasil.MENU_PARENT_ID + " order by MENU_PARENT_ID ASC ) where ROWNUM = 1").SingleOrDefault();

                    var newaction = "";
                    if (action == "index" || action == "baru")
                    {
                        newaction = "Daftar";
                    }
                    else if (action == "create")
                    {
                        newaction = "Tambah Data";
                    }
                    else if (action == "detail")
                    {
                        newaction = "Lihat Data";
                    }
                    else if (action == "update")
                    {
                        newaction = "Ubah Data";
                    }
                    else if (action == "assignkomtek")
                    {
                        newaction = "Persetujuan";
                    }
                    else if (action == "comment")
                    {
                        newaction = "Komentar ";
                    }
                    else if (action == "setting")
                    {
                        newaction = "Setting ";
                    }
                    else if (action == "approval" || action == "approvalusulan")
                    {
                        newaction = "Persetujuan";
                    }
                    else if (action == "pengesahan")
                    {
                        newaction = "Pengesahan";
                    }
                    else
                    {
                        newaction = action;
                    }
                    if (hasil2 != null)
                    {
                        menu += "<li>";
                        menu += "<i class='" + hasil2.MENU_ICON + "'></i> ";
                        menu += "<a href='" + hasil2.MENU_URL + "'>" + hasil2.MENU_NAME + "</a>";
                        menu += "<i class='fa fa-angle-right'></i>";
                        menu += "</li>";
                    }

                    menu += "<li>";
                    menu += "<a href='" + hasil.MENU_URL + "'>" + hasil.MENU_NAME + "</a>";
                    menu += "<i class='fa fa-angle-right'></i>";
                    menu += "</li>";
                    menu += "<li>";
                    menu += "<a href='" + hasil.MENU_URL + "'>" + newaction + " " + hasil.MENU_NAME + "</a>";
                    menu += "</li>";
                }
                return menu;
            }

        }
        public static string buat_title(int parent = 0)
        {
            using (var db = new SISPKEntities())
            {
                var menu = ""; // inisialisasi awal
                var tipe = HttpContext.Current.Request.RequestContext.RouteData.Values["tipe"];
                var controller = HttpContext.Current.Request.RequestContext.RouteData.Values["controller"];
                var action = HttpContext.Current.Request.RequestContext.RouteData.Values["action"];
                var host = "/" + tipe + "/" + controller;
                var hasil = db.Database.SqlQuery<SYS_MENU>("select * from ( select * from SYS_MENU WHERE MENU_URL LIKE '" + host + "%' order by MENU_PARENT_ID ASC ) where ROWNUM = 1").SingleOrDefault();
                if (hasil != null)
                {
                    if (hasil.MENU_PARENT_ID == 0)
                    {
                        menu += hasil.MENU_NAME;
                    }
                    else
                    {
                        var hasil2 = db.Database.SqlQuery<SYS_MENU>("SELECT * FROM SYS_MENU WHERE MENU_ID='" + hasil.MENU_PARENT_ID + "'").SingleOrDefault();
                        menu += hasil2.MENU_NAME + " - " + hasil.MENU_NAME;
                    }
                }

                return menu;
            }

        }
        public static string buat_notif(int id = 1)
        {
            using (var db = new SISPKEntities())
            {
                var notif = ""; // inisialisasi awal
                var BIDANG_ID = Convert.ToInt32(HttpContext.Current.Session["BIDANG_ID"]);
                if (id == 1)
                {
                    if (Convert.ToInt32(HttpContext.Current.Session["IS_KOMTEK"]) != 1)
                    {
                        //var hasil = db.Database.SqlQuery<VIEW_NOTIFIKASI>("SELECT * FROM VIEW_NOTIFIKASI WHERE JUMLAH > 0 AND NOTIF_TYPE = 1 "+((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN(" + BIDANG_ID+",0)" : "")).ToList();
                        var hasil = db.Database.SqlQuery<VIEW_NOTIFIKASI>("SELECT MAX(AA.NOTIF_ID) NOTIF_ID, AA.JENIS_NOTIF, SUM(AA.JUMLAH) JUMLAH, AA.NOTIF_SORT, AA.NOTIF_TYPE, AA.NOTIF_LINK, MAX(AA.KOMTEK_ID) KOMTEK_ID, MAX(AA.KOMTEK_BIDANG_ID) KOMTEK_BIDANG_ID FROM VIEW_NOTIFIKASI AA WHERE AA.JUMLAH > 0 AND NOTIF_TYPE = 1 "+((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN(" + BIDANG_ID+",0)" : "")+" GROUP BY  AA.JENIS_NOTIF, AA.NOTIF_SORT, AA.NOTIF_TYPE, AA.NOTIF_LINK").ToList();

                        if (hasil != null)
                        {
                            foreach (var n in hasil)
                            {
                                notif += "<li><a href='" + n.NOTIF_LINK + "'><span style='float:right;' class='badge badge-info'>" + n.JUMLAH + " </span><span class='details'><i class='fa fa-angle-right'></i>" + n.JENIS_NOTIF + "</span></a></li>";
                            }
                        }
                    }
                    else {
                        var KOMTEK_ID = Convert.ToInt32(HttpContext.Current.Session["KOMTEK_ID"]);
                        var hasil = db.Database.SqlQuery<VIEW_NOTIFIKASI>("SELECT * FROM VIEW_NOTIFIKASI WHERE JUMLAH > 0 AND NOTIF_TYPE = 2 AND KOMTEK_ID = " + KOMTEK_ID).ToList();
                        if (hasil != null)
                        {
                            foreach (var n in hasil)
                            {
                                notif += "<li><a href='" + n.NOTIF_LINK + "'><span style='float:right;' class='badge badge-info'>" + n.JUMLAH + " </span><span class='details'><i class='fa fa-angle-right'></i>" + n.JENIS_NOTIF + "</span></a></li>";
                            }
                        }
                    }
                }
                else {
                    if (Convert.ToInt32(HttpContext.Current.Session["IS_KOMTEK"]) != 1)
                    {
                        var hasil = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(SUM(JUMLAH),0) AS NUMBER) AS JUMLAH FROM VIEW_NOTIFIKASI AA WHERE AA.JUMLAH > 0 AND NOTIF_TYPE = 1 " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID IN(" + BIDANG_ID + ",0)" : "") + "").SingleOrDefault();
                        //var hasil = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(SUM(JUMLAH),0) AS NUMBER) AS JUMLAH FROM VIEW_NOTIFIKASI WHERE JUMLAH > 0 AND NOTIF_TYPE = 1 " + ((BIDANG_ID != 0) ? "AND KOMTEK_BIDANG_ID = " + BIDANG_ID : "")).SingleOrDefault();
                        notif = Convert.ToString(hasil);
                    }
                    else {
                        var KOMTEK_ID = Convert.ToInt32(HttpContext.Current.Session["KOMTEK_ID"]);
                        var hasil = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(SUM(JUMLAH),0) AS NUMBER) AS JUMLAH FROM VIEW_NOTIFIKASI WHERE JUMLAH > 0 AND NOTIF_TYPE = 2 AND KOMTEK_ID = " + KOMTEK_ID).SingleOrDefault();
                        notif = Convert.ToString(hasil);
                    }
                }
                
                return notif;
            }

        }
        public string MD5CONVERTER(string input)
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
        //public static string Menu()
        //{
        //    using (var db = new SISPKEntities())
        //    {
        //        int UserPrivilegeGroupId = Convert.ToInt32(HttpContext.Current.Session["UserPrivilegeGroupId"]);
        //        if (UserPrivilegeGroupId != 0)
        //        {

        //            var menu = db.Database.SqlQuery<SYS_MENU>("SELECT T1.MENU_ID, T1.MENU_PARENT_ID, T1.MENU_URL, T1.MENU_NAME, T1.MENU_SORT, T1.MENU_ICON, T1.MENU_POSITION, T1.MENU_CREATE_BY, T1.MENU_CREATE_DATE, T1.MENU_UPDATE_BY, T1.MENU_UPDATE_DATE, T1.MENU_STATUS FROM SYS_MENU T1 LEFT JOIN SYS_MENU T2 ON T2.MENU_ID = T1.MENU_PARENT_ID WHERE T1.MENU_STATUS = 1 START WITH T1.MENU_PARENT_ID = 0 CONNECT BY PRIOR T1.MENU_ID = T1.MENU_PARENT_ID ORDER SIBLINGS BY T1.MENU_SORT,T2.MENU_SORT").ToList();

        //            var hasilmenu = "";
        //            foreach (var item in menu)
        //            {
        //                if (item.MENU_PARENT_ID == 0)
        //                {
        //                    hasilmenu += item.nama_menu;
        //                }
        //                //hasilmenu += item.nama_menu;
        //            }
        //            return String.Format(hasilmenu);
        //        }
        //        else
        //        {
        //            return String.Format("<li id='parent_1' class='active'><a href='javascript:void(0)'>Please Relogin</a></li>");
        //        }



        //    }
        //}

        //public static string UserInfo(string target)
        //{
        //    using (var db = new ojk_sipaoEntities())
        //    {
        //        int UserId = Convert.ToInt32(HttpContext.Current.Session["UserId"]);
        //        var hasil = "";
        //        if (UserId != 0)
        //        {
        //            var query_UserInfo = (from a in db.Master_sysUsers
        //                                  where a.UserId == UserId
        //                                  select a).First();

        //            if (target == "EmployeeImagePath")
        //            {
        //                var employee = query_UserInfo.Master_orgEmployees;
        //                var images = employee.EmployeeImagePath;
        //                if (images != null)
        //                {
        //                    hasil = Convert.ToString(query_UserInfo.Master_orgEmployees.EmployeeImagePath);
        //                }
        //                else
        //                {
        //                    hasil = Convert.ToString("");
        //                }
        //            }

        //        } return String.Format(hasil);

        //    }
        //}
        //public static string breadcrumb(string target)
        //{
        //    using (var db = new ojk_sipaoEntities())
        //    {
        //        var url = HttpContext.Current.Request.Url.AbsolutePath;
        //        var new_url = (new StringBuilder(url)).Replace("/Create", "").Replace("/Details", "").Replace("/Edit", "").Replace("/Delete", "").ToString();


        //        string segment = "/" + HttpContext.Current.Request.RequestContext.RouteData.Values["tipe"] + "/" + HttpContext.Current.Request.RequestContext.RouteData.Values["controller"];
        //        var query = (from a in db.ojk_fix_breadcrumb
        //                     where a.MenuUrl == segment
        //                     select new
        //                     {
        //                         MenuId = a.MenuId,
        //                         MenuUrl = a.MenuUrl,
        //                         module_id = a.module_id,
        //                         breadcrumb = a.breadcrumb,
        //                         MenuName = a.MenuName
        //                     }).First();
        //        var hasil = "";
        //        if (target == "MenuId")
        //        {
        //            hasil = Convert.ToString(query.MenuId);
        //        }
        //        else if (target == "MenuUrl")
        //        {
        //            hasil = query.MenuUrl;
        //        }
        //        else if (target == "module_id")
        //        {
        //            hasil = Convert.ToString(query.module_id);
        //        }
        //        else if (target == "breadcrumb")
        //        {
        //            hasil = query.breadcrumb;
        //        }
        //        else if (target == "MenuName")
        //        {
        //            hasil = query.MenuName;
        //        }
        //        return String.Format(hasil);
        //    }

        //}
    }
}