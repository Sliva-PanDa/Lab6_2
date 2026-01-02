using System.Collections.Generic;

namespace PublicationPortal.Models
{
    public class Publication
    {
        public int PublicationId { get; set; } // Первичный ключ
        public string Title { get; set; }
        public string Type { get; set; } // статья/тезисы/монография
        public int Year { get; set; }
        public string DoiLink { get; set; }

        // Внешний ключ к таблице Journals
        public int JournalId { get; set; }

        // Навигационное свойство: каждая публикация относится к одному журналу
        public virtual Journal Journal { get; set; }

        // Навигационное свойство для связи "многие-ко-многим" с преподавателями
        public virtual ICollection<PublicationAuthor> PublicationAuthors { get; set; } = new List<PublicationAuthor>();
    }
}