using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Library.ModelsORM
{
    public class User
    {
        [Key]
        public string? User_Email { get; set; }
        public string? User_Password { get; set; }
        [JsonIgnore]
        public string? User_Position { get; set; }
        [JsonIgnore]
        public List<Book>? Books { get; set; }
    }
}
