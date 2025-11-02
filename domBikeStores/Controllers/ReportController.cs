using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using domBikeStores.Models;
using domBikeStores.ViewModels;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using A = DocumentFormat.OpenXml.Drawing;
using iTextElement = iText.Layout.Element;
using iTextImage = iText.IO.Image;
using iTextKernel = iText.Kernel.Pdf;
using iTextLayout = iText.Layout;
using PIC = DocumentFormat.OpenXml.Drawing.Pictures;
using Word = DocumentFormat.OpenXml.Wordprocessing;

namespace domBikeStores.Controllers
{
    public class ReportController : Controller
    {
        private BikeStoresEntities db = new BikeStoresEntities();
        private string reportFolder = "~/Reports/";

        // GET: Report
        public ActionResult Index()
        {
            var vm = BuildReportViewModel();
            return View(vm);
        }

        private ReportViewModel BuildReportViewModel()
        {
            var topProducts = db.order_items
                .GroupBy(oi => new { oi.product_id, oi.products.product_name })
                .Select(g => new ProductSalesReportItem
                {
                    ProductName = g.Key.product_name,
                    QuantitySold = g.Sum(x => x.quantity)
                })
                .OrderByDescending(p => p.QuantitySold)
                .Take(20)
                .ToList();

            var topStores = db.order_items
                .GroupBy(oi => new { oi.orders.store_id, oi.orders.stores.store_name })
                .Select(g => new StorePerformanceReportItem
                {
                    StoreName = g.Key.store_name,
                    TotalProductsSold = g.Sum(x => x.quantity)
                })
                .OrderByDescending(s => s.TotalProductsSold)
                .Take(3)
                .ToList();

            return new ReportViewModel
            {
                ReportTitle = "Sales Performance Reports",
                ChartTypeProducts = "bar",
                ChartTypeStores = "pie",
                TopProducts = topProducts,
                TopStores = topStores,
                DocumentArchive = GetSavedReports()
            };
        }

        // --- POST: Save Report ---
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveReport(ReportViewModel model, string chartDataProducts, string chartDataStores)
        {
            try
            {
                string directory = Server.MapPath(reportFolder);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{model.FileName}_{timestamp}.{model.FileType}";
                string fullPath = Path.Combine(directory, fileName);

                string description = Regex.Replace(model.Description ?? "", "<.*?>", string.Empty);

                var reportType = model.ReportTitle;

                switch (model.FileType.ToLower())
                {
                    case "docx":
                        if (reportType == "Top 20 Products")
                            SaveWord(fullPath, model, chartDataProducts, null, description);
                        else if (reportType == "Top 3 Stores")
                            SaveWord(fullPath, model, null, chartDataStores, description);
                        else
                            SaveWord(fullPath, model, chartDataProducts, chartDataStores, description);
                        break;

                    case "xlsx":
                        if (reportType == "Top 20 Products")
                            SaveExcel(fullPath, model, chartDataProducts, null, description);
                        else if (reportType == "Top 3 Stores")
                            SaveExcel(fullPath, model, null, chartDataStores, description);
                        else
                            SaveExcel(fullPath, model, chartDataProducts, chartDataStores, description);
                        break;

                    default:
                        throw new Exception("Unsupported file type");
                }

                var meta = new { FileName = fileName, FileType = model.FileType, Description = description, DateSaved = DateTime.Now };
                System.IO.File.WriteAllText(Path.Combine(directory, fileName + ".json"), Newtonsoft.Json.JsonConvert.SerializeObject(meta));

                TempData["Message"] = "Report saved successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error saving report: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private byte[] GetImageBytes(string chartData)
        {
            if (string.IsNullOrEmpty(chartData)) return null;

            string base64 = chartData;
            var match = Regex.Match(chartData, @"data:image\/png;base64,(.+)");
            if (match.Success) base64 = match.Groups[1].Value;

            try
            {
                return Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }
        }

        private void SaveWord(string path, ReportViewModel model, string chartDataProducts, string chartDataStores, string description)
        {
            using (var wordDoc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document))
            {
                var mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Word.Document(new Word.Body());

                var body = mainPart.Document.Body;
                body.AppendChild(new Word.Paragraph(new Word.Run(new Word.Text(model.ReportTitle))));
                body.AppendChild(new Word.Paragraph(new Word.Run(new Word.Text($"Description: {description}"))));
                body.AppendChild(new Word.Paragraph(new Word.Run(new Word.Text($"Generated: {DateTime.Now}"))));

                AddImageToWord(mainPart, chartDataProducts, "Top Products Chart");
                AddImageToWord(mainPart, chartDataStores, "Top Stores Chart");
            }
        }

        private void AddImageToWord(MainDocumentPart mainPart, string chartData, string caption)
        {
            var bytes = GetImageBytes(chartData);
            if (bytes == null || bytes.Length == 0) return;

            ImagePart imagePart = mainPart.AddImagePart(ImagePartType.Png);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                imagePart.FeedData(stream);
            }

            long cx = 4500000;
            long cy = 3000000;

            var element = new Drawing(
                new DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline(
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.Extent() { Cx = cx, Cy = cy },
                    new DocumentFormat.OpenXml.Drawing.Wordprocessing.DocProperties() { Id = (UInt32Value)1U, Name = caption ?? "Chart" },
                    new DocumentFormat.OpenXml.Drawing.Graphic(
                        new DocumentFormat.OpenXml.Drawing.GraphicData(
                            new PIC.Picture(
                                new PIC.NonVisualPictureProperties(
                                    new PIC.NonVisualDrawingProperties() { Id = (UInt32Value)0U, Name = caption ?? "Chart" },
                                    new PIC.NonVisualPictureDrawingProperties()
                                ),
                                new PIC.BlipFill(
                                    new A.Blip() { Embed = mainPart.GetIdOfPart(imagePart) },
                                    new A.Stretch(new A.FillRectangle())
                                ),
                                new PIC.ShapeProperties(
                                    new A.Transform2D(
                                        new A.Offset() { X = 0, Y = 0 },
                                        new A.Extents() { Cx = cx, Cy = cy }
                                    ),
                                    new A.PresetGeometry(new A.AdjustValueList()) { Preset = A.ShapeTypeValues.Rectangle }
                                )
                            )
                        )
                        { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" }
                    )
                )
                {
                    DistanceFromTop = (UInt32Value)0U,
                    DistanceFromBottom = (UInt32Value)0U,
                    DistanceFromLeft = (UInt32Value)0U,
                    DistanceFromRight = (UInt32Value)0U
                }
            );

            var paragraph = new Paragraph(new Run(element));
            mainPart.Document.Body.AppendChild(paragraph);

            mainPart.Document.Body.AppendChild(new Paragraph(new Run(new Break())));
        }

        private void SaveExcel(string path, ReportViewModel model, string chartDataProducts, string chartDataStores, string description)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Dominique Cloete");

            using (var package = new ExcelPackage(new FileInfo(path)))
            {
                var sheet = package.Workbook.Worksheets.Add("Report");

                sheet.Cells[1, 1].Value = "Report";
                sheet.Cells[1, 2].Value = model.ReportTitle;
                sheet.Cells[2, 1].Value = "Description";
                sheet.Cells[2, 2].Value = description;
                sheet.Cells[3, 1].Value = "Generated";
                sheet.Cells[3, 2].Value = DateTime.Now.ToString();

                AddChartToExcel(sheet, chartDataProducts, "TopProducts", 4);
                AddChartToExcel(sheet, chartDataStores, "TopStores", 20);

                package.Save();
            }
        }

        private void AddChartToExcel(ExcelWorksheet sheet, string chartData, string name, int startRow, int startCol = 1)
        {
            var bytes = GetImageBytes(chartData);
            if (bytes == null) return;

            var tmpFile = Path.Combine(Path.GetTempPath(), name + ".png");
            System.IO.File.WriteAllBytes(tmpFile, bytes);

            var pic = sheet.Drawings.AddPicture(name, tmpFile);
            pic.SetPosition(startRow - 1, 0, startCol - 1, 0);
        }

        // --- Download ---
        public FileResult Download(string fileName)
        {
            string directory = Server.MapPath(reportFolder);
            string fullPath = Path.Combine(directory, fileName);
            string metaFile = fullPath + ".json";

            if (!System.IO.File.Exists(fullPath))
                throw new FileNotFoundException("Report not found.");

            string description = "N/A";
            if (System.IO.File.Exists(metaFile))
            {
                var meta = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(metaFile));
                description = meta.Description;
            }

            string extension = Path.GetExtension(fullPath).ToLower();
            string mimeType = MimeMapping.GetMimeMapping(fullPath);

            string tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");

            if (extension == ".docx")
            {
                using (var sourceDoc = WordprocessingDocument.Open(fullPath, false))
                using (var newDoc = WordprocessingDocument.Create(tempPath, WordprocessingDocumentType.Document))
                {
                    foreach (var part in sourceDoc.Parts)
                        newDoc.AddPart(part.OpenXmlPart, part.RelationshipId);

                    var body = newDoc.MainDocumentPart.Document.Body;
                    var descPara = body.Elements<Paragraph>().FirstOrDefault(p => p.InnerText.StartsWith("Description:"));
                    if (descPara != null)
                        descPara.Remove();

                    body.InsertAfter(
                        new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                            new DocumentFormat.OpenXml.Wordprocessing.Run(
                                new DocumentFormat.OpenXml.Wordprocessing.Text($"Description: {description}")
                            )
                        ),
                        body.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>().FirstOrDefault()
                    );

                    newDoc.MainDocumentPart.Document.Save();
                }
            }
            else if (extension == ".xlsx")
            {
                using (var package = new OfficeOpenXml.ExcelPackage(new FileInfo(fullPath)))
                {
                    ExcelPackage.License.SetNonCommercialPersonal("Dominique Cloete");
                    var sheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (sheet != null)
                    {
                        sheet.Cells[2, 2].Value = description;
                        package.SaveAs(new FileInfo(tempPath));
                    }
                }
            }
            else
            {
                System.IO.File.Copy(fullPath, tempPath);
            }

            return File(tempPath, mimeType, Path.GetFileName(fullPath));
        }

        // --- Delete ---
        public ActionResult Delete(string fileName)
        {
            string fullPath = Path.Combine(Server.MapPath(reportFolder), fileName);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);
            TempData["Message"] = "Report deleted.";
            return RedirectToAction("Index");
        }

        public ActionResult DeleteAndRedirect(string fileName)
        {
            string fullPath = Path.Combine(Server.MapPath(reportFolder), fileName);
            if (System.IO.File.Exists(fullPath))
                System.IO.File.Delete(fullPath);

            return new HttpStatusCodeResult(200);
        }

        // --- Archive Loader ---
        private List<ReportViewModel.SavedReport> GetSavedReports()
        {
            string directory = Server.MapPath(reportFolder);
            if (!Directory.Exists(directory)) return new List<ReportViewModel.SavedReport>();

            var reports = new List<ReportViewModel.SavedReport>();
            foreach (var file in Directory.GetFiles(directory))
            {
                if (file.EndsWith(".json")) continue;

                string metaFile = file + ".json";
                string description = "N/A";
                if (System.IO.File.Exists(metaFile))
                {
                    var meta = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(metaFile));
                    description = meta.Description;
                }

                var fi = new FileInfo(file);
                reports.Add(new ReportViewModel.SavedReport
                {
                    FileName = fi.Name,
                    FileType = fi.Extension.Trim('.').ToUpper(),
                    Description = description,
                    DateSaved = fi.CreationTime
                });
            }

            return reports.OrderByDescending(r => r.DateSaved).ToList();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateDescription(string fileName, string description)
        {
            try
            {
                string directory = Server.MapPath(reportFolder);
                string metaFile = Path.Combine(directory, fileName + ".json");

                if (System.IO.File.Exists(metaFile))
                {
                    string cleanDescription = Regex.Replace(description ?? "", "<.*?>", string.Empty);

                    var meta = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(System.IO.File.ReadAllText(metaFile));
                    meta.Description = cleanDescription;
                    System.IO.File.WriteAllText(metaFile, Newtonsoft.Json.JsonConvert.SerializeObject(meta));

                    TempData["Message"] = "Description updated successfully!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error updating description: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
