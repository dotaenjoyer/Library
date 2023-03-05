using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace Library.ModelsORM
{
    public class Genre
    {
        [Key]
        public int Genre_Id { get; set; }
        public string? Genre_Name { get; set; }
        public List<Book>? Books { get; set; }
    }   
}
