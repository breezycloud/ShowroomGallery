using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowroomAPI.Context;
using ShowroomAPI.Models;
using ClosedXML.Excel;
using System.Collections.Generic;
using System.Data;
using AspNetCore.Reporting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Cors;

namespace ShowroomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnv;    
        public ReportsController(AppDbContext context, IWebHostEnvironment webHostEnv)
        {
            _context = context;
            _webHostEnv = webHostEnv;
        }
        
        [HttpGet("receipt/{receiptNo}")]
        public async Task<IActionResult> GetReceipt(string receiptNo)
        {
            var templatePath = @"Reports/reportReceipt.rdlc";  
            //var templatePath = $@"{this._webHostEnv.ContentRootPath}\Reports\reportReceipt.rdlc";          
            var transaction = await _context.Transactions.Include(t => t.TransactionDetails)
                                              .ThenInclude(p => p.ProductNoNavigation)
                                              .Where(i => i.InvoiceNo == receiptNo)
                                              .FirstOrDefaultAsync();
            
            var reportData = await GetDataTableAsync(reportOption:"receipt", model:transaction);

            LocalReport localReport = new LocalReport(templatePath);
            localReport.AddDataSource("Receipt", reportData);
            var result = localReport.Execute(RenderType.Pdf, 1, null, "");
            await Task.Delay(0);
            return File(result.MainStream, "application/pdf");
        }
        
        [HttpGet("products")]
        public async Task<ActionResult<byte[]>> ExportProducts()
        {
            var productList = await _context.Products.Include(c => c.CategoryNoNavigation).ToListAsync();
            //var reportData = await GetDataTableAsync(reportOption: "products", products:productList);

            DataTable dt = new DataTable();
            dt.Columns.Add("Model");
            dt.Columns.Add("Name");
            dt.Columns.Add("Description");
            dt.Columns.Add("Category");
            dt.Columns.Add("Quantity");
            dt.Columns.Add("Cost");

            foreach (var item in productList)
            {
                DataRow productsRow = dt.NewRow();
                productsRow["Model"] = item.ModelNo;
                productsRow["Name"] = item.ProductCode;
                productsRow["Description"] = item.Description;
                productsRow["Category"] = item.CategoryNoNavigation.CategoryName;
                productsRow["Quantity"] = item.StocksOnHand;
                productsRow["Cost"] = $"{item.UnitPrice:N}";
                dt.Rows.Add(productsRow);
            }

            var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(dt, "Available Products");
            ws.Rows().AdjustToContents();
            ws.Columns().AdjustToContents();
            

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            var content = stream.ToArray();

            return content;
            // return File(content,
                        //  "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        //  "Products.xlsx");
        }


        public static async Task<DataTable> GetDataTableAsync(string reportOption, IEnumerable<Product> products =null, Transaction model = null)
        {
            DataTable dt = new DataTable();
            switch (reportOption)
            {
                case "receipt":                    
                    dt.Columns.Add("InvoiceNo");
                    dt.Columns.Add("TDate");
                    dt.Columns.Add("Quantity");
                    dt.Columns.Add("ProductCode");
                    dt.Columns.Add("ItemAmount");
                    dt.Columns.Add("TotalAmount");
                    dt.Columns.Add("SubTotal");
                    dt.Columns.Add("Discount");
                    dt.Columns.Add("StaffName");
                    
                    DataRow row = dt.NewRow();
                    row["InvoiceNo"] = model.InvoiceNo;
                    row["TDate"] = model.Tdate.ToShortDateString();
                    foreach (var transactionDetail in model.TransactionDetails)
                    {                        
                        row["Quantity"] = transactionDetail.Quantity;
                        row["ProductCode"] = transactionDetail.ProductNoNavigation.ProductCode;
                        row["ItemAmount"] = $"{transactionDetail.ItemPrice:N}";
                    }
                    row["TotalAmount"] = $"{model.TotalAmount:N}";
                    row["SubTotal"] = $"{model.SubTotal:N}";
                    row["Discount"] = $"{model.Discount:N}";
                    row["StaffName"] = $"{model.Staff.FirstName} {model.Staff.LastName}";
                    dt.Rows.Add(row);
                    break;

                case "products":
                    dt.Columns.Add("Model");
                    dt.Columns.Add("Name");
                    dt.Columns.Add("Description");
                    dt.Columns.Add("Category");
                    dt.Columns.Add("Quantity");
                    dt.Columns.Add("Cost");

                    foreach (var item in products)
                    {
                        DataRow productsRow = dt.NewRow();
                        productsRow["Model"] = item.ModelNo;
                        productsRow["Name"] = item.ProductCode;
                        productsRow["Description"] = item.Description;
                        productsRow["Category"] = item.CategoryNoNavigation.CategoryName;
                        productsRow["Quantity"] = item.StocksOnHand;
                        productsRow["Cost"] = $"{item.UnitPrice:N}";
                        dt.Rows.Add(productsRow);
                    }
                    break;
            }
            await Task.Delay(0);            
            return dt;
        }
    }
}