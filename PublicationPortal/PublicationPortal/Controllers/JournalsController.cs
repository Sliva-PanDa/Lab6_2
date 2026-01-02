using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicationPortal.Data;

namespace PublicationPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JournalsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public JournalsController(ApplicationDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetJournals()
        {
            var journals = await _context.Journals
                .Select(j => new { j.JournalId, j.Name }) // Отдаем только ID и имя
                .ToListAsync();
            return Ok(journals);
        }
    }
}