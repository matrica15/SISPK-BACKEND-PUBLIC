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
using System.Collections;
using System;
using Aspose.Words.Layout;
using Aspose.Words.Fields;
using Aspose.Words.Saving;
using Aspose.Words.Properties;
using SISPK.Filters;
using System.Text;

namespace SISPK.Controllers.Download
{
    public class FilesController : Controller
    {
        private SISPKEntities db = new SISPKEntities();
        [AllowAnonymous]
        [HttpGet]
        public ActionResult SNIInternal(int PROPOSAL_ID = 0, int ACCESS_ID = 0,string TYPE = "pdf")
        {
            TYPE = TYPE.ToLower();
            var Data = db.Database.SqlQuery<VIEW_SNI>("SELECT * FROM VIEW_SNI WHERE SNI_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
            
            if (Data.DSNI_DOC_FILETYPE.ToLower() == "docx" ||  Data.DSNI_DOC_FILETYPE.ToLower() == "doc")
            {
                string dataDir = Server.MapPath("~" + Data.DSNI_DOC_FILE_PATH);
                Stream stream = System.IO.File.OpenRead(dataDir + "" + Data.DSNI_DOC_FILE_NAME + "." + Data.DSNI_DOC_FILETYPE.ToLower());
                var IS_LIMIT_DOWNLOAD = Data.IS_LIMIT_DOWNLOAD;
                Aspose.Words.Document doc = new Aspose.Words.Document(stream);
                var USER_FULL_NAME = ((Convert.ToString(Session["USER_FULL_NAME"]) == "") ? "-" : Convert.ToString(Session["USER_FULL_NAME"]));
                var ACCESS_NAME = ((Convert.ToString(Session["ACCESS_NAME"]) == "") ? "-" : Convert.ToString(Session["ACCESS_NAME"]));

                var STYLE_LIST = db.Database.SqlQuery<string>("SELECT * FROM VIEW_SNI_STYLE_GROUP").SingleOrDefault();
                var AksesSelect = db.Database.SqlQuery<SYS_DOC_ACCESS_DETAIL_SELECT>("SELECT * FROM SYS_DOC_ACCESS_DETAIL_SELECT WHERE DOC_ACCESS_DETAIL_STYLE_STATUS = 0 AND DOC_ACCESS_DETAIL_ACCESS_ID = " + ACCESS_ID + " ORDER BY SNI_STYLE_SORT ASC").ToList();


                var kacang = ((STYLE_LIST != null) ? STYLE_LIST : "");
                if (AksesSelect.Count() > 0)
                {
                    foreach (var i in AksesSelect)
                    {
                        // Get a collection of all paragraph nodes in the document
                        Node[] paragraphs = doc.GetChildNodes(NodeType.Paragraph, true).ToArray();
                        ArrayList nodesToBeDeleted = new ArrayList();
                        bool isParagraphFound = false;
                        var SNI_STYLE_VALUE = Convert.ToString(i.SNI_STYLE_VALUE);
                        var SNI_STYLE_NAME = Convert.ToString(i.SNI_STYLE_NAME);
                        foreach (Paragraph paragraph in paragraphs)
                        {
                            if (paragraph.ParagraphFormat.Style.Name == SNI_STYLE_VALUE)
                            {
                                // Filter Heading 1 paras and find one that contains the search string
                                //if (paragraph.Range.Text.ToLower().StartsWith(SNI_STYLE_NAME.ToLower()))
                                if (RemoveSpecialCharacters(paragraph.Range.Text.ToLower()).StartsWith(RemoveSpecialCharacters(SNI_STYLE_NAME).ToLower()))
                                {
                                    isParagraphFound = true;
                                    // We need to delete all nodes present in between the startPara node
                                    // and the next Paragraph with Heading 1
                                    Paragraph startPara = paragraph;

                                    nodesToBeDeleted.Add(startPara);
                                    startPara = startPara.NextSibling as Paragraph;
                                    var styleName = startPara.ParagraphFormat.Style.Name;

                                    try
                                    {
                                        while (!kacang.Contains(startPara.ParagraphFormat.Style.Name) && startPara.ParagraphFormat.Style.Name != "" && startPara.ParagraphFormat.Style.Name != null)
                                        {
                                            nodesToBeDeleted.Add(startPara);
                                            startPara = startPara.NextSibling as Paragraph;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("An error occurred: '{0}'", e);
                                        break;
                                    }

                                    if (isParagraphFound)
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        // Remove all nodes
                        foreach (Node node in nodesToBeDeleted)
                        {
                            node.Remove();

                        }
                    }
                }
                var DataJmlDownload = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 10").FirstOrDefault();
                
                

               
                var DataWatermark = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 9").FirstOrDefault();
                var watermarkText = DataWatermark.CONFIG_VALUE;
                string watermark2Text = "SNI ini di download Oleh " + USER_FULL_NAME + " Sebagai " + ACCESS_NAME;
                Shape watermark = new Shape(doc, ShapeType.TextPlainText);
                watermark.TextPath.Text = watermarkText + "\r\n" + watermark2Text;

                watermark.TextPath.FontFamily = "Arial";
                double fontSize = 11;
                watermark.TextPath.Size = fontSize;
                watermark.Height = fontSize * 2;
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

                foreach (Section sect in doc.Sections)
                {
                    InsertWatermarkIntoHeader(watermarkPara, sect, HeaderFooterType.HeaderPrimary);
                    InsertWatermarkIntoHeader(watermarkPara, sect, HeaderFooterType.HeaderFirst);
                    InsertWatermarkIntoHeader(watermarkPara, sect, HeaderFooterType.HeaderEven);
                }
                stream.Close();
                MemoryStream dstStream = new MemoryStream();
              
              

                var mime = "";
                
                if (TYPE == "doc")
                {
                    mime = "application/msword";
                    doc.Save(dstStream, SaveFormat.Doc);
                  
                }
                else if (TYPE == "docx")
                {
                    mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    doc.Save(dstStream, SaveFormat.Docx);
                }
                else if (TYPE == "pdf")
                {
                    mime = "application/pdf";
                    doc.Save(dstStream, SaveFormat.Pdf);
                }
                else
                {
                    mime = "application/pdf";
                    doc.Save(dstStream, SaveFormat.Pdf);
                }
                    
                

                byte[] byteInfo = dstStream.ToArray();
                dstStream.Write(byteInfo, 0, byteInfo.Length);
                dstStream.Position = 0;

                Response.ContentType = mime;
                Response.AddHeader("content-disposition", "attachment;  filename=" + Data.DSNI_DOC_FILE_NAME + "." + TYPE);
                Response.BinaryWrite(byteInfo);
                Response.End();
                return new FileStreamResult(dstStream, mime);
            }
            else
            {
                var filePath = Server.MapPath("~" + Data.DSNI_DOC_FILE_PATH);
                var fileName = Data.DSNI_DOC_FILE_NAME;
                var fileType = Data.DSNI_DOC_FILETYPE;
                string dataDir = Server.MapPath("~" + Data.DSNI_DOC_FILE_PATH);
                Stream stream = System.IO.File.OpenRead(dataDir + "" + Data.DSNI_DOC_FILE_NAME + "." + Data.DSNI_DOC_FILETYPE);
                Directory.CreateDirectory(Server.MapPath("~/Upload/Temp"));
                var destPath = Server.MapPath("~/Upload/Temp/");
                if (System.IO.File.Exists(destPath + Data.DSNI_DOC_FILE_NAME.Replace("/", "-").Replace(":", "-").Replace("@", "-") + "." + Data.DSNI_DOC_FILETYPE))
                {
                    System.IO.File.Delete(destPath + Data.DSNI_DOC_FILE_NAME.Replace("/", "-").Replace(":", "-").Replace("@", "-") + "." + Data.DSNI_DOC_FILETYPE);
                }
                System.IO.File.Copy(filePath + fileName + "." + fileType, destPath + Data.DSNI_DOC_FILE_NAME.Replace("/", "-").Replace(":", "-").Replace("@", "-") + "." + fileType);
                return Redirect("/Upload/Temp/" + Data.DSNI_DOC_FILE_NAME.Replace("/", "-").Replace(":", "-").Replace("@", "-") + "." + fileType);
            }
            //return Json(new
            //{
            //    Data,
            //    wew = dataDir + "" + Data.DSNI_DOC_FILE_NAME + "." + Data.DSNI_DOC_FILETYPE
            //}, JsonRequestBehavior.AllowGet);
        }

        public static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '.' || c == '_' || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }


        public void RemoveSequence(Node start, Node end)
        {
            Node curNode = start.NextPreOrder(start.Document);
            while (curNode != null && !curNode.Equals(end))
            {
                //Move to next node
                Node nextNode = curNode.NextPreOrder(start.Document);

                //Check whether current contains end node
                if (curNode.IsComposite)
                {
                    CompositeNode curComposite = (CompositeNode)curNode;
                    if (!curComposite.GetChildNodes(NodeType.Any, true).Contains(end) &&
                        !curComposite.GetChildNodes(NodeType.Any, true).Contains(start))
                    {
                        nextNode = curNode.NextSibling;
                        curNode.Remove();
                    }
                }
                else
                {
                    curNode.Remove();
                }

                curNode = nextNode;
            }
        }


        [AllowAnonymous]
        [HttpGet]
        public ActionResult Index(int fid = 0, string token_key = "", int uid = 0)
        {
            var PROPOSAL_ID = fid;
            var USER_ID = uid;
            
            var DataUserDownload = db.Database.SqlQuery<VIEW_USERS_PUBLIC>("SELECT * FROM VIEW_USERS_PUBLIC WHERE USER_ID = " + USER_ID + " AND USER_PUBLIC_ACTIVATION_KEY = '" + token_key + "'").FirstOrDefault();
            if (DataUserDownload != null)
            {
                var Data = db.Database.SqlQuery<VIEW_SNI>("SELECT * FROM VIEW_SNI WHERE SNI_PROPOSAL_ID = " + PROPOSAL_ID).SingleOrDefault();
                if (Data.DSNI_DOC_FILETYPE.ToLower() == "docx" || Data.DSNI_DOC_FILETYPE.ToLower() == "doc")
                {
                    string dataDir = Server.MapPath("~" + Data.DSNI_DOC_FILE_PATH);
                    Stream stream = System.IO.File.OpenRead(dataDir + "" + Data.DSNI_DOC_FILE_NAME + ".DOCX");
                    var IS_LIMIT_DOWNLOAD = Data.IS_LIMIT_DOWNLOAD;
                    Aspose.Words.Document doc = new Aspose.Words.Document(stream);
                    CustomDocumentProperties props = doc.CustomDocumentProperties;
                    var USER_FULL_NAME = ((Convert.ToString(DataUserDownload.USER_PUBLIC_NAMA_LENGKAP) == "") ? "-" : Convert.ToString(DataUserDownload.USER_PUBLIC_NAMA_LENGKAP));
                    var ACCESS_NAME = ((Convert.ToString(DataUserDownload.ACCESS_NAME) == "") ? "-" : Convert.ToString(DataUserDownload.ACCESS_NAME));

                    //query tag doc untuk masyarakat
                    var STYLE_LIST = db.Database.SqlQuery<string>("SELECT * FROM VIEW_SNI_STYLE_GROUP").SingleOrDefault();
                    var AksesSelect = db.Database.SqlQuery<SYS_DOC_ACCESS_DETAIL_SELECT>("SELECT * FROM SYS_DOC_ACCESS_DETAIL_SELECT WHERE DOC_ACCESS_DETAIL_STYLE_STATUS = 0 AND DOC_ACCESS_DETAIL_ACCESS_ID = 3 ORDER BY SNI_STYLE_SORT ASC").ToList();


                    var kacang = ((STYLE_LIST != null) ? STYLE_LIST : "");
                    if (AksesSelect.Count() > 0)
                    {
                        foreach (var i in AksesSelect)
                        {
                            // Get a collection of all paragraph nodes in the document
                            Node[] paragraphs = doc.GetChildNodes(NodeType.Paragraph, true).ToArray();
                            ArrayList nodesToBeDeleted = new ArrayList();
                            bool isParagraphFound = false;
                            var SNI_STYLE_VALUE = Convert.ToString(i.SNI_STYLE_VALUE);
                            var SNI_STYLE_NAME = Convert.ToString(i.SNI_STYLE_NAME);
                            foreach (Paragraph paragraph in paragraphs)
                            {
                                if (paragraph.ParagraphFormat.Style.Name == SNI_STYLE_VALUE)
                                {
                                    // Filter Heading 1 paras and find one that contains the search string
                                    //if (paragraph.Range.Text.ToLower().StartsWith(SNI_STYLE_NAME.ToLower()))
                                    if (RemoveSpecialCharacters(paragraph.Range.Text.ToLower()).StartsWith(RemoveSpecialCharacters(SNI_STYLE_NAME).ToLower()))
                                    {
                                        isParagraphFound = true;
                                        // We need to delete all nodes present in between the startPara node
                                        // and the next Paragraph with Heading 1
                                        Paragraph startPara = paragraph;

                                        nodesToBeDeleted.Add(startPara);
                                        startPara = startPara.NextSibling as Paragraph;
                                        var styleName = startPara.ParagraphFormat.Style.Name;

                                        try
                                        {
                                            while (!kacang.Contains(startPara.ParagraphFormat.Style.Name) && startPara.ParagraphFormat.Style.Name != "" && startPara.ParagraphFormat.Style.Name != null)
                                            {
                                                nodesToBeDeleted.Add(startPara);
                                                startPara = startPara.NextSibling as Paragraph;
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine("An error occurred: '{0}'", e);
                                            break;
                                        }

                                        if (isParagraphFound)
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            // Remove all nodes
                            foreach (Node node in nodesToBeDeleted)
                            {
                                node.Remove();

                            }
                        }
                    }
                    //END query tag doc untuk masyarakat

                    var DataJmlDownload = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 10").FirstOrDefault();
                    var DataWatermark = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 9").FirstOrDefault();
                    PdfSaveOptions options = new Aspose.Words.Saving.PdfSaveOptions();
                    if (IS_LIMIT_DOWNLOAD == 1)
                    {
                        options.PageCount = Convert.ToInt32(DataJmlDownload.CONFIG_VALUE);
                    }

                    var watermarkText = DataWatermark.CONFIG_VALUE;
                    string watermark2Text = "SNI ini di download Oleh " + USER_FULL_NAME + " Sebagai " + ACCESS_NAME;
                    Shape watermark = new Shape(doc, ShapeType.TextPlainText);
                    watermark.TextPath.Text = watermarkText + "\r\n" + watermark2Text;

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
                else
                {
                    var filePath = Server.MapPath("~" + Data.DSNI_DOC_FILE_PATH);
                    var fileName = Data.DSNI_DOC_FILE_NAME;
                    var fileType = "PDF";
                    string dataDir = Server.MapPath("~" + Data.DSNI_DOC_FILE_PATH);
                    Stream stream = System.IO.File.OpenRead(dataDir + "" + Data.DSNI_DOC_FILE_NAME + "." + Data.DSNI_DOC_FILETYPE);
                    Directory.CreateDirectory(Server.MapPath("~/Upload/Temp"));
                    var destPath = Server.MapPath("~/Upload/Temp/");
                    if (System.IO.File.Exists(destPath + Data.DSNI_DOC_FILE_NAME.Replace("/", "-").Replace(":", "-") + "." + Data.DSNI_DOC_FILETYPE))
                    {
                        System.IO.File.Delete(destPath + Data.DSNI_DOC_FILE_NAME.Replace("/", "-").Replace(":", "-") + "." + Data.DSNI_DOC_FILETYPE);
                    }
                    System.IO.File.Copy(filePath + fileName + "." + fileType, destPath + Data.DSNI_DOC_FILE_NAME.Replace("/", "-").Replace(":", "-") + "." + fileType);
                    return Redirect("/Upload/Temp/" + Data.DSNI_DOC_FILE_NAME.Replace("/", "-").Replace(":", "-") + "." + fileType);
                }
            }
            else
            {
                var Link = db.Database.SqlQuery<SYS_LINK>("SELECT * FROM SYS_LINK WHERE LINK_ID = 1 AND LINK_IS_USE = 1 AND LINK_STATUS = 1").SingleOrDefault();

                return Redirect(Link.LINK_NAME + "/auth/index");
            }

            //return Json(new { Data, wew = "SELECT * FROM VIEW_SNI WHERE SNI_PROPOSAL_ID = " + PROPOSAL_ID, dataDir }, JsonRequestBehavior.AllowGet);
        }

        public static ArrayList GetNodesByPage(int page, Aspose.Words.Document document)
        {

            ArrayList nodes = new ArrayList();

            LayoutCollector lc = new LayoutCollector(document);



            foreach (Paragraph para in document.GetChildNodes(NodeType.Paragraph, true))
            {
                if (lc.GetStartPageIndex(para) == page)

                    nodes.Add(para);

            }

            return nodes;

        }
        public static ArrayList ParagraphsByStyleName(Aspose.Words.Document doc, string styleName)
        {
            // Create an array to collect paragraphs of the specified style.
            ArrayList paragraphsWithStyle = new ArrayList();
            // Get all paragraphs from the document.
            NodeCollection paragraphs = doc.GetChildNodes(NodeType.Paragraph, true);
            // Look through all paragraphs to find those with the specified style.
            foreach (Paragraph paragraph in paragraphs)
            {
                if (paragraph.ParagraphFormat.Style.Name == styleName)
                    paragraphsWithStyle.Add(paragraph);
            }
            return paragraphsWithStyle;
        }
        public static ArrayList RunsByStyleName(Aspose.Words.Document doc, string styleName)
        {
            // Create an array to collect runs of the specified style.
            ArrayList runsWithStyle = new ArrayList();
            // Get all runs from the document.
            NodeCollection runs = doc.GetChildNodes(NodeType.Run, true);
            // Look through all runs to find those with the specified style.
            foreach (Run run in runs)
            {
                if (run.Font.Style.Name == styleName)
                    runsWithStyle.Add(run);
            }
            return runsWithStyle;
        }
        private static void RemoveTableOfContents(Aspose.Words.Document doc, int index)
        {
            // Store the FieldStart nodes of TOC fields in the document for quick access.
            ArrayList fieldStarts = new ArrayList();
            // This is a list to store the nodes found inside the specified TOC. They will be removed
            // at the end of this method.
            ArrayList nodeList = new ArrayList();

            foreach (FieldStart start in doc.GetChildNodes(NodeType.FieldStart, true))
            {
                if (start.FieldType == FieldType.FieldTOC)
                {
                    // Add all FieldStarts which are of type FieldTOC.
                    fieldStarts.Add(start);
                }
            }

            // Ensure the TOC specified by the passed index exists.
            if (index > fieldStarts.Count - 1)
                throw new ArgumentOutOfRangeException("TOC index is out of range");

            bool isRemoving = true;
            // Get the FieldStart of the specified TOC.
            Node currentNode = (Node)fieldStarts[index];

            while (isRemoving)
            {
                // It is safer to store these nodes and delete them all at once later.
                nodeList.Add(currentNode);
                currentNode = currentNode.NextPreOrder(doc);

                // Once we encounter a FieldEnd node of type FieldTOC then we know we are at the end
                // of the current TOC and we can stop here.
                if (currentNode.NodeType == NodeType.FieldEnd)
                {
                    FieldEnd fieldEnd = (FieldEnd)currentNode;
                    if (fieldEnd.FieldType == FieldType.FieldTOC)
                        isRemoving = false;
                }
            }

            // Remove all nodes found in the specified TOC.
            foreach (Node node in nodeList)
            {
                node.Remove();
            }
        }
        public ActionResult Okay2()
        {
            // Load document

            Aspose.Words.Document doc = new Aspose.Words.Document(@"C:\Users\Ghany Cahyadi\Desktop\Res\WEW.docx");


            // Keep track if the Paragraph containing the heading text is found

            bool isParagraphFound = false;

            // Stores nodes that are to be removed from document

            ArrayList nodesToBeDeleted = new ArrayList();

            // Get a collection of all paragraph nodes in the document
            Node[] paragraphs = doc.GetChildNodes(NodeType.Paragraph, true).ToArray();

            foreach (Paragraph paragraph in paragraphs)
            {
                if (paragraph.ParagraphFormat.StyleName.Equals("Judul"))
                {
                    // Filter Heading 1 paras and find one that contains the search string
                    if (paragraph.Range.Text.StartsWith("Ruang lingkup"))
                    {
                        isParagraphFound = true;
                        // We need to delete all nodes present in between the startPara node
                        // and the next Paragraph with Heading 1
                        Paragraph startPara = paragraph;
                        do
                        {
                            nodesToBeDeleted.Add(startPara);
                            startPara = startPara.NextSibling as Paragraph;
                        }
                        while (!startPara.ParagraphFormat.StyleName.Equals("Judul"));
                        // Break the main loop as we got the list of nodes to be removed
                        if (isParagraphFound)
                        {
                            break;
                        }
                    }
                }
            }

            // Remove all nodes

            foreach (Node node in nodesToBeDeleted)
            {
                node.Remove();
            }

            doc.UpdateFields();

            var Jml = doc.PageCount;

            doc.Save(@"C:\Users\Ghany Cahyadi\Desktop\Res\out.docx");
            doc.Save(@"C:\Users\Ghany Cahyadi\Desktop\Res\out.xml");
            return Json(new
            {
                nodesToBeDeleted
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Okay()
        {
            // Load document
            var STYLE = db.Database.SqlQuery<MASTER_SNI_STYLE>("SELECT * FROM MASTER_SNI_STYLE WHERE SNI_STYLE_STATUS = 1 ORDER BY SNI_STYLE_SORT ASC").ToList();

            Aspose.Words.Document doc = new Aspose.Words.Document(@"C:\Users\Ghany Cahyadi\Desktop\Res\WEW.docx");


            // Keep track if the Paragraph containing the heading text is found

            bool isParagraphFound = false;

            // Stores nodes that are to be removed from document

            ArrayList nodesToBeDeleted = new ArrayList();

            bool start = true;
            // Get a collection of all paragraph nodes in the document
            Node[] xxxx = doc.GetChildNodes(NodeType.Run, true).ToArray();
            Node[] paragraphs = doc.GetChildNodes(NodeType.Paragraph, true).ToArray();

            foreach (Paragraph paragraph in paragraphs)
            {
                if (paragraph.ParagraphFormat.StyleName.Equals("Judul"))
                {
                    start = paragraph.Range.Text.StartsWith("Prakata");
                    // Filter Heading 1 paras and find one that contains the search string
                    if (paragraph.Range.Text.StartsWith("Prakata"))
                    {
                        isParagraphFound = true;
                        // We need to delete all nodes present in between the startPara node
                        // and the next Paragraph with Heading 1
                        Paragraph startPara = paragraph;
                        do
                        {
                            nodesToBeDeleted.Add(startPara);
                            startPara = startPara.NextSibling as Paragraph;
                        }
                        while (!startPara.ParagraphFormat.StyleName.Equals("Judul"));
                        // Break the main loop as we got the list of nodes to be removed
                        if (isParagraphFound)
                        {
                            break;
                        }
                    }
                }
                //var AksesSelect = db.Database.SqlQuery<SYS_DOC_ACCESS_DETAIL_SELECT>("SELECT * FROM SYS_DOC_ACCESS_DETAIL_SELECT WHERE DOC_ACCESS_DETAIL_STYLE_STATUS = 0 ORDER BY SNI_STYLE_SORT ASC").ToList();
                //// We are interested in only Paragraphs with Heading 1 style
                //if (AksesSelect.Count() > 0)
                //{
                //    foreach (var i in AksesSelect)
                //    {
                //        //TEST += i.SNI_STYLE_NAME +"|"+i.SNI_STYLE_VALUE + "<BR>";
                //        var SNI_STYLE_VALUE = Convert.ToString(i.SNI_STYLE_VALUE);
                //        var SNI_STYLE_NAME = Convert.ToString(i.SNI_STYLE_NAME);
                //        if (paragraph.ParagraphFormat.StyleName.Equals(SNI_STYLE_VALUE))
                //        {

                //            // Filter Heading 1 paras and find one that contains the search string

                //            if (paragraph.Range.Text.StartsWith(SNI_STYLE_NAME))
                //            {

                //                isParagraphFound = true;

                //                // We need to delete all nodes present in between the startPara node

                //                // and the next Paragraph with Heading 1

                //                Paragraph startPara = paragraph;

                //                do
                //                {

                //                    nodesToBeDeleted.Add(startPara);

                //                    startPara = startPara.NextSibling as Paragraph;

                //                }

                //                while (!startPara.ParagraphFormat.StyleName.Equals(SNI_STYLE_VALUE));



                //                // Break the main loop as we got the list of nodes to be removed

                //                if (isParagraphFound)

                //                    break;

                //            }

                //        }
                //    }
                //}




            }



            // Remove all nodes

            foreach (Node node in nodesToBeDeleted)
            {
                node.Remove();
            }

            doc.UpdateFields();

            var Jml = doc.PageCount;

            doc.Save(@"C:\Users\Ghany Cahyadi\Desktop\Res\out.docx");
            doc.Save(@"C:\Users\Ghany Cahyadi\Desktop\Res\out.xml");
            return Json(new
            {
                //TEST,
                //data = nodesToBeDeleted.Count,
                Jml
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index2(int id = 120)
        {
            string dataDir = Server.MapPath("~/Format/Laporan/");
            Stream stream = System.IO.File.OpenRead(dataDir + "SNI.docx");
            Aspose.Words.Document doc = new Aspose.Words.Document(stream);
            stream.Close();
            MemoryStream dstStream = new MemoryStream();
            ReplaceHelper helper = new ReplaceHelper(doc);
            //var mime = "";


            //MessageBox.Show(String.Format("Document has {0} pages", doc.BuiltInDocumentProperties.Pages));
            //helper.Replace("Data1", "wwww");
            StyleCollection styles = doc.Styles;

            //// Iterate through all paragraphs
            //foreach (Paragraph para in paragraphs)
            //{
            //    // If the paragraph has a page break before set then clear it.
            //    if (para.ParagraphFormat.PageBreakBefore)
            //        para.ParagraphFormat.PageBreakBefore = false;

            //    // Check all runs in the paragraph for page breaks and remove them.
            //    foreach (Run run in para.Runs)
            //    {
            //        if (run.Text.Contains(ControlChar.PageBreak))
            //            run.Text = run.Text.Replace(ControlChar.PageBreak, string.Empty);
            //    }

            //}
            // Define style names as they are specified in the Word document.
            const string paraStyle = "Heading 2";
            const string runStyle = "Intense Emphasis";
            var wew = "";
            // Collect paragraphs with defined styles. 
            // Show the number of collected paragraphs and display the text of this paragraphs.
            ArrayList paragraphs = ParagraphsByStyleName(doc, paraStyle);
            Console.WriteLine(string.Format("Paragraphs with \"{0}\" styles ({1}):", paraStyle, paragraphs.Count));
            foreach (Paragraph paragraph in paragraphs)
            {
                Console.Write(paragraph.ToString(SaveFormat.Text));
                wew += paragraph.ToString(SaveFormat.Text);

            }
            helper.Replace("Data1", wew);
            // Collect runs with defined styles. 
            // Show the number of collected runs and display the text of this runs.
            ArrayList runs = RunsByStyleName(doc, runStyle);
            Console.WriteLine(string.Format("\nRuns with \"{0}\" styles ({1}):", runStyle, runs.Count));
            foreach (Run run in runs)
            {
                Console.WriteLine(run.Range.Text);
            }
            doc.ViewOptions.ZoomPercent = 100;


            doc.Save(dstStream, SaveFormat.Docx);
            var mime = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            byte[] byteInfo = dstStream.ToArray();
            dstStream.Write(byteInfo, 0, byteInfo.Length);
            dstStream.Position = 0;

            Response.ContentType = mime;

            Response.AddHeader("content-disposition", "attachment;  filename=SNI_" + id + ".docx");
            Response.BinaryWrite(byteInfo);
            Response.End();

            return new FileStreamResult(dstStream, mime);
            //var JumlahHalaman = doc.BuiltInDocumentProperties.Pages;
            //return Json(new
            //{
            //    JumlahHalaman,
            //    style = styles[0].Name,
            //    runs
            //}, JsonRequestBehavior.AllowGet);
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
    }
}
