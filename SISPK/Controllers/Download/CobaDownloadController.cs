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
    public class CobaDownloadController : Controller
    {
        //
        // GET: /CobaDownload/
        private SISPKEntities db = new SISPKEntities();
        public ActionResult Coba4()
        {
            // Load document
            Aspose.Words.Document doc = new Aspose.Words.Document(@"C:\Users\Ghany Cahyadi\Desktop\Res\SNI.docx");

            // Keep track if the Paragraph containing the heading text is found
            var USER_FULL_NAME = ((Convert.ToString(Session["USER_FULL_NAME"]) == "") ? "-" : Convert.ToString(Session["USER_FULL_NAME"]));
            var ACCESS_NAME = ((Convert.ToString(Session["ACCESS_NAME"]) == "") ? "-" : Convert.ToString(Session["ACCESS_NAME"]));



            var STYLE_LIST = db.Database.SqlQuery<string>("SELECT * FROM VIEW_SNI_STYLE_GROUP").SingleOrDefault();
            var AksesSelect = db.Database.SqlQuery<SYS_DOC_ACCESS_DETAIL_SELECT>("SELECT * FROM SYS_DOC_ACCESS_DETAIL_SELECT WHERE DOC_ACCESS_DETAIL_STYLE_STATUS = 0 ORDER BY SNI_STYLE_SORT ASC").ToList();


            var kacang = ((STYLE_LIST != null) ? STYLE_LIST : "");
            var wew = "";
            var wew2 = "";

            var found = 0;

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
                        found++;
                        if (paragraph.ParagraphFormat.Style.Name == SNI_STYLE_VALUE)
                        {
                            wew += RemoveSpecialCharacters(paragraph.Range.Text) + " ---- ";
                            // Filter Heading 1 paras and find one that contains the search string
                            if (RemoveSpecialCharacters(paragraph.Range.Text).StartsWith(SNI_STYLE_NAME))
                            {
                               
                                wew2 += RemoveSpecialCharacters(paragraph.GetText()) + " ---- ";
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
            var DataWatermark = db.Database.SqlQuery<SYS_CONFIG>("SELECT * FROM SYS_CONFIG WHERE CONFIG_ID = 9").FirstOrDefault();
            var watermarkText = DataWatermark.CONFIG_VALUE;
            string watermark2Text = "SNI ini di download Oleh " + USER_FULL_NAME + " Sebagai " + ACCESS_NAME;
            Shape watermark = new Shape(doc, ShapeType.TextPlainText);
            watermark.TextPath.Text = watermarkText + "\r\n" + watermark2Text;

            watermark.TextPath.FontFamily = "Arial";
            double fontSize = 11;
            watermark.TextPath.Size = fontSize;
            watermark.Height = fontSize*2;
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

            doc.Save(@"C:\Users\Ghany Cahyadi\Desktop\Res\Coba4.docx");
            return Json(new
            {
                Status = true,
                wew,
                wew2,
                found
            }, JsonRequestBehavior.AllowGet);
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

        public ActionResult Coba3()
        {
            // Load document
            Aspose.Words.Document doc = new Aspose.Words.Document(@"C:\Users\Ghany Cahyadi\Desktop\Res\TestFile.doc");

            // Keep track if the Paragraph containing the heading text is found
            bool isParagraphFound = false;

            // Stores nodes that are to be removed from document
            ArrayList nodesToBeDeleted = new ArrayList();

            // Get a collection of all paragraph nodes in the document
            Node[] paragraphs = doc.GetChildNodes(NodeType.Paragraph, true).ToArray();

            foreach (Paragraph paragraph in paragraphs)
            {
                // We are interested in only Paragraphs with Heading 1 style
                if (paragraph.ParagraphFormat.Style.Name == "Heading 1")
                {
                    // Filter Heading 1 paras and find one that contains the search string
                    if (paragraph.Range.Text.StartsWith("Themes"))
                    {
                        isParagraphFound = true;
                        // We need to delete all nodes present in between the startPara node
                        // and the next Paragraph with Heading 1
                        Paragraph startPara = paragraph;

                        nodesToBeDeleted.Add(startPara);
                        startPara = startPara.NextSibling as Paragraph;
                        var styleName = startPara.ParagraphFormat.Style.Name;
                        while (startPara.ParagraphFormat.Style.Name == styleName)
                        {
                            nodesToBeDeleted.Add(startPara);
                            startPara = startPara.NextSibling as Paragraph;
                        }

                        //do
                        //{
                        //    nodesToBeDeleted.Add(startPara);
                        //    startPara = startPara.NextSibling as Paragraph;
                        //}
                        //while (startPara.ParagraphFormat.Style.Name != "Heading 1");

                        //while (startPara.ParagraphFormat.Style.Name != "Heading 1")
                        //{
                        //    nodesToBeDeleted.Add(startPara);
                        //    startPara = startPara.NextSibling as Paragraph;
                        //}
                        //!startPara.ParagraphFormat.StyleIdentifier.Equals(StyleIdentifier.Heading1)

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

            // Re-build the TOC field
            doc.UpdateFields();

            // Save the final document
            doc.Save(@"C:\Users\Ghany Cahyadi\Desktop\Res\Coba3.docx");
            return Json(new
            {
                Status = true
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Coba2()
        {
            Aspose.Words.Document doc = new Aspose.Words.Document(@"C:\Users\Ghany Cahyadi\Desktop\Res\TestFile.doc");

            // Gather a list of the paragraphs using the respective heading styles.
            ArrayList parasStyleHeading1 = ParagraphsByStyleName(doc, "Heading 1");
            ArrayList parasStyleHeading3 = ParagraphsByStyleName(doc, "Heading 2");

            // Use the first instance of the paragraphs with those styles.
            Node startPara1 = (Node)parasStyleHeading1[0];
            Node endPara1 = (Node)parasStyleHeading3[0];

            // Extract the content between these nodes in the document. Don't include these markers in the extraction.
            ArrayList extractedNodes = ExtractContent(startPara1, endPara1, false);

            // Insert the content into a new separate document and save it to disk.
            Aspose.Words.Document dstDoc = GenerateDocument(doc, extractedNodes);
            dstDoc.Save(@"C:\Users\Ghany Cahyadi\Desktop\Res\Coba2.docx");
            return Json(new
            {
                Status = true
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Coba1()
        {
            Aspose.Words.Document doc = new Aspose.Words.Document(@"C:\Users\Ghany Cahyadi\Desktop\Res\TestFile.doc");
            // Gather the nodes. The GetChild method uses 0-based index
            Paragraph startPara = (Paragraph)doc.FirstSection.GetChild(NodeType.Paragraph, 0, true);
            Paragraph endPara = (Paragraph)doc.FirstSection.GetChild(NodeType.Paragraph, 2, true);
            // Extract the content between these nodes in the document. Include these markers in the extraction.
            ArrayList extractedNodes = ExtractContent(startPara, endPara, true);

            // Insert the content into a new separate document and save it to disk.
            Aspose.Words.Document dstDoc = GenerateDocument(doc, extractedNodes);
            dstDoc.Save(@"C:\Users\Ghany Cahyadi\Desktop\Res\Coba1.docx");
            return Json(new
            {
                Status = true
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Index()
        {
            Aspose.Words.Document doc = new Aspose.Words.Document(@"C:\Users\Ghany Cahyadi\Desktop\Res\TestFile.doc");


            // Define style names as they are specified in the Word document.
            const string paraStyle = "Heading 1";
            const string runStyle = "Intense Emphasis";
            string data_1 = "";
            string data_2 = "";
            // Collect paragraphs with defined styles. 
            // Show the number of collected paragraphs and display the text of this paragraphs.
            ArrayList paragraphs = ParagraphsByStyleName(doc, paraStyle);
            Console.WriteLine(string.Format("Paragraphs with \"{0}\" styles ({1}):", paraStyle, paragraphs.Count));
            foreach (Paragraph paragraph in paragraphs)
            {
                data_1 += paragraph.ToString(SaveFormat.Text) + "<br>";
                Console.Write(paragraph.ToString(SaveFormat.Text));
            }

            // Collect runs with defined styles. 
            // Show the number of collected runs and display the text of this runs.
            ArrayList runs = RunsByStyleName(doc, runStyle);
            Console.WriteLine(string.Format("\nRuns with \"{0}\" styles ({1}):", runStyle, runs.Count));
            foreach (Run run in runs)
            {
                data_2 += run.Range.Text + "<br>";
                Console.WriteLine(run.Range.Text);
            }

            Console.WriteLine("\nExtracted contents based on styles successfully.");
            doc.Save(@"C:\Users\Ghany Cahyadi\Desktop\Res\Out.docx");
            return Json(new
            {
                data_1,
                data_2,
                Status = true
            }, JsonRequestBehavior.AllowGet);
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
        public static ArrayList ExtractContent(Node startNode, Node endNode, bool isInclusive)
        {
            // First check that the nodes passed to this method are valid for use.
            VerifyParameterNodes(startNode, endNode);

            // Create a list to store the extracted nodes.
            ArrayList nodes = new ArrayList();

            // Keep a record of the original nodes passed to this method so we can split marker nodes if needed.
            Node originalStartNode = startNode;
            Node originalEndNode = endNode;

            // Extract content based on block level nodes (paragraphs and tables). Traverse through parent nodes to find them.
            // We will split the content of first and last nodes depending if the marker nodes are inline
            while (startNode.ParentNode.NodeType != NodeType.Body)
                startNode = startNode.ParentNode;

            while (endNode.ParentNode.NodeType != NodeType.Body)
                endNode = endNode.ParentNode;

            bool isExtracting = true;
            bool isStartingNode = true;
            bool isEndingNode = false;
            // The current node we are extracting from the document.
            Node currNode = startNode;

            // Begin extracting content. Process all block level nodes and specifically split the first and last nodes when needed so paragraph formatting is retained.
            // Method is little more complex than a regular extractor as we need to factor in extracting using inline nodes, fields, bookmarks etc as to make it really useful.
            while (isExtracting)
            {
                // Clone the current node and its children to obtain a copy.
                CompositeNode cloneNode = (CompositeNode)currNode.Clone(true);
                isEndingNode = currNode.Equals(endNode);

                if (isStartingNode || isEndingNode)
                {
                    // We need to process each marker separately so pass it off to a separate method instead.
                    if (isStartingNode)
                    {
                        ProcessMarker(cloneNode, nodes, originalStartNode, isInclusive, isStartingNode, isEndingNode);
                        isStartingNode = false;
                    }

                    // Conditional needs to be separate as the block level start and end markers maybe the same node.
                    if (isEndingNode)
                    {
                        ProcessMarker(cloneNode, nodes, originalEndNode, isInclusive, isStartingNode, isEndingNode);
                        isExtracting = false;
                    }
                }
                else
                    // Node is not a start or end marker, simply add the copy to the list.
                    nodes.Add(cloneNode);

                // Move to the next node and extract it. If next node is null that means the rest of the content is found in a different section.
                if (currNode.NextSibling == null && isExtracting)
                {
                    // Move to the next section.
                    Section nextSection = (Section)currNode.GetAncestor(NodeType.Section).NextSibling;
                    currNode = nextSection.Body.FirstChild;
                }
                else
                {
                    // Move to the next node in the body.
                    currNode = currNode.NextSibling;
                }
            }

            // Return the nodes between the node markers.
            return nodes;
        }
        private static void VerifyParameterNodes(Node startNode, Node endNode)
        {
            // The order in which these checks are done is important.
            if (startNode == null)
                throw new ArgumentException("Start node cannot be null");
            if (endNode == null)
                throw new ArgumentException("End node cannot be null");

            if (!startNode.Document.Equals(endNode.Document))
                throw new ArgumentException("Start node and end node must belong to the same document");

            if (startNode.GetAncestor(NodeType.Body) == null || endNode.GetAncestor(NodeType.Body) == null)
                throw new ArgumentException("Start node and end node must be a child or descendant of a body");

            // Check the end node is after the start node in the DOM tree
            // First check if they are in different sections, then if they're not check their position in the body of the same section they are in.
            Section startSection = (Section)startNode.GetAncestor(NodeType.Section);
            Section endSection = (Section)endNode.GetAncestor(NodeType.Section);

            int startIndex = startSection.ParentNode.IndexOf(startSection);
            int endIndex = endSection.ParentNode.IndexOf(endSection);

            if (startIndex == endIndex)
            {
                if (startSection.Body.IndexOf(startNode) > endSection.Body.IndexOf(endNode))
                    throw new ArgumentException("The end node must be after the start node in the body");
            }
            else if (startIndex > endIndex)
                throw new ArgumentException("The section of end node must be after the section start node");
        }

        /// <summary>
        /// Checks if a node passed is an inline node.
        /// </summary>
        private static bool IsInline(Node node)
        {
            // Test if the node is desendant of a Paragraph or Table node and also is not a paragraph or a table a paragraph inside a comment class which is decesant of a pararaph is possible.
            return ((node.GetAncestor(NodeType.Paragraph) != null || node.GetAncestor(NodeType.Table) != null) && !(node.NodeType == NodeType.Paragraph || node.NodeType == NodeType.Table));
        }

        /// <summary>
        /// Removes the content before or after the marker in the cloned node depending on the type of marker.
        /// </summary>
        private static void ProcessMarker(CompositeNode cloneNode, ArrayList nodes, Node node, bool isInclusive, bool isStartMarker, bool isEndMarker)
        {
            // If we are dealing with a block level node just see if it should be included and add it to the list.
            if (!IsInline(node))
            {
                // Don't add the node twice if the markers are the same node
                if (!(isStartMarker && isEndMarker))
                {
                    if (isInclusive)
                        nodes.Add(cloneNode);
                }
                return;
            }

            // If a marker is a FieldStart node check if it's to be included or not.
            // We assume for simplicity that the FieldStart and FieldEnd appear in the same paragraph.
            if (node.NodeType == NodeType.FieldStart)
            {
                // If the marker is a start node and is not be included then skip to the end of the field.
                // If the marker is an end node and it is to be included then move to the end field so the field will not be removed.
                if ((isStartMarker && !isInclusive) || (!isStartMarker && isInclusive))
                {
                    while (node.NextSibling != null && node.NodeType != NodeType.FieldEnd)
                        node = node.NextSibling;

                }
            }

            // If either marker is part of a comment then to include the comment itself we need to move the pointer forward to the Comment
            // node found after the CommentRangeEnd node.
            if (node.NodeType == NodeType.CommentRangeEnd)
            {
                while (node.NextSibling != null && node.NodeType != NodeType.Comment)
                    node = node.NextSibling;

            }

            // Find the corresponding node in our cloned node by index and return it.
            // If the start and end node are the same some child nodes might already have been removed. Subtract the
            // difference to get the right index.
            int indexDiff = node.ParentNode.ChildNodes.Count - cloneNode.ChildNodes.Count;

            // Child node count identical.
            if (indexDiff == 0)
                node = cloneNode.ChildNodes[node.ParentNode.IndexOf(node)];
            else
                node = cloneNode.ChildNodes[node.ParentNode.IndexOf(node) - indexDiff];

            // Remove the nodes up to/from the marker.
            bool isSkip = false;
            bool isProcessing = true;
            bool isRemoving = isStartMarker;
            Node nextNode = cloneNode.FirstChild;

            while (isProcessing && nextNode != null)
            {
                Node currentNode = nextNode;
                isSkip = false;

                if (currentNode.Equals(node))
                {
                    if (isStartMarker)
                    {
                        isProcessing = false;
                        if (isInclusive)
                            isRemoving = false;
                    }
                    else
                    {
                        isRemoving = true;
                        if (isInclusive)
                            isSkip = true;
                    }
                }

                nextNode = nextNode.NextSibling;
                if (isRemoving && !isSkip)
                    currentNode.Remove();
            }

            // After processing the composite node may become empty. If it has don't include it.
            if (!(isStartMarker && isEndMarker))
            {
                if (cloneNode.HasChildNodes)
                    nodes.Add(cloneNode);
            }

        }
        public static Aspose.Words.Document GenerateDocument(Aspose.Words.Document srcDoc, ArrayList nodes)
        {
            // Create a blank document.
            Aspose.Words.Document dstDoc = new Aspose.Words.Document();
            // Remove the first paragraph from the empty document.
            dstDoc.FirstSection.Body.RemoveAllChildren();

            // Import each node from the list into the new document. Keep the original formatting of the node.
            NodeImporter importer = new NodeImporter(srcDoc, dstDoc, ImportFormatMode.KeepSourceFormatting);

            foreach (Node node in nodes)
            {
                Node importNode = importer.ImportNode(node, true);
                dstDoc.FirstSection.Body.AppendChild(importNode);
            }

            // Return the generated document.
            return dstDoc;
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
