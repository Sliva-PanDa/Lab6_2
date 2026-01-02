using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicationPortal.Data;

namespace PublicationPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TeachersController(ApplicationDbContext context) { _context = context; }

        [HttpGet]
        public async Task<IActionResult> GetTeachers()
        {
            var teachers = await _context.Teachers
                .Select(t => new { t.TeacherId, t.FullName }) // Отдаем только ID и ФИО
                .ToListAsync();
            return Ok(teachers);
        }
    }
}