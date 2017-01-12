using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Aspose.Words;
using SISPK.Models;
using SISPK.Helpers;
using Aspose.Words.Tables;
using System.Drawing;
using Aspose.Words.Drawing;
using Aspose.Words.Saving;
using Aspose.Words.Properties;

namespace SISPK.Controllers.Laporan
{
    public class ExportController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        public ActionResult JajakPendapat(int POLLING_ID = 0, int PROPOSAL_ID = 0)
        {
            var DataProposal = (from proposal in db.VIEW_PROPOSAL where proposal.PROPOSAL_ID == PROPOSAL_ID select proposal).SingleOrDefault();
            var DataTanggapan = db.Database.SqlQuery<VIEW_POLLING_DETAIL>("SELECT * FROM VIEW_POLLING_DETAIL WHERE POLLING_DETAIL_POLLING_ID = " + POLLING_ID + " ORDER BY POLLING_DETAIL_INPUT_TYPE ASC,POLLING_DETAIL_PASAL ASC,POLLING_DETAIL_CREATE_DATE DESC").ToList();
            var PROPOSAL_PNPS_CODE_FIXER = DataProposal.PROPOSAL_PNPS_CODE;
            var TGL_SEKARANG = DateTime.Now.ToString("yyyyMMddHHmmss");
            string path = Server.MapPath("~/Upload/Dokumen/RANCANGAN_SNI/JAJAK_PENDAPAT/" + PROPOSAL_PNPS_CODE_FIXER + "/");
            string dataFormat = Server.MapPath("~/Format/Laporan/");
            Stream stream = System.IO.File.OpenRead(dataFormat + "FORMAT_TANGGAPAN_JAJAK_PENDAPAT.docx");
            Aspose.Words.Document Tanggapan = new Aspose.Words.Document(stream);
            ReplaceHelper helper = new ReplaceHelper(Tanggapan);
            helper.Replace("JudulPNPS", DataProposal.PROPOSAL_JUDUL_PNPS);
            DateTime dt = Convert.ToDateTime(DataProposal.POLLING_START_DATE);
            helper.Replace("TanggalJP", dt.ToString("dd/MM/yyyy"));
            helper.Replace("RuangLingkupPNPS", DataProposal.PROPOSAL_RUANG_LINGKUP);


            Paragraph paragraph = new Paragraph(Tanggapan);

            DocumentBuilder builder = new DocumentBuilder(Tanggapan);
            Aspose.Words.Font font = builder.Font;
            font.Bold = false;
            font.Color = System.Drawing.Color.Black;
            font.Italic = false;
            font.Name = "Calibri";
            font.Size = 11;
            builder.MoveToDocumentEnd();

            var number = 0;
            string html =   "<table width='100%' border='1' bordercolor='#111111' cellpadding='2'  style='border-collapse: collapse' cellpadding='0' cellspacing='0'>" +
                            "<tr>" +
                               "<td width='5%' style='text-align:center'><b>Tipe Pemberi Komentar</b></td>" +
                               "<td width='5%' style='text-align:center'><b>No Pasal/No Subpasal</b></td>" +
                               "<td width='15%' style='text-align:center'><b>Nama</b></td>" +
                               "<td width='10%' style='text-align:center'><b>Tipe Tanggapan</b></td>" +
                               "<td width='65%' style='text-align:center'><b>Tanggapan</b></td>" +
                               "</tr>";
            if (DataTanggapan != null)
            {
                foreach (var i in DataTanggapan)
                {
                    number++;
                    html += "<tr>" +
                        "<td style='text-align:center'>" + i.POLLING_DETAIL_INPUT_TYPE_NAME + "</td>" +
                        "<td style='text-align:center'>" + i.POLLING_DETAIL_PASAL + "</td>" +

                                   "<td>" + i.USER_PUBLIC_NAMA + "</td>" +
                                   "<td style='text-align:center'>" + i.PILIHAN + "</td>" +
                                   "<td>" + i.POLLING_DETAIL_REASON + "</td>" +
                                   "</tr>";
                }
            }
            else
            {
                html += "<tr>" +
                       "<td style='text-align:center'>-</td>" +
                       "<td style='text-align:center'>-</td>" +
                       "<td>-</td>" +
                       "<td style='text-align:center'>-</td>" +
                       "<td>-</td>" +
                       "</tr>";
            }
            html += "</table>";

            builder.InsertHtml(html);
            Tanggapan.Save(@"" + path + "DAFTAR_TANGGAPAN_JAJAK_PENDAPAT_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".docx", Aspose.Words.SaveFormat.Docx);
            Tanggapan.Save(@"" + path + "DAFTAR_TANGGAPAN_JAJAK_PENDAPAT_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf", Aspose.Words.SaveFormat.Pdf);
            Tanggapan.Save(@"" + path + "DAFTAR_TANGGAPAN_JAJAK_PENDAPAT_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".xml");


            var mime = "";
            MemoryStream dstStream = new MemoryStream();
            Tanggapan.Save(dstStream, SaveFormat.Pdf);
            mime = "application/pdf";

            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;
            Response.AddHeader("content-disposition", "attachment;  filename=DAFTAR_TANGGAPAN_JAJAK_PENDAPAT_" + PROPOSAL_PNPS_CODE_FIXER + "_" + TGL_SEKARANG + ".pdf");
            Response.BinaryWrite(byteInfo);
            Response.End();
            return new FileStreamResult(dstStream, mime);
        }
        public ActionResult DOWNLOAD_SNI(string Type = "docx", int PROPOSAL_ID = 0, int RSNI_TYPE = 1)
        {
            var Data = db.Database.SqlQuery<VIEW_SNI>("SELECT * FROM VIEW_SNI WHERE SNI_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();

            string dataDir = Server.MapPath("~" + Data.DSNI_DOC_FILE_PATH + "" + Data.DSNI_DOC_FILE_NAME + ".DOCX");
            Stream stream = System.IO.File.OpenRead(dataDir);
            var IS_LIMIT_DOWNLOAD = Data.IS_LIMIT_DOWNLOAD;
            Aspose.Words.Document doc = new Aspose.Words.Document(stream);
            CustomDocumentProperties props = doc.CustomDocumentProperties;

            var DataJmlDownload = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 10").FirstOrDefault();
            var DataWatermark = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 9").FirstOrDefault();
            PdfSaveOptions options = new Aspose.Words.Saving.PdfSaveOptions();
            if (IS_LIMIT_DOWNLOAD == 1)
            {
                options.PageCount = Convert.ToInt32(DataJmlDownload.CONFIG_VALUE);
            }

            var watermarkText = DataWatermark.CONFIG_VALUE;

            Shape watermark = new Shape(doc, ShapeType.TextPlainText);
            watermark.TextPath.Text = watermarkText;

            watermark.TextPath.FontFamily = "Arial";
            double fontSize = 11;
            watermark.TextPath.Size = fontSize;
            watermark.Height = fontSize;
            watermark.Width = watermarkText.Length * fontSize / 2;
            watermark.Rotation = 90;
            watermark.Fill.Color = Color.Pink; // Try LightGray to get more Word-style watermark
            watermark.StrokeColor = Color.Pink; // Try LightGray to get more Word-style watermark
            watermark.RelativeHorizontalPosition = RelativeHorizontalPosition.RightMargin;
            watermark.RelativeVerticalPosition = RelativeVerticalPosition.Page;
            watermark.WrapType = WrapType.None;
            watermark.VerticalAlignment = VerticalAlignment.Center;
            watermark.HorizontalAlignment = HorizontalAlignment.Center;
            watermark.Fill.Opacity = 0.8;
            Paragraph watermarkPara = new Paragraph(doc);
            watermarkPara.AppendChild(watermark);
            watermarkPara.ParagraphBreakFont.Size = 1;

            foreach (Section sect in doc.Sections)
            {
                InsertWatermarkIntoHeader(watermarkPara, sect, HeaderFooterType.HeaderPrimary);
                InsertWatermarkIntoHeader(watermarkPara, sect, HeaderFooterType.HeaderFirst);
                InsertWatermarkIntoHeader(watermarkPara, sect, HeaderFooterType.HeaderEven);
            }
            stream.Close();
            MemoryStream dstStream = new MemoryStream();
            var mime = "";
            doc.Save(dstStream, options);
            mime = "application/pdf";

            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;
            Response.AddHeader("content-disposition", "attachment;  filename=SNI_" + Data.DSNI_DOC_FILE_NAME + ".PDF");
            Response.BinaryWrite(byteInfo);
            Response.End();
            return new FileStreamResult(dstStream, mime);
        }
        public ActionResult TEMPLATE_RSNI(string Type = "docx", int PROPOSAL_ID = 0, int RSNI_TYPE = 1)
        {
            var Data = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();

            string dataDir = Server.MapPath("~/Format/Laporan/");
            Stream stream = System.IO.File.OpenRead(dataDir + "TEMPLATE_RSNI.docx");

            Aspose.Words.Document doc = new Aspose.Words.Document(stream);
            stream.Close();
            if (Data != null)
            {
                ReplaceHelper helper = new ReplaceHelper(doc);
                helper.Replace("DATATAHUN", ConvertTanggal(Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE), "tahun"));
                helper.Replace("JUDUL_PNPS", Data.PROPOSAL_JUDUL_PNPS);
                if (RSNI_TYPE == 7)
                {
                    helper.Replace("RSNI1", "RASNI");
                }
                else
                {
                    helper.Replace("RSNI1", "RSNI" + Convert.ToString(RSNI_TYPE));
                }
                var ICS = db.Database.SqlQuery<VIEW_PROPOSAL_ICS>("SELECT * FROM VIEW_PROPOSAL_ICS WHERE PROPOSAL_ICS_REF_PROPOSAL_ID = " + PROPOSAL_ID).ToList();
                var last = ICS.Last();
                if (ICS != null)
                {
                    var namaics = "";
                    foreach (var i in ICS)
                    {
                        if (i.Equals(last))
                        {
                            namaics += i.ICS_CODE;
                        }
                        else
                        {
                            namaics += i.ICS_CODE + ",";
                        }
                    }
                    helper.Replace("NAMA_ICS", namaics);
                }
            }

            MemoryStream dstStream = new MemoryStream();

            var mime = "";
            doc.Save(dstStream, SaveFormat.Docx);
            mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;

            if (RSNI_TYPE == 7)
            {
                Response.AddHeader("content-disposition", "attachment;  filename=TEMPLATE_RASNI." + Type);
                Response.BinaryWrite(byteInfo);
                Response.End();
            }
            else
            {
                Response.AddHeader("content-disposition", "attachment;  filename=TEMPLATE_RSNI." + Type);
                Response.BinaryWrite(byteInfo);
                Response.End();
            }
            return new FileStreamResult(dstStream, mime);
        }

        public ActionResult TemplateHakPAten(string Type = "docx")
        {
            string dataDir = Server.MapPath("~/Format/Laporan/");
            Stream stream = System.IO.File.OpenRead(dataDir + "FORM_KESEDIAAN_PENCANTUMAN_PATEN_DALAM_SNI.docx");
            Aspose.Words.Document doc = new Aspose.Words.Document(stream);
            stream.Close();

            MemoryStream dstStream = new MemoryStream();

            var mime = "";
            doc.Save(dstStream, SaveFormat.Docx);
            mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;

            Response.AddHeader("content-disposition", "attachment;  filename=FORM_KESEDIAAN_PENCANTUMAN_PATEN_DALAM_SNI." + Type);
            Response.BinaryWrite(byteInfo);
            Response.End();

            return new FileStreamResult(dstStream, mime);
        }

        public ActionResult Formulir_Kaji_Ulang_SNI(string Type = "docx")
        {
            string dataDir = Server.MapPath("~/Format/Laporan/");
            Stream stream = System.IO.File.OpenRead(dataDir + "TEMPLATE_KAJI_ULANG_SNI.docx");
            Aspose.Words.Document doc = new Aspose.Words.Document(stream);
            stream.Close();

            MemoryStream dstStream = new MemoryStream();

            var mime = "";
            doc.Save(dstStream, SaveFormat.Docx);
            mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;

            Response.AddHeader("content-disposition", "attachment;  filename=TEMPLATE_KAJI_ULANG_SNI." + Type);
            Response.BinaryWrite(byteInfo);
            Response.End();

            return new FileStreamResult(dstStream, mime);
        }

        private static void InsertWatermarkIntoHeader(Paragraph watermarkPara, Section sect, HeaderFooterType headerType)
        {

            HeaderFooter header = sect.HeadersFooters[headerType];



            if (header == null)
            {

                // There is no header of the specified type in the current section, create it.

                header = new HeaderFooter(sect.Document, headerType);

                sect.HeadersFooters.Add(header);

            }



            // Insert a clone of the watermark into the header.

            header.AppendChild(watermarkPara.Clone(true));

        }
        public ActionResult FORMAT_LEMBAR_KENDALI(string Type = "docx", int PROPOSAL_ID = 0)
        {
            var Data = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();

            string dataDir = Server.MapPath("~/Format/Laporan/");
            Stream stream = System.IO.File.OpenRead(dataDir + "FORMAT_LEMBAR_KENDALI.docx");

            Aspose.Words.Document doc = new Aspose.Words.Document(stream);
            stream.Close();
            if (Data != null)
            {
                ReplaceHelper helper = new ReplaceHelper(doc);
                var Metode = "";
                if (Data.PROPOSAL_JALUR == 1)
                {
                    Metode = "Perumusan sendiri berdasarkan penelitian";
                }
                else if (Data.PROPOSAL_JALUR == 2)
                {
                    Metode = "Adopsi identik standar atau publikasi internasional";
                }
                else if (Data.PROPOSAL_JALUR == 3)
                {
                    Metode = "Perumusan sendiri berdasarkan penelitian dan Adopsi identik standar atau publikasi internasional";
                }
                if (Data.PROPOSAL_METODE_ADOPSI == 3)
                {
                    //Metode = "Perumusan sendiri berdasarkan penelitian dan Adopsi identik standar atau publikasi internasional\nIdentik Standar yang di Adopsi :" + Data.PROPOSAL_NO_STD_ADOPSI_IDENTIK;
                }
                var APPROVAL_STATUS_SESSION_MTPS = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 2 AND T1.APPROVAL_TYPE = 1").SingleOrDefault();
                var DATA_MTPS = db.Database.SqlQuery<TRX_PROPOSAL_APPROVAL>("SELECT * FROM TRX_PROPOSAL_APPROVAL WHERE APPROVAL_STATUS_PROPOSAL = 2 AND APPROVAL_PROPOSAL_ID = " + Data.PROPOSAL_ID + " AND APPROVAL_STATUS_SESSION = " + APPROVAL_STATUS_SESSION_MTPS).SingleOrDefault();
                //var APPROVAL_STATUS_SESSION_RATEK = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 4 AND T1.APPROVAL_TYPE = 1").SingleOrDefault();
                //var DATA_RATEK = db.Database.SqlQuery<TRX_PROPOSAL_APPROVAL>("SELECT * FROM TRX_PROPOSAL_APPROVAL WHERE APPROVAL_STATUS_PROPOSAL = 4 AND APPROVAL_PROPOSAL_ID = " + Data.PROPOSAL_ID + " AND APPROVAL_STATUS_SESSION = " + APPROVAL_STATUS_SESSION_RATEK).SingleOrDefault();
                //var APPROVAL_STATUS_SESSION_RAKON = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.APPROVAL_STATUS_SESSION),0) AS NUMBER) AS APPROVAL_STATUS_SESSION FROM TRX_PROPOSAL_APPROVAL T1 WHERE T1.APPROVAL_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.APPROVAL_STATUS_PROPOSAL = 5 AND T1.APPROVAL_TYPE = 1").SingleOrDefault();
                //var DATA_RAKON = db.Database.SqlQuery<TRX_PROPOSAL_APPROVAL>("SELECT * FROM TRX_PROPOSAL_APPROVAL WHERE PROPOSAL_RAPAT_PROPOSAL_STATUS = 5 AND APPROVAL_PROPOSAL_ID = " + Data.PROPOSAL_ID + " AND APPROVAL_STATUS_SESSION = " + APPROVAL_STATUS_SESSION_RAKON).SingleOrDefault();

                //doc.Range.Replace("FullTanggalPenugasan", ConvertTanggal(Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE), "full"), false, true);
                helper.Replace("DataBidang", ((Data.BIDANG_CODE == null) ? "" : Data.BIDANG_CODE));
                helper.Replace("KomtekName", ((Data.KOMTEK_CODE == null) ? "" : Data.KOMTEK_CODE + " " + Data.KOMTEK_NAME));
                helper.Replace("SekretariatName", ((Data.KOMTEK_SEKRETARIAT == null) ? "" : Data.KOMTEK_SEKRETARIAT));
                helper.Replace("JudulPNPS", Data.PROPOSAL_JUDUL_PNPS);
                helper.Replace("PerubahanJudul", "");
                helper.Replace("NoSNI", Data.PROPOSAL_NO_SNI_PROPOSAL + " " + Data.PROPOSAL_JUDUL_SNI_PROPOSAL);
                helper.Replace("StatusPNPS", Data.PROPOSAL_JENIS_PERUMUSAN_NAME);
                helper.Replace("MetodePNPS", "" + Metode + "");

                //helper.Replace("DATA_01", ConvertTanggal(Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE), "full"));
                //helper.Replace("DATA_02", ConvertTanggal(Convert.ToDateTime(DATA_MTPS.APPROVAL_DATE), "full"));
                //helper.Replace("DATA_03", Data.PROPOSAL_JENIS_PERUMUSAN_NAME);
                //helper.Replace("DATA_04", ConvertTanggal(Convert.ToDateTime(DATA_RATEK.APPROVAL_DATE), "full"));
                helper.Replace("DATA_04", "");
                helper.Replace("DATA_05", "");
                helper.Replace("DATA_06", "");
                //helper.Replace("DATA_06", ConvertTanggal(Convert.ToDateTime(DATA_RAKON.APPROVAL_DATE), "full"));
                helper.Replace("DATA_07", "");
                helper.Replace("DATA_08", "");
                helper.Replace("DATA_09", "Dilanjutkan");
                helper.Replace("DATA_10", "");
                helper.Replace("DATA_11", "");
                helper.Replace("DATA_12", "");
                helper.Replace("DATA_13", "");
                helper.Replace("DATA_14", "");
                helper.Replace("DATA_15", "");
                helper.Replace("DATA_16", "");
                helper.Replace("DATA_17", "");
                helper.Replace("DATA_18", "");
                helper.Replace("DATA_19", "");
                helper.Replace("DATA_20", "");
                helper.Replace("DATA_21", "");
                helper.Replace("DATA_22", "");
                helper.Replace("DATA_23", "");
                helper.Replace("DATA_24", "");
                helper.Replace("DATA_25", "");
                helper.Replace("DATA_26", "");
                helper.Replace("DATA_27", "");
                helper.Replace("DATA_28", "");
                helper.Replace("DATA_29", "");
                helper.Replace("DATA_30", "");
                helper.Replace("DATA_31", "");
                helper.Replace("DATA_32", "");
                helper.Replace("DATA_33", "");
                helper.Replace("DATA_34", "");
                helper.Replace("DATA_35", "");
                helper.Replace("DATA_36", "");
                helper.Replace("DATA_37", "");
                helper.Replace("DATA_38", "");
                helper.Replace("DATA_39", "");
                helper.Replace("DATA_40", "");
                helper.Replace("DATA_41", "");
                helper.Replace("DATA_42", "");
                helper.Replace("DATA_43", "");
            }

            MemoryStream dstStream = new MemoryStream();

            var mime = "";
            doc.Save(dstStream, SaveFormat.Docx);
            mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;
            Response.AddHeader("content-disposition", "attachment;  filename=FORMAT_LEMBAR_KENDALI." + Type);
            Response.BinaryWrite(byteInfo);
            Response.End();
            return new FileStreamResult(dstStream, mime);
        }
        public ActionResult FORMAT_DAFTAR_HADIR(string Type = "docx", int PROPOSAL_ID = 0, int TipeRapat = 0)
        {
            var Data = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();

            string dataDir = Server.MapPath("~/Format/Laporan/");
            Stream stream = System.IO.File.OpenRead(dataDir + "FORMAT_DAFTAR_HADIR.docx");

            Aspose.Words.Document doc = new Aspose.Words.Document(stream);
            stream.Close();
            var NamaRapat = "";
            if (TipeRapat == 0)
            {
                NamaRapat = "Rapat Teknis";
            }
            else if (TipeRapat == 1)
            {
                NamaRapat = "Rapat Konsensus";
            }
            else if (TipeRapat == 2)
            {
                NamaRapat = "Rapat Koordinasi";
            }
            else if (TipeRapat == 3)
            {
                NamaRapat = "Rapat Rumusan Rancangan Akhir SNI";
            }
            if (Data != null)
            {
                doc.Range.Replace("FullTanggalPenugasan", ConvertTanggal(Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE), "full"), false, true);
                doc.Range.Replace("NamaRapat", NamaRapat, false, true);
                doc.Range.Replace("KodePNPS", Data.PROPOSAL_PNPS_CODE, false, true);
                doc.Range.Replace("NamaKomtek", Data.KOMTEK_CODE + " " + Data.KOMTEK_NAME, false, true);
                doc.Range.Replace("JudulPNPS", Data.PROPOSAL_JUDUL_PNPS, false, true);
            }
            DocumentBuilder builder = new DocumentBuilder(doc);
            Aspose.Words.Font font = builder.Font;
            font.Bold = false;
            font.Color = System.Drawing.Color.Black;
            font.Italic = false;
            font.Name = "Calibri";
            font.Size = 11;
            builder.MoveToDocumentEnd();
            var DataKomtek = (from komtek in db.VIEW_ANGGOTA where komtek.KOMTEK_ANGGOTA_STATUS == 1 && komtek.KOMTEK_ANGGOTA_KOMTEK_ID == Data.KOMTEK_ID orderby komtek.KOMTEK_ANGGOTA_JABATAN select komtek).ToList();
            var no = 0;
            Table myTable = doc.FirstSection.Body.Tables[1];
            Row myRow = myTable.LastRow;
            foreach (var list in DataKomtek)
            {
                no++;
                Row newRow = (Row)myRow.Clone(true);
                myTable.AppendChild(newRow);
                foreach (Cell cell in newRow)
                {
                    cell.FirstParagraph.ChildNodes.Clear();
                }
                builder.MoveToCell(1, no, 0, 0);
                builder.Write("" + no);
                builder.MoveToCell(1, no, 1, 0);
                builder.Write("" + list.KOMTEK_ANGGOTA_NAMA);
                builder.MoveToCell(1, no, 2, 0);
                builder.Write("" + list.STAKEHOLDER);
                builder.MoveToCell(1, no, 3, 0);
                builder.Write("" + list.KOMTEK_ANGGOTA_INSTANSI);
                builder.MoveToCell(1, no, 4, 0);
                builder.Write("");
            }
            Row LastROW = myTable.LastRow;
            LastROW.Remove();

            MemoryStream dstStream = new MemoryStream();

            var mime = "";
            doc.Save(dstStream, SaveFormat.Docx);
            mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;
            Response.AddHeader("content-disposition", "attachment;  filename=FORMAT_DAFTAR_HADIR." + Type);
            Response.BinaryWrite(byteInfo);
            Response.End();
            return new FileStreamResult(dstStream, mime);
        }
        public ActionResult FORMULIR_PENUGASAN_KOMITE_TEKNIS(string Type = "docx", int PROPOSAL_ID = 0)
        {
            var Data = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();

            string dataDir = Server.MapPath("~/Format/Laporan/");
            Stream stream = System.IO.File.OpenRead(dataDir + "FORMULIR_PENUGASAN_KOMITE_TEKNIS.docx");

            Aspose.Words.Document doc = new Aspose.Words.Document(stream);
            stream.Close();
            if (Data != null)
            {
                var DataKomtek = db.Database.SqlQuery<VIEW_ANGGOTA>("SELECT * FROM VIEW_ANGGOTA WHERE KOMTEK_ANGGOTA_KOMTEK_ID = " + Data.KOMTEK_ID + " AND JABATAN = 'Ketua'").SingleOrDefault();

                doc.Range.Replace("FullTanggalPenugasan", ConvertTanggal(Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE), "full"), false, true);

                doc.Range.Replace("HariPenugasan", "", false, true);
                doc.Range.Replace("TanggalPenugasan", "", false, true);

                doc.Range.Replace("NomorPenugasan", "", false, true);
                doc.Range.Replace("LampiranPenugasan", "", false, true);
                doc.Range.Replace("NamaKepalaPPS", "Sumartini Maksum", false, true);
                doc.Range.Replace("NIPKepalaPPS", "Nip : 19561014 198107 2 001", false, true);
                doc.Range.Replace("NamaKetuaKomiteTeknis", DataKomtek.KOMTEK_ANGGOTA_NAMA, false, true);
                doc.Range.Replace("KodeKomtek", Data.KOMTEK_CODE, false, true);
                doc.Range.Replace("NamaKomtek", Data.KOMTEK_NAME, false, true);
                doc.Range.Replace("WaktuPenugasan", "", false, true);
                doc.Range.Replace("TahunPenugasan", ConvertTanggal(Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE), "tahun"), false, true);

            }
            MemoryStream dstStream = new MemoryStream();

            var mime = "";
            if (Type == "pdf")
            {
                doc.Save(dstStream, SaveFormat.Pdf);
                mime = "application/pdf";
            }
            else
            {
                doc.Save(dstStream, SaveFormat.Docx);
                mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            }

            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;
            Response.AddHeader("content-disposition", "attachment;  filename=FORMULIR_PENUGASAN_KOMITE_TEKNIS." + Type);
            Response.BinaryWrite(byteInfo);
            Response.End();
            return new FileStreamResult(dstStream, mime);
        }

        public ActionResult HISTORY_LAPORAN_USULAN(string Type = "docx", int PROPOSAL_ID = 0)
        {
            var Data = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            var Dt_Monitor = db.Database.SqlQuery<TRX_MONITORING>("SELECT * FROM TRX_MONITORING WHERE MONITORING_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();

            if (Data != null)
            {
                string dataDir = Server.MapPath("~/Format/Laporan/");
                Stream stream = System.IO.File.OpenRead(dataDir + "TEMPLATE_HISTORY_USULAN.docx");

                Aspose.Words.Document doc = new Aspose.Words.Document(stream);
                stream.Close();
                ReplaceHelper helper = new ReplaceHelper(doc);


                var dt = Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE);
                string time = dt.ToString("hh:mm tt");
                string PROPOSAL_CREATE_DATE = Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE).ToString("dd-MM-yyyy");

                helper.Replace("{Kode_PNPS}", ((Data.PROPOSAL_CODE == null) ? "" : Data.PROPOSAL_CODE));
                helper.Replace("{No_SNI_Proposal}", ((Data.PROPOSAL_NO_SNI_PROPOSAL == null) ? "" : Data.PROPOSAL_NO_SNI_PROPOSAL));
                helper.Replace("{Kode_Komtek}", ((Data.KOMTEK_CODE == null) ? "" : Data.KOMTEK_CODE));
                helper.Replace("{Jenis_Perumusan}", ((Data.PROPOSAL_JENIS_PERUMUSAN_NAME == null) ? "-" : Data.PROPOSAL_JENIS_PERUMUSAN_NAME));
                if (Data.PROPOSAL_JENIS_PERUMUSAN == 2)//Revisi
                {
                    var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + PROPOSAL_ID + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
                    string PROPOSAL_REVISI_NOMOR_JUDUL = "";
                    if (RevisiList.Count > 0)
                    {
                        foreach (var ad in RevisiList)
                        {
                            PROPOSAL_REVISI_NOMOR_JUDUL += ad.TEXT + ", ";
                        }
                    }

                    if (PROPOSAL_REVISI_NOMOR_JUDUL != "")
                    {
                        helper.Replace("{No_SNI}", PROPOSAL_REVISI_NOMOR_JUDUL);
                    }
                }
                else if (Data.PROPOSAL_JENIS_PERUMUSAN == 3)//Ralat
                {
                    helper.Replace("{No_SNI}", ((Data.PROPOSAL_RALAT_NOMOR == null) ? "-" : Data.PROPOSAL_RALAT_NOMOR));
                }
                else if (Data.PROPOSAL_JENIS_PERUMUSAN == 4)//Amandemen
                {
                    helper.Replace("{No_SNI}", ((Data.PROPOSAL_AMD_NOMOR == null) ? "-" : Data.PROPOSAL_AMD_NOMOR));
                }
                else if (Data.PROPOSAL_JENIS_PERUMUSAN == 5)//Terjemahan
                {
                    helper.Replace("{No_SNI}", ((Data.PROPOSAL_TERJEMAHAN_NOMOR == null) ? "-" : Data.PROPOSAL_TERJEMAHAN_NOMOR));
                }
                else
                {
                    helper.Replace("{No_SNI}", "");
                }
                
                helper.Replace("{Jalur_Perumusan}", ((Data.PROPOSAL_JALUR_NAME == null) ? "-" : Data.PROPOSAL_JALUR_NAME +" "+ Data.PROPOSAL_JENIS_ADOPSI_NAME));

                var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == PROPOSAL_ID orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
                string PROPOSAL_ADOPSI_NOMOR_JUDUL = "";
                if (AdopsiList.Count > 0)
                {
                    foreach (var ad in AdopsiList)
                    {
                        PROPOSAL_ADOPSI_NOMOR_JUDUL += ad.PROPOSAL_ADOPSI_NOMOR_JUDUL + ", ";
                    }
                }

                helper.Replace("{No_Standar}", ((PROPOSAL_ADOPSI_NOMOR_JUDUL == "") ? "-" : PROPOSAL_ADOPSI_NOMOR_JUDUL));
                helper.Replace("{Judul_PNPS}", ((Data.PROPOSAL_JUDUL_PNPS == null) ? "" : Data.PROPOSAL_JUDUL_PNPS));
                helper.Replace("{Judul_RASNI}", ((Data.PROPOSAL_JUDUL_SNI_PROPOSAL == null) ? "" : Data.PROPOSAL_JUDUL_SNI_PROPOSAL));
                if(Dt_Monitor.MONITORING_TGL_APP_PUB_PNPS == null)
                {
                    helper.Replace("{Tgl_Penetapan_PNPS}", "-");
                } else
                {
                    string MONITORING_TGL_APP_PUB_PNPS = Convert.ToDateTime(Dt_Monitor.MONITORING_TGL_APP_PUB_PNPS).ToString("dd-MM-yyyy");
                    helper.Replace("{Tgl_Penetapan_PNPS}", MONITORING_TGL_APP_PUB_PNPS);
                }
                

                var ListTgl = db.Database.SqlQuery<TRX_PROPOSAL_APPROVAL>("SELECT AA.* FROM TRX_PROPOSAL_APPROVAL AA WHERE AA.APPROVAL_PROPOSAL_ID = '" + PROPOSAL_ID + "' ORDER BY AA.APPROVAL_ID ASC").ToList();
                //if (ListTgl.Count > 0)
                //{
                //    foreach (var ad in ListTgl)
                //    {
                //        //PROPOSAL_ADOPSI_NOMOR_JUDUL += ad.PROPOSAL_ADOPSI_NOMOR_JUDUL + ", ";
                //        var dt_Tgl = Convert.ToDateTime(ad.APPROVAL_DATE);
                //        string time_Tgl = dt.ToString("hh:mm tt");
                //        string PROPOSAL_CREATE_DATE_Tgl = Convert.ToDateTime(ad.APPROVAL_DATE).ToString("dd-MM-yyyy");
                //        switch (Convert.ToInt32(ad.APPROVAL_STATUS_PROPOSAL))
                //        {
                //            case 3:
                //                helper.Replace("{Tgl_Penetapan_PNPS}", PROPOSAL_CREATE_DATE_Tgl);
                //                break;
                //            default:
                //                break;
                //        }
                //    }
                //}

                var ListTglPolling = db.Database.SqlQuery<VIEW_POLLING>("SELECT AA.* FROM VIEW_POLLING AA WHERE AA.POLLING_PROPOSAL_ID = '" + PROPOSAL_ID + "' ORDER BY AA.POLLING_ID ASC").ToList();
                if (ListTglPolling.Count > 0)
                {
                    foreach (var pol in ListTglPolling)
                    {
                        //PROPOSAL_ADOPSI_NOMOR_JUDUL += ad.PROPOSAL_ADOPSI_NOMOR_JUDUL + ", ";
                        string Polling_Tgl_Mulai = Convert.ToDateTime(pol.POLLING_START_DATE).ToString("dd-MM-yyyy");
                        string Polling_Tgl_Akhir = Convert.ToDateTime(pol.POLLING_END_DATE).ToString("dd-MM-yyyy");
                        switch (Convert.ToInt32(pol.POLLING_TYPE))
                        {
                            case 7:
                                helper.Replace("{Tgl_JP_Mulai}", Polling_Tgl_Mulai +" s/d "+ Polling_Tgl_Akhir);
                                //helper.Replace("{Tgl_JP_Akhir}", Polling_Tgl_Akhir);
                                break;
                            case 12:
                                helper.Replace("{Tgl_JPU_Mulai}", Polling_Tgl_Mulai + " s/d " + Polling_Tgl_Akhir);
                                //helper.Replace("{Tgl_JPU_Akhir}", Polling_Tgl_Akhir);
                                break;
                            default:
                                break;
                        }
                    }
                }
                helper.Replace("{Tgl_JP_Mulai}", "Tidak Dilakukan");
                //helper.Replace("{Tgl_JP_Akhir}", "-");
                helper.Replace("{Tgl_JPU_Mulai}", "Tidak Dilakukan");
                //helper.Replace("{Tgl_JPU_Akhir}", "-");

                //Tanggal Rapat
                var DetailHistoryRatek = db.Database.SqlQuery<VIEW_PROPOSAL_RAPAT>("SELECT * FROM VIEW_PROPOSAL_RAPAT WHERE PROPOSAL_RAPAT_PROPOSAL_ID = " + PROPOSAL_ID + " AND PROPOSAL_RAPAT_PROPOSAL_STATUS IN (5,6) ").ToList();
                string Tgl_Rapat_Teknis = "";
                int No_Rapat_Teknis = 0;
                string Tgl_Rapat_Konsensus = "";
                int No_Rapat_Konsensus = 0;
                foreach (var i in DetailHistoryRatek)
                {
                    if (i.PROPOSAL_RAPAT_PROPOSAL_STATUS == 5)//jika Rapat Teknis
                    {
                        No_Rapat_Teknis++;
                        Tgl_Rapat_Teknis += No_Rapat_Teknis +". Rapat Teknis ke " + i.PROPOSAL_RAPAT_VERSION + ", Tanggal" + Convert.ToDateTime(i.PROPOSAL_RAPAT_DATE).ToString("dd MMM yyyy")+", Hasil"+ i.PROPOSAL_RAPAT_STATUS_APPROVAL + System.Environment.NewLine;
                    }
                    else //jika Rapat Konsensus
                    {
                        No_Rapat_Konsensus++;
                        Tgl_Rapat_Konsensus += No_Rapat_Konsensus + ". Rapat Konsensus ke " + i.PROPOSAL_RAPAT_VERSION + ", Tanggal" + Convert.ToDateTime(i.PROPOSAL_RAPAT_DATE).ToString("dd MMM yyyy") + ", Hasil" + i.PROPOSAL_RAPAT_STATUS_APPROVAL + System.Environment.NewLine;
                    }

                }
                
                if (Tgl_Rapat_Teknis != "")
                {
                    helper.Replace("{Tgl_RASNI}", Tgl_Rapat_Teknis);
                }
                if (Tgl_Rapat_Konsensus != "")
                {
                    helper.Replace("{Tgl_Rapat_Konsensus}", Tgl_Rapat_Konsensus);
                }

                var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();
                
                MemoryStream dstStream = new MemoryStream();

                var mime = "";
                if (Type == "pdf")
                {
                    doc.Save(dstStream, SaveFormat.Pdf);
                    mime = "application/pdf";
                }
                else
                {
                    doc.Save(dstStream, SaveFormat.Docx);
                    mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                }

                byte[] byteInfo = dstStream.ToArray();
                dstStream.Write(byteInfo, 0, byteInfo.Length);
                dstStream.Position = 0;

                Response.ContentType = mime;
                Response.AddHeader("content-disposition", "attachment;  filename=TEMPLATE_HISTORY_USULAN." + Type);
                Response.BinaryWrite(byteInfo);
                Response.End();
                return new FileStreamResult(dstStream, mime);
            }
            else
            {
                return Json(new { result = Data }, JsonRequestBehavior.AllowGet);
            }
        }


        public ActionResult FORMULIR_PENGAJUAN_USULAN_PERUMUSAN_SNI(string Type = "docx", int PROPOSAL_ID = 0)
        {
            var Data = db.Database.SqlQuery<VIEW_PROPOSAL>("SELECT * FROM VIEW_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();


            if (Data != null)
            {
                string dataDir = Server.MapPath("~/Format/Laporan/");
                Stream stream = System.IO.File.OpenRead(dataDir + "FORMULIR_PENGAJUAN_USULAN_PERUMUSAN_SNI.docx");

                Aspose.Words.Document doc = new Aspose.Words.Document(stream);
                stream.Close();
                ReplaceHelper helper = new ReplaceHelper(doc);


                var dt = Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE);
                string time = dt.ToString("hh:mm tt");
                string PROPOSAL_CREATE_DATE = Convert.ToDateTime(Data.PROPOSAL_CREATE_DATE).ToString("dd-MM-yyyy");
                helper.Replace("data_tgl", PROPOSAL_CREATE_DATE);
                helper.Replace("data_01", Data.KOMTEK_CODE + " " + Data.KOMTEK_NAME);
                helper.Replace("data_02", ((Data.PROPOSAL_KONSEPTOR == null) ? "" : Data.PROPOSAL_KONSEPTOR));
                helper.Replace("data_03", ((Data.PROPOSAL_INSTITUSI == null) ? "" : Data.PROPOSAL_INSTITUSI));
                helper.Replace("data_04", ((Data.PROPOSAL_TIM_NAMA == null) ? "" : Data.PROPOSAL_TIM_NAMA));
                helper.Replace("data_05", ((Data.PROPOSAL_INSTITUSI == null) ? "" : Data.PROPOSAL_INSTITUSI));
                helper.Replace("data_06", ((Data.PROPOSAL_TIM_ALAMAT == null) ? "" : Data.PROPOSAL_TIM_ALAMAT));
                helper.Replace("data_07", ((Data.PROPOSAL_TIM_PHONE == null) ? "" : Data.PROPOSAL_TIM_PHONE));
                helper.Replace("data_08", ((Data.PROPOSAL_TIM_EMAIL == null) ? "" : Data.PROPOSAL_TIM_EMAIL));
                helper.Replace("data_09", ((Data.PROPOSAL_TIM_FAX == null) ? "" : Data.PROPOSAL_TIM_FAX));
                helper.Replace("data_10", ((Data.PROPOSAL_JUDUL_PNPS == null) ? "" : Data.PROPOSAL_JUDUL_PNPS));
                helper.Replace("data_11", ((Data.PROPOSAL_RUANG_LINGKUP == null) ? "" : Data.PROPOSAL_RUANG_LINGKUP));
                //helper.Replace("data_12", ((Data.PROPOSAL_JALUR == 1) ? "✔  Ya" : "✘  Tidak"));
                var AdopsiList = (from an in db.TRX_PROPOSAL_ADOPSI where an.PROPOSAL_ADOPSI_PROPOSAL_ID == PROPOSAL_ID orderby an.PROPOSAL_ADOPSI_NOMOR_JUDUL ascending select an).ToList();
                var RevisiList = db.Database.SqlQuery<VIEW_SNI_SELECT>("SELECT BB.* FROM TRX_PROPOSAL_REV AA INNER JOIN VIEW_SNI_SELECT BB ON AA.PROPOSAL_REV_MERIVISI_ID = BB.ID WHERE AA.PROPOSAL_REV_PROPOSAL_ID = '" + PROPOSAL_ID + "' ORDER BY AA.PROPOSAL_REV_ID ASC").ToList();
                string PROPOSAL_ADOPSI_NOMOR_JUDUL = "";
                if (AdopsiList.Count > 0)
                {
                    foreach (var ad in AdopsiList)
                    {
                        PROPOSAL_ADOPSI_NOMOR_JUDUL += ad.PROPOSAL_ADOPSI_NOMOR_JUDUL + ", ";
                    }
                }
                string PROPOSAL_REVISI_NOMOR_JUDUL = "";
                if (RevisiList.Count > 0)
                {
                    foreach (var ad in RevisiList)
                    {
                        PROPOSAL_REVISI_NOMOR_JUDUL += ad.TEXT + ", ";
                    }
                }

                if (Data.PROPOSAL_JENIS_PERUMUSAN == 1)
                {
                    helper.Replace("data_12", "✔ Ya");
                    helper.Replace("data_13", "");
                    helper.Replace("data_14", "");
                    helper.Replace("data_15", "");
                    helper.Replace("data_terjemahan", "");
                }
                else if (Data.PROPOSAL_JENIS_PERUMUSAN == 2)
                {
                    helper.Replace("data_12", "");
                    helper.Replace("data_13", "✔ Ya");
                    helper.Replace("data_14", "");
                    helper.Replace("data_15", "");
                    helper.Replace("data_terjemahan", "");
                }
                else if (Data.PROPOSAL_JENIS_PERUMUSAN == 3)
                {
                    helper.Replace("data_12", "");
                    helper.Replace("data_13", "");
                    helper.Replace("data_14", "✔ Ya");
                    helper.Replace("data_15", "");
                    helper.Replace("data_terjemahan", "");
                }
                else if (Data.PROPOSAL_JENIS_PERUMUSAN == 4)
                {
                    helper.Replace("data_12", "");
                    helper.Replace("data_13", "");
                    helper.Replace("data_14", "");
                    helper.Replace("data_15", "✔ Ya");
                    helper.Replace("data_terjemahan", "");
                }
                else if (Data.PROPOSAL_JENIS_PERUMUSAN == 5)
                {
                    helper.Replace("data_12", "");
                    helper.Replace("data_13", "");
                    helper.Replace("data_14", "");
                    helper.Replace("data_15", "");
                    helper.Replace("data_terjemahan", "✔ Ya");
                }
                else
                {
                    helper.Replace("data_12", "");
                    helper.Replace("data_13", "");
                    helper.Replace("data_14", "");
                    helper.Replace("data_15", "");
                    helper.Replace("data_terjemahan", "");
                }
                if (Data.PROPOSAL_JALUR == 1)
                {
                    helper.Replace("data_16", "✔ Ya");
                    helper.Replace("data_17", "");
                    helper.Replace("data_23", "");
                    helper.Replace("data_21", "");
                    helper.Replace("data_24", "");
                }
                else if (Data.PROPOSAL_JALUR == 2)
                {
                    if (Data.PROPOSAL_JENIS_ADOPSI == 1)
                    {
                        helper.Replace("data_16", "");
                        helper.Replace("data_17", "✔ Ya");
                        helper.Replace("data_23", "");
                        helper.Replace("data_21", PROPOSAL_ADOPSI_NOMOR_JUDUL);
                        helper.Replace("data_24", "");
                    }
                    else if (Data.PROPOSAL_JENIS_ADOPSI == 2) {
                        helper.Replace("data_16", "");
                        helper.Replace("data_17", "");
                        helper.Replace("data_23", "✔ Ya");
                        helper.Replace("data_21", "");
                        helper.Replace("data_24", PROPOSAL_ADOPSI_NOMOR_JUDUL);

                    }
                }
                else
                {
                    helper.Replace("data_16", "");
                    helper.Replace("data_17", "");
                    helper.Replace("data_21", "");
                    helper.Replace("data_23", "");
                    helper.Replace("data_24", "");
                }
                if (Data.PROPOSAL_JENIS_PERUMUSAN == 5)
                {
                    helper.Replace("data_22", Data.PROPOSAL_TERJEMAHAN_NOMOR + ", " + Data.PROPOSAL_TERJEMAHAN_JUDUL);
                }
                else {
                    helper.Replace("data_22", "");
                }
                
                if (Data.PROPOSAL_JALUR == 2)
                {
                    if (Data.PROPOSAL_METODE_ADOPSI == 1)
                    {
                        helper.Replace("data_18", "✔ Ya");
                        helper.Replace("data_19", "");
                        helper.Replace("data_20", "");
                    }
                    else if (Data.PROPOSAL_METODE_ADOPSI == 2)
                    {
                        helper.Replace("data_18", "");
                        helper.Replace("data_19", "✔ Ya");
                        helper.Replace("data_20", "");
                    }
                    else if (Data.PROPOSAL_METODE_ADOPSI == 3)
                    {
                        helper.Replace("data_18", "");
                        helper.Replace("data_19", "");
                        helper.Replace("data_20", "✔ Ya");
                    }
                }
                else
                {
                    helper.Replace("data_18", "");
                    helper.Replace("data_19", "");
                    helper.Replace("data_20", "");
                }
                
                
                helper.Replace("data_25", ((Data.PROPOSAL_IS_URGENT == 1) ? "✔  Ya" : "✘  Tidak"));
                if (PROPOSAL_REVISI_NOMOR_JUDUL != "")
                {
                    helper.Replace("data_26", PROPOSAL_REVISI_NOMOR_JUDUL);
                }
                else
                {
                    helper.Replace("data_26", "");
                }
                if (Data.PROPOSAL_AMD_SNI_ID != null)
                {
                    helper.Replace("data_26", Data.PROPOSAL_AMD_NOMOR + " " + Data.PROPOSAL_AMD_JUDUL);
                }
                else
                {
                    helper.Replace("data_26", "");
                }
                if (Data.PROPOSAL_RALAT_SNI_ID != null)
                {
                    if (Data.PROPOSAL_JENIS_PERUMUSAN != 5)
                    {
                        helper.Replace("data_26", Data.PROPOSAL_RALAT_NOMOR + " " + Data.PROPOSAL_RALAT_JUDUL);
                    }
                }
                else
                {
                    helper.Replace("data_26", "");
                }

                helper.Replace("data_27", ((Data.PROPOSAL_PASAL == null) ? "" : Data.PROPOSAL_PASAL));

                if (Data.PROPOSAL_IS_HAK_PATEN == 1)
                {
                    helper.Replace("data_28", "✔");
                    helper.Replace("data_29", "");
                }
                else if (Data.PROPOSAL_IS_HAK_PATEN == 0)
                {
                    helper.Replace("data_28", "");
                    helper.Replace("data_29", "✔");
                }
                else {
                    helper.Replace("data_28", "");
                    helper.Replace("data_29", "");
                }
                helper.Replace("data_30", ((Data.PROPOSAL_IS_HAK_PATEN_DESC == null) ? "" : Data.PROPOSAL_IS_HAK_PATEN_DESC));
                helper.Replace("data_31", ((Data.PROPOSAL_TUJUAN == null) ? "" : Data.PROPOSAL_TUJUAN));
                helper.Replace("data_32", ((Data.PROPOSAL_PROGRAM_PEMERINTAH == null) ? "" : Data.PROPOSAL_PROGRAM_PEMERINTAH));
                helper.Replace("data_33", ((Data.PROPOSAL_PIHAK_BERKEPENTINGAN == null) ? "" : Data.PROPOSAL_PIHAK_BERKEPENTINGAN.Replace("|", ", ")));
                helper.Replace("data_34", ((Data.PROPOSAL_INFORMASI == null) ? "" : Data.PROPOSAL_INFORMASI));
                helper.Replace("data_35", ((Data.PROPOSAL_IS_ORG_MENDUKUNG==1)?"Ya":"Tidak"));
                helper.Replace("data_36", ((Data.PROPOSAL_RETEK_ID == null)?"-":Data.PROPOSAL_RETEK_ID));
                helper.Replace("data_37", ((Data.PROPOSAL_LPK_ID == null)?"-":Data.PROPOSAL_LPK_ID));
                helper.Replace("data_38", ((Data.PROPOSAL_IS_DUPLIKASI_DESC == null)?"-":Data.PROPOSAL_IS_DUPLIKASI_DESC));
                var Outline = db.Database.SqlQuery<VIEW_DOCUMENTS>("SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 10 AND DOC_RELATED_ID = " + PROPOSAL_ID + " AND DOC_RELATED_TYPE = 36").FirstOrDefault();
                
                helper.Replace("data_39", ((Outline==null)?"":"Terlampir"));
                helper.Replace("data_40", ((Outline == null) ? "" : PROPOSAL_CREATE_DATE));
                MemoryStream dstStream = new MemoryStream();

                var mime = "";
                if (Type == "pdf")
                {
                    doc.Save(dstStream, SaveFormat.Pdf);
                    mime = "application/pdf";
                }
                else
                {
                    doc.Save(dstStream, SaveFormat.Docx);
                    mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                }

                byte[] byteInfo = dstStream.ToArray();
                dstStream.Write(byteInfo, 0, byteInfo.Length);
                dstStream.Position = 0;

                Response.ContentType = mime;
                Response.AddHeader("content-disposition", "attachment;  filename=FORMULIR_PENGAJUAN_USULAN_PERUMUSAN_SNI." + Type);
                Response.BinaryWrite(byteInfo);
                Response.End();
                return new FileStreamResult(dstStream, mime);
            }
            else
            {
                return Json(new { result = Data }, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult GETDATARSNI1(string Type = "docx", int PROPOSAL_ID = 0, int RSNI_TYPE = 1)
        {
            var DATAPROPOSAL = db.Database.SqlQuery<TRX_PROPOSAL>("SELECT * FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            var QueryGetSNI = "";
            if (RSNI_TYPE == 1)
            {
                QueryGetSNI = "SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 12 AND DOC_RELATED_TYPE = 3 AND DOC_STATUS = 1";
            }
            else if (RSNI_TYPE == 2)
            {
                QueryGetSNI = "SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 13 AND DOC_RELATED_TYPE = 7 AND DOC_STATUS = 1";
            }
            else if (RSNI_TYPE == 3)
            {
                QueryGetSNI = "SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 14 AND DOC_RELATED_TYPE = 11 AND DOC_STATUS = 1";
            }
            else if (RSNI_TYPE == 4)
            {
                QueryGetSNI = "SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 15 AND DOC_RELATED_TYPE = 17 AND DOC_STATUS = 1";
            }
            else if (RSNI_TYPE == 5)
            {
                QueryGetSNI = "SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 16 AND DOC_RELATED_TYPE = 15 AND DOC_STATUS = 1";
            }
            else if (RSNI_TYPE == 6)
            {
                QueryGetSNI = "SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 17 AND DOC_RELATED_TYPE = 23 AND DOC_STATUS = 1";
            }
            else if (RSNI_TYPE == 7)
            {
                QueryGetSNI = "SELECT * FROM VIEW_DOCUMENTS WHERE DOC_FOLDER_ID = 18 AND DOC_RELATED_TYPE = 18 AND DOC_STATUS = 1";
            }
            var DefaultDokumen = db.Database.SqlQuery<VIEW_DOCUMENTS>(QueryGetSNI).FirstOrDefault();
            if (DefaultDokumen != null)
            {
                string path = Server.MapPath("~" + DefaultDokumen.DOC_FILE_PATH + "" + DefaultDokumen.DOC_FILE_NAME + "." + DefaultDokumen.DOC_FILETYPE);
                Stream stream = System.IO.File.OpenRead(path);


                //text = System.IO.File.ReadAllText(@"" + path);
                Aspose.Words.Document doc = new Aspose.Words.Document(stream);
                stream.Close();

                MemoryStream dstStream = new MemoryStream();

                var mime = "";
                if (Type == "pdf")
                {
                    doc.Save(dstStream, SaveFormat.Pdf);
                    mime = "application/pdf";
                }
                else
                {
                    doc.Save(dstStream, SaveFormat.Docx);
                    mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                }

                byte[] byteInfo = dstStream.ToArray();
                dstStream.Write(byteInfo, 0, byteInfo.Length);
                dstStream.Position = 0;

                Response.ContentType = mime;
                Response.AddHeader("content-disposition", "attachment;  filename=RSNI" + Convert.ToString(RSNI_TYPE) + "_" + DATAPROPOSAL.PROPOSAL_JUDUL_PNPS.Replace(" ", "_").ToUpper() + "." + Type);
                Response.BinaryWrite(byteInfo);
                Response.End();
                return new FileStreamResult(dstStream, mime);
            }
            else
            {
                string dataDir = Server.MapPath("~/Upload/Usulan/");
                Stream stream = System.IO.File.OpenRead(dataDir + "blank.docx");

                Aspose.Words.Document doc = new Aspose.Words.Document(stream);
                stream.Close();

                MemoryStream dstStream = new MemoryStream();

                var mime = "";
                if (Type == "pdf")
                {
                    doc.Save(dstStream, SaveFormat.Pdf);
                    mime = "application/pdf";
                }
                else
                {
                    doc.Save(dstStream, SaveFormat.Docx);
                    mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                }

                byte[] byteInfo = dstStream.ToArray();
                dstStream.Write(byteInfo, 0, byteInfo.Length);
                dstStream.Position = 0;

                Response.ContentType = mime;
                Response.AddHeader("content-disposition", "attachment;  filename=RSNI" + Convert.ToString(RSNI_TYPE) + "_" + DATAPROPOSAL.PROPOSAL_JUDUL_PNPS.Replace(" ", "_").ToUpper() + "." + Type);
                Response.BinaryWrite(byteInfo);
                Response.End();
                return new FileStreamResult(dstStream, mime);
            }

        }
        public ActionResult GETDATARSNI2(string Type = "docx", int PROPOSAL_ID = 0)
        {
            var DATAPROPOSAL = db.Database.SqlQuery<TRX_PROPOSAL>("SELECT * FROM TRX_PROPOSAL WHERE PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            var SNI_DOC_VERSION = db.Database.SqlQuery<decimal>("SELECT CAST(NVL(MAX(T1.SNI_DOC_VERSION),0) AS NUMBER) AS SNI_DOC_VERSION FROM TRX_SNI_DOC T1 WHERE T1.SNI_DOC_PROPOSAL_ID = " + PROPOSAL_ID + " AND T1.SNI_DOC_TYPE = 2").SingleOrDefault();
            if (SNI_DOC_VERSION != 0)
            {
                string dataDir = Server.MapPath("~/Upload/Usulan/" + PROPOSAL_ID + "/");
                Stream stream = System.IO.File.OpenRead(dataDir + "DOC_DATA_RSNI2_VER" + SNI_DOC_VERSION + ".docx");

                Aspose.Words.Document doc = new Aspose.Words.Document(stream);
                stream.Close();

                MemoryStream dstStream = new MemoryStream();

                var mime = "";
                if (Type == "pdf")
                {
                    doc.Save(dstStream, SaveFormat.Pdf);
                    mime = "application/pdf";
                }
                else
                {
                    doc.Save(dstStream, SaveFormat.Docx);
                    mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                }

                byte[] byteInfo = dstStream.ToArray();
                dstStream.Write(byteInfo, 0, byteInfo.Length);
                dstStream.Position = 0;

                Response.ContentType = mime;
                Response.AddHeader("content-disposition", "attachment;  filename=RSNI2_" + DATAPROPOSAL.PROPOSAL_JUDUL_PNPS.Replace(" ", "_").ToUpper() + "." + Type);
                Response.BinaryWrite(byteInfo);
                Response.End();
                return new FileStreamResult(dstStream, mime);
            }
            else
            {
                string dataDir = Server.MapPath("~/Upload/Usulan/");
                Stream stream = System.IO.File.OpenRead(dataDir + "blank.docx");

                Aspose.Words.Document doc = new Aspose.Words.Document(stream);
                stream.Close();

                MemoryStream dstStream = new MemoryStream();

                var mime = "";
                if (Type == "pdf")
                {
                    doc.Save(dstStream, SaveFormat.Pdf);
                    mime = "application/pdf";
                }
                else
                {
                    doc.Save(dstStream, SaveFormat.Docx);
                    mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                }

                byte[] byteInfo = dstStream.ToArray();
                dstStream.Write(byteInfo, 0, byteInfo.Length);
                dstStream.Position = 0;

                Response.ContentType = mime;
                Response.AddHeader("content-disposition", "attachment;  filename=RSNI2_" + DATAPROPOSAL.PROPOSAL_JUDUL_PNPS.Replace(" ", "_").ToUpper() + "." + Type);
                Response.BinaryWrite(byteInfo);
                Response.End();
                return new FileStreamResult(dstStream, mime);
            }

        }
        public string ConvertTanggal(DateTime tanggal, string tipe = "")
        {
            var res = "";
            var AngkaBulan = tanggal.ToString("MM");
            var NamaHariEng = tanggal.ToString("dddd");
            var AngkaHari = tanggal.ToString("dd");
            var Tahun = tanggal.ToString("yyyy");
            var Bulan = "";
            var Hari = "";
            switch (NamaHariEng)
            {
                case "Sunday":
                    Hari = "Minggu";
                    break;
                case "Monday":
                    Hari = "Senin";
                    break;
                case "Tuesday":
                    Hari = "Selasa";
                    break;
                case "Wednesday":
                    Hari = "Rabu";
                    break;
                case "Thursday":
                    Hari = "Kamis";
                    break;
                case "Friday":
                    Hari = "Jumat";
                    break;
                case "Saturday":
                    Hari = "Sabtu";
                    break;
                default:
                    return "";
            }
            switch (AngkaBulan)
            {
                case "01":
                    Bulan = "Januari";
                    break;
                case "02":
                    Bulan = "Februari";
                    break;
                case "03":
                    Bulan = "Maret";
                    break;
                case "04":
                    Bulan = "April";
                    break;
                case "05":
                    Bulan = "Mei";
                    break;
                case "06":
                    Bulan = "Juni";
                    break;
                case "07":
                    Bulan = "Juli";
                    break;
                case "08":
                    Bulan = "Agustus";
                    break;
                case "09":
                    Bulan = "September";
                    break;
                case "10":
                    Bulan = "Oktober";
                    break;
                case "11":
                    Bulan = "November";
                    break;
                case "12":
                    Bulan = "Desember";
                    break;
                default:
                    return "";
            }
            if (tipe == "full")
            {
                res = AngkaHari + " " + Bulan + " " + Tahun;
            }
            else if (tipe == "namabulan")
            {
                res = Bulan;
            }
            else if (tipe == "angkabulan")
            {
                res = AngkaBulan;
            }
            else if (tipe == "tahun")
            {
                res = Tahun;
            }
            else if (tipe == "namahari")
            {
                res = Hari;
            }
            else if (tipe == "angkahari")
            {
                res = AngkaHari;
            }
            return res;
        }
    }
}
