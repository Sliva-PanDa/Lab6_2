using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PublicationPortal.Controllers;
using PublicationPortal.Data;
using PublicationPortal.Dtos;
using PublicationPortal.Models;
using Xunit;

namespace PublicationPortal.Tests
{
    public class PublicationsControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            SeedData(context);
            return context;
        }

        private void SeedData(ApplicationDbContext context)
        {
            var department = new Department { DepartmentId = 1, Name = "ИТ", Profile = "Информационные технологии" };
            var teacher1 = new Teacher { TeacherId = 1, FullName = "Иванов И.И.", Department = department, Position = "Доцент", Degree = "к.т.н." };
            var teacher2 = new Teacher { TeacherId = 2, FullName = "Петров П.П.", Department = department, Position = "Профессор", Degree = "д.т.н." };
            var journal = new Journal { JournalId = 1, Name = "Вестник науки", Rating = "ВАК", Publisher = "Наука-Пресс", IssnIsbn = "1234-5678" };

            context.Departments.Add(department);
            context.Teachers.AddRange(teacher1, teacher2);
            context.Journals.Add(journal);
            context.SaveChanges();

            var publication = new Publication { PublicationId = 1, Title = "Тестовая статья", Type = "Статья", Year = 2024, DoiLink = "doi.org/123", JournalId = journal.JournalId };
            context.Publications.Add(publication);
            context.SaveChanges();

            context.PublicationAuthors.AddRange(
                new PublicationAuthor { PublicationId = publication.PublicationId, TeacherId = teacher1.TeacherId },
                new PublicationAuthor { PublicationId = publication.PublicationId, TeacherId = teacher2.TeacherId }
            );
            context.SaveChanges();
        }

        [Fact]
        public async Task GetPublications_WhenCalled_ReturnsOkResultWithPaginatedResult()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new PublicationsController(context);

            // Act
            var result = await controller.GetPublications(); // Вызываем метод с параметрами пагинации по умолчанию

            // Assert
            // 1. Проверяем, что результат - это 200 OK
            var okResult = Assert.IsType<OkObjectResult>(result.Result);

            // 2. Проверяем, что внутри лежит наш ОБЪЕКТ для пагинации
            var paginatedResult = Assert.IsType<PaginatedResult<PublicationDto>>(okResult.Value);

            // 3. Проверяем свойства самого объекта пагинации
            Assert.Equal(1, paginatedResult.TotalCount); // В наших тестовых данных (SeedData) всего одна публикация

            // 4. Теперь получаем список публикаций из свойства Items
            var publicationsOnPage = paginatedResult.Items;

            // 5. Убеждаемся, что на странице одна запись (так как всего одна запись в БД)
            var publication = Assert.Single(publicationsOnPage);

            // 6. Проверяем содержимое этой единственной публикации, как и раньше
            Assert.Equal(1, publication.PublicationId);
            Assert.Equal("Вестник науки", publication.JournalName);
            Assert.Equal(2, publication.AuthorNames.Count);
            Assert.Contains("Иванов И.И.", publication.AuthorNames);
            Assert.Contains("Петров П.П.", publication.AuthorNames);
        }

        [Fact]
        public async Task GetPublication_WithExistingId_ReturnsOkResultWithPublication()
        {
            var context = GetInMemoryDbContext();
            var controller = new PublicationsController(context);
            var result = await controller.GetPublication(1);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var publication = Assert.IsType<PublicationDto>(okResult.Value);
            Assert.Equal(1, publication.PublicationId);
            Assert.Equal("Тестовая статья", publication.Title);
        }

        [Fact]
        public async Task GetPublication_WithNonExistingId_ReturnsNotFoundResult()
        {
            var context = GetInMemoryDbContext();
            var controller = new PublicationsController(context);
            var result = await controller.GetPublication(999);
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreatePublication_WithValidDto_ReturnsCreatedAtActionResult()
        {
            var context = GetInMemoryDbContext();
            var controller = new PublicationsController(context);
            var newPublicationDto = new PublicationCreateDto { Title = "Новая публикация от теста", Type = "Монография", Year = 2025, DoiLink = "test.doi/1", JournalId = 1, AuthorTeacherIds = new List<int> { 1, 2 } };
            var result = await controller.CreatePublication(newPublicationDto);
            Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(2, await context.Publications.CountAsync());
            var createdPublication = await context.Publications.Include(p => p.PublicationAuthors).FirstOrDefaultAsync(p => p.Title == "Новая публикация от теста");
            Assert.NotNull(createdPublication);
            Assert.Equal(2025, createdPublication.Year);
            Assert.Equal(2, createdPublication.PublicationAuthors.Count);
        }

        [Fact]
        public async Task UpdatePublication_WithExistingId_ReturnsNoContentResult()
        {
            var context = GetInMemoryDbContext();
            var controller = new PublicationsController(context);
            var updateDto = new PublicationCreateDto { Title = "Обновленный заголовок", Type = "Статья", Year = 2026, DoiLink = "updated.doi/2", JournalId = 1, AuthorTeacherIds = new List<int> { 2 } };
            var result = await controller.UpdatePublication(1, updateDto);
            Assert.IsType<NoContentResult>(result);
            var updatedPublication = await context.Publications.Include(p => p.PublicationAuthors).FirstOrDefaultAsync(p => p.PublicationId == 1);
            Assert.NotNull(updatedPublication);
            Assert.Equal("Обновленный заголовок", updatedPublication.Title);
            Assert.Equal(2026, updatedPublication.Year);
            Assert.Single(updatedPublication.PublicationAuthors);
            Assert.Equal(2, updatedPublication.PublicationAuthors.First().TeacherId);
        }

        [Fact]
        public async Task DeletePublication_WithExistingId_ReturnsNoContentResult()
        {
            var context = GetInMemoryDbContext();
            var controller = new PublicationsController(context);
            Assert.Single(context.Publications);
            var result = await controller.DeletePublication(1);
            Assert.IsType<NoContentResult>(result);
            Assert.Empty(context.Publications);
            var found = await context.Publications.FindAsync(1);
            Assert.Null(found);
        }
    }
}