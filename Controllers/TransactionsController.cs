using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Reporting;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowroomAPI.Context;
using ShowroomAPI.Models;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;

namespace ShowroomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TransactionsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/Transactions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Transaction>>> GetTransactions()
        {
            return await _context.Transactions.Include(s => s.Staff)
                                              .Include(t => t.TransactionDetails)                                               
                                              .ThenInclude(p => p.ProductNoNavigation)                                              
                                              .ToListAsync();
        }

        // GET: api/Transactions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Transaction>> GetTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }
        [HttpGet("receipt/{receiptNo}")]        
        public async Task<ActionResult> GetReceipt(string receiptNo)
        {
            var templatePath = Path.Combine(_env.ContentRootPath, "Reports", "reportReceipt.rdlc");
                
            var transaction = await _context.Transactions.Include(s => s.Staff)
                                              .Include(t => t.TransactionDetails)
                                              .ThenInclude(p => p.ProductNoNavigation)
                                              .Where(i => i.InvoiceNo == receiptNo)
                                              .FirstOrDefaultAsync();
            
            var reportData = GetDataTableAsync(reportOption:"receipt", model:transaction);

            LocalReport localReport = new LocalReport(templatePath);
            localReport.AddDataSource("Receipt", reportData);
            var result = localReport.Execute(RenderType.Pdf, 1, null, "");            
            return File(result.MainStream, "application/pdf");
        }        
        // PUT: api/Transactions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTransaction(int id, Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return BadRequest();
            }

            _context.Entry(transaction).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TransactionExists(id))
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

        // POST: api/Transactions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Transaction>> PostTransaction(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTransaction", new { id = transaction.Id }, transaction);
        }

        // DELETE: api/Transactions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.Id == id);
        }

        public static DataTable GetDataTableAsync(string reportOption, IEnumerable<Product> products =null, Transaction model = null)
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
            return dt;
        }
    }
}
