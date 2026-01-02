namespace PublicationPortal.Dtos
{
    // Этот класс мы будем отправлять клиенту при запросе публикаций
    public class PublicationDto
    {
        public int PublicationId { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public int Year { get; set; }
        public string DoiLink { get; set; }

        // Связанные данные в удобном виде
        public string JournalName { get; set; }
        public List<string> AuthorNames { get; set; }

        public int JournalId { get; set; }
        public List<int> AuthorTeacherIds { get; set; }
    }
}