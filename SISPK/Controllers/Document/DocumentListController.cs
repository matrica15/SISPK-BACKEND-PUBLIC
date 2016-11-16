using SISPK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SISPK.Helpers;
using System.IO;
using System.Data.Objects;
using Aspose.Words;
using SISPK.Filters;

namespace SISPK.Controllers.Document
{
    [Auth(RoleTipe = 1)]
    public class DocumentListController : Controller
    {
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }

            var queries = "SELECT " +
                    "MFC.FOLDER_ID, " +
                    "MFC.FOLDER_CODE, " +
                    "MFC.FOLDER_PARENT_ID, " +
                    "MFC.FOLDER_ALLOW_ADDFOLDER, " +
                    "MFC.FOLDER_ALLOW_ADDFILE, " +
                    "MFC.FOLDER_ACCESS_ID, " +
                    "MFC.FOLDER_KOMTEK_ID, " +
                    "MFC.FOLDER_USER_ID, " +
                    "MFC.FOLDER_PASSWORD, " +
                    "MFC.FOLDER_ICON, " +
                    "MFC.FOLDER_ICON_COLOR, " +
                    "MFC.FOLDER_IS_SHARED, " +
                    "MFC.FOLDER_SORT, " +
                    "MFC.FOLDER_CREATE_BY, " +
                    "MFC.FOLDER_CREATE_DATE, " +
                    "MFC.FOLDER_UPDATE_BY, " +
                    "MFC.FOLDER_UPDATE_DATE, " +
                    "MFC.FOLDER_APPROVE_BY, " +
                    "MFC.FOLDER_APPROVE_DATE, " +
                    "MFC.FOLDER_STATUS, " +
                    "MFC.FOLDER_LOG_CODE, " +
                    "'<div ctv-context=''";
            if(Session["IS_KOMTEK"].ToString() == "1") {
                queries += "'||(CASE WHEN MFC.FOLDER_PARENT_ID = 0 OR MFC.FOLDER_KOMTEK_ID = " + Session["KOMTEK_ID"] + " THEN MFC.FOLDER_ALLOW_ADDFOLDER ELSE 0 END)||'-";
                queries += "'||(CASE WHEN MFC.FOLDER_KOMTEK_ID = " + Session["KOMTEK_ID"] + " THEN MFC.FOLDER_ALLOW_ADDFILE ELSE 0 END)||'-";
            } else {
                queries += "'||MFC.FOLDER_ALLOW_ADDFOLDER||'-";
                queries += "'||MFC.FOLDER_ALLOW_ADDFILE||'-";
            }
            queries += "'||((SELECT COUNT(FOLDER_ID) FROM MASTER_FOLDERS WHERE FOLDER_PARENT_ID = MFC.FOLDER_ID AND FOLDER_STATUS = 1)+(SELECT COUNT(DOC_ID) FROM TRX_DOCUMENTS WHERE DOC_FOLDER_ID = MFC.FOLDER_ID AND DOC_STATUS = 1))||'-";
            queries += "'||(SELECT COUNT(FOLDER_ID) FROM MASTER_FOLDERS WHERE FOLDER_PARENT_ID = MFC.FOLDER_ID AND FOLDER_STATUS = 1 " + ((Session["IS_KOMTEK"].ToString() == "1") ? "AND NVL(FOLDER_KOMTEK_ID,0) = (CASE WHEN FOLDER_KOMTEK_ID IS NOT NULL THEN " + Session["KOMTEK_ID"] + " ELSE NVL(FOLDER_KOMTEK_ID,0) END) " : "") + ")||'";
            queries += "'' ctv-id=''' || MFC.FOLDER_ID || ''' ctv-ref=''' || MFC.FOLDER_PARENT_ID || ''' class=''ctv-folder'' style=''padding-left: ' || (((LEVEL + 1) * 20) - 10) || 'px; ' || ( " +
                        "CASE " +
                        "WHEN MFC.FOLDER_PARENT_ID > 0 THEN " +
                            "'display:none;' " +
                        "ELSE " +
                            "'' " +
                        "END " +
                    ") || '''><i class=''ctv-arrow-folder fa fa-angle-right'' style=''width: 18px; text-align: center; ' || ( " +
                        "CASE " +
                        "WHEN ( " +
                            "SELECT " +
                                "COUNT (MFP.FOLDER_ID) " +
                            "FROM " +
                                "MASTER_FOLDERS MFP " +
                            "WHERE " +
                                "MFP.FOLDER_PARENT_ID = MFC.FOLDER_ID AND MFP.FOLDER_STATUS = 1 ";
            queries += (Session["IS_KOMTEK"].ToString() == "1") ? "AND NVL(MFP.FOLDER_KOMTEK_ID,0) = (CASE WHEN MFP.FOLDER_KOMTEK_ID IS NOT NULL THEN " + Session["KOMTEK_ID"] + " ELSE NVL(MFP.FOLDER_KOMTEK_ID,0) END) " : "";
            queries += ") = 0 THEN " +
                            "'visibility:hidden;' " +
                        "ELSE " +
                            "'' " +
                        "END " +
                    ") || '''></i> <span class=''ctv-data-name'' style=''cursor: pointer;''><i class=''ctv-folder-icon fa ' || ( " +
                        "CASE " +
                        "WHEN MFC.FOLDER_ICON IS NOT NULL THEN " +
                            "MFC.FOLDER_ICON " +
                        "ELSE " +
                            "'fa-folder' " +
                        "END " +
                    ") || ''' style=''width: 18px; ' || ( " +
                        "CASE " +
                        "WHEN MFC.FOLDER_ICON_COLOR IS NOT NULL THEN " +
                            "'color:' || MFC.FOLDER_ICON_COLOR || ' !important;' " +
                        "ELSE " +
                            "'' " +
                        "END " +
                    ") || '''></i>";

            queries += (Session["IS_KOMTEK"].ToString() == "0") ? "' || (CASE WHEN MKT.KOMTEK_ID IS NOT NULL THEN '<i data-original-title=''Komtek: '||MKT.KOMTEK_NAME||''' data-placement=''top'' data-container=''body'' class=''fa fa-users tooltips'' style=''font-size:10px;''></i> ' ELSE '' END) || '" : "";
            queries += (Session["IS_KOMTEK"].ToString() == "1") ? "' || (CASE WHEN MKT.KOMTEK_ID IS NULL AND FOLDER_ALLOW_ADDFOLDER = 1 AND FOLDER_PARENT_ID > 0 THEN '<i data-original-title=''Folder PPS'' data-placement=''top'' data-container=''body'' class=''fa fa-user tooltips'' style=''font-size:10px;''></i> ' ELSE '' END) || '" : "";
            queries += "<span class=''ctv-folder-name''>' || MFC.FOLDER_NAME || '</span></span></div>' AS FOLDER_NAME " +
                "FROM " +
                    "MASTER_FOLDERS MFC " +
                "LEFT JOIN MASTER_KOMITE_TEKNIS MKT ON MFC.FOLDER_KOMTEK_ID = MKT.KOMTEK_ID " +
                "WHERE " +
            "MFC.FOLDER_STATUS = 1 ";
            queries += (Session["IS_KOMTEK"].ToString() == "1") ? "AND NVL(MFC.FOLDER_KOMTEK_ID,0) = (CASE WHEN MFC.FOLDER_KOMTEK_ID IS NOT NULL THEN " + Session["KOMTEK_ID"] + " ELSE NVL(MFC.FOLDER_KOMTEK_ID,0) END) " : "";
            queries += "START WITH MFC.FOLDER_PARENT_ID = 0 CONNECT BY PRIOR MFC.FOLDER_ID = MFC.FOLDER_PARENT_ID ORDER SIBLINGS BY MFC.FOLDER_SORT, MFC.FOLDER_NAME ASC";

            ViewData["References"] = db.Database.SqlQuery<MASTER_FOLDERS>(queries).ToList();
            var ListAkses = db.Database.SqlQuery<SYS_DOC_ACCESS>("SELECT * FROM SYS_DOC_ACCESS WHERE DOC_ACCESS_STATUS = 1").ToList();
            ViewData["ListAkses"] = ListAkses;
            return View();
            //return Json(new { queries }, JsonRequestBehavior.AllowGet);
            
        }

        public ActionResult ListData(DataTables param, int status, string folderid)
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }

            var default_order = "DOC_ID";
            var limit = 10;

            

            List<string> order_field = new List<string>();
            order_field.Add("DOC_NAME");
            order_field.Add("DOC_DESCRIPTION");

            string order_key = (param.iSortCol_0 == "0") ? "0" : param.iSortCol_0;
            string order = (param.iSortCol_0 == "0") ? default_order : order_field[Convert.ToInt32(order_key)];
            string sort = (param.sSortDir_0 == "") ? "desc" : param.sSortDir_0;
            string search = (param.sSearch == "") ? "" : param.sSearch;

            limit = (param.iDisplayLength == 0) ? limit : param.iDisplayLength;
            var start = (param.iDisplayStart == 0) ? 0 : param.iDisplayStart;

            string proposal_komtek = (Session["IS_KOMTEK"].ToString() == "1") ? "AND NVL(DOC_PROPOSAL_KOMTEK_ID,0) = (CASE WHEN DOC_PROPOSAL_KOMTEK_ID IS NOT NULL THEN " + Session["KOMTEK_ID"] + " ELSE NVL(DOC_PROPOSAL_KOMTEK_ID,0) END) " : "";
            string sni_komtek = (Session["IS_KOMTEK"].ToString() == "1") ? "AND NVL(DOC_SNI_KOMTEK_ID,0) = (CASE WHEN DOC_SNI_KOMTEK_ID IS NOT NULL THEN " + Session["KOMTEK_ID"] + " ELSE NVL(DOC_SNI_KOMTEK_ID,0) END) " : "";

            string where_clause = "DOC_STATUS = '" + status + "' " + (status == 0 ? "" : "AND DOC_FOLDER_ID = '" + folderid + "'") + " " + proposal_komtek + " " + sni_komtek + " ";

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
                        search_clause += "UPPER(" + fields + ")  LIKE UPPER('%" + search + "%')";
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
                inject_clause_select = "SELECT * FROM (SELECT T1.*, ROWNUM ROWNUMBER FROM (SELECT * FROM VIEW_DOCUMENTS WHERE " + where_clause + " " + search_clause + " ORDER BY " + order + " " + sort + ") T1 WHERE ROWNUM <= " + Convert.ToString(limit + start) + ") WHERE ROWNUMBER > " + Convert.ToString(start);
            }
            var CountData = db.Database.SqlQuery<decimal>("SELECT CAST(COUNT(*) AS NUMBER) AS Jml FROM  VIEW_DOCUMENTS " + inject_clause_count);
            var SelectedData = db.Database.SqlQuery<VIEW_DOCUMENTS>(inject_clause_select);

            var result = from list in SelectedData
                         select new string[] 
            {
                "<div style='padding-top:2px;'><table width='100%'>" +
                "<tr width='100%'>" +
                "<td width='24px' valign='top' align='left' style='padding-left:10px;'><i data-original-title='"+Convert.ToString(list.DOC_FILETYPE_NAME)+"' data-placement='left' data-container='body' style=font-size:14px;color:#66aa66;font-weight:bold;' class='tooltips action " + list.DOC_FILETYPE_ICON + "'></i></td>" +
                "<td width='*' valign='top'>" +
                "<div width='100%' class='row_text_on_datatable' style='overflow:hidden;height:18px;'><a data-original-title='"+Convert.ToString(list.DOC_NAME)+"' data-placement='top' data-container='body' class='tooltips' href='javascript:showdetail("+list.DOC_ID+");'>"+Convert.ToString(list.DOC_NAME)+"</a></div>" +
                "</td>" +
                "</tr>" +
                "</table></div>",
                Convert.ToString("<center>") +
                Convert.ToString((status == 1 && list.DOC_EDITABLE == 1 && list.DOC_CREATE_USER_ID.ToString() == Session["USER_ID"].ToString()) ? "<a data-original-title='Ubah' data-placement='top' data-container='body' class='btn purple btn-sm action tooltips' href='/Document/DocumentList/" + ((list.DOC_CREATE_TYPE == 1) ? "EditUpload" : "EditText") + "?docId="+list.DOC_ID+"'><i class='action glyphicon glyphicon-edit'></i></a>":"") +
                Convert.ToString((status == 1 && list.DOC_DELETABLE == 1 && list.DOC_CREATE_USER_ID.ToString() == Session["USER_ID"].ToString()) ? "<a data-original-title='Non Aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Document/DocumentList/Delete/"+list.DOC_ID+"'><i class='action glyphicon glyphicon-remove'></i></a>":"") +
                Convert.ToString((status == 0 && list.DOC_DELETABLE == 1 && list.DOC_CREATE_USER_ID.ToString() == Session["USER_ID"].ToString()) ? "<a data-original-title='Aktifkan' data-placement='top' data-container='body' class='btn red btn-sm action tooltips' href='/Document/DocumentList/Activate/"+list.DOC_ID+"'><i class='action glyphicon glyphicon-refresh'></i></a>":"") +
                Convert.ToString((status == 1) ? "<a data-original-title='Unduh' data-placement='top' data-container='body' class='btn green btn-sm action tooltips dropdown-toggle' target='"+((list.DOC_CREATE_TYPE==2)?"":"_blank")+"' href='"+((list.DOC_CREATE_TYPE==2 || list.DOC_RELATED_TYPE==100)?"javascript:download("+list.DOC_ID+","+list.DOC_RELATED_TYPE+","+list.DOC_RELATED_ID+");":"/Document/DocumentList/Download?docId="+list.DOC_ID)+"'><i class='action glyphicon glyphicon-download-alt'></i></a>":"") +
                //Convert.ToString((status == 1) ? "<a data-original-title='Unduh' data-placement='top' data-container='body' class='btn green btn-sm action tooltips dropdown-toggle' href='"+((list.DOC_CREATE_TYPE==2)?"javascript:download("+list.DOC_ID+");":"/Document/DocumentList/Download?docId="+list.DOC_ID)+"'><i class='action glyphicon glyphicon-download-alt'></i></a>":"") +
                Convert.ToString("<center>")
            };
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = CountData,
                iTotalDisplayRecords = CountData,
                aaData = result.ToArray()
            }, JsonRequestBehavior.AllowGet);

            //return Json(inject_clause_select, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ShowDetail(int docId = 0)
        {
            var type = "";
            var location = "";
            var name = "";
            var description = "";

            if (docId > 0)
            {
                var docs = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_ID = " + docId).SingleOrDefault();
                if (docs != null)
                {
                    var _location = db.Database.SqlQuery<string>("" +
                        "SELECT " +
                            "folders " +
                        "FROM " +
                            "( " +
                                "SELECT " +
                                    "'<span>SISPK</span>' || SUBSTR ( " +
                                        "SYS_CONNECT_BY_PATH ( " +
                                            "'<span>' || FOLDER_NAME || '</span>', " +
                                            "'<span>/</span>' " +
                                        "), " +
                                        "0 " +
                                    ") folders, " +
                                    "CONNECT_BY_ROOT (FOLDER_PARENT_ID) root " +
                                "FROM " +
                                    "MASTER_FOLDERS " +
                                "WHERE " +
                                    "FOLDER_ID = " + docs.DOC_FOLDER_ID + " CONNECT BY PRIOR FOLDER_ID = FOLDER_PARENT_ID " +
                                "ORDER BY " +
                                    "FOLDER_ID " +
                            ") " +
                        "WHERE " +
                            "root = 0 ").SingleOrDefault();
                    type = docs.DOC_FILETYPE.ToLower();
                    location = _location;
                    name = docs.DOC_FILE_NAME;
                    description = docs.DOC_NAME + ((docs.DOC_NAME != "" && docs.DOC_DESCRIPTION != "") ? "<br/><br/>" : "") + docs.DOC_DESCRIPTION;
                }
            }
            return Json(new
            {
                type = type,
                location = location,
                name = name,
                description = description
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateFolder(string folderId = "", string folderName = "")
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            var status = "0";
            var message = "Ada Kesalahan System";
            if (folderId != "" && folderName != "")
            {
                var isExistParent = db.Database.SqlQuery<int>("SELECT COUNT(FOLDER_ID) FROM MASTER_FOLDERS WHERE FOLDER_ID = " + folderId).SingleOrDefault();
                if (isExistParent > 0)
                {
                    var isAllowCreate = db.Database.SqlQuery<int>("SELECT FOLDER_ALLOW_ADDFOLDER FROM MASTER_FOLDERS WHERE ROWNUM = 1 AND FOLDER_ID = " + folderId).SingleOrDefault();
                    if (isAllowCreate == 1)
                    {
                        var isExistData = db.Database.SqlQuery<int>("SELECT COUNT(FOLDER_ID) FROM MASTER_FOLDERS WHERE FOLDER_PARENT_ID = " + folderId + " AND FOLDER_NAME = '" + folderName + "' AND FOLDER_STATUS = 1").SingleOrDefault();
                        if (isExistData == 0)
                        {
                            var UserId = Session["USER_ID"];
                            var KomtekId = Session["KOMTEK_ID"];
                            var logcode = MixHelper.GetLogCode();
                            int lastid = MixHelper.GetSequence("MASTER_FOLDERS");
                            var datenow = MixHelper.ConvertDateNow();

                            var fname = "FOLDER_ID," +
                                        "FOLDER_PARENT_ID," +
                                        "FOLDER_NAME," +
                                        "FOLDER_ALLOW_ADDFOLDER," +
                                        "FOLDER_ALLOW_ADDFILE," +
                                        "FOLDER_KOMTEK_ID," +
                                        "FOLDER_CREATE_BY," +
                                        "FOLDER_CREATE_DATE," +
                                        "FOLDER_STATUS," +
                                        "FOLDER_LOG_CODE";
                            var fvalue = "'" + lastid + "', " +
                                        "'" + folderId + "', " +
                                        "'" + folderName + "', " +
                                        "'1', " +
                                        "'1', " +
                                        "'" + KomtekId + "', " +
                                        "'" + UserId + "', " +
                                        datenow + "," +
                                        "'1'," +
                                        "'" + logcode + "'";
                            db.Database.ExecuteSqlCommand("INSERT INTO MASTER_FOLDERS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

                            String objek = fvalue.Replace("'", "-");
                            MixHelper.InsertLog(logcode, objek, 1);

                            status = lastid.ToString();
                            message = "";
                        }
                        else
                        {
                            status = "0";
                            message = "Nama folder sama dengan nama folder yang lain";
                        }
                    }
                    else
                    {
                        status = "0";
                        message = "TIdak dapat menambahkan folder di Folder ini";
                    }
                }
                else
                {
                    status = "0";
                    message = "Folder tidak tersedia";
                }
            }
            return Json(new { status, message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditFolder(string folderId = "", string folderName = "")
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            var status = "0";
            var message = db.Database.SqlQuery<string>("SELECT FOLDER_NAME FROM MASTER_FOLDERS WHERE ROWNUM = 1 AND FOLDER_ID = " + folderId).SingleOrDefault();
            if (folderId != "" && folderName != "")
            {
                var isAllowCreate = db.Database.SqlQuery<int>("SELECT FOLDER_ALLOW_ADDFOLDER FROM MASTER_FOLDERS WHERE ROWNUM = 1 AND FOLDER_ID = " + folderId).SingleOrDefault();
                var isExistData = db.Database.SqlQuery<int>("SELECT COUNT(FOLDER_ID) FROM MASTER_FOLDERS WHERE FOLDER_PARENT_ID = (SELECT FOLDER_PARENT_ID FROM MASTER_FOLDERS WHERE FOLDER_ID = " + folderId + ") AND FOLDER_NAME = '" + folderName + "' AND FOLDER_STATUS = 1").SingleOrDefault();
                if (isAllowCreate == 1 && isExistData == 0)
                {
                    db.Database.ExecuteSqlCommand("UPDATE MASTER_FOLDERS SET FOLDER_NAME = '" + folderName + "' WHERE FOLDER_ID = " + folderId);
                    status = "1";
                    message = folderName;
                }
            }
            return Json(new { status, message }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteFolder(string folderId = "")
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            var status = "0";
            var message = "Ada Kesalahan System";
            if (folderId != "")
            {
                var isAllowCreate = db.Database.SqlQuery<int>("SELECT FOLDER_ALLOW_ADDFOLDER FROM MASTER_FOLDERS WHERE ROWNUM = 1 AND FOLDER_ID = " + folderId).SingleOrDefault();
                var isEmpty = db.Database.SqlQuery<int>("SELECT ((SELECT COUNT(FOLDER_ID) FROM MASTER_FOLDERS WHERE FOLDER_PARENT_ID = " + folderId + " AND FOLDER_STATUS = 1)+(SELECT COUNT(DOC_ID) FROM TRX_DOCUMENTS WHERE DOC_FOLDER_ID = " + folderId + " AND DOC_STATUS = 1)) FROM DUAL").SingleOrDefault();
                if (isAllowCreate == 1 && isEmpty == 0)
                {
                    db.Database.ExecuteSqlCommand("UPDATE MASTER_FOLDERS SET FOLDER_STATUS = 0 WHERE FOLDER_ID = " + folderId);
                    status = "1";
                    message = "";
                }
                else
                {
                    status = "0";
                    message = "Folder tidak dapat di Hapus";
                }
            }
            return Json(new { status, message }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult CreateUpload(int folderId = 0)
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            if (folderId > 0)
            {
                var isAllowCreate = db.Database.SqlQuery<int>("SELECT FOLDER_ALLOW_ADDFILE FROM MASTER_FOLDERS WHERE ROWNUM = 1 AND FOLDER_STATUS = 1 AND FOLDER_ID = " + folderId).SingleOrDefault();
                if (isAllowCreate == 1)
                {
                    ViewData["folderId"] = folderId;
                    ViewData["folder"] = db.Database.SqlQuery<string>("" +
                        "SELECT " +
                            "folders " +
                        "FROM " +
                            "( " +
                                "SELECT " +
                                    "'<span''><i class=''ctv-folder-icon fa fa-cloud'' style=''width:18px;color:#0e8500 !important;''></i>SISPK</span>' || SUBSTR ( " +
                                        "SYS_CONNECT_BY_PATH ( " +
                                            "'<span''><i class=''ctv-folder-icon fa fa-folder'' style=''width: 18px; color:#e9c607;''></i>' || FOLDER_NAME || '</span>', " +
                                            "'<span style=''font-size:20px;''> / </span>' " +
                                        "), " +
                                        "0 " +
                                    ") folders, " +
                                    "CONNECT_BY_ROOT (FOLDER_PARENT_ID) root " +
                                "FROM " +
                                    "MASTER_FOLDERS " +
                                "WHERE " +
                                    "FOLDER_ID = " + folderId + " CONNECT BY PRIOR FOLDER_ID = FOLDER_PARENT_ID " +
                                "ORDER BY " +
                                    "FOLDER_ID " +
                            ") " +
                        "WHERE " +
                            "root = 0 ").SingleOrDefault();
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
            return View();
        }


        [HttpPost]
        public ActionResult CreateUpload(int folderId = 0, String title = "", String description = "", string relatedId = "")
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }

            if (folderId > 0 && title != "")
            {
                var isAllowCreate = db.Database.SqlQuery<int>("SELECT FOLDER_ALLOW_ADDFILE FROM MASTER_FOLDERS WHERE ROWNUM = 1 AND FOLDER_STATUS = 1 AND FOLDER_ID = " + folderId).SingleOrDefault();
                if (isAllowCreate == 1)
                {
                    var UserId = Session["USER_ID"];
                    var logcode = MixHelper.GetLogCode();
                    int lastid = MixHelper.GetSequence("TRX_DOCUMENTS");
                    var datenow = MixHelper.ConvertDateNow();

                    var getStatus = 0;
                    if (relatedId != "")
                    {
                        getStatus = db.Database.SqlQuery<int>("SELECT PROPOSAL_STATUS FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + relatedId).SingleOrDefault();
                    }


                    Directory.CreateDirectory(Server.MapPath("~/Upload/Referensi/" + folderId + ((Session["IS_KOMTEK"].ToString() == "1") ? "/" + Session["KOMTEK_ID"] : "")));
                    string path = Server.MapPath("~/Upload/Referensi/" + folderId + ((Session["IS_KOMTEK"].ToString() == "1") ? "/" + Session["KOMTEK_ID"] + "/" : "/"));
                    string fileName = Session["USER_ID"] + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff");

                    string pathToDb = "";
                    string NameToDB = "";
                    string FileTypeToDB = "";

                    HttpPostedFileBase file_att = Request.Files["docfile"];
                    var file_name_att = "";
                    string fileExtension = "";
                    if (file_att != null)
                    {
                        string doc_filepath = file_att.FileName;
                        if (doc_filepath.Trim() != "")
                        {
                            pathToDb = "/Upload/Referensi/" + folderId + ((Session["IS_KOMTEK"].ToString() == "1") ? "/" + Session["KOMTEK_ID"] + "/" : "/");
                            doc_filepath = Path.GetFileNameWithoutExtension(file_att.FileName);
                            fileExtension = Path.GetExtension(file_att.FileName);

                            NameToDB = fileName;
                            FileTypeToDB = fileExtension.Replace(".","");

                            file_name_att = fileName;
                            string filePath = path + file_name_att + fileExtension;
                            file_att.SaveAs(filePath);
                        }
                    }

                    var fname = "DOC_ID," +
                                        "DOC_FOLDER_ID," +
                                        "DOC_RELATED_TYPE," +
                                        "DOC_RELATED_ID," +
                                        "DOC_NAME," +
                                        "DOC_DESCRIPTION," +
                                        "DOC_FILE_PATH," +
                                        "DOC_FILE_NAME," +
                                        "DOC_FILETYPE," +
                                        "DOC_CREATE_TYPE," +
                                        "DOC_EDITABLE," +
                                        "DOC_DELETABLE," +
                                        "DOC_CREATE_BY," +
                                        "DOC_CREATE_DATE," +
                                        "DOC_STATUS," +
                                        "DOC_LOG_CODE";
                    var fvalue = "'" + lastid + "', " +
                                    "'" + folderId + "', " +
                                    "'" + ((getStatus == 0) ? "" : getStatus.ToString()) + "', " +
                                    "'" + relatedId + "', " +
                                    "'" + title + "', " +
                                    "'" + description + "', " +
                                    "'" + pathToDb + "', " +
                                    "'" + NameToDB + "', " +
                                    "'" + FileTypeToDB + "', " +
                                    "'1', " +
                                    "'1', " +
                                    "'1', " +
                                    "'" + UserId + "', " +
                                    datenow + "," +
                                    "'1', " +
                                    "'" + logcode + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");

                    String objek = fvalue.Replace("'", "-");
                    MixHelper.InsertLog(logcode, objek, 1);
                    TempData["Notifikasi"] = 1;
                    TempData["NotifikasiText"] = "Data Berhasil Disimpan";
                    return Redirect("?folderId=" + folderId);
                }
                else
                {
                    return Redirect("CreateUpload?folderId=" + folderId);
                }
                
            }
            else
            {
                return Redirect("CreateUpload?folderId=" + folderId);
            }
        }

        public ActionResult EditUpload(int docId = 0)
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            if (docId > 0)
            {
                var isEditUpload = db.Database.SqlQuery<int>("SELECT DOC_CREATE_TYPE FROM TRX_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_ID = " + docId).SingleOrDefault();
                if (isEditUpload == 1)
                {
                    var docs = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_ID = " + docId).SingleOrDefault();
                    ViewData["docId"] = docId;
                    ViewData["folderId"] = docs.DOC_FOLDER_ID;
                    ViewData["folder"] = db.Database.SqlQuery<string>("" +
                        "SELECT " +
                            "folders " +
                        "FROM " +
                            "( " +
                                "SELECT " +
                                    "'<span''><i class=''ctv-folder-icon fa fa-cloud'' style=''width:18px;color:#0e8500 !important;''></i>SISPK</span>' || SUBSTR ( " +
                                        "SYS_CONNECT_BY_PATH ( " +
                                            "'<span''><i class=''ctv-folder-icon fa fa-folder'' style=''width: 18px; color:#e9c607;''></i>' || FOLDER_NAME || '</span>', " +
                                            "'<span style=''font-size:20px;''> / </span>' " +
                                        "), " +
                                        "0 " +
                                    ") folders, " +
                                    "CONNECT_BY_ROOT (FOLDER_PARENT_ID) root " +
                                "FROM " +
                                    "MASTER_FOLDERS " +
                                "WHERE " +
                                    "FOLDER_ID = " + docs.DOC_FOLDER_ID + " CONNECT BY PRIOR FOLDER_ID = FOLDER_PARENT_ID " +
                                "ORDER BY " +
                                    "FOLDER_ID " +
                            ") " +
                        "WHERE " +
                            "root = 0 ").SingleOrDefault();
                    ViewData["title"] = docs.DOC_NAME;
                    ViewData["description"] = docs.DOC_DESCRIPTION;
                    ViewData["relatedId"] = docs.DOC_RELATED_ID;
                    ViewData["relatedName"] = (docs.DOC_RELATED_ID.ToString() != "") ? db.Database.SqlQuery<string>("SELECT PROPOSAL_JUDUL_PNPS FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + docs.DOC_RELATED_ID).SingleOrDefault() : "";
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public ActionResult EditUpload(int docId = 0, int folderId = 0, String title = "", String description = "", string relatedId = "")
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            if (folderId > 0 && title != "")
            {
                var isEditUpload = db.Database.SqlQuery<int>("SELECT DOC_CREATE_TYPE FROM TRX_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_ID = " + docId).SingleOrDefault();
                if (isEditUpload == 1)
                {
                    var docs = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_ID = " + docId).SingleOrDefault();

                    string path = Server.MapPath("~" + docs.DOC_FILE_PATH);
                    string fileName = docs.DOC_FILE_NAME;

                    var UserId = Session["USER_ID"];
                    var datenow = MixHelper.ConvertDateNow();

                    var getStatus = docs.DOC_RELATED_TYPE;
                    if (relatedId != "" && relatedId != docs.DOC_RELATED_ID.ToString())
                    {
                        getStatus = db.Database.SqlQuery<int>("SELECT PROPOSAL_STATUS FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + relatedId).SingleOrDefault();
                    }

                    string FileTypeToDB = docs.DOC_FILETYPE;

                    HttpPostedFileBase file_att = Request.Files["docfile"];
                    var file_name_att = "";
                    string fileExtension = "";
                    if (file_att != null)
                    {
                        string doc_filepath = file_att.FileName;
                        if (doc_filepath.Trim() != "")
                        {
                            doc_filepath = Path.GetFileNameWithoutExtension(file_att.FileName);
                            fileExtension = Path.GetExtension(file_att.FileName);

                            FileTypeToDB = fileExtension.Replace(".", "");

                            file_name_att = fileName;
                            string filePath = path + file_name_att + fileExtension;
                            file_att.SaveAs(filePath);
                        }
                    }


                    var logcode = docs.DOC_LOG_CODE;

                    var fupdate = "DOC_FOLDER_ID = " + docs.DOC_FOLDER_ID + "," +
                                "DOC_RELATED_TYPE = '" + ((getStatus == 0) ? "" : getStatus.ToString()) + "'," +
                                "DOC_RELATED_ID = '" + relatedId + "'," +
                                "DOC_NAME = '" + title + "'," +
                                "DOC_DESCRIPTION = '" + description + "'," +
                                "DOC_FILETYPE = '" + FileTypeToDB + "'," +
                                "DOC_UPDATE_BY = '" + UserId + "'," +
                                "DOC_UPDATE_DATE = " + datenow;

                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET " + fupdate + " WHERE DOC_ID = " + docs.DOC_ID);

                    String objek = fupdate.Replace("'", "-");
                    MixHelper.InsertLog(logcode, objek, 1);



                    TempData["Notifikasi"] = 1;
                    TempData["NotifikasiText"] = "Data Berhasil Diperbaharui";
                    return Redirect("?folderId=" + folderId);
                }
                else
                {
                    return Redirect("EditUpload?docId=" + docId);
                }

            }
            else
            {
                return Redirect("EditUpload?docId=" + docId);
            }
        }

        public ActionResult CreateText(int folderId = 0)
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            if (folderId > 0)
            {
                var isAllowCreate = db.Database.SqlQuery<int>("SELECT FOLDER_ALLOW_ADDFILE FROM MASTER_FOLDERS WHERE ROWNUM = 1 AND FOLDER_STATUS = 1 AND FOLDER_ID = " + folderId).SingleOrDefault();
                if (isAllowCreate == 1)
                {
                    ViewData["folderId"] = folderId;
                    ViewData["folder"] = db.Database.SqlQuery<string>("" +
                        "SELECT " +
                            "folders " +
                        "FROM " +
                            "( " +
                                "SELECT " +
                                    "'<span''><i class=''ctv-folder-icon fa fa-cloud'' style=''width:18px;color:#0e8500 !important;''></i>SISPK</span>' || SUBSTR ( " +
                                        "SYS_CONNECT_BY_PATH ( " +
                                            "'<span''><i class=''ctv-folder-icon fa fa-folder'' style=''width: 18px; color:#e9c607;''></i>' || FOLDER_NAME || '</span>', " +
                                            "'<span style=''font-size:20px;''> / </span>' " +
                                        "), " +
                                        "0 " +
                                    ") folders, " +
                                    "CONNECT_BY_ROOT (FOLDER_PARENT_ID) root " +
                                "FROM " +
                                    "MASTER_FOLDERS " +
                                "WHERE " +
                                    "FOLDER_ID = " + folderId + " CONNECT BY PRIOR FOLDER_ID = FOLDER_PARENT_ID " +
                                "ORDER BY " +
                                    "FOLDER_ID " +
                            ") " +
                        "WHERE " +
                            "root = 0 ").SingleOrDefault();
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateText(String contentDoc = "", int folderId = 0, String title = "", String description = "", string relatedId = "")
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            if (folderId > 0 && title != "")
            {
                var isAllowCreate = db.Database.SqlQuery<int>("SELECT FOLDER_ALLOW_ADDFILE FROM MASTER_FOLDERS WHERE ROWNUM = 1 AND FOLDER_STATUS = 1 AND FOLDER_ID = " + folderId).SingleOrDefault();
                if (isAllowCreate == 1)
                {
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Referensi/" + folderId + ((Session["IS_KOMTEK"].ToString() == "1") ? "/" + Session["KOMTEK_ID"] : "")));
                    string path = Server.MapPath("~/Upload/Referensi/" + folderId + ((Session["IS_KOMTEK"].ToString() == "1") ? "/" + Session["KOMTEK_ID"] + "/" : "/"));
                    string fileName = Session["USER_ID"] + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff");

                    string filePathtxt = path + fileName + ".txt";
                    string filePathdoc = path + fileName + ".docx";
                    string filePathxml = path + fileName + ".xml";


                    string pathToDb = "/Upload/Referensi/" + folderId + ((Session["IS_KOMTEK"].ToString() == "1") ? "/" + Session["KOMTEK_ID"] + "/" : "/");
                    string NameToDB = fileName;
                    string FileTypeToDB = "docx";

                    DocumentBuilder builder = new DocumentBuilder();
                    builder.InsertHtml(contentDoc);
                    System.IO.File.WriteAllText(@"" + filePathtxt, contentDoc);
                    builder.Document.Save(@"" + filePathdoc);

                    Aspose.Words.Document docx = new Aspose.Words.Document(@"" + filePathdoc);
                    docx.Save(@"" + filePathxml);


                    var UserId = Session["USER_ID"];
                    var logcode = MixHelper.GetLogCode();
                    int lastid = MixHelper.GetSequence("MASTER_FOLDERS");
                    var datenow = MixHelper.ConvertDateNow();

                    var getStatus = 0;
                    if (relatedId != "")
                    {
                        getStatus = db.Database.SqlQuery<int>("SELECT PROPOSAL_STATUS FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + relatedId).SingleOrDefault();
                    }



                    var fname = "DOC_ID," +
                                "DOC_FOLDER_ID," +
                                "DOC_RELATED_TYPE," +
                                "DOC_RELATED_ID," +
                                "DOC_NAME," +
                                "DOC_DESCRIPTION," +
                                "DOC_FILE_PATH," +
                                "DOC_FILE_NAME," +
                                "DOC_FILETYPE," +
                                "DOC_CREATE_TYPE," +
                                "DOC_EDITABLE," +
                                "DOC_DELETABLE," +
                                "DOC_CREATE_BY," +
                                "DOC_CREATE_DATE," +
                                "DOC_STATUS," +
                                "DOC_LOG_CODE";
                    var fvalue = "'" + lastid + "', " +
                                    "'" + folderId + "', " +
                                    "'" + ((getStatus == 0) ? "" : getStatus.ToString()) + "', " +
                                    "'" + relatedId + "', " +
                                    "'" + title + "', " +
                                    "'" + description + "', " +
                                    "'" + pathToDb + "', " +
                                    "'" + NameToDB + "', " +
                                    "'" + FileTypeToDB + "', " +
                                    "'2', " +
                                    "'1', " +
                                    "'1', " +
                                    "'" + UserId + "', " +
                                    datenow + "," +
                                    "'1', " +
                                    "'" + logcode + "'";
                    db.Database.ExecuteSqlCommand("INSERT INTO TRX_DOCUMENTS (" + fname + ") VALUES (" + fvalue.Replace("''", "NULL") + ")");


                    String objek = fvalue.Replace("'", "-");
                    MixHelper.InsertLog(logcode, objek, 1);
                    TempData["Notifikasi"] = 1;
                    TempData["NotifikasiText"] = "Data Berhasil Disimpan";
                    return Redirect("?folderId=" + folderId);
                }
                else
                {
                    return Redirect("CreateText?folderId=" + folderId);
                }

            }
            else
            {
                return Redirect("CreateText?folderId=" + folderId);
            }
        }

        public ActionResult EditText(int docId = 0)
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            if (docId > 0)
            {
                var isEditUpload = db.Database.SqlQuery<int>("SELECT DOC_CREATE_TYPE FROM TRX_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_ID = " + docId).SingleOrDefault();
                if (isEditUpload == 2)
                {
                    var docs = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_ID = " + docId).SingleOrDefault();
                    ViewData["docId"] = docId;
                    ViewData["folderId"] = docs.DOC_FOLDER_ID;
                    ViewData["folder"] = db.Database.SqlQuery<string>("" +
                        "SELECT " +
                            "folders " +
                        "FROM " +
                            "( " +
                                "SELECT " +
                                    "'<span''><i class=''ctv-folder-icon fa fa-cloud'' style=''width:18px;color:#0e8500 !important;''></i>SISPK</span>' || SUBSTR ( " +
                                        "SYS_CONNECT_BY_PATH ( " +
                                            "'<span''><i class=''ctv-folder-icon fa fa-folder'' style=''width: 18px; color:#e9c607;''></i>' || FOLDER_NAME || '</span>', " +
                                            "'<span style=''font-size:20px;''> / </span>' " +
                                        "), " +
                                        "0 " +
                                    ") folders, " +
                                    "CONNECT_BY_ROOT (FOLDER_PARENT_ID) root " +
                                "FROM " +
                                    "MASTER_FOLDERS " +
                                "WHERE " +
                                    "FOLDER_ID = " + docs.DOC_FOLDER_ID + " CONNECT BY PRIOR FOLDER_ID = FOLDER_PARENT_ID " +
                                "ORDER BY " +
                                    "FOLDER_ID " +
                            ") " +
                        "WHERE " +
                            "root = 0 ").SingleOrDefault();
                    ViewData["title"] = docs.DOC_NAME;
                    ViewData["description"] = docs.DOC_DESCRIPTION;
                    ViewData["relatedId"] = docs.DOC_RELATED_ID;
                    ViewData["relatedName"] = (docs.DOC_RELATED_ID.ToString() != "") ? db.Database.SqlQuery<string>("SELECT PROPOSAL_JUDUL_PNPS FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + docs.DOC_RELATED_ID).SingleOrDefault() : "";
                    

                    string path = Server.MapPath("~" + docs.DOC_FILE_PATH + docs.DOC_FILE_NAME + ".txt");
                    ViewData["contentDoc"] = System.IO.File.ReadAllText(@"" + path);
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost, ValidateInput(false)]
        public ActionResult EditText(int docId = 0, String contentDoc = "", int folderId = 0, String title = "", String description = "", string relatedId = "")
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            if (folderId > 0 && title != "")
            {
                var isEditUpload = db.Database.SqlQuery<int>("SELECT DOC_CREATE_TYPE FROM TRX_DOCUMENTS WHERE DOC_STATUS = 1 AND DOC_ID = " + docId).SingleOrDefault();
                if (isEditUpload == 2)
                {
                    var docs = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_ID = " + docId).SingleOrDefault();

                    string path = Server.MapPath("~" + docs.DOC_FILE_PATH);
                    string fileName = docs.DOC_FILE_NAME;

                    string filePathtxt = path + fileName + ".txt";
                    string filePathdoc = path + fileName + ".docx";
                    string filePathxml = path + fileName + ".xml";


                    DocumentBuilder builder = new DocumentBuilder();
                    builder.InsertHtml(contentDoc);
                    System.IO.File.WriteAllText(@"" + filePathtxt, contentDoc);
                    builder.Document.Save(@"" + filePathdoc);

                    Aspose.Words.Document docx = new Aspose.Words.Document(@"" + filePathdoc);
                    docx.Save(@"" + filePathxml);



                    var UserId = Session["USER_ID"];
                    var datenow = MixHelper.ConvertDateNow();

                    var getStatus = docs.DOC_RELATED_TYPE;
                    if (relatedId != "" && relatedId != docs.DOC_RELATED_ID.ToString())
                    {
                        getStatus = db.Database.SqlQuery<int>("SELECT PROPOSAL_STATUS FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + relatedId).SingleOrDefault();
                    }


                    var logcode = docs.DOC_LOG_CODE;

                    var fupdate = "DOC_FOLDER_ID = " + docs.DOC_FOLDER_ID + "," +
                                "DOC_RELATED_TYPE = '" +((getStatus == 0) ? "" : getStatus.ToString()) + "'," +
                                "DOC_RELATED_ID = '" + relatedId + "'," +
                                "DOC_NAME = '" + title + "'," +
                                "DOC_DESCRIPTION = '" + description + "'," +
                                "DOC_UPDATE_BY = '" + UserId + "'," +
                                "DOC_UPDATE_DATE = " + datenow;

                    db.Database.ExecuteSqlCommand("UPDATE TRX_DOCUMENTS SET " + fupdate + " WHERE DOC_ID = " + docs.DOC_ID);

                    String objek = fupdate.Replace("'", "-");
                    MixHelper.InsertLog(logcode, objek, 1);



                    TempData["Notifikasi"] = 1;
                    TempData["NotifikasiText"] = "Data Berhasil Diperbaharui";
                    return Redirect("?folderId=" + folderId);
                }
                else
                {
                    return Redirect("EditText?docId=" + docId);
                }

            }
            else
            {
                return Redirect("EditText?docId=" + docId);
            }
        }

        public ActionResult getRSNI(string q = "")
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            var SelectedData = db.Database.SqlQuery<TRX_PROPOSAL>("SELECT * FROM TRX_PROPOSAL WHERE UPPER(PROPOSAL_JUDUL_PNPS) LIKE UPPER('%" + q + "%') AND PROPOSAL_STATUS IN (4,5,6,7,8,9)" + ((Session["IS_KOMTEK"].ToString() == "1") ? " AND NVL(PROPOSAL_KOMTEK_ID, 0) = (CASE WHEN PROPOSAL_KOMTEK_ID IS NOT NULL THEN " + Session["KOMTEK_ID"] + " ELSE NVL(PROPOSAL_KOMTEK_ID, 0) END)" : ""));
            var results = new List<object>();
            foreach (var listField in SelectedData)
            {
                results.Add(new
                {
                    idProposal = listField.PROPOSAL_ID.ToString(),
                    titleProposal = listField.PROPOSAL_JUDUL_PNPS.ToString()
                });
            }

            return Json(results, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int id = 0)
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            TRX_DOCUMENTS docData = db.TRX_DOCUMENTS.Find(id);
            if (docData == null)
            {
                return HttpNotFound();
            }
            ViewData["Categories"] = db.Database.SqlQuery<MASTER_REFERENCES>("SELECT * FROM MASTER_REFERENCES WHERE REF_TYPE = 1 ORDER BY REF_ID ASC").ToList();
            ViewData["Types"] = db.Database.SqlQuery<MASTER_REFERENCES>("SELECT * FROM MASTER_REFERENCES WHERE REF_TYPE = 2 ORDER BY REF_ID ASC").ToList();
            return View(docData);
        }

        public ActionResult Delete(int id = 0)
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            string query_update_group = "UPDATE TRX_DOCUMENTS SET DOC_STATUS = 0 WHERE DOC_ID = " + id;
            db.Database.ExecuteSqlCommand(query_update_group);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di Nonaktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult Activate(int id = 0)
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            string query_update_group = "UPDATE TRX_DOCUMENTS SET DOC_STATUS = 1 WHERE DOC_ID = " + id;
            db.Database.ExecuteSqlCommand(query_update_group);
            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil di Aktifkan";
            return RedirectToAction("Index");
        }

        public ActionResult Download(string convert = "original", int docId = 0)
        {
            if (Session["USER_ID"] == null) { return Redirect("/"); }
            if (docId > 0)
            {
                var docs = db.Database.SqlQuery<TRX_DOCUMENTS>("SELECT * FROM TRX_DOCUMENTS WHERE DOC_ID = " + docId).SingleOrDefault();
                var filePath = Server.MapPath("~" + docs.DOC_FILE_PATH);
                var fileName = docs.DOC_FILE_NAME;
                var fileType = docs.DOC_FILETYPE;
                if (convert == "original")
                {
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Temp"));
                    var destPath = Server.MapPath("~/Upload/Temp/");
                    if (System.IO.File.Exists(destPath + docs.DOC_NAME.Replace("/", "-").Replace(":", "-") + "." + fileType))
                    {
                        System.IO.File.Delete(destPath + docs.DOC_NAME.Replace("/", "-").Replace(":", "-") + "." + fileType);
                    }
                    System.IO.File.Copy(filePath + fileName + "." + fileType, destPath + docs.DOC_NAME.Replace("/", "-").Replace(":", "-") + "." + fileType);
                    return Redirect("/Upload/Temp/" + docs.DOC_NAME.Replace("/", "-").Replace(":", "-") + "." + fileType);
                }
                else if (convert == "pdf")
                {

                    Aspose.Words.Document doc = new Aspose.Words.Document(filePath + fileName + "." + fileType);
                    MemoryStream dstStream = new MemoryStream();

                    doc.Save(dstStream, SaveFormat.Pdf);

                    var mime = "application/pdf";

                    byte[] byteInfo = dstStream.ToArray();
                    dstStream.Write(byteInfo, 0, byteInfo.Length);
                    dstStream.Position = 0;

                    Response.ContentType = mime;
                    Response.AddHeader("content-disposition", "attachment;  filename=" + docs.DOC_NAME + ".pdf");
                    Response.BinaryWrite(byteInfo);
                    Response.End();
                    return new FileStreamResult(dstStream, mime);
                }
            }
            return RedirectToAction("Index");
        }

    }
}
