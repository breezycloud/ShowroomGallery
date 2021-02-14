using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using ClosedXML.Report;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowroomAPI.Context;
using ShowroomAPI.Models;
using Spire.Xls;

namespace ShowroomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockInsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string reportHeader = "DANTUNKURA GALLERY & INVESTMENT";
        private readonly IWebHostEnvironment _env;

        public StockInsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/StockIns
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockIn>>> GetStockIns()
        {
            return await _context.StockIns.ToListAsync();
        }

        // GET: api/StockIns/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StockIn>> GetStockIn(int id)
        {
            var stockIn = await _context.StockIns.FindAsync(id);

            if (stockIn == null)
            {
                return NotFound();
            }

            return stockIn;
        }

        // PUT: api/StockIns/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStockIn(int id, StockIn stockIn)
        {
            if (id != stockIn.StockInNo)
            {
                return BadRequest();
            }

            _context.Entry(stockIn).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockInExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpGet("report/{type}")]
        public async Task<ActionResult> ExportStock(string type, string from, string to)
        {
            byte[] result = null;
            var dateFrom = DateTime.Parse(from);
            var dateTo = DateTime.Parse(to);

            var templatePath = type == "In" ? Path.Combine(_env.ContentRootPath, "Reports", "Stock-In.xlsx") :
                Path.Combine(_env.ContentRootPath, "Reports", "Stock-Out.xlsx");

            switch (type)
            {
                case "In":
                    var reportData = await _context.StockIns.Include(p => p.ProductNoNavigation)
                                                            .Where(s => s.DateIn >= dateFrom  && s.DateIn <= dateTo)
                                                            .OrderBy(i => i.DateIn)
                                                            .ToListAsync();


                    var stock = GetStockInData(reportData);

                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var ws = wb.Worksheets.Add($"Stock {type}");
                        await PageSetting(ws, type);
                        await ReportHeader(ws, 9, 4);
                        await ReportSubtitle(ws, 9, 5, type, from, to);
                        await DrawStockInTable(ws);
                        await InsertStockInContent(ws, stock);
                        await AdjustContents(ws);                        

                        var ms = new MemoryStream();
                        wb.SaveAs(ms);
                        result = ms.ToArray();

                    }                          

                    break;
                case "Out":

                    var transactions = await _context.Transactions.Include(s => s.Staff)
                                                                  .Include(td => td.TransactionDetails)
                                                                  .ThenInclude(p =>p.ProductNoNavigation)
                                                                  .Where(s => s.Tdate >= dateFrom && s.Tdate <= dateTo)
                                                                  .ToListAsync();                    

                    var sales = GetOrder(transactions);

                    var template = new XLTemplate(templatePath);
                    template.AddVariable("Orders", sales);
                    template.Generate();

                    using (var ms = new MemoryStream())
                    {
                        template.SaveAs(ms);
                        result = ms.ToArray();                        
                    }

                    break;
            }

            return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Stock-{type}.xlsx");
        }

        // POST: api/StockIns
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<StockIn>> PostStockIn(StockIn stockIn)
        {
            _context.StockIns.Add(stockIn);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStockIn", new { id = stockIn.StockInNo }, stockIn);
        }

        // DELETE: api/StockIns/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStockIn(int id)
        {
            var stockIn = await _context.StockIns.FindAsync(id);
            if (stockIn == null)
            {
                return NotFound();
            }

            _context.StockIns.Remove(stockIn);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StockInExists(int id)
        {
            return _context.StockIns.Any(e => e.StockInNo == id);
        }
        
        private static DataTable GetStockInData(List<StockIn> stocks)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Date");
            dt.Columns.Add("Name");            
            dt.Columns.Add("Quantity", typeof(double));
           
            foreach (var item in stocks)
            {
                DataRow productsRow = dt.NewRow();
                productsRow["Date"] = item.DateIn.ToShortDateString();
                productsRow["Name"] = item.ProductNoNavigation.ProductCode;                
                productsRow["Quantity"] = item.Quantity;
                dt.Rows.Add(productsRow);
            }

            return dt;
        }

        private List<Order> GetOrder(List<Transaction> transaction)
        {
            List<Order> orders = new List<Order>();

            foreach (var item in transaction)
            {
                Order order = new Order()
                {
                    InvoiceNo = item.InvoiceNo,
                    OrderDate = item.Tdate.ToShortDateString(),
                    Cashier = item.Staff.FirstName,
                    TotalAmount = item.TotalAmount,
                    SubTotal = item.SubTotal,
                    Discount = item.Discount

                };
                orders.Add(order);
                foreach (var o in item.TransactionDetails)
                {
                    OrderItems orderItems = new OrderItems()
                    {
                        Quantity = o.Quantity,
                        ProductName = o.ProductNoNavigation.ProductCode,
                        UnitPrice = o.ProductNoNavigation.UnitPrice,
                        ItemPrice = o.ProductNoNavigation.UnitPrice * (decimal)o.Quantity
                    };
                    order.OrderItems.Add(orderItems);
                }
            }
            return orders;
        }

        private async Task ReportHeader(IXLWorksheet ws, int columnCount, int rowCount)
        {
            ws.Row(1).InsertRowsAbove(4);
            string lastColumnIndex = await GetAlphabet(columnCount);
            ws.Cell("A1").Value = reportHeader;
            ws.Cell("A1").Style.Font.SetBold(true).Font.SetFontName("Georgia").Font.SetFontSize(18);
            ws.Cell("A1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("A1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Range($"A1:{lastColumnIndex}{rowCount}").Merge();            

            await Task.Delay(0);
        }

        private async Task ReportSubtitle(IXLWorksheet ws, int columnCount, int rowCount, string option, string from, string to)
        {            
            var reportType = option == "In" ? "STOCK-IN | REPORT" : "STOCK-OUT | SALES REPORT";
            string lastColumnIndex = await GetAlphabet(columnCount);
            ws.Cell("A5").Value = reportType;
            ws.Cell("A5").Style.Font.SetBold(true).Font.SetFontName("Georgia").Font.SetFontSize(14);
            ws.Cell("A5").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("A5").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range($"A5:{lastColumnIndex}{rowCount}").Merge();

            ws.Cell("A6").Value = $"FROM {from} TO {to}";
            ws.Cell("A6").Style.Font.SetBold(true).Font.SetFontName("Georgia").Font.SetFontSize(11);
            ws.Cell("A6").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell("A6").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Range($"A6:{lastColumnIndex}6").Merge();

            await Task.Delay(0);
        }

        private async Task DrawStockInTable(IXLWorksheet ws)
        {            
            ws.Cell("A7").Value = "DATE";
            ws.Range("A7:B7").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
            ws.Range("A7:B7").Style.Font.FontColor = XLColor.White;
            ws.Range("A7:B7").Style.Font.SetBold(true).Font.SetFontName("Georgia").Font.SetFontSize(10);
            ws.Range("A8:B8").Merge();

            ws.Cell("C7").Value = "ITEM NAME";
            ws.Range("C7:G7").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
            ws.Range("C7:G7").Style.Font.FontColor = XLColor.White;
            ws.Range("C7:G7").Style.Font.SetBold(true).Font.SetFontName("Georgia").Font.SetFontSize(10);
            ws.Range("C8:G8").Merge();

            ws.Cell("H7").Value = "QUANTITY";
            ws.Range("H7:I7").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
            ws.Range("H7:I7").Style.Font.FontColor = XLColor.White;
            ws.Range("H7:I7").Style.Font.SetBold(true).Font.SetFontName("Georgia").Font.SetFontSize(10);
            

            await Task.Delay(0);
        }

        private async Task DrawStockOutTable(IXLWorksheet ws)
        {
            //string fontName = "Arial";
            //double fontSize = 10;
            
            ws.Row(8).Height = 30;

            ws.Cell("A8").Value = "Order No";
            ws.Range("A8").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
            ws.Range("A8").Style.Font.FontColor = XLColor.White;

            ws.Cell("B8").Value = "Order Date";
            ws.Range("B8:C8").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
            ws.Range("B8:C8").Style.Font.FontColor = XLColor.White;
            ws.Range("B8:C8").Merge();

            ws.Cell("D8").Value = "Item Name";
            ws.Range("D8:G8").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
            ws.Range("D8:G8").Style.Font.FontColor = XLColor.White;
            ws.Range("D8:G8").Merge();

            ws.Cell("H8").Value = "Qty";
            ws.Range("H8").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
            ws.Range("H8").Style.Font.FontColor = XLColor.White;

            ws.Cell("I8").Value = "Price";
            ws.Range("I8:J8").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
            ws.Range("I8:J8").Style.Font.FontColor = XLColor.White;
            ws.Range("I8:J8").Merge();

            ws.Cell("K8").Value = "Discount";
            ws.Range("K8:L8").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
            ws.Range("K8:L8").Style.Font.FontColor = XLColor.White;
            ws.Range("K8:L8").Merge();
            
            ws.Cell("M8").Value = "Amount Paid";
            ws.Range("M8:N8").Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
            ws.Range("M8:N8").Style.Font.FontColor = XLColor.White;
            ws.Range("M8:N8").Merge();

            await Task.Delay(0);
        }

        private async Task InsertSalesData(IXLWorksheet ws, List<StockOut> stocks)
        {
            var vs = stocks.Select(i => i.OrderNo).Distinct().ToList();

            int x = 9;
            int y = 9;
            foreach (var item in vs)
            {                

                ws.Cell($"A{x}").Value = item.ToString();
                ws.Range($"A{x}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                                      .SetOutsideBorderColor(XLColor.Black);

                //ws.Cell($"B{x}").Value = item.OrderDate;
                //ws.Range($"B{x}:C{x}").Merge();
                //ws.Range($"B{x}:C{x}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                //                      .SetOutsideBorderColor(XLColor.Black);

                var orderLine = stocks.Where(i => i.OrderNo == item.ToString()).ToList();                
                foreach (var row in orderLine)
                {
                    ws.Cell($"D{y}").Value = row.ProductName;
                    ws.Range($"D{y}:G{y}").Merge();
                    ws.Range($"D{y}:G{y}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                                          .SetOutsideBorderColor(XLColor.Black);

                    ws.Cell($"H{y}").Value = row.Quantity;                    
                    ws.Range($"H{y}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                                          .SetOutsideBorderColor(XLColor.Black);

                    ws.Cell($"I{y}").Value = row.ItemPrice;
                    ws.Range($"I{y}:J{y}").Merge();
                    ws.Range($"I{y}:J{y}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                                          .SetOutsideBorderColor(XLColor.Black);

                    ws.Cell($"K{y}").Value = row.Discount;
                    ws.Range($"K{y}:L{y}").Merge();
                    ws.Range($"K{y}:L{y}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                                          .SetOutsideBorderColor(XLColor.Black);

                    ws.Cell($"M{y}").Value = row.SubTotal;
                    ws.Range($"M{y}:N{y}").Merge();
                    ws.Range($"M{y}:N{y}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                                          .SetOutsideBorderColor(XLColor.Black);
                    y++;
                }
                x++;
            }

            await Task.Delay(0);
        }

        private async Task InsertStockInContent(IXLWorksheet ws, DataTable stocks)
        {
            int x = 8;
            for (int i = 0; i < stocks.Rows.Count; i++)
            {
                ws.Cell($"A{x}").Value = stocks.Rows[i].Field<dynamic>(0);
                ws.Range($"A{x}:B{x}").Merge();
                ws.Range($"A{x}:B{x}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                                      .SetOutsideBorderColor(XLColor.Black);
                ws.Cell($"C{x}").Value = stocks.Rows[i].Field<dynamic>(1);
                ws.Range($"C{x}:G{x}").Merge();
                ws.Range($"C{x}:G{x}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                                      .SetOutsideBorderColor(XLColor.Black);
                ws.Cell($"H{x}").Value = stocks.Rows[i].Field<dynamic>(2);
                ws.Range($"H{x}:I{x}").Merge();
                ws.Range($"H{x}:I{x}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                                      .SetOutsideBorderColor(XLColor.Black);
                x++;
            }

            //Sum quantity and add row
            ws.Cell($"A{x}").Value = "TOTAL";
            ws.Range($"A{x}:B{x}").Merge();
            ws.Range($"A{x}:B{x}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                                      .SetOutsideBorderColor(XLColor.Black);

            ws.Range($"C{x}:G{x}").Merge();
            ws.Range($"C{x}:G{x}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                                  .SetOutsideBorderColor(XLColor.Black);

            var totalQ = stocks.AsEnumerable().Sum(i => i.Field<double>(2));
            ws.Cell($"H{x}").Value = totalQ.ToString();
            ws.Range($"H{x}:I{x}").Merge();
            ws.Range($"H{x}:I{x}").Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin).Border
                                      .SetOutsideBorderColor(XLColor.Black);

            await Task.Delay(0);
        }

        private async Task AdjustContents(IXLWorksheet ws)
        {
            ws.Columns().AdjustToContents(); ws.Rows().AdjustToContents();
            await Task.Delay(0);
        }

        private async Task PageSetting(IXLWorksheet ws, string type)
        {
            switch (type)
            {
                case "Out":
                    ws.PageSetup.SetPageOrientation(XLPageOrientation.Landscape)
                        .SetPaperSize(XLPaperSize.A4Paper)
                        .SetCenterHorizontally(true);
                    break;
                default:
                    ws.PageSetup.SetPageOrientation(XLPageOrientation.Portrait)
                        .SetPaperSize(XLPaperSize.A4Paper)
                        .SetCenterHorizontally(true);
                    break;
            }            

            await Task.Delay(0);
        }


        private static async Task<string> GetAlphabet(int columnCount)
        {
            string strLetter = string.Empty;
            switch (columnCount)
            {
                case 1:
                    strLetter = "A";
                    break;
                case 2:
                    strLetter = "B";
                    break;
                case 3:
                    strLetter = "C";
                    break;
                case 4:
                    strLetter = "D";
                    break;
                case 5:
                    strLetter = "E";
                    break;
                case 6:
                    strLetter = "F";
                    break;
                case 7:
                    strLetter = "G";
                    break;
                case 8:
                    strLetter = "H";
                    break;
                case 9:
                    strLetter = "I";
                    break;
                case 10:
                    strLetter = "J";
                    break;
                case 11:
                    strLetter = "K";
                    break;
                case 12:
                    strLetter = "L";
                    break;
                case 13:
                    strLetter = "M";
                    break;
                case 14:
                    strLetter = "N";
                    break;
                case 15:
                    strLetter = "O";
                    break;
                case 16:
                    strLetter = "P";
                    break;
                case 17:
                    strLetter = "Q";
                    break;
                case 18:
                    strLetter = "R";
                    break;
                case 19:
                    strLetter = "S";
                    break;
                case 20:
                    strLetter = "T";
                    break;
                case 21:
                    strLetter = "U";
                    break;
                case 22:
                    strLetter = "V";
                    break;
                case 23:
                    strLetter = "W";
                    break;
                case 24:
                    strLetter = "X";
                    break;
                case 25:
                    strLetter = "Y";
                    break;
                case 26:
                    strLetter = "Z";
                    break;
            }
            await Task.Delay(0);
            return strLetter;
        }

    }
}
