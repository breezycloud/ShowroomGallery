using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowroomAPI.Context;
using ShowroomAPI.Models;
using Microsoft.AspNetCore.Hosting;
using ClosedXML.Report;
using System;

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
            var templatePath = Path.Combine(_env.ContentRootPath, "Reports", "Receipt.xlsx");
                
            var transaction = await _context.Transactions.Include(s => s.Staff)
                                              .Include(t => t.TransactionDetails)
                                              .ThenInclude(p => p.ProductNoNavigation)
                                              .Where(i => i.InvoiceNo == receiptNo)
                                              .FirstOrDefaultAsync();

            var order = GetOrder(transaction);

            var template = new XLTemplate(templatePath);
            template.AddVariable(order);
            template.Generate();

            using var ms = new MemoryStream();
            template.SaveAs(ms);

            var result = ms.ToArray();
            
            return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    $"Receipt {transaction.InvoiceNo}.xlsx");
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

        private Order GetOrder(Transaction transaction)
        {
            Order order = new Order
            {
                InvoiceNo = transaction.InvoiceNo,
                OrderDate = transaction.Tdate.ToShortDateString(),
                Cashier = transaction.Staff.FirstName,
                TotalAmount = transaction.TotalAmount,
                SubTotal = transaction.SubTotal,
                Discount = transaction.Discount
            };
            foreach (var item in transaction.TransactionDetails)
            {
                OrderItems orderItems = new OrderItems()
                {
                    Quantity = item.Quantity,
                    ProductName = item.ProductNoNavigation.ProductCode,
                    ItemPrice = item.ItemPrice
                };
                order.OrderItems.Add(orderItems);
            }            


            return order;
        }

        private List<Order> GetOrder(List<Transaction> transaction)
        {
            List<Order> orders = new List<Order>();

            foreach (var item in transaction)
            {
                Order order = new Order
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
                        ItemPrice = o.ItemPrice
                    };
                    order.OrderItems.Add(orderItems);
                }
            }


            return orders;
        }

    }
}
