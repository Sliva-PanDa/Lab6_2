using System.Collections.Generic;

namespace PublicationPortal.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Profile { get; set; }

        // Навигационное свойство: у одной кафедры может быть много преподавателей
        public virtual ICollection<Teacher> Teachers { get; set; }
    }
}