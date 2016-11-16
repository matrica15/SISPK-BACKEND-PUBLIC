using SISPK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.SessionState;

namespace SISPK
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        private SISPKEntities db = new SISPKEntities();
        protected void Application_Start()
        {
            var Key = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 11").FirstOrDefault();
            Aspose.Words.License wordsLicense = new Aspose.Words.License();

            wordsLicense.SetLicense(@"" + Key.CONFIG_VALUE);

            Aspose.Cells.License cellsLicense = new Aspose.Cells.License();

            cellsLicense.SetLicense(@"" + Key.CONFIG_VALUE);

            Aspose.Pdf.License pdfLicense = new Aspose.Pdf.License();

            pdfLicense.SetLicense(@"" + Key.CONFIG_VALUE);

            Aspose.Tasks.License TaskLicense = new Aspose.Tasks.License();
            TaskLicense.SetLicense(@"" + Key.CONFIG_VALUE);

            //AreaRegistration.RegisterAllAreas();
            //WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);


        }
        protected void Session_Start(object sender, EventArgs e)
        {
       
        }
        protected void Application_BeginRequest(Object sender, EventArgs e, ActionExecutingContext filterContext)
        {
            
        }
        protected void Application_PreRequestHandlerExecute(Object sender, EventArgs e)
        {
           
        }
        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
           
            //var USER_ID = Convert.ToInt32(HttpContext.Current.Session["USER_ID"]);
            //if (USER_ID == 0)
            //{
            //    Response.RedirectToRoute("Login", null);
            //}
            //else
            //{
            //    Session["SessionExpire"] = false;
            //}
            //var USER_ACCESS_ID = Convert.ToInt32(HttpContext.Current.Session["USER_ACCESS_ID"]);
            //if (USER_ACCESS_ID != 0)
            //{
            //    var host = System.Web.HttpContext.Current.Request.Url.AbsolutePath;

            //    var IsRead = 0;
            //    var IsCreate = 0;
            //    var IsUpdate = 0;
            //    var IsDelete = 0;
            //    var IsPrint = 0;
            //    var IsApprove = 0;
            //    var ResMenu = db.Database.SqlQuery<SYS_MENU>("SELECT * FROM SYS_MENU WHERE MENU_URL='" + host + "'").SingleOrDefault();

            //    var CekIsRead = db.Database.SqlQuery<SYS_ACCESS_DETAIL>("SELECT * FROM SYS_ACCESS_DETAIL WHERE ACCESS_DETAIL_ACCESS_ID = " + USER_ACCESS_ID + " AND ACCESS_DETAIL_MENU_ID = " + ResMenu.MENU_ID + " AND ACCESS_DETAIL_TYPE = 1 AND ACCESS_DETAIL_STATUS = 1").SingleOrDefault();
            //    var CekIsCreate = db.Database.SqlQuery<SYS_ACCESS_DETAIL>("SELECT * FROM SYS_ACCESS_DETAIL WHERE ACCESS_DETAIL_ACCESS_ID = " + USER_ACCESS_ID + " AND ACCESS_DETAIL_MENU_ID = " + ResMenu.MENU_ID + " AND ACCESS_DETAIL_TYPE = 2 AND ACCESS_DETAIL_STATUS = 1").SingleOrDefault();
            //    var CekIsUpdate = db.Database.SqlQuery<SYS_ACCESS_DETAIL>("SELECT * FROM SYS_ACCESS_DETAIL WHERE ACCESS_DETAIL_ACCESS_ID = " + USER_ACCESS_ID + " AND ACCESS_DETAIL_MENU_ID = " + ResMenu.MENU_ID + " AND ACCESS_DETAIL_TYPE = 3 AND ACCESS_DETAIL_STATUS = 1").SingleOrDefault();
            //    var CekIsDelete = db.Database.SqlQuery<SYS_ACCESS_DETAIL>("SELECT * FROM SYS_ACCESS_DETAIL WHERE ACCESS_DETAIL_ACCESS_ID = " + USER_ACCESS_ID + " AND ACCESS_DETAIL_MENU_ID = " + ResMenu.MENU_ID + " AND ACCESS_DETAIL_TYPE = 4 AND ACCESS_DETAIL_STATUS = 1").SingleOrDefault();
            //    var CekIsApprove = db.Database.SqlQuery<SYS_ACCESS_DETAIL>("SELECT * FROM SYS_ACCESS_DETAIL WHERE ACCESS_DETAIL_ACCESS_ID = " + USER_ACCESS_ID + " AND ACCESS_DETAIL_MENU_ID = " + ResMenu.MENU_ID + " AND ACCESS_DETAIL_TYPE = 5 AND ACCESS_DETAIL_STATUS = 1").SingleOrDefault();
            //    var CekIsPrint = db.Database.SqlQuery<SYS_ACCESS_DETAIL>("SELECT * FROM SYS_ACCESS_DETAIL WHERE ACCESS_DETAIL_ACCESS_ID = " + USER_ACCESS_ID + " AND ACCESS_DETAIL_MENU_ID = " + ResMenu.MENU_ID + " AND ACCESS_DETAIL_TYPE = 6 AND ACCESS_DETAIL_STATUS = 1").SingleOrDefault();
            //    IsRead = ((CekIsRead != null) ? 1 : 0);
            //    IsCreate = ((CekIsCreate != null) ? 1 : 0);
            //    IsUpdate = ((CekIsUpdate != null) ? 1 : 0);
            //    IsDelete = ((CekIsDelete != null) ? 1 : 0);
            //    IsApprove = ((CekIsApprove != null) ? 1 : 0);
            //    IsPrint = ((CekIsPrint != null) ? 1 : 0);

            //    System.Web.HttpContext.Current.Session["IsRead"] = IsRead;
            //    System.Web.HttpContext.Current.Session["IsCreate"] = IsCreate;
            //    System.Web.HttpContext.Current.Session["IsUpdate"] = IsUpdate;
            //    System.Web.HttpContext.Current.Session["IsDelete"] = IsDelete;
            //    System.Web.HttpContext.Current.Session["IsApprove"] = IsApprove;
            //    System.Web.HttpContext.Current.Session["IsPrint"] = IsPrint;
            //}
            //else {

            //}
        }
    }
}