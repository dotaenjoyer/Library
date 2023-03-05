using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Library.ModelsORM
{
    public class Author
    {
        [Key]
        public int Author_Id { get; set; }
        public string? Author_Name { get; set; }
        [JsonIgnore]
        public List<Book>? Books { get; set; }
    }
}
