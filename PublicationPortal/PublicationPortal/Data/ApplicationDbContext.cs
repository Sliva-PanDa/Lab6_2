using Microsoft.EntityFrameworkCore;
using PublicationPortal.Models;

namespace PublicationPortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Каждое свойство DbSet<T> будет соответствовать таблице в базе данных
        public DbSet<Department> Departments { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Journal> Journals { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<PublicationAuthor> PublicationAuthors { get; set; }

        // Метод для дополнительной конфигурации моделей
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Здесь мы настраиваем составной первичный ключ для связующей таблицы
            modelBuilder.Entity<PublicationAuthor>()
                .HasKey(pa => new { pa.PublicationId, pa.TeacherId });

            // Настраиваем связь "многие-ко-многим" через PublicationAuthor
            // Указываем, что у PublicationAuthor есть один Publication...
            modelBuilder.Entity<PublicationAuthor>()
                .HasOne(pa => pa.Publication)
                .WithMany(p => p.PublicationAuthors) // ...у которого много PublicationAuthors
                .HasForeignKey(pa => pa.PublicationId); // и внешний ключ - PublicationId

            // И что у PublicationAuthor есть один Teacher...
            modelBuilder.Entity<PublicationAuthor>()
                .HasOne(pa => pa.Teacher)
                .WithMany(t => t.PublicationAuthors) // ...у которого много PublicationAuthors
                .HasForeignKey(pa => pa.TeacherId); // и внешний ключ - TeacherId
        }
    }
}