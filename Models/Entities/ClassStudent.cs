namespace MalikongkongNHS.Models.Entities
{
    public class ClassStudent
    {
        public int Id { get; set; }
 
        public int ClassId { get; set; }
        public int StudentId { get; set; }
 
        public ClassEntity? Class { get; set; }
        public Student? Student { get; set; }
    }
}