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
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Web.Helpers;

namespace SISPK.Controllers.Portal
{
    public class SliderController : Controller
    {
        //
        // GET: /Slider/
        private SISPKEntities db = new SISPKEntities();

        public ActionResult Index()
        {
            //ViewData["slider"] = (from t in db.PORTAL_SLIDER where t.SLIDER_STATUS == 1 select t).ToList();

            ViewData["slider"] = db.Database.SqlQuery<PORTAL_SLIDER>("SELECT * FROM PORTAL_SLIDER WHERE SLIDER_STATUS = '1' ").ToList();
            return View();
        }

        //public ActionResult Upload_Slider()
        //{
        //    return View();
        //}

        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Upload(PORTAL_SLIDER ps, FormCollection fc)
        {
            var UserId = Session["USER_ID"];
            var logcode = MixHelper.GetLogCode();
            int lastid = MixHelper.GetSequence("PORTAL_SLIDER");
            var datenow = MixHelper.ConvertDateNow();

            string path = Server.MapPath("~/Upload/IMAGE_SLIDER/GAMBAR_BESAR/");
            HttpPostedFileBase file_att = Request.Files["ImageBesar"];
            var file_name_att = "";
            var filePath = "";
            var fileExtension = "";
            if (file_att != null)
            {
                //// Create a bitmap of the content of the fileUpload control in memory
                //Bitmap originalBMP = new Bitmap(fc.);

                //// Calculate the new image dimensions
                //int origWidth = originalBMP.Width;
                //int origHeight = originalBMP.Height;
                //int sngRatio = origWidth / origHeight;
                //int newWidth = 100;
                //int newHeight = newWidth / sngRatio;

                string IMAGE_BESAR_PATH = file_att.FileName;
                if (IMAGE_BESAR_PATH.Trim() != "")
                {
                    IMAGE_BESAR_PATH = Path.GetFileNameWithoutExtension(file_att.FileName);
                    fileExtension = Path.GetExtension(file_att.FileName);                    
                    file_name_att = "Image_Besar_" + lastid + fileExtension;
                    filePath = path + file_name_att;
                    //WebImage image = new WebImage(file_att.InputStream);
                    //int width = 500;
                    //int height = (int)Math.Round(((width * 1.0) / image.Width) * image.Height);
                    //image.Resize(width, height, false, true);
                        //img.Resize(500, 256);
                    file_att.SaveAs(filePath);
                }
            }

            string path_ = Server.MapPath("~/Upload/IMAGE_SLIDER/GAMBAR_KECIL/");
            HttpPostedFileBase file_att_ = Request.Files["ImageKecil"];
            var file_name_att_ = "";
            var filePath_ = "";
            var fileExtension_ = "";
            if (file_att_ != null)
            {
                string IMAGE_KECIL_PATH = file_att_.FileName;
                if (IMAGE_KECIL_PATH.Trim() != "")
                {
                    IMAGE_KECIL_PATH = Path.GetFileNameWithoutExtension(file_att_.FileName);
                    fileExtension_ = Path.GetExtension(file_att_.FileName);                    
                    file_name_att_ = "Image_Kecil_" + lastid + fileExtension;
                    filePath_ = path_ + file_name_att_;
                    //WebImage image = new WebImage(file_att.InputStream);
                    //int width = 420;
                    //int height = 404;
                    //image.Resize(width, height, false, false);                  
                        //img.Resize(420, 404);
                    file_att_.SaveAs(filePath_);

                    //ResizeSettings resizeCropSettings = new ResizeSettings("width=420&height=404&format=jpeg&crop=auto");
                    //string fileName = Path.Combine(path_, System.Guid.NewGuid().ToString());
                    //fileName = ImageBuilder.Current.Build(filePath_, file_name_att_, resizeCropSettings, false, true);
                }
            }
            var link_ = "http://localhost:4878/Upload/IMAGE_SLIDER/GAMBAR_KECIL/";
            var links = "http://localhost:4878/Upload/IMAGE_SLIDER/GAMBAR_BESAR/";
            var link = "http://localhost:4878/";
            var PATST = "Upload/IMAGE_SLIDER/GAMBAR_BESAR/";
            var PATSTS = "Upload/IMAGE_SLIDER/GAMBAR_KECIL/";
            var logcodeDOC = MixHelper.GetLogCode();
            var FNAME_DOC = "SLIDER_ID,SLIDER_JUDUL,SLIDER_IMAGE_BIG_PATH,SLIDER_IMAGE_SMALL_PATH,SLIDER_IMAGE_LINK,SLIDER_IMAGE_BIG_LINK,SLIDER_IMAGE_SMALL_LINK,SLIDER_IMAGE_IS_USE,SLIDER_STATUS";
            var FVALUE_DOC = "'" + lastid + "', " +
                        "'" + ps.SLIDER_JUDUL + "', " +
                        "'" + PATST+file_name_att + "', " +
                        "'" + PATSTS+file_name_att_ + "', " +
                        "'" + link + "', " +
                        "'" + links + file_name_att + "', " +
                        "'" + link_ + file_name_att_ + "', " +
                        "1, " +
                        "1";
            db.Database.ExecuteSqlCommand("INSERT INTO PORTAL_SLIDER (" + FNAME_DOC + ") VALUES (" + FVALUE_DOC.Replace("''", "NULL") + ")");
            String objekDOC = FVALUE_DOC.Replace("'", "-");
            MixHelper.InsertLog(logcodeDOC, objekDOC, 1);


            TempData["Notifikasi"] = 1;
            TempData["NotifikasiText"] = "Data Berhasil Disimpan";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Use_Slider(int id = 0)
        {
            db.Database.ExecuteSqlCommand("UPDATE PORTAL_SLIDER SET SLIDER_IMAGE_IS_USE = 1 WHERE SLIDER_ID = " + id);
            int e = 1;
            return Json(new { status = e }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Update_not_use(int id = 0)
        {
            db.Database.ExecuteSqlCommand("UPDATE PORTAL_SLIDER SET SLIDER_IMAGE_IS_USE = 0 WHERE SLIDER_ID = " + id);
            int e = 1;
            return Json(new { status = e }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult hapus(int id = 0)
        {
            db.Database.ExecuteSqlCommand("DELETE PORTAL_SLIDER WHERE SLIDER_ID = " + id);
            int e = 1;
            return Json(new { status = e }, JsonRequestBehavior.AllowGet);
        }

        //private Bitmap ScaleImage(Image oldImage)
        //{
        //    double resizeFactor = 1;

        //    if (oldImage.Width > 150 || oldImage.Height > 150)
        //    {
        //        double widthFactor = Convert.ToDouble(oldImage.Width) / 150;
        //        double heightFactor = Convert.ToDouble(oldImage.Height) / 150;
        //        resizeFactor = Math.Max(widthFactor, heightFactor);

        //    }
        //    int width = Convert.ToInt32(oldImage.Width / resizeFactor);
        //    int height = Convert.ToInt32(oldImage.Height / resizeFactor);
        //    Bitmap newImage = new Bitmap(width, height);
        //    Graphics g = Graphics.FromImage(newImage);
        //    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        //    g.DrawImage(oldImage, 0, 0, newImage.Width, newImage.Height);
        //    return newImage;
        //}
    }
}