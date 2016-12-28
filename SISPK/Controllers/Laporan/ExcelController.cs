using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
    public class ExcelController : Controller
    {
        //
        // GET: /Excel/
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LampiranSNI(string stardate = "", string enddate = "", int komtek = 0, int jenis = 0, int status = 0)
        {

            using (ExcelPackage pck = new ExcelPackage())
            {
                var judul = "";
                var listdata = db.TRX_PROPOSAL.SqlQuery("SELECT * FROM TRX_PROPOSAL").ToList();
                //int listdata = 1;

                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("LAMPIRAN A");
                worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
                worksheet.PrinterSettings.PaperSize = ePaperSize.Folio;
                worksheet.PrinterSettings.FitToPage = true;
                worksheet.PrinterSettings.FitToWidth = 1;
                worksheet.PrinterSettings.FitToHeight = 0;
                worksheet.PrinterSettings.RightMargin = 0.3937M;
                worksheet.PrinterSettings.RepeatRows = new ExcelAddress("4:5");

                worksheet.Cells.Style.Numberformat.Format = "General";
                worksheet.HeaderFooter.OddFooter.RightAlignedText = string.Format("SNI - " + judul + " : Halaman {0}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);

                worksheet.Column(1).Width = 5;
                worksheet.Column(2).Width = 30;
                worksheet.Column(3).Width = 10;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 30;
                worksheet.Column(6).Width = 20;
                worksheet.Column(7).Width = 20;
                worksheet.Column(8).Width = 30;
                worksheet.Column(9).Width = 30;
                worksheet.Column(10).Width = 30;
                worksheet.Column(11).Width = 30;
                worksheet.Column(12).Width = 15;
                worksheet.Column(13).Width = 20;
                worksheet.Column(14).Width = 20;
                worksheet.Column(15).Width = 20;
                worksheet.Column(16).Width = 20;
                worksheet.Column(17).Width = 20;

                worksheet.Cells["A1:Q1"].Merge = true;
                worksheet.Cells["A2:Q2"].Merge = true;
                worksheet.Cells["A3:Q3"].Merge = true;

                worksheet.Cells["A5:Q5"].Merge = true;
                worksheet.Cells["A6:Q6"].Merge = true;

                worksheet.Cells["A7:Q7"].Merge = true;
                worksheet.Cells["A8:Q8"].Merge = true;
                worksheet.Cells["A9:Q9"].Merge = true;

                worksheet.Cells["A1"].Value = "LAMPIRAN A";
                worksheet.Cells["A2"].Value = "(normatif)";
                worksheet.Cells["A3"].Value = "Formulir PNPS";
                worksheet.Cells["A5"].Value = "Formulir A.1 Usulan Nasional Perumusan Standar";
                worksheet.Cells["A6"].Value = "Tahun";

                worksheet.Cells["A7"].Value = "Nama Panitia Teknis";
                worksheet.Cells["A8"].Value = "Kode Panitia Teknis";
                worksheet.Cells["A9"].Value = "Lingkup Panitia Teknis";

                using (ExcelRange rng = worksheet.Cells["A1:Q6"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(Color.Red);
                    rng.Style.Font.Color.SetColor(Color.White);
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                }

                using (ExcelRange rng = worksheet.Cells["A10:Q11"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(Color.Red);
                    rng.Style.Font.Color.SetColor(Color.White);
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    rng.Style.WrapText = true;
                    var border = rng.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }

                worksheet.Cells["A10:A11"].Merge = true;
                worksheet.Cells["A10"].Value = "NO";
                worksheet.Cells["B10:B11"].Merge = true;
                worksheet.Cells["B10"].Value = "Judul / Topik dalam Bahasa Indonesia atau Bahasa Inggris";
                worksheet.Cells["C10:C11"].Merge = true;
                worksheet.Cells["C10"].Value = "ICS";
                worksheet.Cells["D10:G10"].Merge = true;
                worksheet.Cells["D10"].Value = "Justifikasi / Alasan Perumusan Standar";

                worksheet.Cells["D11"].Value = "Regulasi";
                worksheet.Cells["E11"].Value = "Kesepakatan regional / internasional";
                worksheet.Cells["F11"].Value = "Kebutuhan Pasar";
                worksheet.Cells["G11"].Value = "Pertimbangan Lain";

                worksheet.Cells["H10:H11"].Merge = true;
                worksheet.Cells["H10"].Value = "Ruang Lingkup / Batasan Penerapan";

                worksheet.Cells["I10:K10"].Merge = true;
                worksheet.Cells["I10"].Value = "Adopsi";


                worksheet.Cells["I11"].Value = "Standar Internasional (Judul dan Identifikasi)";
                worksheet.Cells["J11"].Value = "Standar Non Internasional (Judul dan Identifikasi)";
                worksheet.Cells["K11"].Value = "Acuan Normatif";

                worksheet.Cells["L10:M10"].Merge = true;
                worksheet.Cells["L10"].Value = "Tidak Adopsi / Pengembangan Sendiri";

                worksheet.Cells["L11"].Value = "Penelitian";
                worksheet.Cells["M11"].Value = "Standar yang digunakan sebagai acuan (judul dan identifikasi)";

                worksheet.Cells["N10:N11"].Merge = true;
                worksheet.Cells["N10"].Value = "Jangka Waktu Penyelesaian Perumusan (Tahun)";

                worksheet.Cells["O10:P10"].Merge = true;
                worksheet.Cells["O10"].Value = "Status Standar";

                worksheet.Cells["O11"].Value = "Baru";
                worksheet.Cells["P11"].Value = "Revisi (No dan Judul Yang di Revisi)";

                worksheet.Cells["Q10:Q11"].Merge = true;
                worksheet.Cells["Q10"].Value = "Keterangan";


                using (ExcelRange rng = worksheet.Cells["A12:Q12"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(Color.Red);
                    rng.Style.Font.Color.SetColor(Color.White);
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    rng.Style.WrapText = true;
                    var border = rng.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }

                worksheet.Cells["A12"].Value = "1";
                worksheet.Cells["B12"].Value = "2";
                worksheet.Cells["C12"].Value = "3";
                worksheet.Cells["D12"].Value = "4";
                worksheet.Cells["E12"].Value = "5";
                worksheet.Cells["F12"].Value = "6";
                worksheet.Cells["G12"].Value = "7";
                worksheet.Cells["H12"].Value = "8";
                worksheet.Cells["I12"].Value = "9";
                worksheet.Cells["J12"].Value = "10";
                worksheet.Cells["K12"].Value = "11";
                worksheet.Cells["L12"].Value = "12";
                worksheet.Cells["M12"].Value = "13";
                worksheet.Cells["N12"].Value = "14";
                worksheet.Cells["O12"].Value = "15";
                worksheet.Cells["P12"].Value = "16";
                worksheet.Cells["Q12"].Value = "17";

                worksheet.View.FreezePanes(13, 1);

                int cell = 13;
                int no = 1;
                if (listdata.Count() > 0)
                {
                    foreach (var list in listdata)
                    {
                        worksheet.Cells["A" + cell].Value = no++;
                        worksheet.Cells["B" + cell].Value = "";
                        worksheet.Cells["C" + cell].Value = "";
                        worksheet.Cells["D" + cell].Value = "";
                        worksheet.Cells["E" + cell].Value = "";
                        worksheet.Cells["F" + cell].Value = "";
                        worksheet.Cells["G" + cell].Value = "";
                        worksheet.Cells["H" + cell].Value = "";
                        worksheet.Cells["I" + cell].Value = "";
                        worksheet.Cells["J" + cell].Value = "";
                        worksheet.Cells["K" + cell].Value = "";
                        worksheet.Cells["L" + cell].Value = "";
                        worksheet.Cells["M" + cell].Value = "";
                        worksheet.Cells["N" + cell].Value = "";
                        worksheet.Cells["O" + cell].Value = "";
                        worksheet.Cells["P" + cell].Value = "";
                        worksheet.Cells["Q" + cell].Value = "";

                        cell++;
                    }
                }
                else
                {
                    cell = 14;
                }
                using (ExcelRange rng = worksheet.Cells["A13:Q" + (cell - 1)])
                {
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    rng.Style.WrapText = true;
                    var border = rng.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }

                //populate our Data
                //int cell = 12;
                //populate our Data
                //long LuasTanah = 0;
                //long Biaya = 0;
                //long NilaiWajar = 0;
                //int no = 1;

                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("content-disposition", "attachment;  filename=DAFTAR_BARANG_TANAH_PER_.xls");
                Response.BinaryWrite(pck.GetAsByteArray());
                Response.End();
                return View();
            }
        }

            public ActionResult Cetak_usulan_baru()
        {
            

            using (ExcelPackage pck = new ExcelPackage())
            {
                //var judul = "";
                var listdata = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM ( SELECT T1.*, ROWNUM ROWNUMBER FROM( SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_STATUS = 0 AND APPROVAL_TYPE = -1 ORDER BY PROPOSAL_CREATE_DATE DESC ) T1 ) WHERE ROWNUMBER > 0").ToList();
                
                //int listdata = 1;

                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("Usulan Baru");
                worksheet.PrinterSettings.Orientation = eOrientation.Landscape;
                worksheet.PrinterSettings.PaperSize = ePaperSize.Folio;
                worksheet.PrinterSettings.FitToPage = true;
                worksheet.PrinterSettings.FitToWidth = 1;
                worksheet.PrinterSettings.FitToHeight = 0;
                worksheet.PrinterSettings.RightMargin = 0.3937M;
                worksheet.PrinterSettings.RepeatRows = new ExcelAddress("4:5");

                worksheet.Cells.Style.Numberformat.Format = "General";
                //worksheet.HeaderFooter.OddFooter.RightAlignedText = string.Format("SNI - " + judul + " : Halaman {0}", ExcelHeaderFooter.PageNumber, ExcelHeaderFooter.NumberOfPages);

                worksheet.Column(1).Width = 5;
                worksheet.Column(2).Width = 15;
                worksheet.Column(3).Width = 20;
                worksheet.Column(4).Width = 20;
                worksheet.Column(5).Width = 30;
                worksheet.Column(6).Width = 50;
                worksheet.Column(7).Width = 10;
                worksheet.Column(8).Width = 10;
                worksheet.Column(9).Width = 30;

                

                worksheet.Cells["A2"].Value = "DAFTAR PENGAJUAN USULAN";
                worksheet.Cells["A3"].Value = "Usulan Baru";

                

                

                ////worksheet.Cells["A10:A11"].Merge = true;
                //worksheet.Cells["A4"].Value = "No";
                ////worksheet.Cells["B10:B11"].Merge = true;
                //worksheet.Cells["B4"].Value = "Tanggal Usulan";
                ////worksheet.Cells["C10:C11"].Merge = true;
                //worksheet.Cells["C4"].Value = "Jenis Pengusul";
                ////worksheet.Cells["D10:G10"].Merge = true;
                //worksheet.Cells["D4"].Value = "Jenis Perumusan";
                //worksheet.Cells["E4"].Value = "Komtek";
                //worksheet.Cells["F4"].Value = "Judul";
                //worksheet.Cells["G4"].Value = "Mendesak";
                //worksheet.Cells["H4"].Value = "Tahapan";
                //worksheet.Cells["I4"].Value = "Status";

                using (ExcelRange rng = worksheet.Cells["A5:I5"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    rng.Style.Fill.BackgroundColor.SetColor(Color.Red);
                    rng.Style.Font.Color.SetColor(Color.White);
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    rng.Style.WrapText = true;
                    var border = rng.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }

                worksheet.Cells["A5"].Value = "No";
                worksheet.Cells["B5"].Value = "Tanggal Usulan";
                worksheet.Cells["C5"].Value = "Jenis Pengusul";
                worksheet.Cells["D5"].Value = "Jenis Perumusan";
                worksheet.Cells["E5"].Value = "Komtek";
                worksheet.Cells["F5"].Value = "Judul";
                worksheet.Cells["G5"].Value = "Mendesak";
                worksheet.Cells["H5"].Value = "Tahapan";
                worksheet.Cells["I5"].Value = "Status";


                worksheet.View.FreezePanes(6, 1);

                int cell = 6;
                int no = 1;
                if (listdata.Count() > 0)
                {
                    foreach (var list in listdata)
                    {
                        worksheet.Cells["A" + cell].Value = no++;
                        worksheet.Cells["B" + cell].Value = list.PROPOSAL_CREATE_DATE_NAME;
                        worksheet.Cells["C" + cell].Value = list.PROPOSAL_TYPE_NAME;
                        worksheet.Cells["D" + cell].Value = list.PROPOSAL_JENIS_PERUMUSAN_NAME;
                        worksheet.Cells["E" + cell].Value = (list.KOMTEK_CODE == null) ? "-" : list.KOMTEK_CODE + "." + list.KOMTEK_NAME;
                        worksheet.Cells["F" + cell].Value = list.PROPOSAL_JUDUL_PNPS;
                        worksheet.Cells["G" + cell].Value = list.PROPOSAL_IS_URGENT_NAME;
                        worksheet.Cells["H" + cell].Value = list.PROPOSAL_TAHAPAN;
                        worksheet.Cells["I" + cell].Value = list.PROPOSAL_STATUS_NAME;

                        cell++;
                    }
                }
                else
                {
                    cell = 7;
                }
                using (ExcelRange rng = worksheet.Cells["A5:I" + (cell - 1)])
                {
                    rng.Style.VerticalAlignment = ExcelVerticalAlignment.Top;
                    rng.Style.WrapText = true;
                    var border = rng.Style.Border;
                    border.Bottom.Style = border.Top.Style = border.Left.Style = border.Right.Style = ExcelBorderStyle.Thin;
                }


                Response.ContentType = "application/vnd.ms-excel";
                Response.AddHeader("content-disposition", "attachment;  filename=DAFTAR_USULAN_BARU.xls");
                Response.BinaryWrite(pck.GetAsByteArray());
                Response.End();
                return View();
            }
        }

    }
}
