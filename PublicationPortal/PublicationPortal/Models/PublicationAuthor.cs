namespace PublicationPortal.Models
{
    public class PublicationAuthor
    {
        // Составной первичный ключ 
        public int PublicationId { get; set; }
        public int TeacherId { get; set; }

        // Навигационные свойства
        public virtual Publication Publication { get; set; }
        public virtual Teacher Teacher { get; set; }
    }
}