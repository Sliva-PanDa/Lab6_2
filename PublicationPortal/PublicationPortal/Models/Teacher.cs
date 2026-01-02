using System.Collections.Generic;

namespace PublicationPortal.Models
{
    public class Teacher
    {
        public int TeacherId { get; set; } // Первичный ключ
        public string FullName { get; set; }
        public string Position { get; set; }
        public string Degree { get; set; }

        // Внешний ключ (Foreign Key) к таблице Departments
        public int DepartmentId { get; set; }

        // Навигационное свойство: каждый преподаватель принадлежит одной кафедре
        public virtual Department Department { get; set; }

        // Навигационное свойство для связи "многие-ко-многим" с публикациями
        public virtual ICollection<PublicationAuthor> PublicationAuthors { get; set; }
    }
}