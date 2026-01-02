using Microsoft.EntityFrameworkCore;
using PublicationPortal.Data;
using PublicationPortal.Models;

var builder = WebApplication.CreateBuilder(args);
// 1. Получаем строку подключения из appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// 2. Добавляем ApplicationDbContext в контейнер сервисов
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// --- Блок для заполнения БД начальными данными ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        if (!context.Publications.Any()) // Проверяем, есть ли уже публикации
        {
            // --- 1. Создаем базовые сущности ---
            var itDept = new Department { Name = "Информационные технологии", Profile = "Разработка ПО и систем" };
            var csDept = new Department { Name = "Компьютерная безопасность", Profile = "Защита информации" };
            context.Departments.AddRange(itDept, csDept);

            var teachers = new List<Teacher>
            {
                new Teacher { FullName = "Иванов И. И.", Department = itDept, Position = "Доцент", Degree = "к.т.н." },
                new Teacher { FullName = "Петров П. П.", Department = itDept, Position = "Профессор", Degree = "д.т.н." },
                new Teacher { FullName = "Сидоров С. С.", Department = csDept, Position = "Ст. преподаватель", Degree = "магистр" },
                new Teacher { FullName = "Кузнецова А. В.", Department = csDept, Position = "Ассистент", Degree = "—" },
                new Teacher { FullName = "Смирнов А. Е.", Department = itDept, Position = "Доцент", Degree = "к.т.н." },
                new Teacher { FullName = "Васильева О. Н.", Department = csDept, Position = "Профессор", Degree = "д.ф.-м.н." }
            };
            context.Teachers.AddRange(teachers);

            var journals = new List<Journal>
            {
                new Journal { Name = "Вестник современной науки", Rating = "ВАК", Publisher = "Издательство 'Наука'", IssnIsbn = "1234-5678" },
                new Journal { Name = "IT-Conf Proceedings", Rating = "Международная", Publisher = "TechEvents", IssnIsbn = "9876-5432" },
                new Journal { Name = "Кибернетика и программирование", Rating = "Scopus", Publisher = "Cybernetics Inc.", IssnIsbn = "5555-4444" },
                new Journal { Name = "Вопросы философии", Rating = "РИНЦ", Publisher = "Академия", IssnIsbn = "1111-2222" }
            };
            context.Journals.AddRange(journals);

            context.SaveChanges(); // Сохраняем, чтобы получить ID

            // --- 2. Генерируем большое количество публикаций ---
            var publications = new List<Publication>();
            var publicationAuthors = new List<PublicationAuthor>();
            var random = new Random();
            string[] types = { "Статья", "Тезисы", "Монография" };

            for (int i = 1; i <= 100; i++)
            {
                var publication = new Publication
                {
                    Title = $"Научная работа №{i}",
                    Type = types[random.Next(types.Length)],
                    Year = 2020 + random.Next(5), // Годы от 2020 до 2024
                    DoiLink = $"https://doi.org/10.1000/xyz{i}",
                    JournalId = journals[random.Next(journals.Count)].JournalId
                };
                publications.Add(publication);
            }
            context.Publications.AddRange(publications);
            context.SaveChanges(); // Сохраняем публикации, чтобы они получили свои ID

            // --- 3. Создаем связи авторов с публикациями ---
            foreach (var pub in publications)
            {
                // Каждой публикации назначаем от 1 до 3 случайных авторов
                var authorCount = random.Next(1, 4);
                var assignedTeachers = teachers.OrderBy(t => random.Next()).Take(authorCount).ToList();

                foreach (var teacher in assignedTeachers)
                {
                    publicationAuthors.Add(new PublicationAuthor { PublicationId = pub.PublicationId, TeacherId = teacher.TeacherId });
                }
            }
            context.PublicationAuthors.AddRange(publicationAuthors);
            context.SaveChanges(); // Сохраняем связи
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// --- Конец блока ---

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseDefaultFiles(); // Эта строка будет искать index.html или default.html как стартовую
app.UseStaticFiles(); // Эта строка разрешает доступ к файлам в wwwroot

app.UseAuthorization();

app.MapControllers();

app.Run();
