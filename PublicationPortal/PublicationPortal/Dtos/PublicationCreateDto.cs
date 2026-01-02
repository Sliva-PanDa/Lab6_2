namespace PublicationPortal.Dtos
{
    // Этот класс клиент будет присылать нам для создания или обновления публикации
    public class PublicationCreateDto
    {
        public string Title { get; set; }
        public string Type { get; set; }
        public int Year { get; set; }
        public string DoiLink { get; set; }
        public int JournalId { get; set; } // ID журнала
        public List<int> AuthorTeacherIds { get; set; } // Список ID преподавателей-авторов

    }
}