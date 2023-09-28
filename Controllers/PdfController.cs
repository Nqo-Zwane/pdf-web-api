using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Microsoft.AspNetCore.Mvc;
using WebApi.utils;
using Models;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class InvoiceController : ControllerBase
    {
        private readonly PdfGenerator _pdfGenerator;

        public InvoiceController(PdfGenerator pdfGenerator)
        {
            _pdfGenerator = pdfGenerator;
        }

        [HttpPost("generate")]
        public IActionResult GenerateInvoice([FromBody] InvoiceRequest request)
        {
            var pdfBytes = _pdfGenerator.GenerateInvoice(request.InvoiceNumber, request.CustomerName, request.TotalAmount, request.Address, request.Date, request.Items);

            return File(pdfBytes, "application/pdf", "invoice.pdf");
        }
        
        [HttpPost("report")]
        public IActionResult GenerateReport([FromBody] ReportRequest request)
        {
            var pdfBytes = _pdfGenerator.GenerateIncomeStatement(request.period, request.totalRevenue, request.totalExpenses, request.netIncome);
                // Create a memory stream from the PDF bytes
            var stream = new MemoryStream(pdfBytes);

            // Return the PDF content as a Blob
            return File(stream, "application/pdf");
        }
        [HttpPost("generate-sales-report")]
        public IActionResult GenerateSalesReport([FromBody] SalesRequest request)
        {
            // Use salesData to generate the sales report PDF
            var pdfBytes = _pdfGenerator.GenerateSalesReport(request);

            // Return the PDF as a response
            return File(pdfBytes, "application/pdf", "sales-report.pdf");
        }

    }

}


