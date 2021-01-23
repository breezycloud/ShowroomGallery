using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShowroomAPI.Context;
using ShowroomAPI.Models;

namespace ShowroomAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockInsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StockInsController(AppDbContext context)
        {
            _context = context;
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
    }
}
