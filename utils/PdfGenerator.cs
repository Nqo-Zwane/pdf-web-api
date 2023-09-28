using System;
using System.IO;
using System.Net;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Models;

namespace WebApi.utils
{
    public class PdfGenerator
    {
        public byte[] GenerateInvoice(string InvoiceNumber, string CustomerName, decimal TotalAmount, string Address, string Date, List<InvoiceItem> Items)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Create a new document
                Document document = new Document();
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();
                // Set font and style
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font headingFont = new Font(baseFont, 18, Font.BOLD, BaseColor.BLACK);
                Font contentFont = new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK);
                Font tableFont = new Font(baseFont, 10, Font.NORMAL, BaseColor.BLACK);

                // Add colored rectangles at the top and bottom of the page
                Rectangle topRectangle = new Rectangle(0, 842, 595, 860); // Position and size for A4 page
                topRectangle.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#012970"));
                writer.DirectContent.Rectangle(topRectangle);

                Rectangle bottomRectangle = new Rectangle(0, 0, 595, 18); // Position and size for A4 page
                bottomRectangle.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#012970"));
                writer.DirectContent.Rectangle(bottomRectangle);

                // Add title text with white color on the colored rectangle
                ColumnText.ShowTextAligned(writer.DirectContent,
                    Element.ALIGN_CENTER,
                    new Phrase("Invoice", headingFont),
                    297.5f, 850, 0); // Adjust the position as needed

                // Add customer details on the left
                PdfPTable leftDetailsTable = new PdfPTable(1);
                leftDetailsTable.DefaultCell.Border = Rectangle.NO_BORDER;

                PdfPCell customerCell = new PdfPCell(new Phrase("Customer Details", headingFont));
                customerCell.Border = Rectangle.NO_BORDER;
                customerCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;

                PdfPCell detailsCell = new PdfPCell(new Phrase("Customer Name: " + CustomerName + "\n" + "Address: " + Address, contentFont));
                detailsCell.Border = Rectangle.NO_BORDER;
                detailsCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;

                leftDetailsTable.AddCell(customerCell);
                leftDetailsTable.AddCell(detailsCell);

                leftDetailsTable.WidthPercentage = 40;
                leftDetailsTable.HorizontalAlignment = Element.ALIGN_LEFT;
                document.Add(leftDetailsTable);

                // Add invoice details on the right
                PdfPTable rightDetailsTable = new PdfPTable(1);
                rightDetailsTable.DefaultCell.Border = Rectangle.NO_BORDER;

                PdfPCell invoiceDetailsCell = new PdfPCell(new Phrase("Invoice Details\nInvoice Date: " + Date + "\n" + "Invoice Number: " + InvoiceNumber, contentFont));
                invoiceDetailsCell.Border = Rectangle.NO_BORDER;
                invoiceDetailsCell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;

                rightDetailsTable.AddCell(invoiceDetailsCell);

                rightDetailsTable.WidthPercentage = 40;
                rightDetailsTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                document.Add(rightDetailsTable);

                // Add table for itemized invoice details with style
                PdfPTable itemTable = new PdfPTable(3);
                itemTable.WidthPercentage = 100;
                itemTable.SpacingBefore = 20;
                itemTable.DefaultCell.Border = Rectangle.BOX; // Add border to cells

                PdfPCell itemHeaderCell = new PdfPCell(new Phrase("Item", tableFont));
                itemHeaderCell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                itemHeaderCell.BackgroundColor = new BaseColor(220, 220, 220); // Light gray background color

                PdfPCell descriptionHeaderCell = new PdfPCell(new Phrase("Description", tableFont));
                descriptionHeaderCell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                descriptionHeaderCell.BackgroundColor = new BaseColor(220, 220, 220);

                PdfPCell amountHeaderCell = new PdfPCell(new Phrase("Amount", tableFont));
                amountHeaderCell.HorizontalAlignment = PdfPCell.ALIGN_CENTER;
                amountHeaderCell.BackgroundColor = new BaseColor(220, 220, 220);

                itemTable.AddCell(itemHeaderCell);
                itemTable.AddCell(descriptionHeaderCell);
                itemTable.AddCell(amountHeaderCell);

                /*        // Example item rows (replace with actual data)
                       string[] items = { "Item 1", "Item 2", "Item 3" };
                       string[] descriptions = { "Description 1", "Description 2", "Description 3" };
                       decimal[] amounts = { 50.00m, 75.00m, 30.00m }; */

                foreach (var item in Items)
                {
                    PdfPCell itemCell = new PdfPCell(new Phrase(item.ItemName, tableFont));
                    PdfPCell descriptionCell = new PdfPCell(new Phrase(item.Description, tableFont));
                    PdfPCell amountCell = new PdfPCell(new Phrase(item.Amount.ToString("C"), tableFont));

                    itemTable.AddCell(itemCell);
                    itemTable.AddCell(descriptionCell);
                    itemTable.AddCell(amountCell);
                }

                document.Add(itemTable);


                // Calculate subtotal
                decimal subtotal = Items.Sum(item => item.Amount);
                PdfPCell subtotalCell = new PdfPCell(new Phrase("Subtotal:", tableFont));
                subtotalCell.Border = Rectangle.NO_BORDER;
                subtotalCell.HorizontalAlignment = PdfPCell.ALIGN_RIGHT;
                PdfPCell subtotalAmountCell = new PdfPCell(new Phrase(subtotal.ToString("C"), tableFont));
                subtotalAmountCell.Border = Rectangle.NO_BORDER;
                subtotalAmountCell.HorizontalAlignment = PdfPCell.ALIGN_LEFT;

                PdfPTable subtotalTable = new PdfPTable(2);
                subtotalTable.WidthPercentage = 100;
                subtotalTable.SpacingBefore = 20;
                subtotalTable.DefaultCell.Border = Rectangle.NO_BORDER;

                subtotalTable.AddCell(subtotalCell);
                subtotalTable.AddCell(subtotalAmountCell);

                document.Add(subtotalTable);

                // Add thank you message
                Paragraph thankYouParagraph = new Paragraph("Thank you for your business!", contentFont);
                thankYouParagraph.Alignment = Element.ALIGN_CENTER;
                thankYouParagraph.SpacingBefore = 20;
                document.Add(thankYouParagraph);
                // Close the document
                document.Close();

                // Get the PDF bytes from the memory stream
                byte[] pdfBytes = memoryStream.ToArray();

                return pdfBytes;
            }
        }
        public byte[] GenerateSalesReport(SalesRequest request)
        {
           using (MemoryStream memoryStream = new MemoryStream())
        {
            // Create a new document
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            // Set font and style
            BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font headingFont = new Font(baseFont, 18, Font.BOLD, BaseColor.BLACK);
            Font contentFont = new Font(baseFont, 12, Font.NORMAL, BaseColor.BLACK);

            // Add a title to the report
            document.Add(new Paragraph("Sales Report", headingFont));

            // Add a date to the report (replace with actual date)
            document.Add(new Paragraph("Date and Time: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), contentFont));


            // Add some space (blank line)
            document.Add(new Paragraph(" "));
            // Add some space (blank line)
            document.Add(new Paragraph(" "));

            // Add a table for sales items
            PdfPTable salesTable = new PdfPTable(3); // Three columns: Product Name, Quantity, Revenue
            salesTable.WidthPercentage = 100;

            // Add table headers
            PdfPCell productNameHeader = new PdfPCell(new Phrase("Product Name", contentFont));
            PdfPCell quantityHeader = new PdfPCell(new Phrase("Quantity", contentFont));
            PdfPCell revenueHeader = new PdfPCell(new Phrase("Revenue", contentFont));

            salesTable.AddCell(productNameHeader);
            salesTable.AddCell(quantityHeader);
            salesTable.AddCell(revenueHeader);

            // Add sales items to the table
            foreach (var item in request.SalesItems)
            {
                PdfPCell productNameCell = new PdfPCell(new Phrase(item.ProductName, contentFont));
                PdfPCell quantityCell = new PdfPCell(new Phrase(item.Quantity.ToString(), contentFont));
                PdfPCell revenueCell = new PdfPCell(new Phrase("R" + item.Revenue.ToString("N2"), contentFont));

                salesTable.AddCell(productNameCell);
                salesTable.AddCell(quantityCell);
                salesTable.AddCell(revenueCell);
            }

            // Add the sales table to the document
            document.Add(salesTable);

            // Close the document
            document.Close();

            // Get the PDF bytes from the memory stream
            byte[] pdfBytes = memoryStream.ToArray();

            return pdfBytes;
            }
        }

        public byte[] GenerateIncomeStatement(string period, decimal totalRevenue, decimal totalExpenses, decimal netIncome)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                // Add content to the PDF
                Document doc = new Document();
                PdfWriter writer = PdfWriter.GetInstance(doc, memoryStream);
                doc.Open();

                // Add content to the PDF
                doc.Add(new Paragraph("Income Statement for " + period));

                // Add the main income statement table
                PdfPTable mainTable = new PdfPTable(2);
                mainTable.DefaultCell.Border = Rectangle.NO_BORDER;
                mainTable.WidthPercentage = 60;
                mainTable.SpacingAfter = 5f;

                // Adding cells to the main table
                AddCell(mainTable, "Total Revenue:", "R " + totalRevenue.ToString("N2"));
                AddCell(mainTable, "Total Expenses:", "R " + totalExpenses.ToString("N2"));
                AddCell(mainTable, "Net Income:", "R " + netIncome.ToString("N2"));

                doc.Add(mainTable);

                // Draw a horizontal line
                DrawHorizontalLine(writer);

                // Add another table with details
                PdfPTable detailsTable = new PdfPTable(2);
                detailsTable.DefaultCell.Border = Rectangle.NO_BORDER;
                detailsTable.WidthPercentage = 80;

                // Adding cells to the details table
                AddCell(detailsTable, "Details:", "");
                AddCell(detailsTable, "Revenue Source A:", "10000");
                AddCell(detailsTable, "Revenue Source B:", "20000");
                AddCell(detailsTable, "Expense Category X:", "7000");
                AddCell(detailsTable, "Expense Category Y:", "8000");

                doc.Add(detailsTable);

                doc.Close();


                // Get the PDF bytes from the memory stream
                byte[] pdfBytes = memoryStream.ToArray();

                return pdfBytes;


            }

        }
        // Helper method to add cells to the table
        static void AddCell(PdfPTable table, string label, string value)
        {
            PdfPCell cellLabel = new PdfPCell(new Phrase(label));
            cellLabel.Border = Rectangle.NO_BORDER;
            cellLabel.BackgroundColor = BaseColor.LIGHT_GRAY; // Set cell background color

            PdfPCell cellValue = new PdfPCell(new Phrase(value));
            cellValue.Border = Rectangle.NO_BORDER;

            table.AddCell(cellLabel);
            table.AddCell(cellValue);
        }
        // Helper method to draw a horizontal line
        static void DrawHorizontalLine(PdfWriter writer)
        {
            PdfContentByte content = writer.DirectContent;
            content.MoveTo(20, writer.PageSize.GetBottom(60));
            content.LineTo(writer.PageSize.Width - 20, writer.PageSize.GetBottom(60));
            content.Stroke();
        }



    }
}

