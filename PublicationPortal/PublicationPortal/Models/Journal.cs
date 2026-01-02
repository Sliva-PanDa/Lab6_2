using System.Collections.Generic;

namespace PublicationPortal.Models
{
    public class Journal
    {
        public int JournalId { get; set; } // Первичный ключ
        public string Name { get; set; }
        public string Rating { get; set; }
        public string Publisher { get; set; }
        public string IssnIsbn { get; set; }

        // Навигационное свойство: в одном журнале может быть много публикаций
        public virtual ICollection<Publication> Publications { get; set; }
    }
}