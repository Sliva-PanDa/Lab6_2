using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicationPortal.Data;
using PublicationPortal.Dtos;
using PublicationPortal.Models;

namespace PublicationPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PublicationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PublicationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // READ ALL WITH PAGINATION: GET /api/publications?pageNumber=1&pageSize=10
        [HttpGet]
        public async Task<ActionResult<PaginatedResult<PublicationDto>>> GetPublications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            // 1. Сначала считаем общее количество записей для пагинации
            var totalCount = await _context.Publications.CountAsync();

            // 2. Получаем нужный "срез" данных из БД
            var publications = await _context.Publications
                .Include(p => p.Journal)
                .Include(p => p.PublicationAuthors)
                    .ThenInclude(pa => pa.Teacher)
                .OrderByDescending(p => p.Year).ThenBy(p => p.Title) // Сортировка для стабильного порядка
                .Skip((pageNumber - 1) * pageSize) // Пропускаем записи предыдущих страниц
                .Take(pageSize) // Берем нужное количество записей
                .Select(p => new PublicationDto
                {
                    PublicationId = p.PublicationId,
                    Title = p.Title,
                    Type = p.Type,
                    Year = p.Year,
                    DoiLink = p.DoiLink,
                    JournalId = p.JournalId,
                    JournalName = p.Journal.Name,
                    AuthorTeacherIds = p.PublicationAuthors.Select(pa => pa.TeacherId).ToList(),
                    AuthorNames = p.PublicationAuthors.Select(pa => pa.Teacher.FullName).ToList()
                })
                .ToListAsync();

            // 3. Формируем и возвращаем результат
            var result = new PaginatedResult<PublicationDto>
            {
                Items = publications,
                TotalCount = totalCount
            };

            return Ok(result);
        }

        // READ ONE: GET /api/publications/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PublicationDto>> GetPublication(int id)
        {
            var publicationDto = await _context.Publications
                .Where(p => p.PublicationId == id) // Фильтруем сначала для производительности
                .Include(p => p.Journal)
                .Include(p => p.PublicationAuthors)
                    .ThenInclude(pa => pa.Teacher)
                .Select(p => new PublicationDto
                {
                    PublicationId = p.PublicationId,
                    Title = p.Title,
                    Type = p.Type,
                    Year = p.Year,
                    DoiLink = p.DoiLink,
                    // Заполняем новые и старые поля
                    JournalId = p.JournalId,
                    JournalName = p.Journal.Name,
                    AuthorTeacherIds = p.PublicationAuthors.Select(pa => pa.TeacherId).ToList(),
                    AuthorNames = p.PublicationAuthors.Select(pa => pa.Teacher.FullName).ToList()
                })
                .FirstOrDefaultAsync(); // Используем FirstOrDefaultAsync без аргументов

            if (publicationDto == null)
            {
                return NotFound(); // Возвращаем 404, если не найдено
            }

            return Ok(publicationDto);
        }

        // CREATE: POST /api/publications
        [HttpPost]
        public async Task<ActionResult<PublicationDto>> CreatePublication(PublicationCreateDto createDto)
        {
            // 1. Создаем новую сущность Publication
            var publication = new Publication
            {
                Title = createDto.Title,
                Type = createDto.Type,
                Year = createDto.Year,
                DoiLink = createDto.DoiLink,
                JournalId = createDto.JournalId
                // PublicationAuthors инициализируется в модели
            };

            // 2. Добавляем связи с авторами
            foreach (var teacherId in createDto.AuthorTeacherIds)
            {
                publication.PublicationAuthors.Add(new PublicationAuthor { TeacherId = teacherId });
            }

            _context.Publications.Add(publication);
            await _context.SaveChangesAsync();

            // --- ИСПРАВЛЕНИЕ ЗДЕСЬ ---
            // 3. Вместо возврата "сырого" объекта publication, мы запрашиваем его DTO-версию
            // Мы можем просто вызвать наш же метод GetPublication, который уже умеет делать правильный DTO
            var createdPublicationDtoResult = await GetPublication(publication.PublicationId);

            // Убедимся, что GetPublication вернул успешный результат
            if (!(createdPublicationDtoResult.Result is OkObjectResult okResult))
            {
                // Если что-то пошло не так при получении DTO, возвращаем ошибку
                return Problem("Не удалось получить данные для созданной публикации.");
            }
            var createdDto = (PublicationDto)okResult.Value;

            // 4. Возвращаем правильный ответ с DTO внутри
            return CreatedAtAction(nameof(GetPublication), new { id = publication.PublicationId }, createdDto);
        }


        // UPDATE: PUT /api/publications/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePublication(int id, PublicationCreateDto updateDto)
        {
            var publication = await _context.Publications
                .Include(p => p.PublicationAuthors)
                .FirstOrDefaultAsync(p => p.PublicationId == id);

            if (publication == null)
            {
                return NotFound();
            }

            // Обновляем основные поля
            publication.Title = updateDto.Title;
            publication.Type = updateDto.Type;
            publication.Year = updateDto.Year;
            publication.DoiLink = updateDto.DoiLink;
            publication.JournalId = updateDto.JournalId;

            // Полностью обновляем список авторов
            publication.PublicationAuthors.Clear();
            foreach (var teacherId in updateDto.AuthorTeacherIds)
            {
                publication.PublicationAuthors.Add(new PublicationAuthor { TeacherId = teacherId });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Обработка ошибок параллелизма, если необходимо
                throw;
            }

            return NoContent(); // Возвращаем 204 No Content - стандарт для успешного PUT
        }

        // DELETE: DELETE /api/publications/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePublication(int id)
        {
            var publication = await _context.Publications.FindAsync(id);
            if (publication == null)
            {
                return NotFound();
            }

            _context.Publications.Remove(publication);
            await _context.SaveChangesAsync();

            return NoContent(); // Возвращаем 204 No Content
        }
    }
}