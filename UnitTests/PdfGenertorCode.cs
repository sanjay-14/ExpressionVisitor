using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
    //public static DataTable ToDataTable<T>(this IEnumerable<T> data)
    //    {
    //        var properties =
    //           TypeDescriptor.GetProperties(typeof(T));
    //        var table = new DataTable();
    //        foreach (PropertyDescriptor prop in properties)
    //            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
    //        foreach (T item in data)
    //        {
    //            var row = table.NewRow();
    //            foreach (PropertyDescriptor prop in properties)
    //            {

    //                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
    //                if (row[prop.Name] != DBNull.Value)
    //                {
    //                    row[prop.Name] = row[prop.Name].ToString();
    //                }
    //            }
    //            table.Rows.Add(row);
    //        }
    //        return table;
    //    }    
    
    //    public static string GetPropertyName<T, TReturn>(Expression<Func<T, TReturn>> expression)
        //{
        //    var body = (MemberExpression)expression.Body;
        //    return body.Member.Name;
        //}

    //public enum ReportType
    //{
    //    Html, Pdf, Export
    //}

    //public interface IReportLog
    //{
    //    void LogReports(ReportType type, ReportDocument reportDocument);
    //}

    //public class PdfConvertorController : Controller
    //{
    //    protected IReportLog LogReport { get; set; }

    //    public ActionResult ViewPdf(string viewName, PdfDocumentModel pdfDocument)
    //    {
    //        return ViewPdf(viewName, pdfDocument, string.Empty);
    //    }

    //    public ActionResult ViewPdf(string viewName, PdfDocumentModel pdfDocument, string fileName)
    //    {
    //        var memStream = new MemoryStream();
    //        PdfGenerator.CreatePdfDocument<PdfViews>(pdfDocument.PdfDocument, memStream, viewName, pdfDocument.ReportDocument);
    //        var buf = new byte[memStream.Position];
    //        memStream.Position = 0;
    //        memStream.Read(buf, 0, buf.Length);
    //        //if (string.IsNullOrEmpty(fileName))
    //        //{
    //        //    /********************************************************
    //        //     * This is to instructing PDFViewer of Browser either built in(in Chrome & Mozilla 19 or higher) or adobe acrobate
    //        //     * to view the Print dialog for automaticlly printing the PDF document
    //        //     ********************************************************/
    //        //    var reader = new PdfReader(buf);
    //        //    var stamperMemoryStream = new MemoryStream();
    //        //    var stamper = new PdfStamper(reader, stamperMemoryStream) { JavaScript = "this.print(true);" };
    //        //    stamper.Close();
    //        //    reader.Close();
    //        //    buf = stamperMemoryStream.ToArray();
    //        //}

    //        return !string.IsNullOrEmpty(fileName) ? File(buf, "application/pdf", fileName) : File(buf, "application/pdf");
    //    }

    //    public ActionResult ReportConditionalView(string viewName, PdfDocumentModel pdfDocument, ReportType type = ReportType.Html)
    //    {
    //        return ReportConditionalView(viewName, pdfDocument, type, null);
    //    }

    //    public ActionResult ReportConditionalView(string viewName, PdfDocumentModel pdfDocument, ReportType type = ReportType.Html, string reportName = "")
    //    {
    //        if (pdfDocument == null) throw new NullReferenceException("pdfDocument not found");
    //        if (pdfDocument.ReportDocument == null) throw new NullReferenceException("Report Document not found");
    //        try
    //        {
    //            if (LogReport != null)
    //                LogReport.LogReports(type, pdfDocument.ReportDocument);
    //            switch (type)
    //            {
    //                case ReportType.Pdf:
    //                    return ViewPdf(viewName, pdfDocument);
    //                case ReportType.Export:
    //                    if (string.IsNullOrEmpty(reportName))
    //                    {
    //                        reportName = "Report.xlsx";
    //                    }
    //                    return ViewExcel(viewName, pdfDocument, reportName);
    //                default:
    //                    return PartialView(viewName, pdfDocument.ReportDocument);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            //this.Logger().Error(ex.Message, ex);
    //            return new HttpStatusCodeResult(500, ex.Message);
    //        }
    //    }

    //    public string RenderActionResultToString(ActionResult result)
    //    {
    //        // Create memory writer.
    //        var sb = new StringBuilder();
    //        var memWriter = new StringWriter(sb);

    //        // Create fake http context to render the view.
    //        var fakeResponse = new HttpResponse(memWriter);
    //        var fakeContext = new HttpContext(System.Web.HttpContext.Current.Request,
    //            fakeResponse);
    //        var fakeControllerContext = new ControllerContext(new HttpContextWrapper(fakeContext), this.ControllerContext.RouteData, this.ControllerContext.Controller);
    //        var oldContext = System.Web.HttpContext.Current;
    //        System.Web.HttpContext.Current = fakeContext;

    //        // Render the view.
    //        result.ExecuteResult(fakeControllerContext);

    //        // Restore old context.
    //        System.Web.HttpContext.Current = oldContext;

    //        // Flush memory and return output.
    //        memWriter.Flush();
    //        return sb.ToString();
    //    }

    //    public ActionResult ViewExcel(string viewName, PdfDocumentModel model)
    //    {
    //        return ViewExcel(viewName, model, string.Empty);
    //    }

    //    public ActionResult ViewExcel(string viewName, PdfDocumentModel model, string fileName)
    //    {
    //        var memStream = new MemoryStream();
    //        using (var context = new ExcelPackage())
    //        {
    //            var worksheet = context.Workbook.Worksheets.Add("Sheet1");
    //            ExcelGenerator.GenerateDocument<ExcelViews>(worksheet, viewName, model.ReportDocument);
    //            context.SaveAs(memStream);
    //        }

    //        var buf = new byte[memStream.Position];
    //        memStream.Position = 0;
    //        memStream.Read(buf, 0, buf.Length);
    //        const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    //        var fileNameWithExtenstion = (fileName.Contains(".pdf")) ? fileName.Replace(".pdf", ".xlsx") : string.Format("{0}.xlsx", fileName);
    //        return !string.IsNullOrEmpty(fileName) ? File(buf, contentType, fileNameWithExtenstion) : File(buf, contentType);
    //    }
    //}
}
